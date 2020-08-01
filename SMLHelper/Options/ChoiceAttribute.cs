namespace SMLHelper.V2.Options
{
    using Json;
    using System;
    using System.Linq;

    /// <summary>
    /// Attribute used to signify the specified member should be represented in the mod's options menu as a
    /// <see cref="ModChoiceOption"/>. Works for either <see cref="int"/> index-based, <see cref="string"/>-based, or
    /// <see cref="Enum"/>-based members.
    /// </summary>
    /// <remarks>
    /// <see cref="Enum"/> choices can also be parsed from their values by merely omitting the <see cref="ChoiceAttribute"/>.
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
    /// <seealso cref="ConfigFile"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ChoiceAttribute : Attribute
    {
        /// <summary>
        /// The list of options that will be displayed.
        /// </summary>
        public string[] Options { get; }

        /// <summary>
        /// Attribute used to signify the specified member should be represented in the mod's options menu as a
        /// <see cref="ModChoiceOption"/>. Works for either <see cref="int"/> index-based, <see cref="string"/>-based, or
        /// <see cref="Enum"/>-based members.
        /// </summary>
        /// <remarks>
        /// <see cref="Enum"/> choices can also be parsed from their values by merely omitting the <see cref="ChoiceAttribute"/>.
        /// </remarks>
        /// <param name="options">The list of options for the user to choose from.</param>
        public ChoiceAttribute(params string[] options)
        {
            Options = options;
        }

        /// <summary>
        /// Attribute used to signify the specified member should be represented in the mod's options menu as a
        /// <see cref="ModChoiceOption"/>. Works for either <see cref="int"/> index-based, <see cref="string"/>-based, or
        /// <see cref="Enum"/>-based members.
        /// </summary>
        /// <remarks>
        /// <see cref="Enum"/> choices can also be parsed from their values by merely omitting the <see cref="ChoiceAttribute"/>.
        /// </remarks>
        /// <param name="options">The list of options for the user to choose from.</param>
        public ChoiceAttribute(params object[] options) : this(options.Select(x => x.ToString()).ToArray()) { }
    }
}
