namespace WinL_Inator;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        using var alphabetizer = new KeyboardAlphabetizer();
        using var mainWindow = new Form1();
        mainWindow.Shown += (_, _) =>
        {
            try
            {
                alphabetizer.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(mainWindow, ex.Message, "Keyboard alphabetizer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        // Disposing the alphabetizer after the message loop removes the hook.
        Application.Run(mainWindow);
    }    
}
