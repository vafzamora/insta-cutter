using System.Drawing;
using System.Drawing.Imaging;

namespace insta_cutter;

public static class ImageProcessor
{
    public static Rectangle GetImageBounds(Image image, Size containerSize)
    {
        if (image == null) return Rectangle.Empty;
        
        // Calculate the actual image rectangle within the container (considering Zoom mode)
        float imageAspect = (float)image.Width / image.Height;
        float containerAspect = (float)containerSize.Width / containerSize.Height;
        
        int imageWidth, imageHeight, imageX, imageY;
        
        if (imageAspect > containerAspect)
        {
            // Image is wider - fit to width
            imageWidth = containerSize.Width;
            imageHeight = (int)(containerSize.Width / imageAspect);
            imageX = 0;
            imageY = (containerSize.Height - imageHeight) / 2;
        }
        else
        {
            // Image is taller - fit to height
            imageHeight = containerSize.Height;
            imageWidth = (int)(containerSize.Height * imageAspect);
            imageY = 0;
            imageX = (containerSize.Width - imageWidth) / 2;
        }
        
        return new Rectangle(imageX, imageY, imageWidth, imageHeight);
    }

    public static Rectangle ConvertSelectionToImageCoordinates(Rectangle selection, Rectangle imageBounds, Size actualImageSize)
    {
        if (imageBounds.IsEmpty) return Rectangle.Empty;
        
        // Calculate scale factors
        float scaleX = (float)actualImageSize.Width / imageBounds.Width;
        float scaleY = (float)actualImageSize.Height / imageBounds.Height;
        
        // Convert selection box coordinates to image coordinates
        int imageX = (int)((selection.X - imageBounds.X) * scaleX);
        int imageY = (int)((selection.Y - imageBounds.Y) * scaleY);
        int imageWidth = (int)(selection.Width * scaleX);
        int imageHeight = (int)(selection.Height * scaleY);
        
        return new Rectangle(imageX, imageY, imageWidth, imageHeight);
    }

    public static void SaveCroppedSquareImages(Image originalImage, Rectangle selectionArea, string baseFileName)
    {
        using (Bitmap originalBitmap = new Bitmap(originalImage))
        {
            // Calculate dimensions for square crops (half width each)
            int squareSize = selectionArea.Height; // Use height as the square dimension
            int halfWidth = selectionArea.Width / 2;
            
            // Adjust square size if it would exceed half width
            if (squareSize > halfWidth)
                squareSize = halfWidth;
            
            // Calculate positioning to center the squares within each half
            int leftSquareX = selectionArea.X + (halfWidth - squareSize) / 2;
            int rightSquareX = selectionArea.X + halfWidth + (halfWidth - squareSize) / 2;
            int squareY = selectionArea.Y + (selectionArea.Height - squareSize) / 2;
            
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

    private static Bitmap CropImage(Bitmap source, Rectangle cropArea)
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

    private static void SaveImageWithQuality(Bitmap image, string fileName)
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
                image.Save(fileName, ImageFormat.Bmp);
                break;
            default:
                SavePngLossless(image, fileName); // Default to lossless PNG
                break;
        }
    }
    
    private static void SaveJpegWithQuality(Bitmap image, string fileName, long quality)
    {
        var jpegCodec = ImageCodecInfo.GetImageEncoders()
            .First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
        
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        
        image.Save(fileName, jpegCodec, encoderParams);
    }
    
    private static void SavePngLossless(Bitmap image, string fileName)
    {
        // PNG is lossless by default, so we can save directly without encoder parameters
        // This preserves maximum quality
        image.Save(fileName, ImageFormat.Png);
    }

    private static string GetOutputFileName(string baseFileName, string suffix)
    {
        string directory = Path.GetDirectoryName(baseFileName) ?? "";
        string nameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
        string extension = Path.GetExtension(baseFileName);
        
        return Path.Combine(directory, $"{nameWithoutExt}{suffix}{extension}");
    }
}