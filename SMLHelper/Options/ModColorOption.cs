
using UnityEngine.Events;
using UnityEngine;
using System;

namespace SMLHelper.Options
{
    /// <summary>
    /// A mod option class for handling an option that can be any <seealso cref="Color"/>.
    /// </summary>
    public class ModColorOption : ModOption<Color, ColorChangedEventArgs>
    {
        /// <summary>
        /// The base method for adding an object to the options panel
        /// </summary>
        /// <param name="panel">The panel to add the option to.</param>
        /// <param name="tabIndex">Where in the panel to add the option.</param>
        public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            GameObject colorPicker = panel.AddColorOption(tabIndex, Label, Value,
                new UnityAction<Color>((Color value) => parentOptions.OnChange<ColorChangedEventArgs, Color>(Id, value)));
            

            GameObject redSlider = panel.AddSliderOption(tabIndex, Label + "Red", Value.r, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(value, Value.g, Value.b);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    parentOptions.OnChange<ColorChangedEventArgs, Color>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}");

            GameObject greenSlider = panel.AddSliderOption(tabIndex, Label + "Green", Value.g, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, value, Value.b);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    parentOptions.OnChange<ColorChangedEventArgs, Color>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}");

            GameObject blueSlider = panel.AddSliderOption(tabIndex, Label + "Blue", Value.b, 0, 1, 0, 0.01f,
                new UnityAction<float>((float value) => {
                    Color color = new Color(Value.r, Value.g, value);
                    colorPicker.GetComponentInChildren<uGUI_ColorChoice>().value = color;
                    parentOptions.OnChange<ColorChangedEventArgs, Color>(Id, color);
                }),
                SliderLabelMode.Percent, "{0:F0}");

            OptionGameObject = colorPicker.transform.parent.gameObject;
            base.AddToPanel(panel, tabIndex);
        }

        private ModColorOption(string id, string label, Color value) : base(label, id, value) { }

        /// <summary>
        /// Creates a new <see cref="ModToggleOption"/> instance.
        /// </summary>
        /// <param name="id">The internal ID for the Color option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="value">The starting value.</param>
        public static ModColorOption Create(string id, string label, Color value)
        {
            return new ModColorOption(id, label, value);
        }

        /// <summary>
        /// Creates a new <see cref="ModBasicColorOption"/> instance.
        /// </summary>
        /// <param name="id">The internal ID for the Color option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        public static ModColorOption Create(string id, string label)
        {
            return Create(id, label, Color.white);
        }

        /// <summary>
        /// The Adjuster for this <see cref="ModOption"/>.
        /// </summary>
        public override Type AdjusterComponent => null;
    }
}
