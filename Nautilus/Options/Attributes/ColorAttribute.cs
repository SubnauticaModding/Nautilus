using System;
using Nautilus.Json;

namespace Nautilus.Options.Attributes;

/// <summary>
/// Attribute used to signify the decorated <see cref="UnityEngine.Color"/> should be represented in the mod's
/// option menu as a <see cref="ModColorOption"/>.
/// </summary>
/// <example>
/// <code>
/// using Nautilus.Json;
/// using Nautilus.Options;
/// 
/// [Menu("My Options Menu")]
/// public class Config : ConfigFile
/// {
///     [ColorPicker("My Color")]
///     public Color MyColor;
/// }
/// </code>
/// </example>
/// <seealso cref="MenuAttribute"/>
/// <seealso cref="ConfigFile"/>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class ColorPickerAttribute : ModOptionAttribute
{
    /// <summary>
    /// Which type of color picker to use.
    /// </summary>
    public bool Advanced { get; set; } = false;

    /// <summary>
    /// The label for the red channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string RedLabel { get; set; } = "Red";

    /// <summary>
    /// The label for the green channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string GreenLabel { get; set; } = "Green";

    /// <summary>
    /// The label for the blue channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string BlueLabel { get; set; } = "Blue";

    /// <summary>
    /// The label for the alpha channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string AlphaLabel { get; set; } = "Alpha";

    /// <summary>
    /// The tooltip for the red channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string RedTooltip { get; set; } = "The <color=\"red\">red</color> level of the color.";

    /// <summary>
    /// The tooltip for the green channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string GreenTooltip { get; set; } = "The <color=\"green\">green</color> level of the color.";

    /// <summary>
    /// The tooltip for the blue channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string BlueTooltip { get; set; } = "The <color=\"blue\">blue</color> level of the color.";

    /// <summary>
    /// The tooltip for the alpha channel.
    /// Values may be either ordinary strings for direct display, or IDs for lookup with <c>Language.Get(string)</c>.
    /// </summary>
    public string AlphaTooltip { get; set; } = "The opaqueness of the color. The lower the value the more transparent.";

    /// <summary>
    /// Signifies the decorated <see cref="UnityEngine.Color"/> should be represented in the mod's option menu
    /// as a <see cref="ModColorOption"/>.
    /// </summary>
    /// <param name="label">The label for the color picker.</param>
    public ColorPickerAttribute(string label = null) : base(label) { }

    /// <summary>
    /// Signifies the decorated <see cref="UnityEngine.Color"/> should be represented in the mod's option menu
    /// as a <see cref="ModColorOption"/>.
    /// </summary>
    public ColorPickerAttribute() { }
}