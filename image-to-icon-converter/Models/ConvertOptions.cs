namespace ImageToIconConverter.Models;

/// <summary>
/// Options for image to ICO conversion.
/// </summary>
public sealed class ConvertOptions
{
    /// <summary>
    /// Gets or sets the input image file path.
    /// </summary>
    public required string InputPath { get; init; }

    /// <summary>
    /// Gets or sets the output ICO file path.
    /// If null, defaults to the input path with .ico extension.
    /// </summary>
    public string? OutputPath { get; init; }

    /// <summary>
    /// Gets or sets the icon sizes to generate.
    /// If null or empty, defaults to all standard sizes (16, 32, 48, 256).
    /// </summary>
    public IconSize[]? Sizes { get; init; }

    /// <summary>
    /// Gets or sets whether to overwrite existing output file without prompting.
    /// </summary>
    public bool Overwrite { get; init; }

    /// <summary>
    /// Gets the effective output path, computing the default if not specified.
    /// </summary>
    public string GetEffectiveOutputPath()
    {
        if (!string.IsNullOrEmpty(OutputPath))
        {
            return OutputPath;
        }

        var directory = Path.GetDirectoryName(InputPath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(InputPath);
        return Path.Combine(directory, fileNameWithoutExtension + ".ico");
    }

    /// <summary>
    /// Gets the effective icon sizes, using defaults if not specified.
    /// </summary>
    public IconSize[] GetEffectiveSizes()
    {
        return Sizes is { Length: > 0 } ? Sizes : IconSizeExtensions.DefaultSizes;
    }
}