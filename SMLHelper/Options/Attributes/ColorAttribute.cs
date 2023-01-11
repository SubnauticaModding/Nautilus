namespace SMLHelper.Options.Attributes
{
    using Json;
    using System;

    /// <summary>
    /// Attribute used to signify the decorated <see cref="UnityEngine.Color"/> should be represented in the mod's
    /// option menu as a <see cref="ModBasicColorOption"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// using SMLHelper.Json;
    /// using SMLHelper.Options;
    /// 
    /// [Menu("My Options Menu")]
    /// public class Config : ConfigFile
    /// {
    ///     [ColorPicker("My Toggle")]
    ///     public Color MyToggle;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="ConfigFile"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColorAttribute : ModOptionAttribute
    {
        /// <summary>
        /// Which type of color picker to use.
        /// </summary>
        public bool Basic { get; set; } = true;

        /// <summary>
        /// Signifies the decorated <see cref="UnityEngine.Color"/> should be represented in the mod's option menu
        /// as a <see cref="ModBasicColorOption"/>.
        /// </summary>
        /// <param name="label">The label for the toggle.</param>
        public ColorAttribute(string label = null) : base(label) { }

        /// <summary>
        /// Signifies the decorated <see cref="UnityEngine.Color"/> should be represented in the mod's option menu
        /// as a <see cref="ModBasicColorOption"/>.
        /// </summary>
        public ColorAttribute() { }
    }
}