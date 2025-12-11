using ImageToIconConverter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageToIconConverter.Services;

/// <summary>
/// Interface for loading and validating images.
/// </summary>
public interface IImageLoader
{
    /// <summary>
    /// Loads an image from the specified file path.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <returns>The loaded image with RGBA32 pixel format.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="NotSupportedException">Thrown when the image format is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown when image dimensions exceed maximum allowed.</exception>
    Image<Rgba32> LoadImage(string filePath);

    /// <summary>
    /// Validates that the file exists and has a supported format.
    /// </summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <returns>The detected image format if valid.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="NotSupportedException">Thrown when the image format is not supported.</exception>
    ImageFormat ValidateFile(string filePath);

    /// <summary>
    /// Gets the maximum allowed image dimension.
    /// </summary>
    int MaxImageDimension { get; }
}