# Implementation Tasks

## 1. Project Structure Setup

- [x] 1.1 Create directory structure (Commands/, Services/, Models/, Utils/)
- [x] 1.2 Remove placeholder UnitTest1.cs from test project
- [x] 1.3 Update Program.cs with CLI scaffolding (custom argument parsing)

## 2. Core Models

- [x] 2.1 Create IconSize enum (Size16, Size32, Size48, Size256)
- [x] 2.2 Create ConvertOptions model (input path, output path, sizes, overwrite flag)
- [x] 2.3 Create ConversionResult model (success/failure, output path, error messages)

## 3. Image Loading Service

- [x] 3.1 Create IImageLoader interface
- [x] 3.2 Implement ImageLoader service using ImageSharp
- [x] 3.3 Add format detection via magic bytes (PNG, JPEG, BMP)
- [x] 3.4 Add validation for maximum image dimensions (4096x4096)
- [x] 3.5 Write unit tests for ImageLoader

## 4. ICO Generation Service

- [x] 4.1 Create IIcoGenerator interface
- [x] 4.2 Implement IcoGenerator service
- [x] 4.3 Implement ICO header writing (6 bytes)
- [x] 4.4 Implement image directory entry writing (16 bytes per entry)
- [x] 4.5 Implement image resizing with ImageSharp (Lanczos3)
- [x] 4.6 Implement PNG embedding for 256x256 size
- [x] 4.7 Implement BMP embedding for smaller sizes (16, 32, 48)
- [x] 4.8 Write unit tests for IcoGenerator

## 5. CLI Root Command

- [x] 5.1 Configure root command with required --input option
- [x] 5.2 Add optional --output option (defaults to input path with .ico extension)
- [x] 5.3 Add optional --sizes option (defaults to all standard sizes)
- [x] 5.4 Add optional --overwrite/-y flag for non-interactive operation
- [x] 5.5 Implement error handling with user-friendly messages
- [x] 5.6 Write integration tests for CLI

## 6. Integration and Testing

- [x] 6.1 Create end-to-end tests using test images from TestData/
- [x] 6.2 Test conversion from PNG to ICO
- [x] 6.3 Test conversion from JPEG to ICO
- [x] 6.4 Test conversion from BMP to ICO
- [x] 6.5 Verify generated ICO files are valid

## 7. Documentation

- [x] 7.1 Update README.md with usage examples
- [x] 7.2 Add inline XML documentation to public APIs