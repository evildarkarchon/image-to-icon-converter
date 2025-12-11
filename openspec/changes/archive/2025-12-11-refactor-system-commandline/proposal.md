# Change: Refactor CLI to use System.CommandLine

## Why

The current CLI implementation uses a homebrew argument parsing solution with manual switch-case handling spanning ~170 lines of code. The project already lists `System.CommandLine 2.0.1` as a dependency in `project.md`, but it's not actually being used. Switching to System.CommandLine provides:

1. **Built-in help generation** - Automatic, consistent help output without manual maintenance
2. **Tab completion support** - Shell completion out-of-the-box
3. **Validation & binding** - Type-safe option parsing with automatic validation
4. **Reduced maintenance burden** - Less custom code to maintain
5. **Industry standard** - Well-documented, tested library from Microsoft

## What Changes

- **Program.cs**: Replace manual argument parsing with System.CommandLine `RootCommand` and `Option<T>` definitions
- **Remove**: `ShowHelp()`, `ParseArguments()`, `HasFlag()`, `ParseSizes()` methods (~120 lines)
- **Add**: Command setup with options, handlers, and built-in help/version support
- **Preserve**: Existing exit codes, conversion logic, and service interfaces

## Impact

- **Affected specs**: `cli` (requirements remain unchanged, implementation improves)
- **Affected code**: 
  - `image-to-icon-converter/Program.cs` - Major refactor
  - `image-to-icon-converter.Tests/ProgramTests.cs` - Minor updates for new invocation pattern
  - `image-to-icon-converter/image-to-icon-converter.csproj` - Add System.CommandLine package reference
- **Breaking changes**: None - All CLI options and exit codes remain identical
- **Risk**: Low - Well-tested library replacement, existing tests validate behavior