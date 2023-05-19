using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Nautilus.Options;

/// <summary>
/// 
/// </summary>
public static class ConfigEntryExtensions
{

    /// <summary>
    /// Converts a Bepinex ConfigEntry/<int/> into a ModSliderOption that will update the value when the slider changes.
    /// </summary>
    /// <param name="configEntry">A </param>
    /// <returns><see cref="ModToggleOption"/></returns>
    public static ModToggleOption ToModToggleOption(this ConfigEntry<bool> configEntry)
    {
        ModToggleOption optionItem = ModToggleOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}", 
            configEntry.Definition.Key, configEntry.Value, tooltip: configEntry.Description.Description);
        optionItem.OnChanged += (_, e) =>
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
    public static ModSliderOption ToModSliderOption(this ConfigEntry<int> configEntry, int? minValue = null, int? maxValue = null, int step = 1)
    {
        step = Mathf.Max(1, step);

        if (configEntry.Description.AcceptableValues is AcceptableValueRange<int> valueRange)
        {
            minValue ??= valueRange.MinValue;
            maxValue ??= valueRange.MaxValue;
        }

        ModSliderOption optionItem = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}", 
            configEntry.Definition.Key, minValue ?? 0, maxValue ?? 100, configEntry.Value,
            defaultValue: (int)configEntry.DefaultValue, step: step, tooltip: configEntry.Description.Description);
        optionItem.OnChanged += (_, e) =>
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
    public static ModSliderOption ToModSliderOption(this ConfigEntry<float> configEntry, float? minValue = null, float? maxValue = null, float step = 0.01f, string floatFormat = "{0:F2}")
    {
        step = Mathf.Max(0.0001f, step);

        if (configEntry.Description.AcceptableValues is AcceptableValueRange<float> valueRange)
        {
            minValue ??= valueRange.MinValue;
            maxValue ??= valueRange.MaxValue;
        }

        ModSliderOption optionItem = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}", 
            configEntry.Definition.Key, minValue ?? 0f, maxValue ?? 1f, configEntry.Value,
            defaultValue: (float)configEntry.DefaultValue, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItem.OnChanged += (_, e) =>
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
        ModSliderOption optionItemX = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_X", 
            configEntry.Definition.Key + " X", minValue, maxValue, configEntry.Value.x,
            defaultValue: ((Vector2)configEntry.DefaultValue).x,valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemX.OnChanged += (_, e) =>
        {
            configEntry.Value = new(e.Value, configEntry.Value.y);
        };
        optionItems.Add(optionItemX);
        ModSliderOption optionItemY = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Y", 
            configEntry.Definition.Key + " Y", minValue, maxValue, configEntry.Value.y,
            defaultValue: ((Vector2)configEntry.DefaultValue).y, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemY.OnChanged += (_, e) =>
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
        ModSliderOption optionItemX = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_X", 
            configEntry.Definition.Key + " X", minValue, maxValue, configEntry.Value.x,
            defaultValue: ((Vector3)configEntry.DefaultValue).x, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemX.OnChanged += (_, e) =>
        {
            configEntry.Value = new(e.Value, configEntry.Value.y, configEntry.Value.z);
        };
        optionItems.Add(optionItemX);
        ModSliderOption optionItemY = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Y", 
            configEntry.Definition.Key + " Y", minValue, maxValue, configEntry.Value.y,
            defaultValue: ((Vector3)configEntry.DefaultValue).y,valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemY.OnChanged += (_, e) =>
        {
            configEntry.Value = new(configEntry.Value.x, e.Value, configEntry.Value.z);
        };
        optionItems.Add(optionItemY);
        ModSliderOption optionItemZ = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Z", 
            configEntry.Definition.Key + " Z", minValue, maxValue, configEntry.Value.z,
            defaultValue: ((Vector3)configEntry.DefaultValue).z, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemZ.OnChanged += (_, e) =>
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
        ModSliderOption optionItemX = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_X", 
            configEntry.Definition.Key + " X", minValue, maxValue, configEntry.Value.x,
            defaultValue: ((Vector4)configEntry.DefaultValue).x, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemX.OnChanged += (_, e) =>
        {
            configEntry.Value = new(e.Value, configEntry.Value.y, configEntry.Value.z, configEntry.Value.w);
        };
        optionItems.Add(optionItemX);
        ModSliderOption optionItemY = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Y", 
            configEntry.Definition.Key + " Y", minValue, maxValue, configEntry.Value.y,
            defaultValue: ((Vector4)configEntry.DefaultValue).y, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemY.OnChanged += (_, e) =>
        {
            configEntry.Value = new(configEntry.Value.x, e.Value, configEntry.Value.z, configEntry.Value.w);
        };
        optionItems.Add(optionItemY);
        ModSliderOption optionItemZ = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_Z", 
            configEntry.Definition.Key + " Z", minValue, maxValue, configEntry.Value.z,
            defaultValue: ((Vector4)configEntry.DefaultValue).z, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemZ.OnChanged += (_, e) =>
        {
            configEntry.Value = new(configEntry.Value.x, configEntry.Value.y, e.Value, configEntry.Value.w);
        };
        optionItems.Add(optionItemZ);
        ModSliderOption optionItemW = ModSliderOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}_W", 
            configEntry.Definition.Key + " W", minValue, maxValue, configEntry.Value.w,
            defaultValue: ((Vector4)configEntry.DefaultValue).w, valueFormat: floatFormat, step: step, tooltip: configEntry.Description.Description);
        optionItemW.OnChanged += (_, e) =>
        {
            configEntry.Value = new(configEntry.Value.x, configEntry.Value.y, configEntry.Value.z, e.Value);
        };
        optionItems.Add(optionItemW);

        return optionItems;
    }

    /// <summary>
    /// Converts a Bepinex ConfigEntry/<Color/> into 4 ModSliderOption that will update the value when the slider changes.
    /// </summary>
    /// <param name="configEntry">A bepinex config entry</param>
    /// <param name="basic">Whether to use the basic or advanced color picker</param>
    /// <remarks>Does not support use of <see cref="AcceptableValueList{T}"/>.</remarks>
    /// <returns><see cref="ModColorOption"/></returns>
    public static ModColorOption ToModColorOption(this ConfigEntry<Color> configEntry, bool basic = false)
    {
        ModColorOption optionItem = ModColorOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}",
            configEntry.Definition.Key, configEntry.Value, advanced: !basic, tooltip: configEntry.Description.Description);
        optionItem.OnChanged += (_, e) =>
        {
            configEntry.Value = e.Value;
        };
        return optionItem;
    }

    /// <summary>
    /// Converts a Bepinex ConfigEntry/<KeyCode/> into a ModKeyBindOption that will update the value when the keybind changes.
    /// </summary>
    /// <param name="configEntry">A </param>
    /// <returns><see cref="ModKeybindOption"/></returns>
    public static ModKeybindOption ToModKeybindOption(this ConfigEntry<KeyCode> configEntry)
    {
        ModKeybindOption optionItem = ModKeybindOption.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}",
            configEntry.Definition.Key, GameInput.GetPrimaryDevice(), configEntry.Value, tooltip: configEntry.Description.Description);
        optionItem.OnChanged += (_, e) =>
        {
            configEntry.Value = e.Value;
        };

        return optionItem;
    }

    /// <summary>
    /// Converts an Enum ConfigEntry into a ModChoiceOption that will update the value when the choice changes.
    /// </summary>
    /// <param name="configEntry">A </param>
    /// <param name="options">Array of valid options if not using the whole Enum</param>
    /// <returns><see cref="ModKeybindOption"/></returns>
    public static ModChoiceOption<T> ToModChoiceOption<T>(this ConfigEntry<T> configEntry, IEnumerable<T> options = null) where T : Enum
    {
        T[] viableValues = options?.ToArray<T>() ?? (T[])Enum.GetValues(typeof(T));

        ModChoiceOption<T> optionItem = ModChoiceOption<T>.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}",
            configEntry.Definition.Key, viableValues, configEntry.Value ?? (T)configEntry.DefaultValue ?? viableValues[0], tooltip: configEntry.Description.Description);
        optionItem.OnChanged += (_, e) =>
        {
            configEntry.Value = (T)Enum.Parse(typeof(T), e.Value.ToString());
        };

        return optionItem;
    }


    /// <summary>
    /// Converts a ConfigEntry into a ModChoiceOption that will update the value when the choice changes.
    /// </summary>
    /// <param name="configEntry">A </param>
    /// <param name="options"></param>
    /// <returns><see cref="ModKeybindOption"/></returns>
    public static ModChoiceOption<T> ToModChoiceOption<T>(this ConfigEntry<T> configEntry, T[] options = null) where T : IEquatable<T>
    {
        ModChoiceOption<T> optionItem;

        if(configEntry.Description.AcceptableValues is AcceptableValueList<T> valueList)
            options = valueList.AcceptableValues;

        if(options == null)
            throw new ArgumentException("Could not get values from ConfigEntry");

        optionItem = ModChoiceOption<T>.Create($"{configEntry.Definition.Section}_{configEntry.Definition.Key}",
            configEntry.Definition.Key, options, configEntry.Value ?? (T) configEntry.DefaultValue ?? options[0], tooltip: configEntry.Description.Description);

        optionItem.OnChanged += (_, e) =>
        {
            configEntry.Value = (T)e.Value; 
        };

        return optionItem;
    }
}