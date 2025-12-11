using ImageToIconConverter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageToIconConverter.Services;

/// <summary>
/// Implementation of <see cref="IIcoGenerator"/> for generating ICO files.
/// </summary>
public sealed class IcoGenerator : IIcoGenerator
{
    // ICO file constants
    private const ushort IcoReserved = 0;
    private const ushort IcoType = 1; // 1 = ICO, 2 = CUR
    private const int IcoHeaderSize = 6;
    private const int IcoDirectoryEntrySize = 16;

    // BMP constants
    private const int BitmapInfoHeaderSize = 40;

    /// <inheritdoc/>
    public void GenerateIco(Image<Rgba32> sourceImage, IconSize[] sizes, string outputPath)
    {
        var icoData = GenerateIcoData(sourceImage, sizes);

        // Ensure output directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(outputPath, icoData);
    }

    /// <inheritdoc/>
    public byte[] GenerateIcoData(Image<Rgba32> sourceImage, IconSize[] sizes)
    {
        if (sizes.Length == 0)
        {
            throw new ArgumentException("At least one icon size must be specified.", nameof(sizes));
        }

        // Sort sizes for consistent output
        var sortedSizes = sizes.OrderBy(s => (int)s).ToArray();

        // Generate resized images and their data
        var imageDataList = new List<(IconSize Size, byte[] Data)>();

        foreach (var size in sortedSizes)
        {
            using var resizedImage = ResizeImage(sourceImage, (int)size);
            var imageData = size.ShouldUsePng()
                ? GeneratePngData(resizedImage)
                : GenerateBmpData(resizedImage);
            imageDataList.Add((size, imageData));
        }

        // Calculate file structure
        var imageCount = imageDataList.Count;
        var directorySize = imageCount * IcoDirectoryEntrySize;
        var dataOffset = IcoHeaderSize + directorySize;

        // Calculate total size
        var totalSize = dataOffset + imageDataList.Sum(x => x.Data.Length);

        // Create output buffer
        var buffer = new byte[totalSize];
        using var stream = new MemoryStream(buffer);
        using var writer = new BinaryWriter(stream);

        // Write ICO header
        writer.Write(IcoReserved);        // Reserved
        writer.Write(IcoType);            // Type (1 = ICO)
        writer.Write((ushort)imageCount); // Number of images

        // Write directory entries
        var currentOffset = dataOffset;
        foreach (var (size, data) in imageDataList)
        {
            var sizeValue = (int)size;

            // Width (0 = 256)
            writer.Write((byte)(sizeValue == 256 ? 0 : sizeValue));
            // Height (0 = 256)
            writer.Write((byte)(sizeValue == 256 ? 0 : sizeValue));
            // Color count (0 = 256+ colors)
            writer.Write((byte)0);
            // Reserved
            writer.Write((byte)0);
            // Color planes
            writer.Write((ushort)1);
            // Bits per pixel
            writer.Write((ushort)32);
            // Image data size
            writer.Write((uint)data.Length);
            // Image data offset
            writer.Write((uint)currentOffset);

            currentOffset += data.Length;
        }

        // Write image data
        foreach (var (_, data) in imageDataList)
        {
            writer.Write(data);
        }

        return buffer;
    }

    /// <summary>
    /// Resizes the source image to the target size using high-quality resampling.
    /// </summary>
    private static Image<Rgba32> ResizeImage(Image<Rgba32> source, int targetSize)
    {
        // Clone the image to avoid modifying the original
        var resized = source.Clone();

        resized.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(targetSize, targetSize),
            Sampler = KnownResamplers.Lanczos3,
            Mode = ResizeMode.Max,
            PadColor = Color.Transparent
        }));

        // If the result is not exactly square (due to aspect ratio preservation),
        // we need to center it on a square canvas
        if (resized.Width != targetSize || resized.Height != targetSize)
        {
            var canvas = new Image<Rgba32>(targetSize, targetSize, Color.Transparent);
            var x = (targetSize - resized.Width) / 2;
            var y = (targetSize - resized.Height) / 2;

            canvas.Mutate(ctx => ctx.DrawImage(resized, new Point(x, y), 1f));
            resized.Dispose();
            return canvas;
        }

        return resized;
    }

    /// <summary>
    /// Generates PNG data for the image (used for 256x256 icons).
    /// </summary>
    private static byte[] GeneratePngData(Image<Rgba32> image)
    {
        using var stream = new MemoryStream();
        image.Save(stream, new PngEncoder
        {
            ColorType = PngColorType.RgbWithAlpha,
            CompressionLevel = PngCompressionLevel.BestCompression
        });
        return stream.ToArray();
    }

    /// <summary>
    /// Generates BMP data for the image (used for icons smaller than 256x256).
    /// ICO format uses DIB (Device Independent Bitmap) without the BITMAPFILEHEADER.
    /// </summary>
    private static byte[] GenerateBmpData(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;

        // Calculate row stride (each row must be 4-byte aligned)
        var rowStride = width * 4; // 4 bytes per pixel (BGRA)
        if (rowStride % 4 != 0)
        {
            rowStride += 4 - (rowStride % 4);
        }

        // Calculate AND mask size (1 bit per pixel, rows 4-byte aligned)
        var andMaskRowStride = (width + 31) / 32 * 4;
        var andMaskSize = andMaskRowStride * height;

        // Total size: BITMAPINFOHEADER + XOR mask (pixel data) + AND mask
        var pixelDataSize = rowStride * height;
        var totalSize = BitmapInfoHeaderSize + pixelDataSize + andMaskSize;

        var buffer = new byte[totalSize];
        using var stream = new MemoryStream(buffer);
        using var writer = new BinaryWriter(stream);

        // Write BITMAPINFOHEADER
        writer.Write(BitmapInfoHeaderSize);   // biSize
        writer.Write(width);                   // biWidth
        writer.Write(height * 2);              // biHeight (doubled for XOR+AND masks)
        writer.Write((ushort)1);               // biPlanes
        writer.Write((ushort)32);              // biBitCount
        writer.Write(0);                       // biCompression (BI_RGB)
        writer.Write(pixelDataSize + andMaskSize); // biSizeImage
        writer.Write(0);                       // biXPelsPerMeter
        writer.Write(0);                       // biYPelsPerMeter
        writer.Write(0);                       // biClrUsed
        writer.Write(0);                       // biClrImportant

        // Write pixel data (XOR mask) - bottom to top, BGRA format
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = image[x, y];
                writer.Write(pixel.B);
                writer.Write(pixel.G);
                writer.Write(pixel.R);
                writer.Write(pixel.A);
            }

            // Write padding if needed
            var padding = rowStride - (width * 4);
            for (var p = 0; p < padding; p++)
            {
                writer.Write((byte)0);
            }
        }

        // Write AND mask (all zeros for fully opaque with alpha channel)
        // The AND mask is used for transparency on older systems
        // With 32-bit color, the alpha channel handles transparency
        for (var i = 0; i < andMaskSize; i++)
        {
            writer.Write((byte)0);
        }

        return buffer;
    }
}