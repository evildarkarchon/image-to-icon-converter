using ImageToIconConverter.Models;
using ImageToIconConverter.Services;

namespace ImageToIconConverter.Tests;

public class ImageLoaderTests
{
    private readonly ImageLoader _imageLoader = new();
    private static readonly string TestDataPath = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "TestData");

    [Fact]
    public void ValidateFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(TestDataPath, "nonexistent.png");

        Assert.Throws<FileNotFoundException>(() => _imageLoader.ValidateFile(nonExistentPath));
    }

    [Fact]
    public void ValidateFile_EmptyPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _imageLoader.ValidateFile(""));
        Assert.Throws<ArgumentException>(() => _imageLoader.ValidateFile("   "));
    }

    [Fact]
    public void ValidateFile_ValidPngFile_ReturnsPngFormat()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null)
        {
            return; // Skip if no test files available
        }

        var format = _imageLoader.ValidateFile(pngFile);

        Assert.Equal(ImageFormat.Png, format);
    }

    [Fact]
    public void ValidateFile_ValidJpegFile_ReturnsJpegFormat()
    {
        var jpgFile = GetFirstTestFile("jpg");
        if (jpgFile is null)
        {
            return; // Skip if no test files available
        }

        var format = _imageLoader.ValidateFile(jpgFile);

        Assert.Equal(ImageFormat.Jpeg, format);
    }

    [Fact]
    public void LoadImage_ValidPngFile_ReturnsImage()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null)
        {
            return; // Skip if no test files available
        }

        using var image = _imageLoader.LoadImage(pngFile);

        Assert.NotNull(image);
        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
    }

    [Fact]
    public void LoadImage_ValidJpegFile_ReturnsImage()
    {
        var jpgFile = GetFirstTestFile("jpg");
        if (jpgFile is null)
        {
            return; // Skip if no test files available
        }

        using var image = _imageLoader.LoadImage(jpgFile);

        Assert.NotNull(image);
        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
    }

    [Fact]
    public void LoadImage_NonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(TestDataPath, "nonexistent.png");

        Assert.Throws<FileNotFoundException>(() => _imageLoader.LoadImage(nonExistentPath));
    }

    [Fact]
    public void MaxImageDimension_DefaultValue_Is4096()
    {
        Assert.Equal(ImageLoader.DefaultMaxImageDimension, _imageLoader.MaxImageDimension);
        Assert.Equal(4096, _imageLoader.MaxImageDimension);
    }

    [Fact]
    public void Constructor_CustomMaxDimension_SetsMaxImageDimension()
    {
        var customLoader = new ImageLoader(1024);

        Assert.Equal(1024, customLoader.MaxImageDimension);
    }

    /// <summary>
    /// Gets the first valid test file of the specified type from TestData directory.
    /// Actually tries to load the image to ensure it's valid (not just magic bytes).
    /// </summary>
    private static string? GetFirstTestFile(string subfolder)
    {
        var dir = Path.Combine(TestDataPath, subfolder);
        if (!Directory.Exists(dir))
        {
            return null;
        }

        // Find a file that can actually be loaded by ImageSharp
        foreach (var file in Directory.GetFiles(dir))
        {
            var format = ImageToIconConverter.Models.ImageFormatDetector.DetectFormat(file);
            if (format == ImageToIconConverter.Models.ImageFormat.Unknown)
            {
                continue;
            }

            // Try to actually load the image
            try
            {
                using var image = SixLabors.ImageSharp.Image.Load(file);
                return file; // This file is valid
            }
            catch
            {
                // This file is corrupted, try next
            }
        }

        return null;
    }
}