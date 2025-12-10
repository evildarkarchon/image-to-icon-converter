# Project Context

## Purpose

Image to Icon Converter is a fast, cross-platform command-line tool that converts various image formats (PNG, JPEG, BMP, SVG, GIF, TIFF, WebP) to Windows ICO icon format. The application generates multi-resolution icons with customizable sizes and color depths, optimized for native AOT compilation to provide fast startup and minimal dependencies.

### Goals
- Convert common image formats to ICO with high quality
- Support multi-resolution icon generation (16x16, 32x32, 48x48, 256x256)
- Provide batch processing capabilities
- Achieve fast startup through AOT compilation
- Maintain cross-platform compatibility (Windows, Linux, macOS)

## Tech Stack

- **Runtime**: .NET 10
- **Language**: C# with nullable reference types enabled
- **Image Processing**: SixLabors.ImageSharp 3.1.12
- **CLI Parsing**: System.CommandLine 2.0.1
- **Testing**: xUnit 2.9.3 with coverlet for code coverage
- **Build**: Native AOT compilation with `PublishAot=true`

## Project Conventions

### Code Style

**Namespaces**: Use file-scoped namespaces
```csharp
namespace ImageToIconConverter.Services;
```

**Naming Conventions**:
| Element         | Convention  | Example           |
| --------------- | ----------- | ----------------- |
| Classes         | PascalCase  | `ImageConverter`  |
| Interfaces      | IPascalCase | `IImageConverter` |
| Methods         | PascalCase  | `ConvertToIcon`   |
| Properties      | PascalCase  | `OutputPath`      |
| Private fields  | _camelCase  | `_imageProcessor` |
| Local variables | camelCase   | `inputFile`       |
| Constants       | PascalCase  | `DefaultIconSize` |
| Parameters      | camelCase   | `sourceImage`     |

**Preferred Patterns**:
- Primary constructors for simple dependency injection
- Expression-bodied members for simple operations
- Collection expressions (`[".png", ".jpg"]`)
- Async/await with `Async` suffix and `CancellationToken` support

**Error Handling**:
- Use specific exception types (`FileNotFoundException`, `NotSupportedException`)
- Present user-friendly error messages in CLI output
- Return non-zero exit codes on failure

### Architecture Patterns

**Project Structure**:
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

**Key Patterns**:
- Interface-driven design for testability
- Clean separation of concerns (Commands → Services → Models)
- AOT-compatible patterns (avoid reflection, use source generators)
- Proper resource disposal with `using` statements

### Testing Strategy

**Framework**: xUnit with coverlet for code coverage

**Test Naming**: `MethodName_StateUnderTest_ExpectedBehavior`
```csharp
[Fact]
public void ConvertToIcon_ValidPngInput_CreatesIcoFile()
```

**Test Categories**:
- **Unit Tests**: Individual components in isolation
- **Integration Tests**: File I/O and format conversion
- **End-to-End Tests**: CLI commands with actual files

**Test Data**:
- Store in `image-to-icon-converter.Tests/TestData/`
- Keep files small (< 100KB)
- Include valid and invalid samples for each format
- Include edge cases (1x1, large images, transparent PNGs)

### Git Workflow

**Branching Strategy**:
- `main` - stable, production-ready code
- `feature/*` - new feature development
- Feature branches merged via Pull Request

**Commit Guidelines**:
- Clear, descriptive commit messages
- Reference issues where applicable

**Contributing Process**:
1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Follow coding conventions
4. Write tests for new functionality
5. Ensure all tests pass (`dotnet test`)
6. Submit Pull Request

## Domain Context

### ICO File Format

**Structure**:
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

**Standard Icon Sizes**:
| Size    | Usage                             |
| ------- | --------------------------------- |
| 16x16   | Small icons, taskbar, tree views  |
| 24x24   | Toolbar icons (optional)          |
| 32x32   | Desktop icons, standard views     |
| 48x48   | Large icons view                  |
| 64x64   | Extra large icons (optional)      |
| 256x256 | Thumbnail view, high-DPI displays |

**Color Depths**:
- 32-bit ARGB: Full color with alpha transparency (default)
- 24-bit RGB: Full color without transparency
- 8-bit Indexed: 256 colors for legacy compatibility

### Image Format Detection

Always verify format by reading magic bytes, not file extension:
```csharp
private static bool IsPng(ReadOnlySpan<byte> data) =>
    data.Length >= 8 &&
    data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
```

## Important Constraints

### AOT Compilation Constraints
- **No reflection**: Avoid `System.Reflection.Emit` and dynamic type loading
- **Static analysis**: All types must be statically analyzable
- **Source generators**: Use where possible instead of runtime reflection
- **JSON serialization**: Mark types with `[JsonSerializable]` attribute
- **Trimming warnings**: Address with `[DynamicallyAccessedMembers]` or redesign

### Globalization
- **Invariant globalization enabled**: Do not use culture-specific formatting
- Results in smaller binary size

### Memory Efficiency
- Use `Span<T>` and `stackalloc` for byte manipulation
- Dispose images properly with `using` statements
- Use `ArrayPool<T>` for large buffers

### Cross-Platform
- Must work on Windows, Linux, and macOS
- Use platform-agnostic APIs
- No native dependencies except what ImageSharp provides

## External Dependencies

### Runtime Dependencies

| Package              | Version | Purpose                                                           |
| -------------------- | ------- | ----------------------------------------------------------------- |
| SixLabors.ImageSharp | 3.1.12  | Cross-platform image processing (PNG, JPEG, BMP, GIF, TIFF, WebP) |
| System.CommandLine   | 2.0.1   | Modern CLI argument parsing with help generation                  |

### Test Dependencies

| Package                   | Version | Purpose                      |
| ------------------------- | ------- | ---------------------------- |
| xunit                     | 2.9.3   | Unit testing framework       |
| xunit.runner.visualstudio | 3.1.4   | VS Test Explorer integration |
| Microsoft.NET.Test.Sdk    | 17.14.1 | Test SDK                     |
| coverlet.collector        | 6.0.4   | Code coverage collection     |

### Future Considerations

- **SVG Support**: May require Svg.Skia or SkiaSharp for rendering SVG to raster before conversion
- **Logging**: Microsoft.Extensions.Logging.Abstractions if logging is needed (AOT-compatible)
