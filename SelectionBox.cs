using System.Drawing;
using System.Windows.Forms;

namespace insta_cutter;

public class SelectionBox
{
    private Rectangle _bounds = new Rectangle(50, 50, 200, 100);
    private bool _isDragging = false;
    private bool _isResizing = false;
    private Point _lastMousePosition;
    private ResizeHandle _resizeHandle = ResizeHandle.None;
    private const int HANDLE_SIZE = 8;
    
    // Relative position tracking for window resize
    private float _relativeX = 0f;
    private float _relativeY = 0.25f;
    private float _relativeWidth = 1f;
    private float _relativeHeight = 0.5f;

    public Rectangle Bounds => _bounds;
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

    public void InitializeForImage(Rectangle imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        
        // Create selection box spanning full width of image
        int boxHeight = imageBounds.Width / 2; // Maintain 2:1 aspect ratio
        
        // Center vertically within the image
        int boxY = imageBounds.Y + (imageBounds.Height - boxHeight) / 2;
        
        _bounds = new Rectangle(imageBounds.X, boxY, imageBounds.Width, boxHeight);
        
        // Ensure it fits within image bounds
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CreateAtPoint(Point location, Rectangle imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        
        // Start with a reasonable size (100x50 for 2:1 ratio)
        int width = 100;
        int height = 50;
        
        // Center the box on the click point
        int x = location.X - width / 2;
        int y = location.Y - height / 2;
        
        _bounds = new Rectangle(x, y, width, height);
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HandleMouseDown(Point location, Rectangle imageBounds)
    {
        _lastMousePosition = location;
        
        // Check if clicking on a resize handle
        _resizeHandle = GetResizeHandle(location);
        if (_resizeHandle != ResizeHandle.None)
        {
            _isResizing = true;
            return true;
        }
        // Check if clicking inside the selection box
        else if (_bounds.Contains(location))
        {
            _isDragging = true;
            return true;
        }
        // Create new selection box
        else
        {
            CreateAtPoint(location, imageBounds);
            _isResizing = true;
            _resizeHandle = ResizeHandle.BottomRight;
            return true;
        }
    }

    public bool HandleMouseMove(Point location, Rectangle imageBounds)
    {
        if (_isDragging)
        {
            int deltaX = location.X - _lastMousePosition.X;
            int deltaY = location.Y - _lastMousePosition.Y;
            
            // Move the selection box
            _bounds.X += deltaX;
            _bounds.Y += deltaY;
            
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
            _ => _bounds.Contains(location) ? Cursors.SizeAll : Cursors.Cross
        };
    }

    public void Draw(Graphics graphics)
    {
        // Draw the selection box
        using (Pen pen = new Pen(Color.Red, 2))
        {
            graphics.DrawRectangle(pen, _bounds);
        }
        
        // Draw resize handles
        DrawResizeHandles(graphics);
    }

    public void UpdateFromRelativeCoordinates(Rectangle imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        
        // Convert relative coordinates back to absolute
        int x = imageBounds.X + (int)(_relativeX * imageBounds.Width);
        int y = imageBounds.Y + (int)(_relativeY * imageBounds.Height);
        int width = (int)(_relativeWidth * imageBounds.Width);
        int height = (int)(_relativeHeight * imageBounds.Height);
        
        _bounds = new Rectangle(x, y, width, height);
        ConstrainToImageBounds(imageBounds);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateRelativeCoordinates(Rectangle imageBounds)
    {
        if (imageBounds.IsEmpty || imageBounds.Width == 0 || imageBounds.Height == 0) return;
        
        // Convert absolute coordinates to relative (0.0 to 1.0)
        _relativeX = (float)(_bounds.X - imageBounds.X) / imageBounds.Width;
        _relativeY = (float)(_bounds.Y - imageBounds.Y) / imageBounds.Height;
        _relativeWidth = (float)_bounds.Width / imageBounds.Width;
        _relativeHeight = (float)_bounds.Height / imageBounds.Height;
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
            new Rectangle(_bounds.Left - HANDLE_SIZE/2, _bounds.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // TopLeft
            new Rectangle(_bounds.Right - HANDLE_SIZE/2, _bounds.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // TopRight
            new Rectangle(_bounds.Left - HANDLE_SIZE/2, _bounds.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // BottomLeft
            new Rectangle(_bounds.Right - HANDLE_SIZE/2, _bounds.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // BottomRight
            new Rectangle(_bounds.Left - HANDLE_SIZE/2, _bounds.Y + _bounds.Height/2 - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // Left
            new Rectangle(_bounds.Right - HANDLE_SIZE/2, _bounds.Y + _bounds.Height/2 - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // Right
            new Rectangle(_bounds.X + _bounds.Width/2 - HANDLE_SIZE/2, _bounds.Top - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE), // Top
            new Rectangle(_bounds.X + _bounds.Width/2 - HANDLE_SIZE/2, _bounds.Bottom - HANDLE_SIZE/2, HANDLE_SIZE, HANDLE_SIZE) // Bottom
        };
    }

    private void DrawResizeHandles(Graphics graphics)
    {
        Rectangle[] handles = GetResizeHandleRectangles();
        
        using (Brush brush = new SolidBrush(Color.Red))
        {
            foreach (Rectangle handle in handles)
            {
                graphics.FillRectangle(brush, handle);
            }
        }
    }

    private void ResizeBox(Point currentLocation, Rectangle imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        
        int newWidth, newHeight;
        int newX = _bounds.X;
        int newY = _bounds.Y;
        
        switch (_resizeHandle)
        {
            case ResizeHandle.Right:
            case ResizeHandle.BottomRight:
                newWidth = Math.Max(20, Math.Min(currentLocation.X - _bounds.X, imageBounds.Right - _bounds.X));
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
                newWidth = Math.Max(20, Math.Min(_bounds.Right - currentLocation.X, _bounds.Right - imageBounds.X));
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
                newHeight = Math.Max(10, Math.Min(currentLocation.Y - _bounds.Y, imageBounds.Bottom - _bounds.Y));
                newWidth = newHeight * 2; // Maintain 2:1 aspect ratio
                // Ensure width doesn't exceed image bounds
                if (_bounds.X + newWidth > imageBounds.Right)
                {
                    newWidth = imageBounds.Right - _bounds.X;
                    newHeight = newWidth / 2;
                }
                break;
                
            case ResizeHandle.Top:
                newHeight = Math.Max(10, Math.Min(_bounds.Bottom - currentLocation.Y, _bounds.Bottom - imageBounds.Y));
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
                newWidth = Math.Max(20, Math.Min(currentLocation.X - _bounds.X, imageBounds.Right - _bounds.X));
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
                newWidth = Math.Max(20, Math.Min(_bounds.Right - currentLocation.X, _bounds.Right - imageBounds.X));
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
        
        _bounds = new Rectangle(newX, newY, newWidth, newHeight);
        // Final constraint check to ensure everything is within bounds
        ConstrainToImageBounds(imageBounds);
        UpdateRelativeCoordinates(imageBounds);
    }

    private void ConstrainToImageBounds(Rectangle imageBounds)
    {
        if (imageBounds.IsEmpty) return;
        
        // Keep selection box within image bounds
        if (_bounds.X < imageBounds.X) _bounds.X = imageBounds.X;
        if (_bounds.Y < imageBounds.Y) _bounds.Y = imageBounds.Y;
        if (_bounds.Right > imageBounds.Right) _bounds.X = imageBounds.Right - _bounds.Width;
        if (_bounds.Bottom > imageBounds.Bottom) _bounds.Y = imageBounds.Bottom - _bounds.Height;
        
        // Ensure minimum size
        if (_bounds.Width < 20)
        {
            _bounds.Width = 20;
            _bounds.Height = 10;
        }
        if (_bounds.Height < 10)
        {
            _bounds.Height = 10;
            _bounds.Width = 20;
        }
    }
}