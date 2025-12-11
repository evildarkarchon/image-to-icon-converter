using System.CommandLine;
using System.CommandLine.Invocation;
using ImageToIconConverter.Models;
using ImageToIconConverter.Services;

namespace ImageToIconConverter;

/// <summary>
/// Entry point for the image-to-icon-converter CLI application.
/// </summary>
public static class Program
{
    private const string Version = "1.0.0";

    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code indicating success (0) or failure (non-zero).</returns>
    public static int Main(string[] args)
    {
        return Run(args);
    }

    /// <summary>
    /// Runs the CLI with the provided arguments.
    /// </summary>
    internal static int Run(string[] args)
    {
        var rootCommand = CreateRootCommand();
        return rootCommand.Invoke(args);
    }

    /// <summary>
    /// Creates and configures the root command with all options.
    /// </summary>
    internal static RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("Convert images to ICO format for use as Windows icons")
        {
            Name = "image-to-icon-converter"
        };

        // Define options
        var inputOption = new Option<FileInfo?>(
            aliases: ["--input", "-i"],
            description: "Input image file path (PNG, JPEG, or BMP)")
        {
            IsRequired = true
        };

        var outputOption = new Option<FileInfo?>(
            aliases: ["--output", "-o"],
            description: "Output ICO file path (default: input path with .ico extension)");

        var sizesOption = new Option<string?>(
            aliases: ["--sizes", "-s"],
            description: "Comma-separated icon sizes to generate (default: 16,32,48,256). Valid sizes: 16, 32, 48, 256");

        var overwriteOption = new Option<bool>(
            aliases: ["--overwrite", "-y"],
            description: "Overwrite output file if it exists without prompting",
            getDefaultValue: () => false);

        // Add options to command
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(sizesOption);
        rootCommand.AddOption(overwriteOption);

        // Set up the handler
        rootCommand.SetHandler((InvocationContext context) =>
        {
            var input = context.ParseResult.GetValueForOption(inputOption);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var sizes = context.ParseResult.GetValueForOption(sizesOption);
            var overwrite = context.ParseResult.GetValueForOption(overwriteOption);

            // Validate input was provided (should be enforced by IsRequired)
            if (input is null)
            {
                Console.Error.WriteLine("Error: --input is required. Use --help for usage information.");
                context.ExitCode = (int)ExitCode.InvalidArguments;
                return;
            }

            // Parse sizes if provided
            IconSize[]? parsedSizes = null;
            if (!string.IsNullOrWhiteSpace(sizes))
            {
                parsedSizes = ParseSizes(sizes);
                if (parsedSizes is null)
                {
                    Console.Error.WriteLine($"Error: Invalid sizes '{sizes}'. Valid sizes are: 16, 32, 48, 256");
                    context.ExitCode = (int)ExitCode.InvalidArguments;
                    return;
                }
            }

            var options = new ConvertOptions
            {
                InputPath = input.FullName,
                OutputPath = output?.FullName,
                Sizes = parsedSizes,
                Overwrite = overwrite
            };

            var result = ExecuteConversion(options);

            if (result.Success)
            {
                Console.WriteLine($"Successfully created: {result.OutputPath}");
            }
            else
            {
                Console.Error.WriteLine($"Error: {result.ErrorMessage}");
            }

            context.ExitCode = (int)result.ExitCode;
        });

        return rootCommand;
    }

    /// <summary>
    /// Parses a comma-separated sizes string into an array of IconSize.
    /// </summary>
    internal static IconSize[]? ParseSizes(string sizesString)
    {
        var parts = sizesString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var sizes = new List<IconSize>();

        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var value) || !IconSizeExtensions.TryParse(value, out var iconSize))
            {
                return null;
            }
            sizes.Add(iconSize);
        }

        return sizes.Count > 0 ? [.. sizes] : null;
    }

    /// <summary>
    /// Executes the image to ICO conversion.
    /// </summary>
    internal static ConversionResult ExecuteConversion(ConvertOptions options)
    {
        var imageLoader = new ImageLoader();
        var icoGenerator = new IcoGenerator();

        return ExecuteConversion(options, imageLoader, icoGenerator);
    }

    /// <summary>
    /// Executes the image to ICO conversion with specified services.
    /// </summary>
    internal static ConversionResult ExecuteConversion(
        ConvertOptions options,
        IImageLoader imageLoader,
        IIcoGenerator icoGenerator)
    {
        // Validate input file exists
        if (!File.Exists(options.InputPath))
        {
            return ConversionResult.Failed(
                $"Input file not found: {options.InputPath}",
                ExitCode.InputFileNotFound);
        }

        // Validate input format
        try
        {
            imageLoader.ValidateFile(options.InputPath);
        }
        catch (NotSupportedException ex)
        {
            return ConversionResult.Failed(ex.Message, ExitCode.UnsupportedFormat);
        }

        // Get effective output path
        var outputPath = options.GetEffectiveOutputPath();

        // Check if output file exists
        if (File.Exists(outputPath) && !options.Overwrite)
        {
            return ConversionResult.Failed(
                $"Output file already exists: {outputPath}. Use --overwrite (-y) to replace it.",
                ExitCode.OutputWriteFailed);
        }

        // Load and convert the image
        try
        {
            using var image = imageLoader.LoadImage(options.InputPath);
            var sizes = options.GetEffectiveSizes();

            icoGenerator.GenerateIco(image, sizes, outputPath);

            return ConversionResult.Succeeded(outputPath);
        }
        catch (InvalidOperationException ex)
        {
            return ConversionResult.Failed(ex.Message, ExitCode.InvalidArguments);
        }
        catch (IOException ex)
        {
            return ConversionResult.Failed(
                $"Failed to write output file: {ex.Message}",
                ExitCode.OutputWriteFailed);
        }
    }
}
