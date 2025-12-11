using ImageToIconConverter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageToIconConverter.Services;

/// <summary>
/// Implementation of <see cref="IImageLoader"/> using ImageSharp.
/// </summary>
public sealed class ImageLoader : IImageLoader
{
    /// <summary>
    /// Maximum allowed image dimension (width or height) in pixels.
    /// </summary>
    public const int DefaultMaxImageDimension = 4096;

    /// <inheritdoc/>
    public int MaxImageDimension { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageLoader"/> class.
    /// </summary>
    /// <param name="maxImageDimension">Maximum allowed image dimension. Defaults to 4096.</param>
    public ImageLoader(int maxImageDimension = DefaultMaxImageDimension)
    {
        MaxImageDimension = maxImageDimension;
    }

    /// <inheritdoc/>
    public Image<Rgba32> LoadImage(string filePath)
    {
        // Validate the file first
        ValidateFile(filePath);

        // Load the image using ImageSharp
        var image = Image.Load<Rgba32>(filePath);

        // Validate dimensions
        if (image.Width > MaxImageDimension || image.Height > MaxImageDimension)
        {
            image.Dispose();
            throw new InvalidOperationException(
                $"Image dimensions ({image.Width}x{image.Height}) exceed maximum allowed ({MaxImageDimension}x{MaxImageDimension}).");
        }

        return image;
    }

    /// <inheritdoc/>
    public ImageFormat ValidateFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Input file not found: {filePath}", filePath);
        }

        var format = ImageFormatDetector.DetectFormat(filePath);

        if (format == ImageFormat.Unknown)
        {
            throw new NotSupportedException(
                $"Unsupported image format. Supported formats: {string.Join(", ", ImageFormatDetector.SupportedExtensions)}");
        }

        return format;
    }
}