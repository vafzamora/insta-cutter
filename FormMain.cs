using Microsoft.Win32;

namespace insta_cutter;

public partial class FormMain : Form
{
    private enum ThemeMode
    {
        Light,
        Dark,
        System
    }

    private ThemeMode currentTheme = ThemeMode.System;
    private bool hasImageLoaded = false;
    private string currentImagePath = string.Empty;
    private SelectionBox selectionBox = new SelectionBox();

    public FormMain()
    {
        InitializeComponent();
        ApplyTheme(ThemeMode.System);
        
        // Enable mouse events and painting on PictureBox
        pictureBox1.MouseDown += PictureBox1_MouseDown;
        pictureBox1.MouseMove += PictureBox1_MouseMove;
        pictureBox1.MouseUp += PictureBox1_MouseUp;
        pictureBox1.Paint += PictureBox1_Paint;
        pictureBox1.Resize += PictureBox1_Resize;
        pictureBox1.Cursor = Cursors.Cross;
        
        // Subscribe to selection box events
        selectionBox.SelectionChanged += (s, e) => pictureBox1.Invalidate();
    }

    private void loadToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            try
            {
                // Load and display the selected image
                pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
                hasImageLoaded = true;
                currentImagePath = openFileDialog1.FileName;
                
                // Initialize selection box to center and full width of image
                InitializeSelectionBox();
                
                // Update the form title to show the loaded file name
                this.Text = $"FormMain - {Path.GetFileName(openFileDialog1.FileName)}";
                
                // Invalidate to show selection box
                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!hasImageLoaded || pictureBox1.Image == null)
        {
            MessageBox.Show("Please load an image first.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        // Set default filename based on current image
        string defaultName = Path.GetFileNameWithoutExtension(currentImagePath);
        saveFileDialog1.FileName = defaultName;
        
        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
        {
            try
            {
                Rectangle imageBounds = GetImageBounds();
                Rectangle selectionInImageCoords = ImageProcessor.ConvertSelectionToImageCoordinates(
                    selectionBox.Bounds, imageBounds, pictureBox1.Image.Size);
                
                ImageProcessor.SaveCroppedSquareImages(pictureBox1.Image, selectionInImageCoords, saveFileDialog1.FileName);
                MessageBox.Show("Images saved successfully!", "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving images: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void lightThemeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ApplyTheme(ThemeMode.Light);
    }

    private void darkThemeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ApplyTheme(ThemeMode.Dark);
    }

    private void systemThemeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ApplyTheme(ThemeMode.System);
    }

    private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (!hasImageLoaded || e.Button != MouseButtons.Left)
            return;
            
        Rectangle imageBounds = GetImageBounds();
        selectionBox.HandleMouseDown(e.Location, imageBounds);
    }

    private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!hasImageLoaded)
        {
            pictureBox1.Cursor = Cursors.Default;
            return;
        }
            
        Rectangle imageBounds = GetImageBounds();
        bool handled = selectionBox.HandleMouseMove(e.Location, imageBounds);
        
        if (!handled)
        {
            // Update cursor based on position
            pictureBox1.Cursor = selectionBox.GetCursorForLocation(e.Location);
        }
    }

    private void PictureBox1_MouseUp(object? sender, MouseEventArgs e)
    {
        if (!hasImageLoaded) return;
        selectionBox.HandleMouseUp();
    }

    private void PictureBox1_Paint(object? sender, PaintEventArgs e)
    {
        if (!hasImageLoaded) return;
        selectionBox.Draw(e.Graphics);
    }

    private void PictureBox1_Resize(object? sender, EventArgs e)
    {
        if (hasImageLoaded)
        {
            Rectangle imageBounds = GetImageBounds();
            selectionBox.UpdateFromRelativeCoordinates(imageBounds);
        }
    }

    private Rectangle GetImageBounds()
    {
        if (pictureBox1.Image == null) return Rectangle.Empty;
        return ImageProcessor.GetImageBounds(pictureBox1.Image, pictureBox1.Size);
    }

    private void InitializeSelectionBox()
    {
        Rectangle imageBounds = GetImageBounds();
        if (!imageBounds.IsEmpty)
        {
            selectionBox.InitializeForImage(imageBounds);
        }
    }

    private void ApplyTheme(ThemeMode theme)
    {
        currentTheme = theme;
        
        // Update menu checkmarks
        lightThemeToolStripMenuItem.Checked = (theme == ThemeMode.Light);
        darkThemeToolStripMenuItem.Checked = (theme == ThemeMode.Dark);
        systemThemeToolStripMenuItem.Checked = (theme == ThemeMode.System);

        Color backColor, foreColor, menuBackColor, menuForeColor;
        
        switch (theme)
        {
            case ThemeMode.Light:
                backColor = Color.White;
                foreColor = Color.Black;
                menuBackColor = Color.White;
                menuForeColor = Color.Black;
                break;
            case ThemeMode.Dark:
                backColor = Color.FromArgb(32, 32, 32);
                foreColor = Color.White;
                menuBackColor = Color.FromArgb(45, 45, 48);
                menuForeColor = Color.White;
                break;
            case ThemeMode.System:
            default:
                bool isDarkMode = IsSystemDarkMode();
                if (isDarkMode)
                {
                    backColor = Color.FromArgb(32, 32, 32);
                    foreColor = Color.White;
                    menuBackColor = Color.FromArgb(45, 45, 48);
                    menuForeColor = Color.White;
                }
                else
                {
                    backColor = Color.White;
                    foreColor = Color.Black;
                    menuBackColor = Color.White;
                    menuForeColor = Color.Black;
                }
                break;
        }

        // Apply colors to form and controls
        this.BackColor = backColor;
        this.ForeColor = foreColor;
        
        menuStrip1.BackColor = menuBackColor;
        menuStrip1.ForeColor = menuForeColor;
        
        // Set custom renderer for better dark mode menu appearance
        if (theme == ThemeMode.Dark || (theme == ThemeMode.System && IsSystemDarkMode()))
        {
            menuStrip1.Renderer = new DarkMenuRenderer();
        }
        else
        {
            menuStrip1.Renderer = new ToolStripProfessionalRenderer();
        }
        
        // Apply to all menu items
        ApplyMenuItemColors(menuStrip1.Items, menuBackColor, menuForeColor);
        
        pictureBox1.BackColor = backColor;
    }

    private void ApplyMenuItemColors(ToolStripItemCollection items, Color backColor, Color foreColor)
    {
        Color selectedBackColor = GetSelectedBackColor(backColor);
        
        foreach (ToolStripItem item in items)
        {
            item.BackColor = backColor;
            item.ForeColor = foreColor;
            
            if (item is ToolStripMenuItem menuItem)
            {
                // Set hover/selected colors for better visibility in dark mode
                menuItem.BackColor = backColor;
                menuItem.ForeColor = foreColor;
                
                // Apply custom renderer for hover effects
                if (currentTheme == ThemeMode.Dark || (currentTheme == ThemeMode.System && IsSystemDarkMode()))
                {
                    // Custom styling will be handled by the menu renderer
                }
                
                if (menuItem.HasDropDownItems)
                {
                    menuItem.DropDown.BackColor = backColor;
                    menuItem.DropDown.ForeColor = foreColor;
                    ApplyMenuItemColors(menuItem.DropDownItems, backColor, foreColor);
                }
            }
        }
    }
    
    private Color GetSelectedBackColor(Color baseBackColor)
    {
        // For dark themes, use a lighter gray for selection
        if (baseBackColor.R < 128) // Dark background
        {
            return Color.FromArgb(70, 70, 74); // Lighter gray for selection
        }
        else // Light background
        {
            return Color.FromArgb(230, 230, 230); // Light gray for selection
        }
    }

    private bool IsSystemDarkMode()
    {
        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                var value = key?.GetValue("AppsUseLightTheme");
                return value is int intValue && intValue == 0;
            }
        }
        catch
        {
            return false; // Default to light mode if unable to detect
        }
    }
}