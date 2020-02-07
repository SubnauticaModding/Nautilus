namespace SMLHelper.V2.Options
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Abstract class that provides the framework for your mod's in-game configuration options.
    /// </summary>
    public abstract partial class ModOptions
    {
        /// <summary>
        /// The name of this set of configuration options.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Obtains the <see cref="ModOption"/>s that belong to this instance. Can be null.
        /// </summary>
        public List<ModOption> Options
        {
            get => _options?.Values.ToList();
        }

        // This is a dictionary now in case we want to get the ModOption quickly
        // based on the provided ID.
        private Dictionary<string, ModOption> _options;

        private void AddOption(ModOption option)
        {
            _options.Add(option.Id, option);
            option.SetParent(this);
        }

        internal void AddOptionsToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            panel.AddHeading(tabIndex, Name);

            _options = new Dictionary<string, ModOption>(); // we need to do this every time we adding options
            BuildModOptions();

            _options.Values.ForEach(option => option.AddToPanel(panel, tabIndex));
        }

        /// <summary>
        /// Creates a new instance of <see cref="ModOptions"/>.
        /// </summary>
        /// <param name="name">The name that will display above this section of options in the in-game menu.</param>
        public ModOptions(string name)
        {
            Name = name;
        }

        /// <summary>
        /// <para>Builds up the configuration the options.</para>
        /// <para>This method should be composed of calls into the following methods: 
        /// <seealso cref="AddSliderOption"/> | <seealso cref="AddToggleOption"/> | <seealso cref="AddChoiceOption(string, string, string[], int)"/> | <seealso cref="AddKeybindOption(string, string, GameInput.Device, KeyCode)"/>.</para>
        /// <para>Make sure you have subscribed to the events in the constructor to handle what happens when the value is changed:
        /// <seealso cref="SliderChanged"/> | <seealso cref="ToggleChanged"/> | <seealso cref="ChoiceChanged"/> | <seealso cref="KeybindChanged"/>.</para>
        /// </summary>
        public abstract void BuildModOptions();
    }

    /// <summary>
    /// The common abstract class to all mod options.
    /// </summary>
    public abstract class ModOption
    {
        /// <summary>
        /// The internal ID that identifies this option.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The display text to be shown for this option in the in-game menus.
        /// </summary>
        public string Label { get; }

        /// <summary> UI GameObject for this option </summary>
        public GameObject OptionGameObject { get; protected set; }

        /// <summary> Parent <see cref="ModOptions"/> for this option </summary>
        protected ModOptions parentOptions;

        internal void SetParent(ModOptions parent)
        {
            if (parentOptions == null)
                parentOptions = parent;
            else
                V2.Logger.Log($"ModOption.SetParent: parent already setted for {Id}", LogLevel.Warn);
        }

        // adds UI GameObject to panel and updates OptionGameObject
        internal abstract void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex);

        /// <summary>
        /// Base constructor for all mod options.
        /// </summary>
        /// <param name="label">The display text to show on the in-game menus.</param>
        /// <param name="id">The internal ID if this option.</param>
        internal ModOption(string label, string id)
        {
            Label = label;
            Id = id;
        }
    }
}
