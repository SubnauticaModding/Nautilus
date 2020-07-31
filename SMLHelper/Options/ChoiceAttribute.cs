using System;
using System.Linq;

namespace SMLHelper.V2.Options
{
    /// <summary>
    /// Attribute used to signify the specified <see cref="int"/> should be represented in the mod's options menu as a
    /// <see cref="ModChoiceOption"/>, where the <see cref="int"/> represents the index of the selected choice in the given array.
    /// </summary>
    /// <remarks>
    /// Attribute will be ignored for <see cref="Enum"/>-based Choice options, and the <see cref="Enum"/> will instead be parsed
    /// to infer the relevant choices to display.
    /// </remarks>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// 
    /// public enum CustomChoice { One, Two, Three }
    /// 
    /// [Menu("My Options Menu")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My index-based choice"), Choice("One", "Two", "Three")]
    ///     public int MyIndexBasedChoice;
    ///     
    ///     [Label("My enum-based choice")]
    ///     public CustomChoice MyEnumBasedChoice;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="LabelAttribute"/>
    /// <seealso cref="ModChoiceOption"/>
    /// <seealso cref="Json.ConfigFile"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ChoiceAttribute : Attribute
    {
        /// <summary>
        /// The list of options that will be displayed.
        /// </summary>
        public string[] Options { get; }

        /// <summary>
        /// Sifnigies the specified <see cref="int"/> should be represented in the mod's options menu as a
        /// <see cref="ModChoiceOption"/>, where the <see cref="int"/> represents the index of the selected choice in the given array.
        /// </summary>
        /// <remarks>
        /// Attribute will be ignored for <see cref="Enum"/>-based Choice options, and the <see cref="Enum"/> will instead be parsed
        /// to infer the relevant choices to display.
        /// </remarks>
        /// <param name="options">The list of options for the user to choose from.</param>
        public ChoiceAttribute(params string[] options) => Options = options;

        /// <summary>
        /// Sifnigies the specified <see cref="int"/> should be represented in the mod's options menu as a
        /// <see cref="ModChoiceOption"/>, where the <see cref="int"/> represents the index of the selected choice in the given array.
        /// </summary>
        /// <remarks>
        /// Attribute will be ignored for <see cref="Enum"/>-based Choice options, and the <see cref="Enum"/> will instead be parsed
        /// to infer the relevant choices to display.
        /// </remarks>
        /// <param name="options">The list of options for the user to choose from.</param>
        public ChoiceAttribute(params object[] options) : this(options.Select(x => x.ToString()).ToArray()) { }
    }
}
