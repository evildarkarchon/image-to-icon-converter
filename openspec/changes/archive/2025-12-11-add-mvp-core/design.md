# MVP Core Design

## Context

This is a greenfield implementation establishing the foundational architecture for the image-to-icon converter CLI tool. The design must support AOT compilation constraints, maintain cross-platform compatibility, and follow .NET best practices for CLI applications.

**Stakeholders**: End users needing to convert images to ICO format for Windows application icons.

**Constraints**:
- AOT compilation (no reflection)
- Invariant globalization
- Cross-platform (Windows, Linux, macOS)
- Dependencies limited to ImageSharp and System.CommandLine

## Goals / Non-Goals

### Goals
- Simple, intuitive CLI interface
- High-quality image resizing for icons
- Correct ICO file format generation
- Clear error messages for common failure cases
- Testable architecture with interface-driven design

### Non-Goals
- GUI interface (CLI only for MVP)
- SVG support (deferred to future iteration)
- Batch processing (single file conversion for MVP)
- Custom color depth selection (32-bit ARGB only for MVP)

## Decisions

### 1. Service Architecture

**Decision**: Use interface-based services with constructor injection.

```
Program.cs (CLI entry)
    └── ConvertCommand
            ├── IImageLoader (loads and validates images)
            └── IIcoGenerator (generates ICO files)
```

**Rationale**: 
- Enables unit testing with mocks
- Supports future DI container integration
- AOT-compatible (no reflection required)

### 2. ICO Format Implementation

**Decision**: Use PNG embedding for 256x256, BMP format for smaller sizes.

| Size    | Format | Reason                                             |
| ------- | ------ | -------------------------------------------------- |
| 16x16   | BMP    | Smaller file size, universal compatibility         |
| 32x32   | BMP    | Standard format for this size                      |
| 48x48   | BMP    | Standard format for this size                      |
| 256x256 | PNG    | Compression efficiency, Windows Vista+ requirement |

**Rationale**: This follows Windows icon best practices. PNG at 256x256 provides better compression for large images while BMP for smaller sizes ensures maximum compatibility.

### 3. Image Resizing Strategy

**Decision**: Use Lanczos3 resampler from ImageSharp for high-quality downscaling.

```csharp
image.Mutate(x => x.Resize(new ResizeOptions
{
    Size = new Size(targetSize, targetSize),
    Sampler = KnownResamplers.Lanczos3,
    Mode = ResizeMode.Max,
    PadColor = Color.Transparent
}));
```

**Rationale**: 
- Lanczos3 provides excellent quality for downscaling
- ResizeMode.Max maintains aspect ratio
- Transparent padding handles non-square images

### 4. Format Detection

**Decision**: Detect image format via magic bytes, not file extension.

| Format | Magic Bytes (hex)       |
| ------ | ----------------------- |
| PNG    | 89 50 4E 47 0D 0A 1A 0A |
| JPEG   | FF D8 FF                |
| BMP    | 42 4D                   |

**Rationale**: File extensions can be incorrect; magic bytes are authoritative.

### 5. CLI Interface Design

**Decision**: Use System.CommandLine with root command handling conversion directly.

```
image-to-icon-converter --input <path> [--output <path>] [--sizes <sizes>] [--overwrite]

Options:
  -i, --input    (required) Input image file path
  -o, --output   Output ICO file path (default: input path with .ico extension)
  -s, --sizes    Icon sizes to generate (default: 16,32,48,256)
  -y, --overwrite Overwrite output file without prompting
```

**Rationale**:
- Direct invocation is simpler for single-purpose tool
- Short aliases for common options improve usability
- Sensible defaults reduce required arguments
- Future subcommands (e.g., `info`, `batch`) can be added later if needed

### 6. Error Handling

**Decision**: Return specific exit codes and user-friendly messages.

| Exit Code | Meaning              |
| --------- | -------------------- |
| 0         | Success              |
| 1         | Invalid arguments    |
| 2         | Input file not found |
| 3         | Unsupported format   |
| 4         | Output write failed  |

**Rationale**: Scripts can handle specific error conditions; users get clear feedback.

## Alternatives Considered

### ImageMagick vs ImageSharp
- **ImageMagick**: More formats, but requires native binaries
- **ImageSharp**: Pure .NET, AOT-compatible, sufficient format support
- **Decision**: ImageSharp for AOT compatibility and simpler deployment

### System.CommandLine vs Spectre.Console.Cli
- **System.CommandLine**: Microsoft-supported, stable API
- **Spectre.Console.Cli**: Richer features, active development
- **Decision**: System.CommandLine for stability and Microsoft backing

## Risks / Trade-offs

| Risk                                    | Mitigation                                          |
| --------------------------------------- | --------------------------------------------------- |
| Large images consume significant memory | Set maximum input size validation (e.g., 4096x4096) |
| ICO format complexity                   | Comprehensive test suite with known-good ICO files  |
| AOT compilation issues                  | Test AOT publish early and often during development |

## Migration Plan

Not applicable - greenfield implementation.

## Open Questions

1. Should we support animated GIF to ICO conversion? (Proposed: No for MVP)
2. Should we add verbose/quiet output modes? (Proposed: Defer to post-MVP)
3. Maximum input image dimensions? (Proposed: 4096x4096 for MVP)