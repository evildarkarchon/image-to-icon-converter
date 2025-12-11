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
        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (int)ExitCode.OutputWriteFailed;
        }
    }

    /// <summary>
    /// Runs the CLI with the provided arguments.
    /// </summary>
    internal static int Run(string[] args)
    {
        // Handle help and version
        if (args.Length == 0 || HasFlag(args, "--help", "-h", "-?"))
        {
            ShowHelp();
            return args.Length == 0 ? (int)ExitCode.InvalidArguments : (int)ExitCode.Success;
        }

        if (HasFlag(args, "--version", "-v"))
        {
            Console.WriteLine($"image-to-icon-converter {Version}");
            return (int)ExitCode.Success;
        }

        // Parse arguments
        var options = ParseArguments(args);
        if (options is null)
        {
            return (int)ExitCode.InvalidArguments;
        }

        // Execute conversion
        var result = ExecuteConversion(options);

        if (result.Success)
        {
            Console.WriteLine($"Successfully created: {result.OutputPath}");
        }
        else
        {
            Console.Error.WriteLine($"Error: {result.ErrorMessage}");
        }

        return (int)result.ExitCode;
    }

    /// <summary>
    /// Shows help information.
    /// </summary>
    private static void ShowHelp()
    {
        Console.WriteLine("""
            image-to-icon-converter - Convert images to ICO format for use as Windows icons

            Usage:
              image-to-icon-converter --input <path> [options]
              image-to-icon-converter -i <path> [options]

            Options:
              -i, --input <path>     (Required) Input image file path (PNG, JPEG, or BMP)
              -o, --output <path>    Output ICO file path (default: input path with .ico extension)
              -s, --sizes <sizes>    Comma-separated icon sizes to generate (default: 16,32,48,256)
                                     Valid sizes: 16, 32, 48, 256
              -y, --overwrite        Overwrite output file if it exists without prompting
              -h, --help             Show this help message
              -v, --version          Show version information

            Examples:
              image-to-icon-converter -i logo.png
              image-to-icon-converter -i logo.png -o app.ico
              image-to-icon-converter -i logo.png -s 16,32,48 -y
              image-to-icon-converter --input logo.png --sizes 256 --overwrite
            """);
    }

    /// <summary>
    /// Parses command-line arguments into ConvertOptions.
    /// </summary>
    private static ConvertOptions? ParseArguments(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;
        string? sizesString = null;
        bool overwrite = false;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "-i":
                case "--input":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("Error: --input requires a file path argument.");
                        return null;
                    }
                    inputPath = args[++i];
                    break;

                case "-o":
                case "--output":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("Error: --output requires a file path argument.");
                        return null;
                    }
                    outputPath = args[++i];
                    break;

                case "-s":
                case "--sizes":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("Error: --sizes requires a comma-separated list of sizes.");
                        return null;
                    }
                    sizesString = args[++i];
                    break;

                case "-y":
                case "--overwrite":
                    overwrite = true;
                    break;

                default:
                    // Check if it's an unknown option
                    if (arg.StartsWith('-'))
                    {
                        Console.Error.WriteLine($"Error: Unknown option '{arg}'. Use --help for usage information.");
                        return null;
                    }
                    // If no input specified yet and this is a standalone argument, treat as input
                    if (inputPath is null)
                    {
                        inputPath = arg;
                    }
                    else
                    {
                        Console.Error.WriteLine($"Error: Unexpected argument '{arg}'. Use --help for usage information.");
                        return null;
                    }
                    break;
            }
        }

        // Validate required input
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            Console.Error.WriteLine("Error: --input is required. Use --help for usage information.");
            return null;
        }

        // Parse sizes if provided
        IconSize[]? sizes = null;
        if (!string.IsNullOrWhiteSpace(sizesString))
        {
            sizes = ParseSizes(sizesString);
            if (sizes is null)
            {
                Console.Error.WriteLine($"Error: Invalid sizes '{sizesString}'. Valid sizes are: 16, 32, 48, 256");
                return null;
            }
        }

        return new ConvertOptions
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            Sizes = sizes,
            Overwrite = overwrite
        };
    }

    /// <summary>
    /// Checks if any of the specified flags are present in args.
    /// </summary>
    private static bool HasFlag(string[] args, params string[] flags)
    {
        return args.Any(arg => flags.Contains(arg, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Parses a comma-separated sizes string into an array of IconSize.
    /// </summary>
    private static IconSize[]? ParseSizes(string sizesString)
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
