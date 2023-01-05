namespace SMLHelper.Options
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using SMLHelper.Options.Utility;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Contains all the information about a choice changed event.
    /// </summary>
    public class ChoiceChangedEventArgs : ConfigOptionEventArgs<KeyValuePair<int, string>>
    {
        /// <summary>
        /// Constructs a new <see cref="ChoiceChangedEventArgs"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ModChoiceOption"/> that was changed.</param>
        /// <param name="value">The value of the <see cref="ModChoiceOption"/> as a string.</param>
        public ChoiceChangedEventArgs(string id, KeyValuePair<int, string> value) : base(id, value) { }
    }

    /// <summary>
    /// A mod option class for handling an option that can select one item from a list of values.
    /// </summary>
    public class ModChoiceOption : ModOption<KeyValuePair<int, string>>
    {
        /// <summary>
        /// The array of readable string options to choose between in the <see cref="ModChoiceOption"/>.
        /// </summary>
        public string[] Options { get; }

        /// <summary>
        /// The currently selected index among the options array.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The tooltip to show when hovering over the option.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// The base method for adding an object to the options panel
        /// </summary>
        /// <param name="panel">The panel to add the option to.</param>
        /// <param name="tabIndex">Where in the panel to add the option.</param>
        public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            uGUI_Choice choice = panel.AddChoiceOption(tabIndex, Label, Options, Index,
                new UnityAction<int>((int index) => {
                    OnChange<ChoiceChangedEventArgs, KeyValuePair<int, string>>(Id, new KeyValuePair<int, string>(index, Options[index]));
                    parentOptions.OnChange<ChoiceChangedEventArgs, KeyValuePair<int, string>>(Id, new KeyValuePair<int, string>(index, Options[index])); 
                }), Tooltip);

            OptionGameObject = choice.transform.parent.transform.parent.gameObject; // :(

            base.AddToPanel(panel, tabIndex);
        }

        private ModChoiceOption(string id, string label, string[] options, int index, string tooltip) : base(label, id, new KeyValuePair<int, string>(index, options[index]))
        {
            Options = options;
            Index = index;
            Tooltip = tooltip;
        }

        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="index">The starting value.</param>
        /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
        public static ModChoiceOption Factory(string id, string label, string[] options, int index, string tooltip = null)
        {
            if (Validator.ValidateChoiceOrDropdownOption(id, label, options, index))
            {
                return new ModChoiceOption(id, label, options, index, tooltip);
            }
            // Should never happen
            throw new NotImplementedException("ModChoiceOption Factory could not create instance");
        }
        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="value">The starting value.</param>
        /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
        public static ModChoiceOption Factory(string id, string label, string[] options, string value, string tooltip = null)
        {
            int index = Array.IndexOf(options, value);
            if (index < 0)
            {
                index = 0;
            }

            return Factory(id, label, options, index, tooltip);
        }
        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="index">The starting value.</param>
        /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
        public static ModChoiceOption Factory(string id, string label, object[] options, int index, string tooltip = null)
        {
            string[] strOptions = new string[options.Length];

            for (int i = 0; i < options.Length; i++)
            {
                strOptions[i] = options[i].ToString();
            }

            return Factory(id, label, strOptions, index, tooltip);
        }
        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="value">The starting value.</param>
        /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
        public static ModChoiceOption Factory(string id, string label, object[] options, object value, string tooltip = null)
        {
            int index = Array.IndexOf(options, value);
            return Factory(id, label, options, index, tooltip);
        }
        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance, automatically using the values of an enum
        /// </summary>
        /// <typeparam name="T">The enum which will be used to populate the options</typeparam>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="value">The starting value</param>
        /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
        public static ModChoiceOption Factory<T>(string id, string label, T value, string tooltip = null) where T : Enum
        {
            string[] options = Enum.GetNames(typeof(T));
            string valueString = value.ToString();

            return Factory(id, label, options, valueString, tooltip);
        }

        private class ChoiceOptionAdjust: ModOptionAdjust
        {
            private const float spacing = 10f;

            public IEnumerator Start()
            {
                SetCaptionGameObject("Choice/Caption");
                yield return null; // skip one frame

                RectTransform rect = gameObject.transform.Find("Choice/Background") as RectTransform;

                float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
                float widthChoice = rect.rect.width;
                float widthText = CaptionWidth + spacing;

                if (widthText + widthChoice > widthAll)
                {
                    rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - widthChoice);
                }

                Destroy(this);
            }
        }
        /// <summary>
        /// The Adjuster for this <see cref="ModOption"/>.
        /// </summary>
        public override Type AdjusterComponent => typeof(ChoiceOptionAdjust);
    }
}
