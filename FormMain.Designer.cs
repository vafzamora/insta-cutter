namespace insta_cutter;

partial class FormMain
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private MenuStrip menuStrip1;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem loadToolStripMenuItem;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem viewToolStripMenuItem;
    private ToolStripMenuItem themeToolStripMenuItem;
    private ToolStripMenuItem lightThemeToolStripMenuItem;
    private ToolStripMenuItem darkThemeToolStripMenuItem;
    private ToolStripMenuItem systemThemeToolStripMenuItem;
    private PictureBox pictureBox1;
    private OpenFileDialog openFileDialog1;
    private SaveFileDialog saveFileDialog1;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        menuStrip1 = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        loadToolStripMenuItem = new ToolStripMenuItem();
        saveToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        viewToolStripMenuItem = new ToolStripMenuItem();
        themeToolStripMenuItem = new ToolStripMenuItem();
        lightThemeToolStripMenuItem = new ToolStripMenuItem();
        darkThemeToolStripMenuItem = new ToolStripMenuItem();
        systemThemeToolStripMenuItem = new ToolStripMenuItem();
        pictureBox1 = new PictureBox();
        openFileDialog1 = new OpenFileDialog();
        saveFileDialog1 = new SaveFileDialog();
        menuStrip1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(800, 24);
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadToolStripMenuItem, saveToolStripMenuItem, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "&File";
        // 
        // loadToolStripMenuItem
        // 
        loadToolStripMenuItem.Name = "loadToolStripMenuItem";
        loadToolStripMenuItem.Size = new Size(100, 22);
        loadToolStripMenuItem.Text = "&Load";
        loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
        // 
        // saveToolStripMenuItem
        // 
        saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        saveToolStripMenuItem.Size = new Size(100, 22);
        saveToolStripMenuItem.Text = "&Save";
        saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(100, 22);
        exitToolStripMenuItem.Text = "E&xit";
        exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
        // 
        // viewToolStripMenuItem
        // 
        viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { themeToolStripMenuItem });
        viewToolStripMenuItem.Name = "viewToolStripMenuItem";
        viewToolStripMenuItem.Size = new Size(44, 20);
        viewToolStripMenuItem.Text = "&View";
        // 
        // themeToolStripMenuItem
        // 
        themeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { lightThemeToolStripMenuItem, darkThemeToolStripMenuItem, systemThemeToolStripMenuItem });
        themeToolStripMenuItem.Name = "themeToolStripMenuItem";
        themeToolStripMenuItem.Size = new Size(109, 22);
        themeToolStripMenuItem.Text = "&Theme";
        // 
        // lightThemeToolStripMenuItem
        // 
        lightThemeToolStripMenuItem.Name = "lightThemeToolStripMenuItem";
        lightThemeToolStripMenuItem.Size = new Size(114, 22);
        lightThemeToolStripMenuItem.Text = "&Light";
        lightThemeToolStripMenuItem.Click += lightThemeToolStripMenuItem_Click;
        // 
        // darkThemeToolStripMenuItem
        // 
        darkThemeToolStripMenuItem.Name = "darkThemeToolStripMenuItem";
        darkThemeToolStripMenuItem.Size = new Size(114, 22);
        darkThemeToolStripMenuItem.Text = "&Dark";
        darkThemeToolStripMenuItem.Click += darkThemeToolStripMenuItem_Click;
        // 
        // systemThemeToolStripMenuItem
        // 
        systemThemeToolStripMenuItem.Checked = true;
        systemThemeToolStripMenuItem.Name = "systemThemeToolStripMenuItem";
        systemThemeToolStripMenuItem.Size = new Size(114, 22);
        systemThemeToolStripMenuItem.Text = "&System";
        systemThemeToolStripMenuItem.Click += systemThemeToolStripMenuItem_Click;
        // 
        // pictureBox1
        // 
        pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pictureBox1.Location = new Point(12, 37);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(776, 401);
        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox1.TabIndex = 1;
        pictureBox1.TabStop = false;
        // 
        // openFileDialog1
        // 
        openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|JPEG Files|*.jpg;*.jpeg|PNG Files|*.png|BMP Files|*.bmp";
        openFileDialog1.Title = "Select an Image File";
        // 
        // saveFileDialog1
        // 
        saveFileDialog1.Filter = "PNG Files|*.png|JPEG Files|*.jpg|BMP Files|*.bmp";
        saveFileDialog1.Title = "Save Cropped Images";
        saveFileDialog1.DefaultExt = "png";
        // 
        // FormMain
        // 
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(pictureBox1);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "FormMain";
        Text = "FormMain";
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}