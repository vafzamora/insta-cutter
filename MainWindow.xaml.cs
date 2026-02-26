using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media;

namespace insta_cutter;

public partial class MainWindow : Window
{
    private enum AppThemeMode { Light, Dark, System }

    private AppThemeMode currentTheme = AppThemeMode.System;
    private bool hasImageLoaded = false;
    private string currentImagePath = string.Empty;
    private SelectionBox selectionBox = new SelectionBox();
    private System.Drawing.Image? drawingImage;
    private SelectionVisual selectionVisual;

    public MainWindow()
    {
        InitializeComponent();

        selectionVisual = new SelectionVisual(selectionBox);
        imageGrid.Children.Add(selectionVisual);

        selectionBox.SelectionChanged += (s, e) => selectionVisual.InvalidateVisual();

        ApplyTheme(AppThemeMode.System);
    }

    private void LoadMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|JPEG Files|*.jpg;*.jpeg|PNG Files|*.png|BMP Files|*.bmp",
            Title = "Select an Image File"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                drawingImage?.Dispose();
                drawingImage = System.Drawing.Image.FromFile(dialog.FileName);
                imageControl.Source = new BitmapImage(new Uri(dialog.FileName));
                hasImageLoaded = true;
                currentImagePath = dialog.FileName;

                this.Title = $"MainWindow - {Path.GetFileName(dialog.FileName)}";
                imageGrid.Cursor = Cursors.Cross;

                // Defer initialization until WPF layout has measured the image control
                Dispatcher.InvokeAsync(InitializeSelectionBox, System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (!hasImageLoaded || drawingImage == null)
        {
            MessageBox.Show("Please load an image first.", "No Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PNG Files|*.png|JPEG Files|*.jpg|BMP Files|*.bmp",
            Title = "Save Cropped Images",
            DefaultExt = "png",
            FileName = Path.GetFileNameWithoutExtension(currentImagePath)
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var imageBounds = GetDrawingImageBounds();
                var selectionRect = ImageProcessor.ToDrawingRectangle(selectionBox.Bounds);
                var selectionInImageCoords = ImageProcessor.ConvertSelectionToImageCoordinates(
                    selectionRect, imageBounds, drawingImage.Size);

                ImageProcessor.SaveCroppedSquareImages(drawingImage, selectionInImageCoords, dialog.FileName);
                MessageBox.Show("Images saved successfully!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving images: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void LightThemeMenuItem_Click(object sender, RoutedEventArgs e) => ApplyTheme(AppThemeMode.Light);
    private void DarkThemeMenuItem_Click(object sender, RoutedEventArgs e) => ApplyTheme(AppThemeMode.Dark);
    private void SystemThemeMenuItem_Click(object sender, RoutedEventArgs e) => ApplyTheme(AppThemeMode.System);

    private void ImageGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!hasImageLoaded || e.LeftButton != MouseButtonState.Pressed) return;
        var imageBounds = GetWpfImageBounds();
        selectionBox.HandleMouseDown(e.GetPosition(imageGrid), imageBounds);
        imageGrid.CaptureMouse();
    }

    private void ImageGrid_MouseMove(object sender, MouseEventArgs e)
    {
        if (!hasImageLoaded)
        {
            imageGrid.Cursor = Cursors.Arrow;
            return;
        }

        var imageBounds = GetWpfImageBounds();
        bool handled = selectionBox.HandleMouseMove(e.GetPosition(imageGrid), imageBounds);

        if (!handled)
        {
            imageGrid.Cursor = selectionBox.GetCursorForLocation(e.GetPosition(imageGrid));
        }
    }

    private void ImageGrid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!hasImageLoaded) return;
        selectionBox.HandleMouseUp();
        imageGrid.ReleaseMouseCapture();
    }

    private void ImageControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (hasImageLoaded)
        {
            var imageBounds = GetWpfImageBounds();
            selectionBox.UpdateFromRelativeCoordinates(imageBounds);
        }
    }

    private System.Drawing.Rectangle GetDrawingImageBounds()
    {
        if (drawingImage == null) return System.Drawing.Rectangle.Empty;
        return ImageProcessor.GetImageBounds(
            drawingImage,
            new System.Drawing.Size((int)imageControl.ActualWidth, (int)imageControl.ActualHeight));
    }

    private Rect GetWpfImageBounds()
    {
        var db = GetDrawingImageBounds();
        return new Rect(db.X, db.Y, db.Width, db.Height);
    }

    private void InitializeSelectionBox()
    {
        var imageBounds = GetWpfImageBounds();
        if (!imageBounds.IsEmpty)
            selectionBox.InitializeForImage(imageBounds);
    }

    private void ApplyTheme(AppThemeMode theme)
    {
        currentTheme = theme;

        lightThemeMenuItem.IsChecked = (theme == AppThemeMode.Light);
        darkThemeMenuItem.IsChecked = (theme == AppThemeMode.Dark);
        systemThemeMenuItem.IsChecked = (theme == AppThemeMode.System);

        bool isDark = theme == AppThemeMode.Dark || (theme == AppThemeMode.System && IsSystemDarkMode());

        if (isDark)
        {
            SetThemeResources(
                Color.FromRgb(32, 32, 32), Colors.White,
                Color.FromRgb(45, 45, 48), Color.FromRgb(70, 70, 74));
        }
        else
        {
            SetThemeResources(
                Colors.White, Colors.Black,
                Colors.White, Color.FromRgb(230, 230, 230));
        }
    }

    private static void SetThemeResources(Color bg, Color fg, Color menuBg, Color menuSelected)
    {
        Application.Current.Resources["AppBackground"] = new SolidColorBrush(bg);
        Application.Current.Resources["AppForeground"] = new SolidColorBrush(fg);
        Application.Current.Resources["MenuBackground"] = new SolidColorBrush(menuBg);
        Application.Current.Resources["MenuForeground"] = new SolidColorBrush(fg);
        Application.Current.Resources["MenuSelectedBackground"] = new SolidColorBrush(menuSelected);
    }

    private static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int intValue && intValue == 0;
        }
        catch
        {
            return false;
        }
    }

    private sealed class SelectionVisual : FrameworkElement
    {
        private readonly SelectionBox _selectionBox;

        public SelectionVisual(SelectionBox selectionBox)
        {
            _selectionBox = selectionBox;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            _selectionBox.Draw(dc);
        }
    }
}
