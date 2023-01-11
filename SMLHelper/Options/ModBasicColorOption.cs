using System;
using UnityEngine;
using UnityEngine.Events;

namespace SMLHelper.Options
{
    /// <summary>
    /// Contains all the information about a color changed event.
    /// </summary>
    public class ColorChangedEventArgs : ConfigOptionEventArgs<Color>
    {
        /// <summary>
        /// Constructs a new <see cref="ToggleChangedEventArgs"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ModToggleOption"/> that was changed.</param>
        /// <param name="value">The new value for the <see cref="ModToggleOption"/>.</param>
        public ColorChangedEventArgs(string id, Color value) : base(id, value) { }
    }

    /// <summary>
    /// A basic mod option class for handling an option that can be any <seealso cref="Color"/>.
    /// </summary>
    public class ModBasicColorOption : ModOption<Color, ColorChangedEventArgs>
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
            OptionGameObject = colorPicker.transform.parent.gameObject;

            base.AddToPanel(panel, tabIndex);
        }

        private ModBasicColorOption(string id, string label, Color value) : base(label, id, value) { }

        /// <summary>
        /// Creates a new <see cref="ModToggleOption"/> instance.
        /// </summary>
        /// <param name="id">The internal ID for the Color option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="value">The starting value.</param>
        public static ModBasicColorOption Create(string id, string label, Color value)
        {
            return new ModBasicColorOption(id, label, value);
        }

        /// <summary>
        /// Creates a new <see cref="ModBasicColorOption"/> instance.
        /// </summary>
        /// <param name="id">The internal ID for the Color option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        public static ModBasicColorOption Create(string id, string label)
        {
            return Create(id, label, Color.white);
        }

        /// <summary>
        /// The Adjuster for this <see cref="ModOption"/>.
        /// </summary>
        public override Type AdjusterComponent => null;
    }
}