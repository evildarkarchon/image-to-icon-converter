using ImageToIconConverter;
using ImageToIconConverter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageToIconConverter.Tests;

public class ProgramTests
{
    private readonly string _testDataPath = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "TestData");

    [Fact]
    public void Run_WithHelp_ReturnsSuccess()
    {
        var exitCode = Program.Run(["--help"]);

        Assert.Equal((int)ExitCode.Success, exitCode);
    }

    [Fact]
    public void Run_WithShortHelp_ReturnsSuccess()
    {
        var exitCode = Program.Run(["-h"]);

        Assert.Equal((int)ExitCode.Success, exitCode);
    }

    [Fact]
    public void Run_WithVersion_ReturnsSuccess()
    {
        var exitCode = Program.Run(["--version"]);

        Assert.Equal((int)ExitCode.Success, exitCode);
    }

    [Fact]
    public void Run_NoArguments_ReturnsInvalidArguments()
    {
        var exitCode = Program.Run([]);

        Assert.Equal((int)ExitCode.InvalidArguments, exitCode);
    }

    [Fact]
    public void Run_MissingInput_ReturnsInvalidArguments()
    {
        var exitCode = Program.Run(["--output", "test.ico"]);

        Assert.Equal((int)ExitCode.InvalidArguments, exitCode);
    }

    [Fact]
    public void Run_InputWithoutValue_ReturnsInvalidArguments()
    {
        var exitCode = Program.Run(["--input"]);

        Assert.Equal((int)ExitCode.InvalidArguments, exitCode);
    }

    [Fact]
    public void Run_InvalidSizes_ReturnsInvalidArguments()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        var exitCode = Program.Run(["--input", pngFile, "--sizes", "invalid"]);

        Assert.Equal((int)ExitCode.InvalidArguments, exitCode);
    }

    [Fact]
    public void Run_InvalidSizeValue_ReturnsInvalidArguments()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        // 20 is not a valid icon size
        var exitCode = Program.Run(["--input", pngFile, "--sizes", "20"]);

        Assert.Equal((int)ExitCode.InvalidArguments, exitCode);
    }

    [Fact]
    public void Run_UnknownOption_ReturnsInvalidArguments()
    {
        var exitCode = Program.Run(["--unknown"]);

        Assert.Equal((int)ExitCode.InvalidArguments, exitCode);
    }

    [Fact]
    public void Run_NonExistentInput_ReturnsInputFileNotFound()
    {
        var exitCode = Program.Run(["--input", "nonexistent.png"]);

        Assert.Equal((int)ExitCode.InputFileNotFound, exitCode);
    }

    [Fact]
    public void Run_ValidPngInput_CreatesIcoFile()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        var tempOutput = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.ico");

        try
        {
            var exitCode = Program.Run(["-i", pngFile, "-o", tempOutput]);

            Assert.Equal((int)ExitCode.Success, exitCode);
            Assert.True(File.Exists(tempOutput));
        }
        finally
        {
            if (File.Exists(tempOutput))
                File.Delete(tempOutput);
        }
    }

    [Fact]
    public void Run_ValidJpgInput_CreatesIcoFile()
    {
        var jpgFile = GetFirstTestFile("jpg");
        if (jpgFile is null) return;

        var tempOutput = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.ico");

        try
        {
            var exitCode = Program.Run(["-i", jpgFile, "-o", tempOutput]);

            Assert.Equal((int)ExitCode.Success, exitCode);
            Assert.True(File.Exists(tempOutput));
        }
        finally
        {
            if (File.Exists(tempOutput))
                File.Delete(tempOutput);
        }
    }

    [Fact]
    public void Run_SpecificSizes_CreatesIcoWithRequestedSizes()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        var tempOutput = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.ico");

        try
        {
            var exitCode = Program.Run(["-i", pngFile, "-o", tempOutput, "-s", "16,32"]);

            Assert.Equal((int)ExitCode.Success, exitCode);
            Assert.True(File.Exists(tempOutput));

            // Verify the ICO has 2 images
            var icoData = File.ReadAllBytes(tempOutput);
            var imageCount = BitConverter.ToUInt16(icoData, 4);
            Assert.Equal(2, imageCount);
        }
        finally
        {
            if (File.Exists(tempOutput))
                File.Delete(tempOutput);
        }
    }

    [Fact]
    public void Run_OutputExistsWithoutOverwrite_ReturnsOutputWriteFailed()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        var tempOutput = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.ico");

        try
        {
            // Create the output file first
            File.WriteAllText(tempOutput, "existing file");

            var exitCode = Program.Run(["-i", pngFile, "-o", tempOutput]);

            Assert.Equal((int)ExitCode.OutputWriteFailed, exitCode);
        }
        finally
        {
            if (File.Exists(tempOutput))
                File.Delete(tempOutput);
        }
    }

    [Fact]
    public void Run_OutputExistsWithOverwrite_OverwritesFile()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        var tempOutput = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.ico");

        try
        {
            // Create the output file first
            File.WriteAllText(tempOutput, "existing file");

            var exitCode = Program.Run(["-i", pngFile, "-o", tempOutput, "-y"]);

            Assert.Equal((int)ExitCode.Success, exitCode);

            // Verify it's now a valid ICO
            var icoData = File.ReadAllBytes(tempOutput);
            Assert.Equal(0, BitConverter.ToUInt16(icoData, 0)); // Reserved
            Assert.Equal(1, BitConverter.ToUInt16(icoData, 2)); // Type = ICO
        }
        finally
        {
            if (File.Exists(tempOutput))
                File.Delete(tempOutput);
        }
    }

    [Fact]
    public void Run_DefaultOutput_CreatesIcoNextToInput()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        // Create a temp copy of the input file
        var tempDir = Path.Combine(Path.GetTempPath(), $"test_dir_{Guid.NewGuid()}");
        var tempInput = Path.Combine(tempDir, "testimage.png");
        var expectedOutput = Path.Combine(tempDir, "testimage.ico");

        try
        {
            Directory.CreateDirectory(tempDir);
            File.Copy(pngFile, tempInput);

            var exitCode = Program.Run(["-i", tempInput]);

            Assert.Equal((int)ExitCode.Success, exitCode);
            Assert.True(File.Exists(expectedOutput));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ExecuteConversion_ValidOptions_Succeeds()
    {
        var pngFile = GetFirstTestFile("png");
        if (pngFile is null) return;

        var tempOutput = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.ico");

        try
        {
            var options = new ConvertOptions
            {
                InputPath = pngFile,
                OutputPath = tempOutput,
                Sizes = [IconSize.Size16, IconSize.Size32],
                Overwrite = false
            };

            var result = Program.ExecuteConversion(options);

            Assert.True(result.Success);
            Assert.Equal(tempOutput, result.OutputPath);
            Assert.Equal(ExitCode.Success, result.ExitCode);
        }
        finally
        {
            if (File.Exists(tempOutput))
                File.Delete(tempOutput);
        }
    }

    /// <summary>
    /// Gets the first valid test file of the specified type from TestData directory.
    /// Actually tries to load the image to ensure it's valid (not just magic bytes).
    /// </summary>
    private string? GetFirstTestFile(string subfolder)
    {
        var dir = Path.Combine(_testDataPath, subfolder);
        if (!Directory.Exists(dir))
        {
            return null;
        }

        // Find a file that can actually be loaded by ImageSharp
        foreach (var file in Directory.GetFiles(dir))
        {
            var format = ImageFormatDetector.DetectFormat(file);
            if (format == ImageFormat.Unknown)
            {
                continue;
            }

            // Try to actually load the image
            try
            {
                using var image = Image.Load(file);
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