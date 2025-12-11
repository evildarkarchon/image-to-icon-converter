# Image to Icon Converter

A fast, cross-platform CLI tool that converts images (PNG, JPEG, BMP) to Windows ICO format with multi-resolution support.

## Features

- **Multiple Input Formats**: Supports PNG, JPEG, and BMP images
- **Multi-Resolution Icons**: Generate icons with multiple sizes in a single ICO file
- **High-Quality Resizing**: Uses Lanczos3 resampling for crisp icon rendering
- **Transparency Support**: Preserves alpha channel from PNG images
- **AOT Compatible**: Compiles to native code for fast startup and small deployment
- **Cross-Platform**: Runs on Windows, Linux, and macOS

## Installation

### From Source

```bash
# Clone the repository
git clone https://github.com/yourusername/image-to-icon-converter.git
cd image-to-icon-converter

# Build the project
dotnet build

# Run the application
dotnet run --project image-to-icon-converter -- --input your-image.png
```

### Publish as Native Executable

```bash
# Windows
dotnet publish -c Release -r win-x64

# Linux
dotnet publish -c Release -r linux-x64

# macOS
dotnet publish -c Release -r osx-x64
```

## Usage

### Basic Usage

Convert an image to ICO format:

```bash
image-to-icon-converter --input logo.png
```

This creates `logo.ico` in the same directory with all default icon sizes (16x16, 32x32, 48x48, 256x256).

### Specify Output Path

```bash
image-to-icon-converter --input logo.png --output app-icon.ico
```

### Select Specific Sizes

```bash
# Only 16x16 and 32x32
image-to-icon-converter --input logo.png --sizes 16,32

# Only 256x256 (high-resolution)
image-to-icon-converter --input logo.png --sizes 256
```

### Overwrite Existing File

```bash
image-to-icon-converter --input logo.png --overwrite
```

### All Options

```
image-to-icon-converter - Convert images to ICO format for use as Windows icons

Usage:
  image-to-icon-converter --input <path> [options]
  image-to-icon-converter -i <path> [options]

Options:
  -i, --input <path>     (Required) Input image file path (PNG, JPEG, or BMP)
  -o, --output <path>    Output ICO file path (default: input path with .ico extension)
  -s, --sizes <sizes>    Comma-separated icon sizes to generate (default: 16,32,48,256)
                         Valid sizes: 16, 32, 48, 256
  -y, --overwrite        Overwrite output file if it exists without prompting
  -h, --help             Show help message
  -v, --version          Show version information
```

### Exit Codes

| Code | Meaning              |
| ---- | -------------------- |
| 0    | Success              |
| 1    | Invalid arguments    |
| 2    | Input file not found |
| 3    | Unsupported format   |
| 4    | Output write failed  |

## Icon Sizes

The converter supports standard Windows icon sizes:

| Size    | Use Case                                      |
| ------- | --------------------------------------------- |
| 16x16   | Small icons, taskbar, menus                   |
| 32x32   | Standard icons, desktop                       |
| 48x48   | Large icons, Windows Explorer                 |
| 256x256 | Extra large icons (Vista+), high-DPI displays |

For 256x256 icons, PNG compression is used automatically for better file size.

## Requirements

- .NET 10.0 or later (for building)
- No runtime dependencies when published as AOT

## Development

### Project Structure

```
image-to-icon-converter/
├── image-to-icon-converter/          # Main CLI application
│   ├── Program.cs                    # Entry point and CLI parsing
│   ├── Models/                       # Data models
│   │   ├── IconSize.cs               # Icon size enumeration
│   │   ├── ImageFormat.cs            # Image format detection
│   │   ├── ConvertOptions.cs         # Conversion options
│   │   └── ConversionResult.cs       # Conversion result
│   └── Services/                     # Business logic
│       ├── IImageLoader.cs           # Image loading interface
│       ├── ImageLoader.cs            # Image loading implementation
│       ├── IIcoGenerator.cs          # ICO generation interface
│       └── IcoGenerator.cs           # ICO generation implementation
├── image-to-icon-converter.Tests/    # Unit tests (xUnit)
└── image-to-icon-converter.slnx      # Solution file
```

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Code Style

The project follows C# conventions:
- PascalCase for public members
- _camelCase for private fields
- File-scoped namespaces
- Nullable reference types enabled

## License

See [LICENSE.txt](LICENSE.txt) for details.
