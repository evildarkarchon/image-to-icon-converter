## ADDED Requirements

### Requirement: ICO File Structure

The ICO generator SHALL produce valid Windows ICO files conforming to the ICO format specification.

#### Scenario: Valid ICO header
- **WHEN** an ICO file is generated
- **THEN** the file begins with a 6-byte header containing reserved (0), type (1), and image count

#### Scenario: Image directory entries
- **WHEN** an ICO file is generated with multiple sizes
- **THEN** each size has a 16-byte directory entry with width, height, color info, and data offset

### Requirement: Multi-Resolution Icon Generation

The ICO generator SHALL support generating icons with multiple resolutions in a single file.

#### Scenario: Generate all standard sizes
- **WHEN** default sizes are requested
- **THEN** the ICO file contains 16x16, 32x32, 48x48, and 256x256 icon images

#### Scenario: Generate subset of sizes
- **WHEN** sizes 16,32 are requested
- **THEN** the ICO file contains only 16x16 and 32x32 icon images

#### Scenario: Generate single size
- **WHEN** only size 256 is requested
- **THEN** the ICO file contains only the 256x256 icon image

### Requirement: Image Format Selection

The ICO generator SHALL use appropriate image formats based on icon size.

#### Scenario: Large icon uses PNG format
- **WHEN** 256x256 icon is generated
- **THEN** the icon image data is stored as compressed PNG

#### Scenario: Small icons use BMP format
- **WHEN** 16x16, 32x32, or 48x48 icon is generated
- **THEN** the icon image data is stored as uncompressed BMP

### Requirement: High-Quality Image Resizing

The ICO generator SHALL resize images with high quality to maintain visual fidelity.

#### Scenario: Downscale large image
- **WHEN** a 512x512 image is converted to 32x32 icon
- **THEN** Lanczos3 resampling is used for high-quality downscaling

#### Scenario: Preserve aspect ratio
- **WHEN** a non-square image (200x100) is converted
- **THEN** the image is resized to fit within the target size while maintaining aspect ratio
- **AND** transparent padding is applied to make the result square

### Requirement: Color Depth Support

The ICO generator SHALL support 32-bit ARGB color depth with alpha transparency.

#### Scenario: Generate with transparency
- **WHEN** a PNG with alpha channel is converted
- **THEN** transparency is preserved in the generated ICO file

#### Scenario: Opaque image handling
- **WHEN** a JPEG (no alpha) is converted
- **THEN** the ICO file contains fully opaque icons (alpha = 255)

### Requirement: BMP Data Format for Small Icons

The ICO generator SHALL produce correct BMP data for icons smaller than 256x256.

#### Scenario: BMP header format
- **WHEN** a small icon (16, 32, or 48) is generated
- **THEN** the image uses BITMAPINFOHEADER format with height doubled (for XOR and AND masks)

#### Scenario: Pixel data ordering
- **WHEN** BMP icon data is written
- **THEN** pixel rows are stored bottom-to-top as per BMP specification

### Requirement: PNG Data Format for Large Icons

The ICO generator SHALL produce correct PNG data for 256x256 icons.

#### Scenario: PNG compression
- **WHEN** a 256x256 icon is generated
- **THEN** the image data is stored as a valid PNG stream with compression

#### Scenario: Size indicator
- **WHEN** a 256x256 icon directory entry is written
- **THEN** the width and height fields contain 0 (indicating 256)