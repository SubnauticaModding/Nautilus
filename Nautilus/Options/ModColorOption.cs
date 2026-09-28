using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nautilus.Options;

/// <summary>
/// Contains all the information about a color changed event.
/// </summary>
public class ColorChangedEventArgs : ConfigOptionEventArgs<Color>
{
    /// <summary>
    /// Constructs a new <see cref="ToggleChangedEventArgs"/>.
    /// </summary>
    /// <param name="id">The ID of the <see cref="ModColorOption"/> that was changed.</param>
    /// <param name="value">The new value for the <see cref="ModColorOption"/>.</param>
    public ColorChangedEventArgs(string id, Color value) : base(id, value) { }
}

/// <summary>
/// A basic mod option class for handling an option that can be any <seealso cref="Color"/>.
/// </summary>
public class ModColorOption : ModOption<Color, ColorChangedEventArgs>
{
    /// <summary>
    /// Whether to use an advanced UI
    /// </summary>
    public bool Advanced { get; set; }

    /// <summary>
    /// The tooltip to show when hovering over the option's items.
    /// </summary>
    public string Tooltip { get; }

    /// <summary>
    /// When using Advanced UI, whether to show the Alpha controls
    /// </summary>
    public bool AdvancedExposeAlphaChannel { get; set; }

    /// <summary>
    /// The label for the red channel slider when using advanced mode.
    /// </summary>
    public string RedLabel { get; }

    /// <summary>
    /// The label for the green channel slider when using advanced mode.
    /// </summary>
    public string GreenLabel { get; }

    /// <summary>
    /// The label for the blue channel slider when using advanced mode.
    /// </summary>
    public string BlueLabel { get; }

    /// <summary>
    /// The label for the alpha channel slider when using advanced mode.
    /// </summary>
    public string AlphaLabel { get; }

    /// <summary>
    /// The tooltip for the red channel slider when using advanced mode.
    /// </summary>
    public string RedTooltip { get; }

    /// <summary>
    /// The tooltip for the green channel slider when using advanced mode.
    /// </summary>
    public string GreenTooltip { get; }

    /// <summary>
    /// The tooltip for the blue channel slider when using advanced mode.
    /// </summary>
    public string BlueTooltip { get; }

    /// <summary>
    /// The tooltip for the alpha channel slider when using advanced mode.
    /// </summary>
    public string AlphaTooltip { get; }

    /// <summary>
    /// The base method for adding an object to the options panel
    /// </summary>
    /// <param name="panel">The panel to add the option to.</param>
    /// <param name="tabIndex">Where in the panel to add the option.</param>
    public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
    {
        UnityAction<Color> callback = new UnityAction<Color>((Color value) => {
            OnChange(Id, value);
            parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, value);
        });

        GameObject colorPicker = panel.AddColorOption(tabIndex, Label, Value, callback);

        // Add tooltip
        colorPicker.transform.Find("Choice").gameObject.EnsureComponent<MenuTooltip>().key = Tooltip;

        if (Advanced)
        {
            UnityEngine.Object.Destroy(colorPicker.transform.Find("Choice/Background/ButtonLeft").gameObject);
            UnityEngine.Object.Destroy(colorPicker.transform.Find("Choice/Background/ButtonRight").gameObject);

            GameObject redSlider = panel.AddSliderOption(tabIndex, RedLabel, Value.r, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(value, Value.g, Value.b, Value.a);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", RedTooltip);

            GameObject greenSlider = panel.AddSliderOption(tabIndex, GreenLabel, Value.g, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, value, Value.b, Value.a);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", GreenTooltip);

            GameObject blueSlider = panel.AddSliderOption(tabIndex, BlueLabel, Value.b, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, Value.g, value, Value.a);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", BlueTooltip);

            if(AdvancedExposeAlphaChannel)
            {
                GameObject alphaSlider = panel.AddSliderOption(tabIndex, AlphaLabel, Value.a, 0, 1, 1, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, Value.g, Value.b, value);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", AlphaTooltip);
            }
        }

        OptionGameObject = colorPicker.transform.parent.gameObject;
        base.AddToPanel(panel, tabIndex);
    }

    private ModColorOption(string id, string label, Color value, bool advanced = false, string tooltip = null,
        bool advancedExposeAlphaChannel = true, string redLabel = "Red", string greenLabel = "Green",
        string blueLabel = "Blue", string alphaLabel = "Alpha",
        string redTooltip = "The <color=\"red\">red</color> level of the color.",
        string greenTooltip = "The <color=\"green\">green</color> level of the color.",
        string blueTooltip = "The <color=\"blue\">blue</color> level of the color.",
        string alphaTooltip = "The opaqueness of the color. The lower the value the more transparent.") : base(label, id, value)
    {
        Advanced = advanced;
        Tooltip = tooltip;
        AdvancedExposeAlphaChannel = advancedExposeAlphaChannel;
        RedLabel = redLabel;
        GreenLabel = greenLabel;
        BlueLabel = blueLabel;
        AlphaLabel = alphaLabel;
        RedTooltip = redTooltip;
        GreenTooltip = greenTooltip;
        BlueTooltip = blueTooltip;
        AlphaTooltip = alphaTooltip;
    }

    /// <summary>
    /// Creates a new <see cref="ModColorOption"/> instance.
    /// </summary>
    /// <param name="id">The internal ID for the Color option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="value">The starting value.</param>
    /// <param name="advanced">Whether to use an advanced display.</param>
    /// <param name="tooltip">The tooltip to show when hovering over the options.</param>
    /// <param name="advancedExposeAlphaChannel">When using advanced display. Whether to expose alpha channel control.</param>
    /// <param name="redLabel">The label for the red channel slider when using advanced display.</param>
    /// <param name="greenLabel">The label for the green channel slider when using advanced display.</param>
    /// <param name="blueLabel">The label for the blue channel slider when using advanced display.</param>
    /// <param name="alphaLabel">The label for the alpha channel slider when using advanced display.</param>
    /// <param name="redTooltip">The tooltip for the red channel slider when using advanced display.</param>
    /// <param name="greenTooltip">The tooltip for the green channel slider when using advanced display.</param>
    /// <param name="blueTooltip">The tooltip for the blue channel slider when using advanced display.</param>
    /// <param name="alphaTooltip">The tooltip for the alpha channel slider when using advanced display.</param>
    public static ModColorOption Create(string id, string label, Color value, bool advanced = false, string tooltip = null, bool advancedExposeAlphaChannel = true, 
        string redLabel = "Red", 
        string greenLabel = "Green",
        string blueLabel = "Blue", 
        string alphaLabel = "Alpha",
        string redTooltip = "The <color=\"red\">red</color> level of the color.",
        string greenTooltip = "The <color=\"green\">green</color> level of the color.",
        string blueTooltip = "The <color=\"blue\">blue</color> level of the color.",
        string alphaTooltip = "The opaqueness of the color. The lower the value the more transparent.")
    {
        return new ModColorOption(id, label, value, advanced, tooltip, advancedExposeAlphaChannel, redLabel, greenLabel, blueLabel, alphaLabel, redTooltip, greenTooltip, blueTooltip, alphaTooltip);
    }

    /// <summary>
    /// Creates a new <see cref="ModColorOption"/> instance.
    /// </summary>
    /// <param name="id">The internal ID for the Color option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    public static ModColorOption Create(string id, string label)
    {
        return Create(id, label, Color.white);
    }

    /// <summary>
    /// The Adjuster for this <see cref="OptionItem"/>.
    /// </summary>
    public override Type AdjusterComponent => null;
}