namespace ImageToIconConverter.Models;

/// <summary>
/// Result of an image to ICO conversion operation.
/// </summary>
public sealed class ConversionResult
{
    /// <summary>
    /// Gets whether the conversion was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the path to the output ICO file (if successful).
    /// </summary>
    public string? OutputPath { get; init; }

    /// <summary>
    /// Gets the error message (if failed).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the exit code for CLI usage.
    /// </summary>
    public ExitCode ExitCode { get; init; }

    /// <summary>
    /// Creates a successful conversion result.
    /// </summary>
    /// <param name="outputPath">The path to the created ICO file.</param>
    public static ConversionResult Succeeded(string outputPath) => new()
    {
        Success = true,
        OutputPath = outputPath,
        ExitCode = ExitCode.Success
    };

    /// <summary>
    /// Creates a failed conversion result.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exitCode">The exit code for CLI usage.</param>
    public static ConversionResult Failed(string errorMessage, ExitCode exitCode) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        ExitCode = exitCode
    };
}

/// <summary>
/// Exit codes for CLI usage indicating the result of operations.
/// </summary>
public enum ExitCode
{
    /// <summary>Conversion completed successfully.</summary>
    Success = 0,

    /// <summary>Invalid command-line arguments.</summary>
    InvalidArguments = 1,

    /// <summary>Input file was not found.</summary>
    InputFileNotFound = 2,

    /// <summary>Input file format is not supported.</summary>
    UnsupportedFormat = 3,

    /// <summary>Failed to write output file.</summary>
    OutputWriteFailed = 4
}