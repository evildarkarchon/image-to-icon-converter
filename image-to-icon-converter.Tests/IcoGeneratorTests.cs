using ImageToIconConverter.Models;
using ImageToIconConverter.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageToIconConverter.Tests;

public class IcoGeneratorTests
{
    private readonly IcoGenerator _icoGenerator = new();

    [Fact]
    public void GenerateIcoData_SingleSize_CreatesValidIcoHeader()
    {
        using var image = new Image<Rgba32>(64, 64, Color.Red);
        var sizes = new[] { IconSize.Size32 };

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // ICO header: Reserved (2) + Type (2) + Count (2) = 6 bytes
        Assert.True(icoData.Length >= 6);

        // Reserved should be 0
        Assert.Equal(0, BitConverter.ToUInt16(icoData, 0));

        // Type should be 1 (ICO)
        Assert.Equal(1, BitConverter.ToUInt16(icoData, 2));

        // Count should be 1
        Assert.Equal(1, BitConverter.ToUInt16(icoData, 4));
    }

    [Fact]
    public void GenerateIcoData_MultipleSizes_CreatesCorrectImageCount()
    {
        using var image = new Image<Rgba32>(256, 256, Color.Blue);
        var sizes = new[] { IconSize.Size16, IconSize.Size32, IconSize.Size48, IconSize.Size256 };

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // Count should be 4
        Assert.Equal(4, BitConverter.ToUInt16(icoData, 4));
    }

    [Fact]
    public void GenerateIcoData_Size256_UsesZeroInDirectoryEntry()
    {
        using var image = new Image<Rgba32>(256, 256, Color.Green);
        var sizes = new[] { IconSize.Size256 };

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // Directory entry starts at offset 6
        // Width (byte 0) and Height (byte 1) should be 0 for 256x256
        Assert.Equal(0, icoData[6]); // Width
        Assert.Equal(0, icoData[7]); // Height
    }

    [Fact]
    public void GenerateIcoData_SmallSize_UsesActualSizeInDirectoryEntry()
    {
        using var image = new Image<Rgba32>(64, 64, Color.Yellow);
        var sizes = new[] { IconSize.Size32 };

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // Directory entry starts at offset 6
        Assert.Equal(32, icoData[6]); // Width
        Assert.Equal(32, icoData[7]); // Height
    }

    [Fact]
    public void GenerateIcoData_AllDefaultSizes_CreatesValidIco()
    {
        using var image = new Image<Rgba32>(512, 512, Color.Purple);
        var sizes = IconSizeExtensions.DefaultSizes;

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        Assert.True(icoData.Length > 0);

        // Verify header
        Assert.Equal(0, BitConverter.ToUInt16(icoData, 0)); // Reserved
        Assert.Equal(1, BitConverter.ToUInt16(icoData, 2)); // Type
        Assert.Equal(4, BitConverter.ToUInt16(icoData, 4)); // Count

        // Total size should be reasonable (header + 4 directory entries + image data)
        // Minimum: 6 (header) + 4*16 (directory) = 70 bytes
        Assert.True(icoData.Length >= 70);
    }

    [Fact]
    public void GenerateIcoData_EmptySizes_ThrowsArgumentException()
    {
        using var image = new Image<Rgba32>(64, 64, Color.White);

        Assert.Throws<ArgumentException>(() =>
            _icoGenerator.GenerateIcoData(image, []));
    }

    [Fact]
    public void GenerateIcoData_PreservesTransparency()
    {
        // Create an image with transparency
        using var image = new Image<Rgba32>(32, 32);
        for (var y = 0; y < 32; y++)
        {
            for (var x = 0; x < 32; x++)
            {
                // Semi-transparent red
                image[x, y] = new Rgba32(255, 0, 0, 128);
            }
        }

        var sizes = new[] { IconSize.Size16 };
        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // Verify 32-bit color depth in directory entry
        // Bits per pixel is at offset 6 (header) + 6 (width, height, color count, reserved, planes) = 12
        var bitsPerPixel = BitConverter.ToUInt16(icoData, 12);
        Assert.Equal(32, bitsPerPixel);
    }

    [Fact]
    public void GenerateIco_WritesFileToPath()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.ico");

        try
        {
            using var image = new Image<Rgba32>(64, 64, Color.Cyan);
            var sizes = new[] { IconSize.Size32 };

            _icoGenerator.GenerateIco(image, sizes, tempPath);

            Assert.True(File.Exists(tempPath));

            var fileData = File.ReadAllBytes(tempPath);
            Assert.True(fileData.Length > 0);

            // Verify it's a valid ICO file
            Assert.Equal(0, BitConverter.ToUInt16(fileData, 0)); // Reserved
            Assert.Equal(1, BitConverter.ToUInt16(fileData, 2)); // Type
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void GenerateIco_CreatesDirectoryIfNotExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"test_dir_{Guid.NewGuid()}");
        var tempPath = Path.Combine(tempDir, "test.ico");

        try
        {
            using var image = new Image<Rgba32>(32, 32, Color.Magenta);
            var sizes = new[] { IconSize.Size16 };

            _icoGenerator.GenerateIco(image, sizes, tempPath);

            Assert.True(Directory.Exists(tempDir));
            Assert.True(File.Exists(tempPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void GenerateIcoData_NonSquareImage_PadsCorrectly()
    {
        // Create a rectangular image
        using var image = new Image<Rgba32>(200, 100, Color.Orange);
        var sizes = new[] { IconSize.Size32 };

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // Should create valid ICO data
        Assert.True(icoData.Length > 0);

        // Verify it's 32x32 in directory
        Assert.Equal(32, icoData[6]); // Width
        Assert.Equal(32, icoData[7]); // Height
    }

    [Fact]
    public void GenerateIcoData_Size256_UsesPngFormat()
    {
        using var image = new Image<Rgba32>(512, 512, Color.Brown);
        var sizes = new[] { IconSize.Size256 };

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // Get the image data offset from directory entry (offset 18-21, uint32)
        var imageDataOffset = BitConverter.ToUInt32(icoData, 18);

        // PNG signature starts with: 89 50 4E 47
        Assert.Equal(0x89, icoData[imageDataOffset]);
        Assert.Equal(0x50, icoData[imageDataOffset + 1]);
        Assert.Equal(0x4E, icoData[imageDataOffset + 2]);
        Assert.Equal(0x47, icoData[imageDataOffset + 3]);
    }

    [Fact]
    public void GenerateIcoData_SmallSizes_UseBmpFormat()
    {
        using var image = new Image<Rgba32>(64, 64, Color.Silver);
        var sizes = new[] { IconSize.Size32 };

        var icoData = _icoGenerator.GenerateIcoData(image, sizes);

        // Get the image data offset from directory entry
        var imageDataOffset = BitConverter.ToUInt32(icoData, 18);

        // BMP data (DIB) starts with BITMAPINFOHEADER, which starts with size = 40
        var headerSize = BitConverter.ToUInt32(icoData, (int)imageDataOffset);
        Assert.Equal(40u, headerSize);
    }
}