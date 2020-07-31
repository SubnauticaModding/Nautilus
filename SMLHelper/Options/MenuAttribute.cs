using System;

namespace SMLHelper.V2.Options
{
    /// <summary>
    /// Attribute used to signify a <see cref="ModOptions"/> menu should be automatically generated from a
    /// <see cref="Json.ConfigFile"/>, as well as  specifying options for handling the <see cref="Json.ConfigFile"/>
    /// and <see cref="ModOptions"/> menu.
    /// </summary>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Interfaces;
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// using QModManager.Utility;
    /// using UnityEngine;
    /// 
    /// [Menu("SMLHelper Example Mod")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My index-based choice"), Tooltip("A simple tooltip"), Choice("Choice 1", "Choice 2", "Choice 3"), OnChange(nameof(MyGenericValueChangedEvent))]
    ///     public int ChoiceIndex;
    ///
    ///     [Label("My enum-based choice"), OnChange(nameof(MyGenericValueChangedEvent))]
    ///     public CustomChoice ChoiceEnum;
    /// 
    ///     [Label("My keybind"), OnChange(nameof(MyGenericValueChangedEvent))]
    ///     public KeyCode KeybindKey;
    /// 
    ///     [Label("My slider"), Slider(0, 50, DefaultValue = 25, Format = "{0:F2}"), OnChange(nameof(MyGenericValueChangedEvent))]
    ///     public float SliderValue;
    /// 
    ///     [Label("My checkbox"), OnChange(nameof(MyCheckboxToggleEvent)), OnChange(nameof(MyGenericValueChangedEvent))]
    ///     public bool ToggleValue;
    /// 
    ///     [Label("My button")]
    ///     public void MyButtonClickEvent(ButtonClickedEventArgs e)
    ///     {
    ///         Logger.Log(Logger.Level.Info, "Button was clicked!");
    ///         Logger.Log(Logger.Level.Info, $"{e.Id}");
    ///     }
    /// 
    ///     public void MyCheckboxToggleEvent(ToggleChangedEventArgs e)
    ///     {
    ///         Logger.Log(Logger.Level.Info, "Checkbox value was changed!");
    ///         Logger.Log(Logger.Level.Info, $"{e.Value}");
    ///     }
    /// 
    ///     private void MyGenericValueChangedEvent(IModOptionEventArgs e)
    ///     {
    ///         Logger.Log(Logger.Level.Info, "Generic value changed!");
    ///         Logger.Log(Logger.Level.Info, $"{e.Id}: {e.GetType()}");
    /// 
    ///         switch (e)
    ///         {
    ///             case KeybindChangedEventArgs keybindChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, keybindChangedEventArgs.KeyName);
    ///                 break;
    ///             case ChoiceChangedEventArgs choiceChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, choiceChangedEventArgs.Value);
    ///                 break;
    ///             case SliderChangedEventArgs sliderChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, sliderChangedEventArgs.Value.ToString());
    ///                 break;
    ///             case ToggleChangedEventArgs toggleChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, toggleChangedEventArgs.Value.ToString());
    ///                 break;
    ///         }
    ///      }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="LabelAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="ChoiceAttribute"/>
    /// <seealso cref="OnChangeAttribute"/>
    /// <seealso cref="SliderAttribute"/>
    /// <seealso cref="ButtonAttribute"/>
    /// <seealso cref="ModOptions"/>
    /// <seealso cref="Json.ConfigFile"/>
    /// <seealso cref="QModManager.Utility.Logger"/>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MenuAttribute : Attribute
    {
        /// <summary>
        /// Specifies after which events the config file should be saved to disk automatically.
        /// </summary>
        /// <remarks>
        /// This enumeration has a <see cref="FlagsAttribute"/> that allows a bitwise combination of its member values.
        /// </remarks>
        [Flags]
        public enum SaveEvents : short
        {
            /// <summary>
            /// Never automatically save.
            /// </summary>
            None = 0,
            /// <summary>
            /// Save whenever any value is changed.
            /// </summary>
            ChangeValue = 1,
            /// <summary>
            /// Save when the player saves the game.
            /// </summary>
            SaveGame = 2,
            /// <summary>
            /// Save when the player quits the game.
            /// </summary>
            QuitGame = 4
        }

        /// <summary>
        /// The display name for the generated options menu.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The events after which the config file will be saved to disk automatically.
        /// </summary>
        /// <seealso cref="SaveEvents"/>
        public SaveEvents SaveOn { get; set; } = SaveEvents.ChangeValue;

        /// <summary>
        /// Whether or not members with no relevant attributes should be ignored when generating the menu.
        /// </summary>
        public bool IgnoreUnattributedMembers { get; set; } = false;

        /// <summary>
        /// Signifies a <see cref="ModOptions"/> menu should be automatically generated from a <see cref="Json.ConfigFile"/>.
        /// </summary>
        /// <param name="name">The display name for the generated options menu.</param>
        public MenuAttribute(string name) => Name = name;
    }
}
