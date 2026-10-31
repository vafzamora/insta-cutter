using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using insta_cutter;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        TestSelection();
        TestCoordinates();
        TestExport();
        Console.WriteLine("All crop regression checks passed.");
    }

    private static void TestSelection()
    {
        var imageBounds = new Rect(10, 20, 400, 300);
        var selection = new SelectionBox();
        selection.SetCircular(true, Rect.Empty);
        selection.InitializeForImage(imageBounds);
        Assert(selection.Bounds == new Rect(60, 20, 300, 300), "Initial circle is centered.");
        Assert(!selection.HandleMouseDown(new Point(0, 0), imageBounds), "Ignore clicks outside the image.");

        foreach (var handle in new[] { "left", "right", "top", "bottom" })
        {
            selection.CreateAtPoint(new Point(210, 170), imageBounds);
            Rect before = selection.Bounds;
            Point start = handle switch
            {
                "left" => new Point(before.Left, before.Y + before.Height / 2),
                "right" => new Point(before.Right, before.Y + before.Height / 2),
                "top" => new Point(before.X + before.Width / 2, before.Top),
                _ => new Point(before.X + before.Width / 2, before.Bottom)
            };
            Point end = handle switch
            {
                "left" => new Point(start.X - 30, start.Y),
                "right" => new Point(start.X + 30, start.Y),
                "top" => new Point(start.X, start.Y - 30),
                _ => new Point(start.X, start.Y + 30)
            };
            selection.HandleMouseDown(start, imageBounds);
            Assert(selection.IsResizing, $"{handle} handle starts resizing.");
            selection.HandleMouseMove(end, imageBounds);
            Assert(Math.Abs(selection.Bounds.Width - 130) < 0.001, $"{handle} handle changes diameter.");
            AssertCircleWithinImage(selection, imageBounds);
            selection.HandleMouseMove(new Point(-1000, -1000), imageBounds);
            AssertCircleWithinImage(selection, imageBounds);
            selection.HandleMouseMove(new Point(1000, 1000), imageBounds);
            AssertCircleWithinImage(selection, imageBounds);
            selection.HandleMouseUp();
            Assert(!selection.IsResizing, "Mouse up ends resizing.");
        }

        selection.CreateAtPoint(new Point(210, 170), imageBounds);
        selection.HandleMouseDown(new Point(210, 170), imageBounds);
        Assert(selection.IsDragging, "Clicking inside the circle starts moving.");
        selection.HandleMouseMove(new Point(1000, 1000), imageBounds);
        AssertCircleWithinImage(selection, imageBounds);
        selection.HandleMouseUp();

        selection.CreateAtPoint(new Point(210, 170), imageBounds);
        selection.HandleMouseDown(new Point(165, 125), imageBounds);
        Assert(!selection.IsResizing, "A bounding-square corner is not a circular resize handle.");
        selection.HandleMouseUp();

        var resizedImage = new Rect(5, 10, 200, 150);
        selection.UpdateFromRelativeCoordinates(resizedImage);
        AssertCircleWithinImage(selection, resizedImage);
        Assert(Math.Abs(selection.Bounds.Width - 50) < 0.001, "Window resizing scales the circle.");
        selection.SetCircular(false, resizedImage);
        Assert(selection.Bounds.Width == 2 * selection.Bounds.Height, "Square-pair mode retains its 2:1 ratio.");
        selection.SetCircular(true, resizedImage);
        selection.UpdateFromRelativeCoordinates(new Rect(0, 0, 8, 6));
        AssertCircleWithinImage(selection, new Rect(0, 0, 8, 6));

        selection.SetCircular(false, imageBounds);
        selection.InitializeForImage(imageBounds);
        Assert(selection.Bounds == new Rect(10, 70, 400, 200), "Original square-pair initialization is preserved.");
    }

    private static void TestCoordinates()
    {
        var imageBounds = new Rect(12.5, 20.25, 500.5, 250.25);
        var selection = new Rect(62.55, 45.275, 100.1, 100.1);
        var result = ImageProcessor.ConvertCircularSelectionToImageCoordinates(selection, imageBounds, new Size(1001, 500));
        Assert(result == new Rectangle(100, 50, 200, 200), "Fractional display coordinates retain pixel precision.");
        result = ImageProcessor.ConvertCircularSelectionToImageCoordinates(
            new Rect(412.9, 170.5, 100.1, 100.1), imageBounds, new Size(1001, 500));
        Assert(result.Width == result.Height && new Rectangle(0, 0, 1001, 500).Contains(result),
            "Rounding near image edges keeps the crop square and in bounds.");
        AssertThrows<ArgumentException>(() => ImageProcessor.ConvertCircularSelectionToImageCoordinates(
            Rect.Empty, imageBounds, new Size(1001, 500)));
    }

    private static void TestExport()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"insta-cutter-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var source = new Bitmap(200, 160, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(source))
                graphics.Clear(System.Drawing.Color.Blue);
            source.SetPixel(70, 60, System.Drawing.Color.Red);
            source.SetPixel(71, 60, System.Drawing.Color.FromArgb(128, 0, 255, 0));
            source.SetPixel(72, 60, System.Drawing.Color.Transparent);

            string output = Path.Combine(directory, "circle.png");
            ImageProcessor.SaveCircularImage(source, new Rectangle(20, 10, 100, 100), output);
            using (var saved = new Bitmap(output))
            {
                Assert(saved.Width == 100 && saved.Height == 100, "Circular PNG has square pixel dimensions.");
                Assert(saved.RawFormat.Guid == ImageFormat.Png.Guid, "Circular output is PNG.");
                foreach (var corner in new[] { (0, 0), (99, 0), (0, 99), (99, 99) })
                    Assert(saved.GetPixel(corner.Item1, corner.Item2).A == 0, "Corners outside the circle are transparent.");
                Assert(saved.GetPixel(50, 50).ToArgb() == System.Drawing.Color.Red.ToArgb(), "Crop uses the selected source coordinates.");
                Assert(saved.GetPixel(51, 50).A == 128, "Existing partial transparency is preserved.");
                Assert(saved.GetPixel(52, 50).A == 0, "Existing full transparency is preserved.");
                Assert(saved.GetPixel(25, 50).A == 255, "Image inside the circle remains opaque.");
            }
            Assert(Directory.GetFiles(directory).Length == 1, "Circular export creates only one file.");
            AssertThrows<ArgumentException>(() => ImageProcessor.SaveCircularImage(
                source, new Rectangle(20, 10, 100, 100), Path.Combine(directory, "circle.jpg")));
            AssertThrows<ArgumentException>(() => ImageProcessor.SaveCircularImage(
                source, new Rectangle(20, 10, 100, 50), output));

            ImageProcessor.SaveCroppedSquareImages(source, new Rectangle(0, 0, 200, 100), Path.Combine(directory, "pair.png"));
            foreach (string suffix in new[] { "_left", "_right" })
            {
                using var saved = new Bitmap(Path.Combine(directory, $"pair{suffix}.png"));
                Assert(saved.Width == 100 && saved.Height == 100, "Original square-pair export is preserved.");
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void AssertCircleWithinImage(SelectionBox selection, Rect imageBounds)
    {
        Assert(Math.Abs(selection.Bounds.Width - selection.Bounds.Height) < 0.001, "Selection remains circular.");
        Assert(imageBounds.Contains(selection.Bounds), "Circle remains inside the image.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertThrows<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }
}
