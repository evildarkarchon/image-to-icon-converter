using ImageToIconConverter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageToIconConverter.Services;

/// <summary>
/// Interface for generating ICO files from images.
/// </summary>
public interface IIcoGenerator
{
    /// <summary>
    /// Generates an ICO file from the source image.
    /// </summary>
    /// <param name="sourceImage">The source image to convert.</param>
    /// <param name="sizes">The icon sizes to generate.</param>
    /// <param name="outputPath">The path to write the ICO file.</param>
    void GenerateIco(Image<Rgba32> sourceImage, IconSize[] sizes, string outputPath);

    /// <summary>
    /// Generates ICO file data from the source image.
    /// </summary>
    /// <param name="sourceImage">The source image to convert.</param>
    /// <param name="sizes">The icon sizes to generate.</param>
    /// <returns>The ICO file data as a byte array.</returns>
    byte[] GenerateIcoData(Image<Rgba32> sourceImage, IconSize[] sizes);
}