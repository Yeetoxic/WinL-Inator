namespace WinL_Inator;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        BuildMainWindow();
        //
        // pos
        //
        Shown += (_, _) =>
        {
            var workingArea = Screen.FromControl(this).WorkingArea;

            Location = new Point(
                workingArea.Right - Width - 10,
                workingArea.Top + 10
            );
        };
    }

    private void BuildMainWindow()
    {
        var windowLayout = new TableLayoutPanel();
        var messageLayout = new TableLayoutPanel();
        var titleLabel = new Label();
        var messageLabel = new Label();
        var hintLabel = new Label();
        windowLayout.SuspendLayout();
        messageLayout.SuspendLayout();
        SuspendLayout();
        //
        // windowLayout
        //
        windowLayout.ColumnCount = 1;
        windowLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        windowLayout.Controls.Add(messageLayout, 0, 0);
        windowLayout.Dock = DockStyle.Fill;
        windowLayout.Name = "windowLayout";
        windowLayout.Padding = new Padding(24);
        windowLayout.RowCount = 1;
        windowLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        windowLayout.TabIndex = 0;
        //
        // messageLayout
        //
        messageLayout.Anchor = AnchorStyles.None;
        messageLayout.BorderStyle = BorderStyle.FixedSingle;
        messageLayout.ColumnCount = 1;
        messageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        messageLayout.Controls.Add(titleLabel, 0, 0);
        messageLayout.Controls.Add(messageLabel, 0, 1);
        messageLayout.Controls.Add(hintLabel, 0, 2);
        messageLayout.Name = "messageLayout";
        messageLayout.Padding = new Padding(16);
        messageLayout.RowCount = 3;
        messageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
        messageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
        messageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
        messageLayout.Size = new Size(480, 220);
        messageLayout.TabIndex = 0;
        //
        // titleLabel
        //
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.Name = "titleLabel";
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Ever try Win+L?";
        titleLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // messageLabel
        //
        messageLabel.Dock = DockStyle.Fill;
        messageLabel.Name = "messageLabel";
        messageLabel.TabIndex = 1;
        messageLabel.Text = "How about an alphabetized keyboard too? >:)";
        messageLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // hintLabel
        //
        hintLabel.Dock = DockStyle.Fill;
        hintLabel.Name = "hintLabel";
        hintLabel.TabIndex = 2;
        hintLabel.Text = "Alt+1 to remove this from your system";
        hintLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(8F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(640, 360);
        Controls.Add(windowLayout);
        Font = new Font("Consolas", 11F, FontStyle.Regular, GraphicsUnit.Point);
        ForeColor = Color.Gainsboro;
        MinimumSize = new Size(560, 320);
        Name = "Form1";
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        Text = "WinL-Inator";
        windowLayout.ResumeLayout(false);
        messageLayout.ResumeLayout(false);
        ResumeLayout(false);
    }

}
