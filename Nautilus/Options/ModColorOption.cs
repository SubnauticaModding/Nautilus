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

            GameObject redSlider = panel.AddSliderOption(tabIndex, "Red", Value.r, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(value, Value.g, Value.b, Value.a);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", "The <color=\"red\">red</color> level of the color.");

            GameObject greenSlider = panel.AddSliderOption(tabIndex, "Green", Value.g, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, value, Value.b, Value.a);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", "The <color=\"green\">green</color> level of the color.");

            GameObject blueSlider = panel.AddSliderOption(tabIndex, "Blue", Value.b, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, Value.g, value, Value.a);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", "The <color=\"blue\">blue</color> level of the color.");

            GameObject alphaSlider = panel.AddSliderOption(tabIndex, "Alpha", Value.a, 0, 1, 1, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, Value.g, Value.b, value);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    OnChange(Id, color);
                    parentOptions.OnChange<Color, ColorChangedEventArgs>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}", "The opaqueness of the color. The lower the value the more transparent.");
        }

        OptionGameObject = colorPicker.transform.parent.gameObject;
        base.AddToPanel(panel, tabIndex);
    }

    private ModColorOption(string id, string label, Color value, bool advanced = false, string tooltip = null) : base(label, id, value)
    {
        Advanced = advanced;
        Tooltip= tooltip;
    }

    /// <summary>
    /// Creates a new <see cref="ModColorOption"/> instance.
    /// </summary>
    /// <param name="id">The internal ID for the Color option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="value">The starting value.</param>
    /// <param name="advanced">Whether to use an advanced display.</param>
    /// <param name="tooltip">The tooltip to show when hovering over the options.</param>
    public static ModColorOption Create(string id, string label, Color value, bool advanced = false, string tooltip = null)
    {
        return new ModColorOption(id, label, value, advanced, tooltip);
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