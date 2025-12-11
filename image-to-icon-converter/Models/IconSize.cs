namespace ImageToIconConverter.Models;

/// <summary>
/// Represents the standard icon sizes supported by the converter.
/// </summary>
public enum IconSize
{
    /// <summary>16x16 pixels - Small icons, taskbar</summary>
    Size16 = 16,

    /// <summary>32x32 pixels - Standard icons</summary>
    Size32 = 32,

    /// <summary>48x48 pixels - Large icons</summary>
    Size48 = 48,

    /// <summary>256x256 pixels - Extra large (Vista+), stored as PNG</summary>
    Size256 = 256
}

/// <summary>
/// Extension methods for IconSize.
/// </summary>
public static class IconSizeExtensions
{
    /// <summary>
    /// Gets the default set of icon sizes for conversion.
    /// </summary>
    public static readonly IconSize[] DefaultSizes =
    [
        IconSize.Size16,
        IconSize.Size32,
        IconSize.Size48,
        IconSize.Size256
    ];

    /// <summary>
    /// Determines whether this size should be stored as PNG in the ICO file.
    /// </summary>
    /// <param name="size">The icon size.</param>
    /// <returns>True if PNG format should be used; false for BMP format.</returns>
    public static bool ShouldUsePng(this IconSize size) => size == IconSize.Size256;

    /// <summary>
    /// Tries to parse an integer value to an IconSize.
    /// </summary>
    /// <param name="value">The integer value to parse.</param>
    /// <param name="iconSize">The parsed IconSize if successful.</param>
    /// <returns>True if parsing was successful; otherwise false.</returns>
    public static bool TryParse(int value, out IconSize iconSize)
    {
        iconSize = value switch
        {
            16 => IconSize.Size16,
            32 => IconSize.Size32,
            48 => IconSize.Size48,
            256 => IconSize.Size256,
            _ => default
        };
        return value is 16 or 32 or 48 or 256;
    }
}