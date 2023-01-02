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
    public class ChoiceChangedEventArgs : ModEventArgs<KeyValuePair<int, string>>
    {
        /// <summary>
        /// The ID of the <see cref="ModChoiceOption"/> that was changed.
        /// </summary>
        public override string Id { get; }

        /// <summary>
        /// The value of the <see cref="ModChoiceOption"/> as a string.
        /// </summary>
        public override KeyValuePair<int, string> Value { get; }

        /// <summary>
        /// Constructs a new <see cref="ChoiceChangedEventArgs"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="ModChoiceOption"/> that was changed.</param>
        /// <param name="value">The value of the <see cref="ModChoiceOption"/> as a string.</param>
        public ChoiceChangedEventArgs(string id, KeyValuePair<int, string> value)
        {
            this.Id = id;
            this.Value = value;
        }
    }

    /// <summary>
    /// A mod option class for handling an option that can select one item from a list of values.
    /// </summary>
    public class ModChoiceOption : ModOption
    {
        /// <summary>
        /// The array of readable string options to choose between in the <see cref="ModChoiceOption"/>.
        /// </summary>
        public string[] Options { get; }

        /// <summary>
        /// The currently selected index among the options array.
        /// </summary>
        public int Index { get; }

        internal override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            uGUI_Choice choice = panel.AddChoiceOption(tabIndex, Label, Options, Index,
                new UnityAction<int>((int index) => parentOptions.OnChange<ChoiceChangedEventArgs, KeyValuePair<int, string>>(Id, (KeyValuePair<int, string>)Value)));

            OptionGameObject = choice.transform.parent.transform.parent.gameObject; // :(

            base.AddToPanel(panel, tabIndex);
        }

        /// <summary>
        /// Instantiates a new <see cref="ModChoiceOption"/> for handling an option that can select one item from a list of values.
        /// </summary>
        /// <param name="id">The internal ID of this option.</param>
        /// <param name="label">The display text to show on the in-game menus.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="index">The starting value.</param>
        internal ModChoiceOption(string id, string label, string[] options, int index) : base(label, id, typeof(KeyValuePair<int, string>), new KeyValuePair<int, string>(index, options[index]))
        {
            this.Options = options;
            this.Index = index;
        }

        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="index">The starting value.</param>
        public static ModChoiceOption Factory(string id, string label, string[] options, int index)
        {
            if (Validator.ValidateChoiceOrDropdownOption(id, label, options, index))
            {
                return new ModChoiceOption(id, label, options, index);
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
        public static ModChoiceOption Factory(string id, string label, string[] options, string value)
        {
            int index = Array.IndexOf(options, value);
            if (index < 0)
            {
                index = 0;
            }

            return Factory(id, label, options, index);
        }
        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="index">The starting value.</param>
        public static ModChoiceOption Factory(string id, string label, object[] options, int index)
        {
            string[] strOptions = new string[options.Length];

            for (int i = 0; i < options.Length; i++)
            {
                strOptions[i] = options[i].ToString();
            }

            return Factory(id, label, strOptions, index);
        }
        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance.
        /// </summary>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="options">The collection of available values.</param>
        /// <param name="value">The starting value.</param>
        public static ModChoiceOption Factory(string id, string label, object[] options, object value)
        {
            int index = Array.IndexOf(options, value);
            return Factory(id, label, options, index);
        }
        /// <summary>
        /// Adds a new <see cref="ModChoiceOption"/> to this instance, automatically using the values of an enum
        /// </summary>
        /// <typeparam name="T">The enum which will be used to populate the options</typeparam>
        /// <param name="id">The internal ID for the choice option.</param>
        /// <param name="label">The display text to use in the in-game menu.</param>
        /// <param name="value">The starting value</param>
        public static ModChoiceOption Factory<T>(string id, string label, T value) where T : Enum
        {
            string[] options = Enum.GetNames(typeof(T));
            string valueString = value.ToString();

            return Factory(id, label, options, valueString);
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
        internal override Type AdjusterComponent => typeof(ChoiceOptionAdjust);
    }
}
