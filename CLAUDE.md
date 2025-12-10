<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->

# Image to Icon Converter - Project Rules

## Project Overview

This is a C# CLI application that converts various image formats (PNG, JPEG, BMP, SVG) to the ICO icon format. The application supports multi-resolution icon generation and is designed for AOT (Ahead-of-Time) compilation.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run --project image-to-icon-converter

# Run tests
dotnet test

# Publish AOT build (release)
dotnet publish -c Release
```

## Architecture

### Project Structure

```
image-to-icon-converter/
├── image-to-icon-converter/          # Main CLI application
│   ├── Program.cs                    # Entry point and CLI parsing
│   ├── Commands/                     # Command implementations
│   ├── Services/                     # Business logic services
│   │   ├── IImageConverter.cs        # Image conversion interface
│   │   ├── ImageConverter.cs         # Main conversion implementation
│   │   └── IcoWriter.cs              # ICO file format writer
│   ├── Models/                       # Data models
│   └── Utils/                        # Utility classes
├── image-to-icon-converter.Tests/    # Unit tests (xUnit)
└── image-to-icon-converter.slnx      # Solution file
```

### Key Design Decisions

1. **AOT Compilation**: The project uses `PublishAot=true` for native compilation. Avoid reflection-heavy patterns and ensure all types are statically analyzable.

2. **Invariant Globalization**: Enabled for smaller binary size. Do not use culture-specific formatting.

3. **Nullable Reference Types**: Enabled project-wide. All reference types must be explicitly nullable or non-nullable.

## C# Style Guidelines

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `ImageConverter` |
| Interfaces | IPascalCase | `IImageConverter` |
| Methods | PascalCase | `ConvertToIcon` |
| Properties | PascalCase | `OutputPath` |
| Private fields | _camelCase | `_imageProcessor` |
| Local variables | camelCase | `inputFile` |
| Constants | PascalCase | `DefaultIconSize` |
| Parameters | camelCase | `sourceImage` |

### Code Style

```csharp
// Preferred: File-scoped namespaces
namespace ImageToIconConverter.Services;

// Preferred: Primary constructors for simple DI
public class ImageConverter(ILogger logger) : IImageConverter
{
    // Preferred: Expression-bodied members for simple operations
    public bool IsValidFormat(string extension) =>
        SupportedFormats.Contains(extension.ToLowerInvariant());

    // Preferred: Collection expressions
    private static readonly string[] SupportedFormats = [".png", ".jpg", ".jpeg", ".bmp", ".svg"];
}
```

### Error Handling

```csharp
// Use specific exception types
public void LoadImage(string path)
{
    if (!File.Exists(path))
        throw new FileNotFoundException($"Image file not found: {path}", path);

    if (!IsValidFormat(Path.GetExtension(path)))
        throw new NotSupportedException($"Unsupported image format: {Path.GetExtension(path)}");
}

// For CLI, catch and present user-friendly messages
try
{
    converter.Convert(options);
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
```

### Async Patterns

- Use `async`/`await` for I/O-bound operations
- Suffix async methods with `Async`
- Use `CancellationToken` for cancellable operations

```csharp
public async Task<byte[]> LoadImageAsync(string path, CancellationToken cancellationToken = default)
{
    return await File.ReadAllBytesAsync(path, cancellationToken);
}
```

## Preferred Libraries

### Image Processing

**Primary: ImageSharp** (`SixLabors.ImageSharp`)
- Cross-platform, no native dependencies
- AOT-compatible
- Supports PNG, JPEG, BMP, GIF, TIFF, WebP

```csharp
// Example usage
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

using var image = await Image.LoadAsync(inputPath);
image.Mutate(x => x.Resize(256, 256));
```

**SVG Support: Svg.Skia** or **SkiaSharp**
- For rendering SVG to raster images before ICO conversion

### CLI Parsing

**Primary: System.CommandLine**
- Modern, attribute-based CLI parsing
- Built-in help generation
- Tab completion support

```csharp
var rootCommand = new RootCommand("Convert images to ICO format");

var inputOption = new Option<FileInfo>(
    aliases: ["--input", "-i"],
    description: "Input image file path") { IsRequired = true };

rootCommand.AddOption(inputOption);
```

### Logging (if needed)

**Primary: Microsoft.Extensions.Logging.Abstractions**
- Lightweight logging abstraction
- AOT-compatible

## ICO File Format Guidelines

### Standard Icon Sizes

Generate these sizes for maximum compatibility:
- 16x16 - Small icons, taskbar
- 32x32 - Standard icons
- 48x48 - Large icons
- 256x256 - Extra large (Vista+), stored as PNG

### Color Depths

- 32-bit (ARGB) - Default, supports transparency
- 24-bit (RGB) - Legacy support
- 8-bit (256 colors) - Legacy support

### ICO Structure

```
ICO Header (6 bytes)
├── Reserved: 0
├── Type: 1 (icon)
└── Image Count: N

Image Directory (16 bytes × N)
├── Width, Height (0 = 256)
├── Color count, Reserved
├── Color planes, Bits per pixel
├── Image data size
└── Image data offset

Image Data
├── BMP format (for sizes < 256)
└── PNG format (for 256x256)
```

## Testing Guidelines

### Test Project

Uses xUnit with the following conventions:

```csharp
namespace ImageToIconConverter.Tests;

public class ImageConverterTests
{
    // Naming: MethodName_StateUnderTest_ExpectedBehavior
    [Fact]
    public void ConvertToIcon_ValidPngInput_CreatesIcoFile()
    {
        // Arrange
        var converter = new ImageConverter();
        var input = GetTestImagePath("valid.png");

        // Act
        var result = converter.ConvertToIcon(input);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".bmp")]
    public void IsValidFormat_SupportedExtensions_ReturnsTrue(string extension)
    {
        var converter = new ImageConverter();
        Assert.True(converter.IsValidFormat(extension));
    }
}
```

### Test Categories

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test file I/O and format conversion
- **End-to-End Tests**: Test CLI commands with actual files

### Test Data

Store test images in `image-to-icon-converter.Tests/TestData/`:
- Keep files small (< 100KB)
- Include valid and invalid samples for each format
- Include edge cases (1x1, large images, transparent PNGs)

## Adding New Features

### Feature Implementation Checklist

1. **Design**: Create interface in `Services/` if needed
2. **Implement**: Add implementation class
3. **Test**: Write unit tests covering:
   - Happy path
   - Edge cases
   - Error conditions
4. **CLI**: Add command/option in `Program.cs`
5. **Documentation**: Update README.md with usage examples

### Adding a New Input Format

1. Add format extension to supported formats list
2. Implement format-specific loading in `ImageConverter`
3. Add test images for the new format
4. Update README.md supported formats section

### Adding a New CLI Option

```csharp
// 1. Define the option
var newOption = new Option<int>(
    aliases: ["--size", "-s"],
    description: "Output icon size",
    getDefaultValue: () => 256);

// 2. Add to command
convertCommand.AddOption(newOption);

// 3. Handle in command handler
convertCommand.SetHandler(async (input, output, size) =>
{
    // Implementation
}, inputOption, outputOption, newOption);
```

## Performance Considerations

### AOT Compatibility

- Avoid `System.Reflection.Emit`
- Use source generators where possible
- Mark types for JSON serialization with `[JsonSerializable]`

### Memory Efficiency

```csharp
// Prefer spans for byte manipulation
Span<byte> buffer = stackalloc byte[256];

// Dispose images properly
using var image = Image.Load(path);

// Use ArrayPool for large buffers
var pool = ArrayPool<byte>.Shared;
var buffer = pool.Rent(minimumLength);
try
{
    // Use buffer
}
finally
{
    pool.Return(buffer);
}
```

## Common Issues

### AOT Trimming Warnings

If you see trimming warnings:
1. Add `[DynamicallyAccessedMembers]` attributes
2. Use `[RequiresUnreferencedCode]` for unavoidable reflection
3. Consider redesigning to avoid reflection

### Image Format Detection

Always verify image format by reading magic bytes, not file extension:
```csharp
private static bool IsPng(ReadOnlySpan<byte> data) =>
    data.Length >= 8 &&
    data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
```

## Code Review Checklist

- [ ] Follows naming conventions
- [ ] Nullable annotations are correct
- [ ] No reflection that breaks AOT
- [ ] Unit tests added/updated
- [ ] Error messages are user-friendly
- [ ] No hardcoded paths or magic numbers
- [ ] Disposable resources are properly disposed
