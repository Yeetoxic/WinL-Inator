using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WinL_Inator;

public sealed class KeyboardAlphabetizer : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;
    private const uint LLKHF_INJECTED = 0x10;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const string QwertyLetters = "QWERTYUIOPASDFGHJKLZXCVBNM";

    private IntPtr hookHandle = IntPtr.Zero;
    // Remember the choice until key-up, even if modifiers change while held.
    private readonly Dictionary<uint, ushort> heldKeys = new();
    private bool disposed;

    // Keep this alive. Otherwise the GC can collect our callback.
    private readonly LowLevelKeyboardProc hookProc;

    public KeyboardAlphabetizer()
    {
        hookProc = HookCallback;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (hookHandle != IntPtr.Zero)
            return;

        // Call Start/Stop on the UI thread, which pumps the hook's messages.
        IntPtr moduleHandle = GetModuleHandle(null);

        hookHandle = SetWindowsHookEx(
            WH_KEYBOARD_LL,
            hookProc,
            moduleHandle,
            0
        );

        if (hookHandle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not install keyboard hook.");
    }

    public void Stop()
    {
        if (hookHandle == IntPtr.Zero)
            return;

        if (!UnhookWindowsHookEx(hookHandle))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not remove keyboard hook.");

        hookHandle = IntPtr.Zero;
        // Release any synthetic keys still down when the app closes.
        foreach (var key in heldKeys)
        {
            if (key.Key != key.Value)
                SendKey(key.Value, keyUp: true);
        }
        heldKeys.Clear();
    }

    public void Dispose()
    {
        Stop();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code < 0)
            return CallNextHookEx(hookHandle, code, wParam, lParam);

        int message = wParam.ToInt32();
        bool keyDown = message is WM_KEYDOWN or WM_SYSKEYDOWN;
        bool keyUp = message is WM_KEYUP or WM_SYSKEYUP;
        if (!keyDown && !keyUp)
            return CallNextHookEx(hookHandle, code, wParam, lParam);

        var data = Marshal.PtrToStructure<KeyboardHookData>(lParam);
        // Our SendInput events must not be remapped recursively.
        if ((data.Flags & LLKHF_INJECTED) != 0 || data.VirtualKey < 'A' || data.VirtualKey > 'Z')
            return CallNextHookEx(hookHandle, code, wParam, lParam);

        bool alreadyHeld = heldKeys.TryGetValue(data.VirtualKey, out ushort mappedKey);
        if (!alreadyHeld)
        {
            if (keyUp)
                return CallNextHookEx(hookHandle, code, wParam, lParam);

            // Keep shortcuts such as Ctrl+C, Alt+F4 and Win+L intact.
            mappedKey = ShortcutModifierDown()
                ? (ushort)data.VirtualKey
                : MapLetter(data.VirtualKey);
        }

        if (mappedKey == data.VirtualKey)
        {
            if (keyDown)
                heldKeys[data.VirtualKey] = mappedKey;
            else
                heldKeys.Remove(data.VirtualKey);
            return CallNextHookEx(hookHandle, code, wParam, lParam);
        }

        if (SendKey(mappedKey, keyUp))
        {
            if (keyDown)
                heldKeys[data.VirtualKey] = mappedKey;
            else
                heldKeys.Remove(data.VirtualKey);
            return new IntPtr(1);
        }

        // If injection fails on the first down, preserve the original key pair.
        if (!alreadyHeld)
        {
            heldKeys[data.VirtualKey] = (ushort)data.VirtualKey;
            return CallNextHookEx(hookHandle, code, wParam, lParam);
        }

        // Retain a failed synthetic release for a retry during Stop().
        return new IntPtr(1);
    }

    private static ushort MapLetter(uint virtualKey)
    {
        int index = QwertyLetters.IndexOf((char)virtualKey);
        return index < 0 ? (ushort)virtualKey : (ushort)('A' + index);
    }

    private static bool ShortcutModifierDown() =>
        IsKeyDown(0x11) || IsKeyDown(0x12) || IsKeyDown(0x5B) || IsKeyDown(0x5C);

    private static bool IsKeyDown(int virtualKey) => (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    private static bool SendKey(ushort virtualKey, bool keyUp)
    {
        var input = new Input
        {
            Type = 1, // INPUT_KEYBOARD
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = virtualKey,
                    Flags = keyUp ? KEYEVENTF_KEYUP : 0
                }
            }
        };
        return SendInput(1, new[] { input }, Marshal.SizeOf<Input>()) == 1;
    }

    private delegate IntPtr LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardHookData
    {
        public uint VirtualKey;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KeyboardInput Keyboard;
        // INPUT's union must accommodate MOUSEINPUT, including on x64.
        [FieldOffset(0)] public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr module, uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? moduleName);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, Input[] inputs, int size);
}
