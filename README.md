# Insta-Cutter

A powerful WPF application for precise image cropping with an elastic selection box. Perfect for creating square image pairs from rectangular selections that can be posted to Instagram as horizontal rolling carousel content.

## ✨ Features

### 🖼️ Image Management
- **Multi-format Support**: Load JPG, JPEG, PNG, and BMP images
- **Aspect Ratio Preservation**: Images maintain their original proportions when displayed
- **High-Quality Processing**: Lossless image operations with no quality degradation

### 🎯 Precision Selection
- **Elastic Selection Box**: Draggable and resizable selection area with 2:1 aspect ratio
- **Smart Constraints**: Selection automatically stays within image boundaries
- **Visual Feedback**: Clear resize handles and cursor indicators
- **Relative Positioning**: Selection maintains position when window is resized

### 💾 Advanced Saving
- **Dual Output**: Automatically creates two square images from selected area
- **Quality Preservation**: Uses high-quality encoder settings for lossless saving
- **Multiple Formats**: Save as PNG (lossless), JPEG (max quality), or BMP (uncompressed)
- **Smart Naming**: Automatically generates `_left` and `_right` suffixed filenames

### 🎨 Modern UI
- **Theme Support**: Light, Dark, and System theme modes
- **Professional Menus**: Custom dark theme with optimized selection colors
- **Responsive Design**: Clean interface that adapts to window resizing
- **Intuitive Controls**: Standard WPF experience with modern styling

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 or later
- Windows operating system

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/[your-username]/insta-cutter.git
   cd insta-cutter
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## 📖 Usage

### Basic Workflow
1. **Load Image**: File → Load and select your image file
2. **Position Selection**: The elastic box appears spanning the full image width
3. **Adjust Selection**: Drag to move, use handles to resize (maintains 2:1 ratio)
4. **Save Results**: File → Save to export two square images

### Selection Controls
- **Move**: Click and drag inside the selection box
- **Resize**: Use the red square handles around the perimeter
- **Create New**: Click outside the current selection to start fresh

### Theme Options
- **Light Theme**: Clean white interface
- **Dark Theme**: Modern dark gray with optimized contrast
- **System Theme**: Automatically matches your Windows theme preference

## 🏗️ Architecture

The application follows .NET best practices with clear separation of concerns:

### Core Classes
- **`MainWindow`**: Presentation layer handling UI events and theme management
- **`SelectionBox`**: Business logic for elastic box functionality and coordinate management
- **`ImageProcessor`**: Data processing layer for image operations and high-quality saving

### Key Design Patterns
- **Single Responsibility**: Each class has one focused purpose
- **Event-Driven Architecture**: Loose coupling between UI and business logic
- **Relative Coordinate System**: Resolution-independent selection positioning

## 🛠️ Technical Highlights

### Image Processing
- Pixel-perfect coordinate conversion between display and image space
- High-quality graphics rendering with bicubic interpolation
- Custom encoder parameters for maximum quality preservation
- Efficient memory management with proper resource disposal

### Selection System
- Real-time aspect ratio maintenance (2:1 width:height)
- Comprehensive boundary checking and constraint enforcement
- Smooth resize operations with multiple handle types
- Relative positioning system for window resize compatibility

### Theme Engine
- Registry-based system theme detection
- WPF `ResourceDictionary` with dynamic `SolidColorBrush` resources
- Optimized color palette for accessibility and visual appeal
- Automatic theme switching with immediate visual feedback

## 📁 Project Structure

```
insta-cutter/
├── App.xaml                 # WPF application entry point
├── App.xaml.cs              # Application code-behind
├── MainWindow.xaml          # Main window XAML layout
├── MainWindow.xaml.cs       # Main window and presentation logic
├── SelectionBox.cs          # Elastic selection box implementation
├── ImageProcessor.cs        # Image processing and file operations
├── insta-cutter.csproj      # Project configuration
├── docs/
│   └── chat.md             # Development conversation log
└── README.md               # This file
```

## 🎯 Use Cases

- **Social Media Content**: Create perfectly sized image pairs for platforms
- **Design Work**: Extract square sections from rectangular images
- **Photography**: Crop landscape photos into square formats
- **Digital Art**: Prepare images for specific aspect ratio requirements

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## 📋 Requirements

- **Framework**: .NET 10.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Target OS**: Windows
- **Dependencies**: `System.Drawing.Common` (for pixel-level image processing)

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 🏷️ Version History

- **v1.0.0**: Initial release with core functionality
  - Image loading and display
  - Elastic selection box with 2:1 aspect ratio
  - High-quality dual image export
  - Multi-theme support
  - Professional UI with custom dark theme

## 🐛 Known Issues

None currently. Please report any bugs through GitHub Issues.

## 📞 Support

If you encounter any issues or have questions:
1. Check the [Issues](../../issues) page
2. Create a new issue with detailed description
3. Include steps to reproduce any bugs

---

**Made with ❤️ using .NET and WPF**