namespace ImageToIconConverter.Models;

/// <summary>
/// Supported image formats for input files.
/// </summary>
public enum ImageFormat
{
    /// <summary>Unknown or unsupported format.</summary>
    Unknown,

    /// <summary>PNG (Portable Network Graphics) format.</summary>
    Png,

    /// <summary>JPEG (Joint Photographic Experts Group) format.</summary>
    Jpeg,

    /// <summary>BMP (Bitmap) format.</summary>
    Bmp
}

/// <summary>
/// Utility class for detecting image formats via magic bytes.
/// </summary>
public static class ImageFormatDetector
{
    // PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    // JPEG magic bytes: FF D8 FF
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];

    // BMP magic bytes: 42 4D
    private static readonly byte[] BmpMagic = [0x42, 0x4D];

    /// <summary>
    /// Detects the image format from file path by reading magic bytes.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <returns>The detected image format, or Unknown if not recognized.</returns>
    public static ImageFormat DetectFormat(string filePath)
    {
        Span<byte> buffer = stackalloc byte[8];

        using var stream = File.OpenRead(filePath);
        var bytesRead = stream.Read(buffer);

        if (bytesRead < 2)
        {
            return ImageFormat.Unknown;
        }

        return DetectFormat(buffer[..bytesRead]);
    }

    /// <summary>
    /// Detects the image format from magic bytes.
    /// </summary>
    /// <param name="data">The first few bytes of the file.</param>
    /// <returns>The detected image format, or Unknown if not recognized.</returns>
    public static ImageFormat DetectFormat(ReadOnlySpan<byte> data)
    {
        // Check PNG: 89 50 4E 47 0D 0A 1A 0A
        if (data.Length >= 8 &&
            data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
            data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            return ImageFormat.Png;
        }

        // Check JPEG: FF D8 FF (followed by various APP markers like E0, E1, E2, etc.)
        if (data.Length >= 3 &&
            data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            return ImageFormat.Jpeg;
        }

        // Check BMP: 42 4D
        if (data.Length >= 2 &&
            data[0] == 0x42 && data[1] == 0x4D)
        {
            return ImageFormat.Bmp;
        }

        return ImageFormat.Unknown;
    }

    /// <summary>
    /// Gets the supported file extensions for display purposes.
    /// </summary>
    public static readonly string[] SupportedExtensions = [".png", ".jpg", ".jpeg", ".bmp"];
}