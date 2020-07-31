using System;

namespace SMLHelper.V2.Options
{
    /// <summary>
    /// Attribute used to signify the specified <see cref="float"/>, <see cref="double"/> or <see cref="int"/> should be represented
    /// in the mod's option menu as a <see cref="ModSliderOption"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// 
    /// [Menu("My Options Menu")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My slider"), Slider(0, 50, DefaultValue = 25, Format = "{0:F2}")]
    ///     public float MySlider;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="Json.ConfigFile"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SliderAttribute : Attribute
    {
        /// <summary>
        /// The minimum value of the slider.
        /// </summary>
        public float Min { get; set; } = 0;

        /// <summary>
        /// The maximum value of the slider.
        /// </summary>
        public float Max { get; set; } = 100;

        /// <summary>
        /// The default value of the slider.
        /// </summary>
        public float DefaultValue { get; set; }

        /// <summary>
        /// The format to use when displaying the value, e.g. "{0:F2}" or "{0:F0} %"
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Signifies the specified <see cref="float"/>, <see cref="double"/> or <see cref="int"/> should be represented in the mod's
        /// options menu as a <see cref="ModSliderOption"/>.
        /// </summary>
        /// <param name="min">The minimum value of the slider.</param>
        /// <param name="max">The maximum value of the slider.</param>
        public SliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }

        internal SliderAttribute() { }
    }
}
