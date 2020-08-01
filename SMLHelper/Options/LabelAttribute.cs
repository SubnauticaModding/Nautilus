namespace SMLHelper.V2.Options
{
    using Json;
    using System;

    /// <summary>
    /// Attribute used to specify the label to display for the given field in the mod's options menu.
    /// </summary>
    /// <remarks>
    /// Can also be used to manually specify the order in which to display options in the menu, or to specify the Id.
    /// </remarks>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// 
    /// [Menu("My Options Menu")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My Cool Slider", Id = "myCoolSlider")]
    ///     public float MyCoolSlider;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="ConfigFile"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LabelAttribute : Attribute
    {
        /// <summary>
        /// The label to use when displaying the field in the mod's options menu.
        /// </summary>
        public string Label { get; internal set; }

        /// <summary>
        /// The Id to be used for the field in the mod's option menu. If none is specified, one will be automatically generated when
        /// your <see cref="ConfigFile"/> is registered to the <see cref="Handlers.OptionsPanelHandler"/>. This means it will
        /// change every time the game is launched, but is guaranteed to be unique. If you would like to specify an Id to use for
        /// internal comparisons, you can do so here.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString().Replace('-', '_');

        /// <summary>
        /// The order in which to display fields in the mod's option menu, in ascending order. If none is specified, the order will be
        /// automatically set.
        /// </summary>
        public int Order { get; set; } = i++;
        private static int i = 0;

        /// <summary>
        /// Specifies the label to display for the given field in the mod's option menu.
        /// </summary>
        public LabelAttribute(string label = null)
        {
            Label = label;
        }
    }
}
