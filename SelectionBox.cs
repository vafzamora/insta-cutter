using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace insta_cutter;

public class SelectionBox
{
    private Rect _bounds = new Rect(50, 50, 200, 100);
    private bool _isDragging = false;
    private bool _isResizing = false;
    private Point _lastMousePosition;
    private ResizeHandle _resizeHandle = ResizeHandle.None;
    private const double HANDLE_SIZE = 12;
    private const double MIN_SELECTION_WIDTH = 20;
    private const double MIN_SELECTION_HEIGHT = 10;
    private Rect _imageBounds = Rect.Empty;
    private static readonly SolidColorBrush OverlayBrush = CreateFrozenOverlayBrush();
    private static readonly Pen YellowPen = CreateFrozenYellowPen();
    private static readonly Pen YellowDashedPen = CreateFrozenYellowDashedPen();

    // Relative position tracking for window resize
    private float _relativeX = 0f;
    private float _relativeY = 0.25f;
    private float _relativeWidth = 1f;
    private float _relativeHeight = 0.5f;

    public Rect Bounds => _bounds;
    public bool IsCircular { get; private set; }
    public bool IsDragging => _isDragging;
    public bool IsResizing => _isResizing;

    public event EventHandler? SelectionChanged;

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

    public void InitializeForImage(Rect imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        HandleMouseUp();

        double boxWidth = IsCircular ? Math.Min(imageBounds.Width, imageBounds.Height) : imageBounds.Width;
        double boxHeight = IsCircular ? boxWidth : boxWidth / 2;

        // Center vertically within the image
        double boxY = imageBounds.Y + (imageBounds.Height - boxHeight) / 2;

        _bounds = new Rect(imageBounds.X + (imageBounds.Width - boxWidth) / 2, boxY, boxWidth, boxHeight);

        // Ensure it fits within image bounds
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetCircular(bool isCircular, Rect imageBounds)
    {
        HandleMouseUp();
        if (IsCircular == isCircular) return;
        IsCircular = isCircular;
        if (imageBounds.IsEmpty) return;

        double centerX = _bounds.X + _bounds.Width / 2;
        double centerY = _bounds.Y + _bounds.Height / 2;
        double height = Math.Min(_bounds.Width, _bounds.Height);
        double width = isCircular ? height : height * 2;
        _bounds = new Rect(centerX - width / 2, centerY - height / 2, width, height);
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CreateAtPoint(Point location, Rect imageBounds)
    {
        if (imageBounds.IsEmpty) return;

        double width = 100;
        double height = IsCircular ? width : 50;

        // Center the box on the click point
        double x = location.X - width / 2;
        double y = location.Y - height / 2;

        _bounds = new Rect(x, y, width, height);
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HandleMouseDown(Point location, Rect imageBounds)
    {
        if (imageBounds.IsEmpty) return false;
        _lastMousePosition = location;

        // Check if clicking on a resize handle
        _resizeHandle = GetResizeHandle(location);
        if (_resizeHandle != ResizeHandle.None)
        {
            _isResizing = true;
            return true;
        }
        // Check if clicking inside the selection box
        else if (ContainsSelection(location))
        {
            _isDragging = true;
            return true;
        }
        // Create new selection box
        else
        {
            if (!imageBounds.Contains(location)) return false;
            CreateAtPoint(location, imageBounds);
            _isDragging = IsCircular;
            _isResizing = !IsCircular;
            _resizeHandle = IsCircular ? ResizeHandle.None : ResizeHandle.BottomRight;
            return true;
        }
    }

    public bool HandleMouseMove(Point location, Rect imageBounds)
    {
        if (_isDragging)
        {
            double deltaX = location.X - _lastMousePosition.X;
            double deltaY = location.Y - _lastMousePosition.Y;

            // Move the selection box
            _bounds = new Rect(_bounds.X + deltaX, _bounds.Y + deltaY, _bounds.Width, _bounds.Height);

            // Keep within bounds
            ConstrainToImageBounds(imageBounds);
            UpdateRelativeCoordinates(imageBounds);

            _lastMousePosition = location;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        else if (_isResizing)
        {
            ResizeBox(location, imageBounds);
            _lastMousePosition = location;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }

    public void HandleMouseUp()
    {
        _isDragging = false;
        _isResizing = false;
        _resizeHandle = ResizeHandle.None;
    }

    public Cursor GetCursorForLocation(Point location)
    {
        ResizeHandle handle = GetResizeHandle(location);

        return handle switch
        {
            ResizeHandle.TopLeft or ResizeHandle.BottomRight => Cursors.SizeNWSE,
            ResizeHandle.TopRight or ResizeHandle.BottomLeft => Cursors.SizeNESW,
            ResizeHandle.Left or ResizeHandle.Right => Cursors.SizeWE,
            ResizeHandle.Top or ResizeHandle.Bottom => Cursors.SizeNS,
            _ => ContainsSelection(location) ? Cursors.SizeAll : Cursors.Cross
        };
    }

    public void Draw(DrawingContext dc)
    {
        // Draw semi-transparent overlay over the image area outside the selection box
        if (!_imageBounds.IsEmpty)
        {
            var imageGeometry = new RectangleGeometry(_imageBounds);
            Geometry selectionGeometry = IsCircular ? new EllipseGeometry(_bounds) : new RectangleGeometry(_bounds);
            var dimmingGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, imageGeometry, selectionGeometry);
            dimmingGeometry.Freeze();
            dc.DrawGeometry(OverlayBrush, null, dimmingGeometry);
        }

        // Draw the selection box
        if (IsCircular)
        {
            dc.DrawEllipse(null, YellowPen,
                new Point(_bounds.X + _bounds.Width / 2, _bounds.Y + _bounds.Height / 2),
                _bounds.Width / 2, _bounds.Height / 2);
        }
        else
        {
            dc.DrawRectangle(null, YellowPen, _bounds);
            double centerX = _bounds.X + _bounds.Width / 2;
            dc.DrawLine(YellowDashedPen, new Point(centerX, _bounds.Top), new Point(centerX, _bounds.Bottom));
        }

        // Draw resize handles
        DrawResizeHandles(dc);
    }

    public void UpdateFromRelativeCoordinates(Rect imageBounds)
    {
        if (imageBounds.IsEmpty) return;

        // Convert relative coordinates back to absolute
        double x = imageBounds.X + _relativeX * imageBounds.Width;
        double y = imageBounds.Y + _relativeY * imageBounds.Height;
        double width = _relativeWidth * imageBounds.Width;
        double height = _relativeHeight * imageBounds.Height;

        _bounds = new Rect(x, y, width, height);
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateRelativeCoordinates(Rect imageBounds)
    {
        if (imageBounds.IsEmpty || imageBounds.Width == 0 || imageBounds.Height == 0) return;

        // Convert absolute coordinates to relative (0.0 to 1.0)
        _relativeX = (float)((_bounds.X - imageBounds.X) / imageBounds.Width);
        _relativeY = (float)((_bounds.Y - imageBounds.Y) / imageBounds.Height);
        _relativeWidth = (float)(_bounds.Width / imageBounds.Width);
        _relativeHeight = (float)(_bounds.Height / imageBounds.Height);
    }

    private ResizeHandle GetResizeHandle(Point location)
    {
        Rect[] handles = GetResizeHandleRects();

        if (!IsCircular)
        {
            if (handles[0].Contains(location)) return ResizeHandle.TopLeft;
            if (handles[1].Contains(location)) return ResizeHandle.TopRight;
            if (handles[2].Contains(location)) return ResizeHandle.BottomLeft;
            if (handles[3].Contains(location)) return ResizeHandle.BottomRight;
        }
        if (handles[4].Contains(location)) return ResizeHandle.Left;
        if (handles[5].Contains(location)) return ResizeHandle.Right;
        if (handles[6].Contains(location)) return ResizeHandle.Top;
        if (handles[7].Contains(location)) return ResizeHandle.Bottom;

        return ResizeHandle.None;
    }

    private bool ContainsSelection(Point location)
    {
        if (!IsCircular) return _bounds.Contains(location);
        double radius = _bounds.Width / 2;
        double dx = location.X - (_bounds.X + radius);
        double dy = location.Y - (_bounds.Y + radius);
        return dx * dx + dy * dy <= radius * radius;
    }

    private Rect[] GetResizeHandleRects()
    {
        return new Rect[]
        {
            new Rect(_bounds.Left - HANDLE_SIZE/2, _bounds.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE),    // TopLeft
            new Rect(_bounds.Right - HANDLE_SIZE/2, _bounds.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE),   // TopRight
            new Rect(_bounds.Left - HANDLE_SIZE/2, _bounds.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // BottomLeft
            new Rect(_bounds.Right - HANDLE_SIZE/2, _bounds.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE),// BottomRight
            new Rect(_bounds.Left - HANDLE_SIZE/2, _bounds.Y + _bounds.Height/2 - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE),  // Left
            new Rect(_bounds.Right - HANDLE_SIZE/2, _bounds.Y + _bounds.Height/2 - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // Right
            new Rect(_bounds.X + _bounds.Width/2 - HANDLE_SIZE/2, _bounds.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE),   // Top
            new Rect(_bounds.X + _bounds.Width/2 - HANDLE_SIZE/2, _bounds.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE) // Bottom
        };
    }

    private static SolidColorBrush CreateFrozenOverlayBrush()
    {
        var brush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        brush.Freeze();
        return brush;
    }

    private static Pen CreateFrozenYellowPen()
    {
        var pen = new Pen(Brushes.Yellow, 2);
        pen.Freeze();
        return pen;
    }

    private static Pen CreateFrozenYellowDashedPen()
    {
        var pen = new Pen(Brushes.Yellow, 1) { DashStyle = DashStyles.Dash };
        pen.Freeze();
        return pen;
    }

    private void DrawResizeHandles(DrawingContext dc)
    {
        Rect[] handles = GetResizeHandleRects();
        for (int i = IsCircular ? 4 : 0; i < handles.Length; i++)
        {
            dc.DrawRectangle(Brushes.Yellow, null, handles[i]);
        }
    }

    private void ResizeBox(Point currentLocation, Rect imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        if (IsCircular)
        {
            ResizeCircle(currentLocation, imageBounds);
            return;
        }

        double newWidth, newHeight;
        double newX = _bounds.X;
        double newY = _bounds.Y;

        switch (_resizeHandle)
        {
            case ResizeHandle.Right:
            case ResizeHandle.BottomRight:
                newWidth = Math.Max(MIN_SELECTION_WIDTH, Math.Min(currentLocation.X - _bounds.X, imageBounds.Right - _bounds.X));
                newHeight = newWidth / 2; // Maintain 2:1 aspect ratio
                // Ensure height doesn't exceed image bounds
                if (_bounds.Y + newHeight > imageBounds.Bottom)
                {
                    newHeight = imageBounds.Bottom - _bounds.Y;
                    newWidth = newHeight * 2;
                }
                break;

            case ResizeHandle.Left:
            case ResizeHandle.TopLeft:
                newWidth = Math.Max(MIN_SELECTION_WIDTH, Math.Min(_bounds.Right - currentLocation.X, _bounds.Right - imageBounds.X));
                newHeight = newWidth / 2;
                newX = _bounds.Right - newWidth;
                if (_resizeHandle == ResizeHandle.TopLeft)
                {
                    newY = _bounds.Bottom - newHeight;
                    // Ensure top doesn't go above image bounds
                    if (newY < imageBounds.Y)
                    {
                        newY = imageBounds.Y;
                        newHeight = _bounds.Bottom - newY;
                        newWidth = newHeight * 2;
                        newX = _bounds.Right - newWidth;
                    }
                }
                break;

            case ResizeHandle.Bottom:
                newHeight = Math.Max(MIN_SELECTION_HEIGHT, Math.Min(currentLocation.Y - _bounds.Y, imageBounds.Bottom - _bounds.Y));
                newWidth = newHeight * 2; // Maintain 2:1 aspect ratio
                // Ensure width doesn't exceed image bounds
                if (_bounds.X + newWidth > imageBounds.Right)
                {
                    newWidth = imageBounds.Right - _bounds.X;
                    newHeight = newWidth / 2;
                }
                break;

            case ResizeHandle.Top:
                newHeight = Math.Max(MIN_SELECTION_HEIGHT, Math.Min(_bounds.Bottom - currentLocation.Y, _bounds.Bottom - imageBounds.Y));
                newWidth = newHeight * 2;
                newY = _bounds.Bottom - newHeight;
                // Ensure width doesn't exceed image bounds
                if (_bounds.X + newWidth > imageBounds.Right)
                {
                    newWidth = imageBounds.Right - _bounds.X;
                    newHeight = newWidth / 2;
                    newY = _bounds.Bottom - newHeight;
                }
                break;

            case ResizeHandle.TopRight:
                newWidth = Math.Max(MIN_SELECTION_WIDTH, Math.Min(currentLocation.X - _bounds.X, imageBounds.Right - _bounds.X));
                newHeight = newWidth / 2;
                newY = _bounds.Bottom - newHeight;
                // Ensure top doesn't go above image bounds
                if (newY < imageBounds.Y)
                {
                    newY = imageBounds.Y;
                    newHeight = _bounds.Bottom - newY;
                    newWidth = newHeight * 2;
                }
                break;

            case ResizeHandle.BottomLeft:
                newWidth = Math.Max(MIN_SELECTION_WIDTH, Math.Min(_bounds.Right - currentLocation.X, _bounds.Right - imageBounds.X));
                newHeight = newWidth / 2;
                newX = _bounds.Right - newWidth;
                // Ensure bottom doesn't go below image bounds
                if (_bounds.Y + newHeight > imageBounds.Bottom)
                {
                    newHeight = imageBounds.Bottom - _bounds.Y;
                    newWidth = newHeight * 2;
                    newX = _bounds.Right - newWidth;
                }
                break;

            default:
                return;
        }

        _bounds = new Rect(newX, newY, newWidth, newHeight);
        // Final constraint check to ensure everything is within bounds
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
    }

    private void ResizeCircle(Point location, Rect imageBounds)
    {
        double centerX = _bounds.X + _bounds.Width / 2;
        double centerY = _bounds.Y + _bounds.Height / 2;
        bool horizontal = _resizeHandle is ResizeHandle.Left or ResizeHandle.Right;
        bool fromStart = _resizeHandle is ResizeHandle.Left or ResizeHandle.Top;
        double anchor = horizontal
            ? (fromStart ? _bounds.Right : _bounds.Left)
            : (fromStart ? _bounds.Bottom : _bounds.Top);
        double requested = fromStart
            ? anchor - (horizontal ? location.X : location.Y)
            : (horizontal ? location.X : location.Y) - anchor;
        double available = horizontal
            ? (fromStart ? anchor - imageBounds.Left : imageBounds.Right - anchor)
            : (fromStart ? anchor - imageBounds.Top : imageBounds.Bottom - anchor);
        double crossAxisAvailable = horizontal
            ? 2 * Math.Min(centerY - imageBounds.Top, imageBounds.Bottom - centerY)
            : 2 * Math.Min(centerX - imageBounds.Left, imageBounds.Right - centerX);
        double maximum = Math.Min(available, crossAxisAvailable);
        double diameter = Math.Clamp(requested, Math.Min(MIN_SELECTION_WIDTH, maximum), maximum);
        double start = fromStart ? anchor - diameter : anchor;
        _bounds = horizontal
            ? new Rect(start, centerY - diameter / 2, diameter, diameter)
            : new Rect(centerX - diameter / 2, start, diameter, diameter);
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
    }

    private void ConstrainToImageBounds(Rect imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        _imageBounds = imageBounds;

        double x = _bounds.X, y = _bounds.Y, w = _bounds.Width, h = _bounds.Height;

        double aspectRatio = IsCircular ? 1 : 2;
        double maximumWidth = Math.Min(imageBounds.Width, imageBounds.Height * aspectRatio);
        w = Math.Clamp(w, Math.Min(MIN_SELECTION_WIDTH, maximumWidth), maximumWidth);
        h = w / aspectRatio;
        x = Math.Clamp(x, imageBounds.Left, Math.Max(imageBounds.Left, imageBounds.Right - w));
        y = Math.Clamp(y, imageBounds.Top, Math.Max(imageBounds.Top, imageBounds.Bottom - h));

        _bounds = new Rect(x, y, w, h);
    }
}
