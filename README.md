# Image to Icon Converter

A fast, cross-platform command-line tool that converts images to Windows ICO icon format with support for multi-resolution icons.

## Features

- Convert PNG, JPEG, BMP, and SVG images to ICO format
- Generate multi-resolution icons (16x16, 32x32, 48x48, 256x256)
- Batch processing for multiple files
- Custom output sizes and color depths
- Native AOT compilation for fast startup
- Cross-platform support (Windows, Linux, macOS)

## Installation

### Pre-built Binaries

Download the latest release for your platform from the [Releases](../../releases) page.

**Windows:**
```powershell
# Extract and add to PATH
Expand-Archive img2ico-win-x64.zip -DestinationPath C:\Tools\img2ico
$env:PATH += ";C:\Tools\img2ico"
```

**Linux/macOS:**
```bash
# Extract and make executable
tar -xzf img2ico-linux-x64.tar.gz
chmod +x img2ico
sudo mv img2ico /usr/local/bin/
```

### Building from Source

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

```bash
# Clone the repository
git clone https://github.com/yourusername/image-to-icon-converter.git
cd image-to-icon-converter

# Build
dotnet build

# Run
dotnet run --project image-to-icon-converter -- --help

# Publish native AOT binary
dotnet publish -c Release
```

The compiled binary will be in `image-to-icon-converter/bin/Release/net10.0/publish/`.

## Usage

### Basic Conversion

Convert a single image to ICO format:

```bash
img2ico --input logo.png --output logo.ico
```

Or using short options:

```bash
img2ico -i logo.png -o logo.ico
```

### Multi-Resolution Icons

Generate an icon containing multiple sizes (recommended for Windows applications):

```bash
img2ico -i logo.png -o logo.ico --sizes 16,32,48,256
```

This creates an ICO file with 16x16, 32x32, 48x48, and 256x256 variants embedded.

### Custom Single Size

Create an icon with a specific size:

```bash
img2ico -i logo.png -o logo.ico --size 64
```

### Batch Processing

Convert all PNG files in a directory:

```bash
img2ico --input-dir ./images --output-dir ./icons --pattern "*.png"
```

Convert multiple specific files:

```bash
img2ico -i file1.png file2.jpg file3.bmp -o ./icons/
```

### SVG Conversion

Convert SVG to ICO with specified render size:

```bash
img2ico -i vector-logo.svg -o logo.ico --svg-size 512 --sizes 16,32,48,256
```

The `--svg-size` option specifies the resolution at which to rasterize the SVG before resizing to icon dimensions.

### Color Depth Options

Specify color depth for the output:

```bash
# 32-bit with alpha transparency (default)
img2ico -i logo.png -o logo.ico --depth 32

# 24-bit RGB (no transparency)
img2ico -i logo.png -o logo.ico --depth 24

# 8-bit (256 colors, legacy support)
img2ico -i logo.png -o logo.ico --depth 8
```

### Preserve Aspect Ratio

By default, images are resized to square dimensions. To preserve aspect ratio with padding:

```bash
img2ico -i wide-logo.png -o logo.ico --preserve-aspect --background transparent
```

### Additional Options

```bash
img2ico --help
```

```
Description:
  Convert images to ICO icon format

Usage:
  img2ico [options]

Options:
  -i, --input <file>          Input image file path (required)
  -o, --output <file>         Output ICO file path [default: input name with .ico]
  --input-dir <directory>     Input directory for batch processing
  --output-dir <directory>    Output directory for batch processing
  --pattern <glob>            File pattern for batch processing [default: *.*]
  --size <pixels>             Single output size in pixels [default: 256]
  --sizes <list>              Comma-separated list of sizes (e.g., 16,32,48,256)
  --depth <bits>              Color depth: 8, 24, or 32 [default: 32]
  --svg-size <pixels>         Render size for SVG input [default: 512]
  --preserve-aspect           Preserve aspect ratio with padding
  --background <color>        Background color for padding [default: transparent]
  --overwrite                 Overwrite existing output files
  -v, --verbose               Enable verbose output
  --version                   Show version information
  -?, -h, --help              Show help and usage information
```

## Supported Input Formats

| Format | Extensions | Notes |
|--------|------------|-------|
| PNG | `.png` | Full support including transparency |
| JPEG | `.jpg`, `.jpeg` | Converted to opaque icon |
| BMP | `.bmp` | Windows bitmap format |
| SVG | `.svg` | Rasterized at specified resolution |
| GIF | `.gif` | First frame only, transparency supported |
| TIFF | `.tif`, `.tiff` | First page only |
| WebP | `.webp` | Full support including transparency |

## ICO Output Specifications

### Icon Sizes

Standard Windows icon sizes and their typical uses:

| Size | Usage |
|------|-------|
| 16x16 | Small icons, taskbar, tree views |
| 24x24 | Toolbar icons (optional) |
| 32x32 | Desktop icons, standard views |
| 48x48 | Large icons view |
| 64x64 | Extra large icons (optional) |
| 256x256 | Thumbnail view, high-DPI displays |

### Color Depth

- **32-bit ARGB**: Full color with alpha transparency. Recommended for modern applications.
- **24-bit RGB**: Full color without transparency. Use when transparency is not needed.
- **8-bit Indexed**: 256 colors with optional transparency. For legacy compatibility.

### ICO File Structure

Icons with sizes below 256x256 are stored in BMP format (uncompressed) for maximum compatibility. The 256x256 variant is stored as PNG to reduce file size, following Windows Vista+ conventions.

## Examples

### Application Icon

Create a complete application icon with all standard sizes:

```bash
img2ico -i app-logo.png -o app.ico --sizes 16,32,48,256
```

### Favicon

Create a favicon for a website:

```bash
img2ico -i logo.png -o favicon.ico --sizes 16,32,48
```

### High-Quality Icon from SVG

Create a high-quality icon from a vector source:

```bash
img2ico -i logo.svg -o logo.ico --svg-size 1024 --sizes 16,32,48,64,256
```

### Batch Convert Marketing Assets

Convert all PNG files in a folder:

```bash
img2ico --input-dir ./marketing/logos --output-dir ./marketing/icons --pattern "*.png" --sizes 32,256
```

### Replace Existing Icons

Convert and overwrite existing ICO files:

```bash
img2ico -i new-logo.png -o existing.ico --overwrite --sizes 16,32,48,256
```

## System Requirements

- **Operating System**: Windows 10+, Linux (glibc 2.17+), macOS 10.15+
- **Architecture**: x64, ARM64
- **Disk Space**: ~10 MB for the application
- **Memory**: Minimum 64 MB, varies with image size

### Build Requirements

- .NET 10 SDK or later
- For AOT publishing: Native C++ toolchain (MSVC on Windows, Clang on Linux/macOS)

## Dependencies

This project uses the following libraries:

- [ImageSharp](https://github.com/SixLabors/ImageSharp) - Cross-platform image processing
- [System.CommandLine](https://github.com/dotnet/command-line-api) - Command-line parsing

## Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Follow** the coding conventions in [CLAUDE.md](CLAUDE.md)
4. **Write** tests for new functionality
5. **Ensure** all tests pass (`dotnet test`)
6. **Commit** your changes (`git commit -m 'Add amazing feature'`)
7. **Push** to the branch (`git push origin feature/amazing-feature`)
8. **Open** a Pull Request

### Development Setup

```bash
# Clone your fork
git clone https://github.com/yourusername/image-to-icon-converter.git
cd image-to-icon-converter

# Build and run tests
dotnet build
dotnet test

# Run with arguments
dotnet run --project image-to-icon-converter -- -i test.png -o test.ico
```

### Code Style

- Follow C# naming conventions (PascalCase for public members, camelCase for locals)
- Use file-scoped namespaces
- Enable nullable reference types
- Write XML documentation for public APIs
- See [CLAUDE.md](CLAUDE.md) for detailed guidelines

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgments

- [ICO file format specification](https://en.wikipedia.org/wiki/ICO_(file_format))
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) for excellent cross-platform image processing
