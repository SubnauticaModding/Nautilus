namespace SMLHelper.Options
{
    using System.Collections.Generic;
    using BepInEx.Configuration;
    using UnityEngine;

    /// <summary>
    /// 
    /// </summary>
    public static class ConfigEntryExtensions
    {

        /// <summary>
        /// Converts a Bepinex ConfigEntry/<int/> into a ModSliderOption that will update the value when the slider changes.
        /// </summary>
        /// <param name="configEntry">A </param>
        /// <returns><see cref="ModChoiceOption"/></returns>
        public static ModToggleOption ToModToggleOption(this ConfigEntry<bool> configEntry)
        {
            ModToggleOption optionItem = ModToggleOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}", 
                configEntry.Definition.Key, configEntry.Value, tooltip: configEntry.Description.Description);
            optionItem.OnChanged += (s, e) =>
            {
                configEntry.Value = e.Value;
            };
            return optionItem;
        }

        /// <summary>
        /// Converts a Bepinex ConfigEntry/<int/> into a ModSliderOption that will update the value when the slider changes.
        /// </summary>
        /// <param name="configEntry">A </param>
        /// <param name="minValue">Sets the lowest allowed value of the slider. default: 0 </param>
        /// <param name="maxValue">Sets the highest allowed value of the slider. default: 100</param>
        /// <param name="step">The snapping value of the slider. Minimum value: 1, Default value: 1</param>
        /// <returns><see cref="ModSliderOption"/></returns>
        public static ModSliderOption ToModSliderOption(this ConfigEntry<int> configEntry, int minValue = 0, int maxValue = 100, int step = 1)
        {
            step = Mathf.Max(1, step);
            ModSliderOption optionItem = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}", 
                configEntry.Definition.Key, minValue, maxValue, configEntry.Value, step: step, tooltip: configEntry.Description.Description);
            optionItem.OnChanged += (s, e) =>
            {
                configEntry.Value = (int)e.Value;
            };
            return optionItem;
        }

        /// <summary>
        /// Converts a Bepinex ConfigEntry/<float/> into a ModSliderOption that will update the value when the slider changes.
        /// </summary>
        /// <param name="configEntry">A </param>
        /// <param name="minValue">Sets the lowest allowed value of the slider. default: 0f</param>
        /// <param name="maxValue">Sets the highest allowed value of the slider. default: 1f</param>
        /// <param name="step">The snapping value of the slider. Minimum value: 0.0001f, Default  0.01f</param>
        /// <param name="floatFormat">The formatting string used on the float value. Default value: "{0:F2}" shows 2 decimals</param>
        /// <returns><see cref="ModSliderOption"/></returns>
        public static ModSliderOption ToModSliderOption(this ConfigEntry<float> configEntry, float minValue = 0f, float maxValue = 1f, float step = 0.01f, string floatFormat = "{0:F2}")
        {
            step = Mathf.Max(0.0001f, step);
            ModSliderOption optionItem = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}", 
                configEntry.Definition.Key, minValue, maxValue, configEntry.Value, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItem.OnChanged += (s, e) =>
            {
                configEntry.Value = e.Value;
            };
            return optionItem;
        }

        /// <summary>
        /// Converts a Bepinex ConfigEntry/<Vector2/> into 2 ModSliderOption that will update the value when the slider changes.
        /// </summary>
        /// <param name="configEntry">A </param>
        /// <param name="minValue">Sets the lowest allowed value of the slider. default: 0f</param>
        /// <param name="maxValue">Sets the highest allowed value of the slider. default: 1f</param>
        /// <param name="step">The snapping value of the slider. Minimum value: 0.01f</param>
        /// <param name="floatFormat">The formatting string used on the float value. Default value: "{0:F2}" shows 2 decimals</param>
        /// <returns><see cref="ModSliderOption"/></returns>
        public static List<ModSliderOption> ToModSliderOptions(this ConfigEntry<Vector2> configEntry, float minValue, float maxValue, float step, string floatFormat = "{0:F2}")
        {
            var optionItems = new List<ModSliderOption>();
            step = Mathf.Max(0.01f, step);
            ModSliderOption optionItemX = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_X", 
                configEntry.Definition.Key + " X", minValue, maxValue, configEntry.Value.x, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemX.OnChanged += (s, e) =>
            {
                configEntry.Value = new(e.Value, configEntry.Value.y);
            };
            optionItems.Add(optionItemX);
            ModSliderOption optionItemY = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Y", 
                configEntry.Definition.Key + " Y", minValue, maxValue, configEntry.Value.y, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemY.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.x, e.Value);
            };
            optionItems.Add(optionItemY);

            return optionItems;
        }

        /// <summary>
        /// Converts a Bepinex ConfigEntry/<Vector3/> into 3 ModSliderOption that will update the value when the slider changes.
        /// </summary>
        /// <param name="configEntry">A </param>
        /// <param name="minValue">Sets the lowest allowed value of the slider. default: 0f</param>
        /// <param name="maxValue">Sets the highest allowed value of the slider. default: 1f</param>
        /// <param name="step">The snapping value of the slider. Minimum value: 0.01f</param>
        /// <param name="floatFormat">The formatting string used on the float value. Default value: "{0:F2}" shows 2 decimals</param>
        /// <returns><see cref="ModSliderOption"/></returns>
        public static List<ModSliderOption> ToModSliderOptions(this ConfigEntry<Vector3> configEntry, float minValue, float maxValue, float step, string floatFormat = "{0:F2}")
        {
            var optionItems = new List<ModSliderOption>();
            step = Mathf.Max(0.01f, step);
            ModSliderOption optionItemX = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_X", 
                configEntry.Definition.Key + " X", minValue, maxValue, configEntry.Value.x, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemX.OnChanged += (s, e) =>
            {
                configEntry.Value = new(e.Value, configEntry.Value.y, configEntry.Value.z);
            };
            optionItems.Add(optionItemX);
            ModSliderOption optionItemY = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Y", 
                configEntry.Definition.Key + " Y", minValue, maxValue, configEntry.Value.y, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemY.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.x, e.Value, configEntry.Value.z);
            };
            optionItems.Add(optionItemY);
            ModSliderOption optionItemZ = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Z", 
                configEntry.Definition.Key + " Z", minValue, maxValue, configEntry.Value.z, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemZ.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.x, configEntry.Value.y, e.Value);
            };
            optionItems.Add(optionItemZ);

            return optionItems;
        }

        /// <summary>
        /// Converts a Bepinex ConfigEntry/<Vector4/> into 4 ModSliderOption that will update the value when the slider changes.
        /// </summary>
        /// <param name="configEntry">A </param>
        /// <param name="minValue">Sets the lowest allowed value of the slider. default: 0f</param>
        /// <param name="maxValue">Sets the highest allowed value of the slider. default: 1f</param>
        /// <param name="step">The snapping value of the slider. Minimum value: 0.01f</param>
        /// <param name="floatFormat">The formatting string used on the float value. Default value: "{0:F2}" shows 2 decimals</param>
        /// <returns><see cref="ModSliderOption"/></returns>
        public static List<ModSliderOption> ToModSliderOptions(this ConfigEntry<Vector4> configEntry, float minValue, float maxValue, float step, string floatFormat = "{0:F2}")
        {
            var optionItems = new List<ModSliderOption>();
            step = Mathf.Max(0.01f, step);
            ModSliderOption optionItemX = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_X", 
                configEntry.Definition.Key + " X", minValue, maxValue, configEntry.Value.x, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemX.OnChanged += (s, e) =>
            {
                configEntry.Value = new(e.Value, configEntry.Value.y, configEntry.Value.z, configEntry.Value.w);
            };
            optionItems.Add(optionItemX);
            ModSliderOption optionItemY = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Y", 
                configEntry.Definition.Key + " Y", minValue, maxValue, configEntry.Value.y, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemY.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.x, e.Value, configEntry.Value.z, configEntry.Value.w);
            };
            optionItems.Add(optionItemY);
            ModSliderOption optionItemZ = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Z", 
                configEntry.Definition.Key + " Z", minValue, maxValue, configEntry.Value.z, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemZ.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.x, configEntry.Value.y, e.Value, configEntry.Value.w);
            };
            optionItems.Add(optionItemZ);
            ModSliderOption optionItemW = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_W", 
                configEntry.Definition.Key + " W", minValue, maxValue, configEntry.Value.w, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
            optionItemW.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.x, configEntry.Value.y, configEntry.Value.z, e.Value);
            };
            optionItems.Add(optionItemW);

            return optionItems;
        }

        /// <summary>
        /// Converts a Bepinex ConfigEntry/<Color/> into 4 ModSliderOption that will update the value when the slider changes.
        /// </summary>
        /// <param name="configEntry">A </param>
        /// <returns><see cref="ModSliderOption"/></returns>
        public static List<ModSliderOption> ToModSliderOptions(this ConfigEntry<Color> configEntry)
        {
            var optionItems = new List<ModSliderOption>();
            ModSliderOption optionItemR = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_R", 
                configEntry.Definition.Key + " R", 0f, 1f, configEntry.Value.r, valueFormat: "{0:F2}", step: 0.01f, tooltip: configEntry.Description.Description);
            optionItemR.OnChanged += (s, e) =>
            {
                configEntry.Value = new(e.Value, configEntry.Value.g, configEntry.Value.b, configEntry.Value.a);
            };
            optionItems.Add(optionItemR);
            ModSliderOption optionItemY = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_G", 
                configEntry.Definition.Key + " G", 0f, 1f, configEntry.Value.g, valueFormat: "{0:F2}", step: 0.01f, tooltip: configEntry.Description.Description);
            optionItemY.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.r, e.Value, configEntry.Value.b, configEntry.Value.a);
            };
            optionItems.Add(optionItemY);
            ModSliderOption optionItemZ = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_B", 
                configEntry.Definition.Key + " B", 0f, 1f, configEntry.Value.b, valueFormat: "{0:F2}", step: 0.01f, tooltip: configEntry.Description.Description);
            optionItemZ.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.r, configEntry.Value.g, e.Value, configEntry.Value.a);
            };
            optionItems.Add(optionItemZ);
            ModSliderOption optionItemW = ModSliderOption.Factory($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_A", 
                configEntry.Definition.Key + " A", 0f, 1f, configEntry.Value.a, valueFormat: "{0:F2}", step: 0.01f, tooltip: configEntry.Description.Description);
            optionItemW.OnChanged += (s, e) =>
            {
                configEntry.Value = new(configEntry.Value.r, configEntry.Value.g, configEntry.Value.b, e.Value);
            };
            optionItems.Add(optionItemW);

            return optionItems;
        }
    }
}
