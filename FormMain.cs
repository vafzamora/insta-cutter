using Microsoft.Win32;

namespace insta_cutter;

public class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkColorTable()) { }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = Color.White;
        base.OnRenderItemText(e);
    }
}

public class DarkColorTable : ProfessionalColorTable
{
    public override Color MenuItemSelected => Color.FromArgb(70, 70, 74); // Lighter gray for selection
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(70, 70, 74);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(70, 70, 74);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(60, 60, 64);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(60, 60, 64);
    public override Color MenuItemBorder => Color.FromArgb(80, 80, 84);
    public override Color MenuBorder => Color.FromArgb(60, 60, 64);
    public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
    public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
    public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
    public override Color SeparatorDark => Color.FromArgb(60, 60, 64);
    public override Color SeparatorLight => Color.FromArgb(70, 70, 74);
}

public partial class FormMain : Form
{
    private enum ThemeMode
    {
        Light,
        Dark,
        System
    }

    private ThemeMode currentTheme = ThemeMode.System;
    
    // Selection box properties
    private Rectangle selectionBox = new Rectangle(50, 50, 200, 100); // 2:1 aspect ratio
    private bool isDragging = false;
    private bool isResizing = false;
    private Point lastMousePosition;
    private ResizeHandle resizeHandle = ResizeHandle.None;
    private const int HANDLE_SIZE = 8;
    private bool hasImageLoaded = false;
    
    // Relative position tracking for window resize
    private float relativeX = 0f; // X position as percentage of image width
    private float relativeY = 0.25f; // Y position as percentage of image height
    private float relativeWidth = 1f; // Width as percentage of image width
    private float relativeHeight = 0.5f; // Height as percentage of image height
    
    // Current image file path for saving
    private string currentImagePath = string.Empty;
    
    private enum ResizeHandle
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Left,
        Right,
        Top,
        Bottom
    }

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
                SaveCroppedImages(saveFileDialog1.FileName);
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

    private void PictureBox1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (!hasImageLoaded || e.Button != MouseButtons.Left)
            return;
            
        lastMousePosition = e.Location;
            
            // Check if clicking on a resize handle
            resizeHandle = GetResizeHandle(e.Location);
            if (resizeHandle != ResizeHandle.None)
            {
                isResizing = true;
            }
            // Check if clicking inside the selection box
            else if (selectionBox.Contains(e.Location))
            {
                isDragging = true;
            }
            // Create new selection box
            else
            {
                CreateSelectionBoxAtPoint(e.Location);
                isResizing = true;
                resizeHandle = ResizeHandle.BottomRight;
            }
    }

    private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!hasImageLoaded)
        {
            pictureBox1.Cursor = Cursors.Default;
            return;
        }
            
        if (isDragging)
        {
            int deltaX = e.X - lastMousePosition.X;
            int deltaY = e.Y - lastMousePosition.Y;
            
            // Move the selection box
            selectionBox.X += deltaX;
            selectionBox.Y += deltaY;
            
            // Keep within bounds
            ConstrainSelectionBox();
            // Update relative coordinates
            UpdateRelativeCoordinates();
            
            lastMousePosition = e.Location;
            pictureBox1.Invalidate();
        }
        else if (isResizing)
        {
            ResizeSelectionBox(e.Location);
            pictureBox1.Invalidate();
        }
        else
        {
            // Update cursor based on position
            UpdateCursor(e.Location);
        }
    }

    private void PictureBox1_MouseUp(object? sender, MouseEventArgs e)
    {
        if (!hasImageLoaded) return;
            
        isDragging = false;
        isResizing = false;
        resizeHandle = ResizeHandle.None;
    }

    private void PictureBox1_Paint(object? sender, PaintEventArgs e)
    {
        if (!hasImageLoaded) return;
            
        // Draw the selection box
        using (Pen pen = new Pen(Color.Red, 2))
        {
            e.Graphics.DrawRectangle(pen, selectionBox);
        }
        
        // Draw resize handles
        DrawResizeHandles(e.Graphics);
    }

    private ResizeHandle GetResizeHandle(Point location)
    {
        Rectangle[] handles = GetResizeHandleRectangles();
        
        if (handles[0].Contains(location)) return ResizeHandle.TopLeft;
        if (handles[1].Contains(location)) return ResizeHandle.TopRight;
        if (handles[2].Contains(location)) return ResizeHandle.BottomLeft;
        if (handles[3].Contains(location)) return ResizeHandle.BottomRight;
        if (handles[4].Contains(location)) return ResizeHandle.Left;
        if (handles[5].Contains(location)) return ResizeHandle.Right;
        if (handles[6].Contains(location)) return ResizeHandle.Top;
        if (handles[7].Contains(location)) return ResizeHandle.Bottom;
        
        return ResizeHandle.None;
    }

    private Rectangle[] GetResizeHandleRectangles()
    {
        return new Rectangle[]
        {
            new Rectangle(selectionBox.Left - HANDLE_SIZE/2, selectionBox.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // TopLeft
            new Rectangle(selectionBox.Right - HANDLE_SIZE/2, selectionBox.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // TopRight
            new Rectangle(selectionBox.Left - HANDLE_SIZE/2, selectionBox.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // BottomLeft
            new Rectangle(selectionBox.Right - HANDLE_SIZE/2, selectionBox.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // BottomRight
            new Rectangle(selectionBox.Left - HANDLE_SIZE/2, selectionBox.Y + selectionBox.Height/2 - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // Left
            new Rectangle(selectionBox.Right - HANDLE_SIZE/2, selectionBox.Y + selectionBox.Height/2 - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // Right
            new Rectangle(selectionBox.X + selectionBox.Width/2 - HANDLE_SIZE/2, selectionBox.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // Top
            new Rectangle(selectionBox.X + selectionBox.Width/2 - HANDLE_SIZE/2, selectionBox.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE) // Bottom
        };
    }

    private void DrawResizeHandles(Graphics g)
    {
        Rectangle[] handles = GetResizeHandleRectangles();
        
        using (Brush brush = new SolidBrush(Color.Red))
        {
            foreach (Rectangle handle in handles)
            {
                g.FillRectangle(brush, handle);
            }
        }
    }

    private void ResizeSelectionBox(Point currentLocation)
    {
        Rectangle imageBounds = GetImageBounds();
        if (imageBounds.IsEmpty) return;
        
        int newWidth, newHeight;
        int newX = selectionBox.X;
        int newY = selectionBox.Y;
        
        switch (resizeHandle)
        {
            case ResizeHandle.Right:
            case ResizeHandle.BottomRight:
                newWidth = Math.Max(20, Math.Min(currentLocation.X - selectionBox.X, imageBounds.Right - selectionBox.X));
                newHeight = newWidth / 2; // Maintain 2:1 aspect ratio
                // Ensure height doesn't exceed image bounds
                if (selectionBox.Y + newHeight > imageBounds.Bottom)
                {
                    newHeight = imageBounds.Bottom - selectionBox.Y;
                    newWidth = newHeight * 2;
                }
                break;
                
            case ResizeHandle.Left:
            case ResizeHandle.TopLeft:
                newWidth = Math.Max(20, Math.Min(selectionBox.Right - currentLocation.X, selectionBox.Right - imageBounds.X));
                newHeight = newWidth / 2;
                newX = selectionBox.Right - newWidth;
                if (resizeHandle == ResizeHandle.TopLeft)
                {
                    newY = selectionBox.Bottom - newHeight;
                    // Ensure top doesn't go above image bounds
                    if (newY < imageBounds.Y)
                    {
                        newY = imageBounds.Y;
                        newHeight = selectionBox.Bottom - newY;
                        newWidth = newHeight * 2;
                        newX = selectionBox.Right - newWidth;
                    }
                }
                break;
                
            case ResizeHandle.Bottom:
                newHeight = Math.Max(10, Math.Min(currentLocation.Y - selectionBox.Y, imageBounds.Bottom - selectionBox.Y));
                newWidth = newHeight * 2; // Maintain 2:1 aspect ratio
                // Ensure width doesn't exceed image bounds
                if (selectionBox.X + newWidth > imageBounds.Right)
                {
                    newWidth = imageBounds.Right - selectionBox.X;
                    newHeight = newWidth / 2;
                }
                break;
                
            case ResizeHandle.Top:
                newHeight = Math.Max(10, Math.Min(selectionBox.Bottom - currentLocation.Y, selectionBox.Bottom - imageBounds.Y));
                newWidth = newHeight * 2;
                newY = selectionBox.Bottom - newHeight;
                // Ensure width doesn't exceed image bounds
                if (selectionBox.X + newWidth > imageBounds.Right)
                {
                    newWidth = imageBounds.Right - selectionBox.X;
                    newHeight = newWidth / 2;
                    newY = selectionBox.Bottom - newHeight;
                }
                break;
                
            case ResizeHandle.TopRight:
                newWidth = Math.Max(20, Math.Min(currentLocation.X - selectionBox.X, imageBounds.Right - selectionBox.X));
                newHeight = newWidth / 2;
                newY = selectionBox.Bottom - newHeight;
                // Ensure top doesn't go above image bounds
                if (newY < imageBounds.Y)
                {
                    newY = imageBounds.Y;
                    newHeight = selectionBox.Bottom - newY;
                    newWidth = newHeight * 2;
                }
                break;
                
            case ResizeHandle.BottomLeft:
                newWidth = Math.Max(20, Math.Min(selectionBox.Right - currentLocation.X, selectionBox.Right - imageBounds.X));
                newHeight = newWidth / 2;
                newX = selectionBox.Right - newWidth;
                // Ensure bottom doesn't go below image bounds
                if (selectionBox.Y + newHeight > imageBounds.Bottom)
                {
                    newHeight = imageBounds.Bottom - selectionBox.Y;
                    newWidth = newHeight * 2;
                    newX = selectionBox.Right - newWidth;
                }
                break;
                
            default:
                return;
        }
        
        selectionBox = new Rectangle(newX, newY, newWidth, newHeight);
        // Final constraint check to ensure everything is within bounds
        ConstrainSelectionBox();
        // Update relative coordinates
        UpdateRelativeCoordinates();
    }

    private void ConstrainSelectionBox()
    {
        Rectangle imageBounds = GetImageBounds();
        if (imageBounds.IsEmpty) return;
        
        // Keep selection box within image bounds
        if (selectionBox.X < imageBounds.X) selectionBox.X = imageBounds.X;
        if (selectionBox.Y < imageBounds.Y) selectionBox.Y = imageBounds.Y;
        if (selectionBox.Right > imageBounds.Right) selectionBox.X = imageBounds.Right - selectionBox.Width;
        if (selectionBox.Bottom > imageBounds.Bottom) selectionBox.Y = imageBounds.Bottom - selectionBox.Height;
        
        // Ensure minimum size
        if (selectionBox.Width < 20)
        {
            selectionBox.Width = 20;
            selectionBox.Height = 10;
        }
        if (selectionBox.Height < 10)
        {
            selectionBox.Height = 10;
            selectionBox.Width = 20;
        }
    }

    private void UpdateCursor(Point location)
    {
        ResizeHandle handle = GetResizeHandle(location);
        
        switch (handle)
        {
            case ResizeHandle.TopLeft:
            case ResizeHandle.BottomRight:
                pictureBox1.Cursor = Cursors.SizeNWSE;
                break;
            case ResizeHandle.TopRight:
            case ResizeHandle.BottomLeft:
                pictureBox1.Cursor = Cursors.SizeNESW;
                break;
            case ResizeHandle.Left:
            case ResizeHandle.Right:
                pictureBox1.Cursor = Cursors.SizeWE;
                break;
            case ResizeHandle.Top:
            case ResizeHandle.Bottom:
                pictureBox1.Cursor = Cursors.SizeNS;
                break;
            default:
                if (selectionBox.Contains(location))
                    pictureBox1.Cursor = Cursors.SizeAll;
                else
                    pictureBox1.Cursor = Cursors.Cross;
                break;
        }
    }
    
    private Rectangle GetImageBounds()
    {
        if (pictureBox1.Image == null) return Rectangle.Empty;
        
        // Calculate the actual image rectangle within the PictureBox (considering Zoom mode)
        Image img = pictureBox1.Image;
        float imageAspect = (float)img.Width / img.Height;
        float boxAspect = (float)pictureBox1.Width / pictureBox1.Height;
        
        int imageWidth, imageHeight, imageX, imageY;
        
        if (imageAspect > boxAspect)
        {
            // Image is wider - fit to width
            imageWidth = pictureBox1.Width;
            imageHeight = (int)(pictureBox1.Width / imageAspect);
            imageX = 0;
            imageY = (pictureBox1.Height - imageHeight) / 2;
        }
        else
        {
            // Image is taller - fit to height
            imageHeight = pictureBox1.Height;
            imageWidth = (int)(pictureBox1.Height * imageAspect);
            imageY = 0;
            imageX = (pictureBox1.Width - imageWidth) / 2;
        }
        
        return new Rectangle(imageX, imageY, imageWidth, imageHeight);
    }
    
    private void InitializeSelectionBox()
    {
        Rectangle imageBounds = GetImageBounds();
        if (imageBounds.IsEmpty) return;
        
        // Create selection box spanning full width of image
        int boxHeight = imageBounds.Width / 2; // Maintain 2:1 aspect ratio
        
        // Center vertically within the image
        int boxY = imageBounds.Y + (imageBounds.Height - boxHeight) / 2;
        
        selectionBox = new Rectangle(imageBounds.X, boxY, imageBounds.Width, boxHeight);
        
        // Ensure it fits within image bounds
        ConstrainSelectionBox();
        // Update relative coordinates
        UpdateRelativeCoordinates();
    }
    
    private void PictureBox1_Resize(object? sender, EventArgs e)
    {
        if (hasImageLoaded)
        {
            // Recalculate selection box position based on relative coordinates
            UpdateSelectionBoxFromRelative();
            pictureBox1.Invalidate();
        }
    }
    
    private void UpdateRelativeCoordinates()
    {
        Rectangle imageBounds = GetImageBounds();
        if (imageBounds.IsEmpty || imageBounds.Width == 0 || imageBounds.Height == 0) return;
        
        // Convert absolute coordinates to relative (0.0 to 1.0)
        relativeX = (float)(selectionBox.X - imageBounds.X) / imageBounds.Width;
        relativeY = (float)(selectionBox.Y - imageBounds.Y) / imageBounds.Height;
        relativeWidth = (float)selectionBox.Width / imageBounds.Width;
        relativeHeight = (float)selectionBox.Height / imageBounds.Height;
    }
    
    private void UpdateSelectionBoxFromRelative()
    {
        Rectangle imageBounds = GetImageBounds();
        if (imageBounds.IsEmpty) return;
        
        // Convert relative coordinates back to absolute
        int x = imageBounds.X + (int)(relativeX * imageBounds.Width);
        int y = imageBounds.Y + (int)(relativeY * imageBounds.Height);
        int width = (int)(relativeWidth * imageBounds.Width);
        int height = (int)(relativeHeight * imageBounds.Height);
        
        selectionBox = new Rectangle(x, y, width, height);
        ConstrainSelectionBox();
    }
    
    private void CreateSelectionBoxAtPoint(Point location)
    {
        Rectangle imageBounds = GetImageBounds();
        if (imageBounds.IsEmpty) return;
        
        // Start with a reasonable size (100x50 for 2:1 ratio)
        int width = 100;
        int height = 50;
        
        // Center the box on the click point
        int x = location.X - width / 2;
        int y = location.Y - height / 2;
        
        selectionBox = new Rectangle(x, y, width, height);
        ConstrainSelectionBox();
        UpdateRelativeCoordinates();
    }
    
    private void SaveCroppedImages(string baseFileName)
    {
        if (pictureBox1.Image == null) return;
        
        // Get the selection area in image coordinates
        Rectangle imageSelection = ConvertSelectionToImageCoordinates();
        
        using (Bitmap originalBitmap = new Bitmap(pictureBox1.Image))
        {
            // Calculate dimensions for square crops (half width each)
            int squareSize = imageSelection.Height; // Use height as the square dimension
            int halfWidth = imageSelection.Width / 2;
            
            // Adjust square size if it would exceed half width
            if (squareSize > halfWidth)
                squareSize = halfWidth;
            
            // Calculate positioning to center the squares within each half
            int leftSquareX = imageSelection.X + (halfWidth - squareSize) / 2;
            int rightSquareX = imageSelection.X + halfWidth + (halfWidth - squareSize) / 2;
            int squareY = imageSelection.Y + (imageSelection.Height - squareSize) / 2;
            
            // Create left square crop
            Rectangle leftRect = new Rectangle(leftSquareX, squareY, squareSize, squareSize);
            using (Bitmap leftCrop = CropImage(originalBitmap, leftRect))
            {
                string leftFileName = GetOutputFileName(baseFileName, "_left");
                SaveImageWithQuality(leftCrop, leftFileName);
            }
            
            // Create right square crop
            Rectangle rightRect = new Rectangle(rightSquareX, squareY, squareSize, squareSize);
            using (Bitmap rightCrop = CropImage(originalBitmap, rightRect))
            {
                string rightFileName = GetOutputFileName(baseFileName, "_right");
                SaveImageWithQuality(rightCrop, rightFileName);
            }
        }
    }
    
    private Rectangle ConvertSelectionToImageCoordinates()
    {
        if (pictureBox1.Image == null) return Rectangle.Empty;
        
        // Get image bounds in PictureBox coordinates
        Rectangle imageBounds = GetImageBounds();
        if (imageBounds.IsEmpty) return Rectangle.Empty;
        
        // Calculate scale factors
        float scaleX = (float)pictureBox1.Image.Width / imageBounds.Width;
        float scaleY = (float)pictureBox1.Image.Height / imageBounds.Height;
        
        // Convert selection box coordinates to image coordinates
        int imageX = (int)((selectionBox.X - imageBounds.X) * scaleX);
        int imageY = (int)((selectionBox.Y - imageBounds.Y) * scaleY);
        int imageWidth = (int)(selectionBox.Width * scaleX);
        int imageHeight = (int)(selectionBox.Height * scaleY);
        
        return new Rectangle(imageX, imageY, imageWidth, imageHeight);
    }
    
    private Bitmap CropImage(Bitmap source, Rectangle cropArea)
    {
        // Ensure crop area is within image bounds
        cropArea.Intersect(new Rectangle(0, 0, source.Width, source.Height));
        
        // Create bitmap with same pixel format as source to preserve quality
        Bitmap cropped = new Bitmap(cropArea.Width, cropArea.Height, source.PixelFormat);
        
        using (Graphics g = Graphics.FromImage(cropped))
        {
            // Set high-quality rendering settings
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            
            // Draw the cropped portion with pixel-perfect accuracy
            g.DrawImage(source, new Rectangle(0, 0, cropArea.Width, cropArea.Height), cropArea, GraphicsUnit.Pixel);
        }
        return cropped;
    }
    
    private string GetOutputFileName(string baseFileName, string suffix)
    {
        string directory = Path.GetDirectoryName(baseFileName) ?? "";
        string nameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
        string extension = Path.GetExtension(baseFileName);
        
        return Path.Combine(directory, $"{nameWithoutExt}{suffix}{extension}");
    }
    
    private void SaveImageWithQuality(Bitmap image, string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                SaveJpegWithQuality(image, fileName, 100); // Maximum quality
                break;
            case ".png":
                SavePngLossless(image, fileName);
                break;
            case ".bmp":
                image.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                break;
            default:
                SavePngLossless(image, fileName); // Default to lossless PNG
                break;
        }
    }
    
    private void SaveJpegWithQuality(Bitmap image, string fileName, long quality)
    {
        var jpegCodec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
            .First(codec => codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
        
        var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
        encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
            System.Drawing.Imaging.Encoder.Quality, quality);
        
        image.Save(fileName, jpegCodec, encoderParams);
    }
    
    private void SavePngLossless(Bitmap image, string fileName)
    {
        // PNG is lossless by default, so we can save directly without encoder parameters
        // This preserves maximum quality
        image.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
    }
    
    private System.Drawing.Imaging.ImageFormat GetImageFormat(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
            ".png" => System.Drawing.Imaging.ImageFormat.Png,
            ".bmp" => System.Drawing.Imaging.ImageFormat.Bmp,
            _ => System.Drawing.Imaging.ImageFormat.Png
        };
    }
}