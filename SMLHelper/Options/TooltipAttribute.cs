namespace SMLHelper.V2.Options
{
    using Json;
    using System;

    /// <summary>
    /// Attribute used to signify a tooltip to display whenever the user hovers their mouse over the <see cref="ModOption"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// 
    /// [Menu("My Options Menu")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My Cool Slider", Id = "myCoolSlider"), Tooltip("This slider is really cool")]
    ///     public float MyCoolSlider;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="LabelAttribute"/>
    /// <seealso cref="ConfigFile"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class TooltipAttribute : Attribute
    {
        /// <summary>
        /// The tooltip to display whenever the user hovers their mouse over the <see cref="ModOption"/>.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// Signifies a tooltip to display whenever the user hovers their mouse over the <see cref="ModOption"/>.
        /// </summary>
        /// <param name="tooltip">The tooltip to display whenever the user hovers their mouse over the
        /// <see cref="ModOption"/>.</param>
        public TooltipAttribute(string tooltip) => Tooltip = tooltip;
    }
}
