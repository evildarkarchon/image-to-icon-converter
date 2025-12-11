## ADDED Requirements

### Requirement: Image to ICO Conversion

The CLI SHALL convert an input image to ICO format when invoked with required options.

#### Scenario: Convert with required input option
- **WHEN** user runs `image-to-icon-converter --input image.png`
- **THEN** an ICO file is created at `image.ico` in the same directory

#### Scenario: Convert with custom output path
- **WHEN** user runs `image-to-icon-converter --input image.png --output custom.ico`
- **THEN** an ICO file is created at `custom.ico`

#### Scenario: Convert with specific sizes
- **WHEN** user runs `image-to-icon-converter --input image.png --sizes 16,32`
- **THEN** an ICO file is created containing only 16x16 and 32x32 icon images

### Requirement: Input Validation

The CLI SHALL validate that the input file exists and is a supported format before conversion.

#### Scenario: Input file not found
- **WHEN** user runs `image-to-icon-converter --input nonexistent.png`
- **THEN** exit code 2 is returned
- **AND** error message indicates the file was not found

#### Scenario: Unsupported format
- **WHEN** user runs `image-to-icon-converter --input document.pdf`
- **THEN** exit code 3 is returned
- **AND** error message indicates the format is not supported

### Requirement: Output File Handling

The CLI SHALL handle output file conflicts safely.

#### Scenario: Output file exists without overwrite flag
- **WHEN** user runs `image-to-icon-converter --input image.png` and `image.ico` already exists
- **THEN** exit code 4 is returned
- **AND** error message prompts user to use --overwrite flag

#### Scenario: Output file exists with overwrite flag
- **WHEN** user runs `image-to-icon-converter --input image.png --overwrite` and `image.ico` already exists
- **THEN** the existing file is overwritten
- **AND** exit code 0 is returned

### Requirement: Help and Usage Information

The CLI SHALL provide built-in help documentation.

#### Scenario: Display help
- **WHEN** user runs `image-to-icon-converter --help`
- **THEN** usage information is displayed including all options and their descriptions

#### Scenario: Display version
- **WHEN** user runs `image-to-icon-converter --version`
- **THEN** the application version is displayed

### Requirement: Exit Codes

The CLI SHALL return meaningful exit codes for scripting support.

#### Scenario: Successful conversion
- **WHEN** conversion completes successfully
- **THEN** exit code 0 is returned

#### Scenario: Invalid arguments
- **WHEN** required arguments are missing
- **THEN** exit code 1 is returned
- **AND** usage information is displayed