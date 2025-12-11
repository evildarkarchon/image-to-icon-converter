using ImageToIconConverter.Models;

namespace ImageToIconConverter.Tests;

public class ImageFormatDetectorTests
{
    [Fact]
    public void DetectFormat_PngMagicBytes_ReturnsPng()
    {
        // PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
        byte[] pngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

        var format = ImageFormatDetector.DetectFormat(pngBytes);

        Assert.Equal(ImageFormat.Png, format);
    }

    [Fact]
    public void DetectFormat_JpegMagicBytes_ReturnsJpeg()
    {
        // JPEG magic bytes: FF D8 FF
        byte[] jpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

        var format = ImageFormatDetector.DetectFormat(jpegBytes);

        Assert.Equal(ImageFormat.Jpeg, format);
    }

    [Fact]
    public void DetectFormat_BmpMagicBytes_ReturnsBmp()
    {
        // BMP magic bytes: 42 4D
        byte[] bmpBytes = [0x42, 0x4D, 0x00, 0x00, 0x00, 0x00];

        var format = ImageFormatDetector.DetectFormat(bmpBytes);

        Assert.Equal(ImageFormat.Bmp, format);
    }

    [Fact]
    public void DetectFormat_InvalidMagicBytes_ReturnsUnknown()
    {
        byte[] unknownBytes = [0x00, 0x00, 0x00, 0x00];

        var format = ImageFormatDetector.DetectFormat(unknownBytes);

        Assert.Equal(ImageFormat.Unknown, format);
    }

    [Fact]
    public void DetectFormat_EmptyData_ReturnsUnknown()
    {
        byte[] emptyBytes = [];

        var format = ImageFormatDetector.DetectFormat(emptyBytes);

        Assert.Equal(ImageFormat.Unknown, format);
    }

    [Fact]
    public void DetectFormat_TooShortData_ReturnsUnknown()
    {
        byte[] shortBytes = [0x89];

        var format = ImageFormatDetector.DetectFormat(shortBytes);

        Assert.Equal(ImageFormat.Unknown, format);
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".bmp")]
    public void SupportedExtensions_ContainsCommonExtensions(string extension)
    {
        Assert.Contains(extension, ImageFormatDetector.SupportedExtensions);
    }
}