using System;
using BepInEx.Logging;
using Nautilus.Extensions;
using Nautilus.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace Nautilus.Json.Converters;

/// <summary>
/// A <see cref="JsonConverter"/> for handling <see cref="KeyCode"/>s.
/// </summary>
public class KeyCodeConverter : JsonConverter
{
    /// <summary>
    /// The method for writing the <paramref name="value"/> data to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        KeyCode keyCode = (KeyCode)value;
        writer.WriteValue(keyCode.KeyCodeToString());
    }

    /// <summary>
    /// The method for reading the <see cref="KeyCode"/> data from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object ReadJson(JsonReader reader, Type objectType,
        object existingValue, JsonSerializer serializer)
    {
        string s = (string)reader.Value;
        return StringToKeyCode(s);
    }

    /// <summary>
    /// The method for determining whether the current <paramref name="objectType"/> can be processed by this
    /// <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(KeyCode);
    }
    
    private static KeyCode StringToKeyCode(string s)
    {
        switch (s)
        {
            case "0":
                return KeyCode.Alpha0;
            case "1":
                return KeyCode.Alpha1;
            case "2":
                return KeyCode.Alpha2;
            case "3":
                return KeyCode.Alpha3;
            case "4":
                return KeyCode.Alpha4;
            case "5":
                return KeyCode.Alpha5;
            case "6":
                return KeyCode.Alpha6;
            case "7":
                return KeyCode.Alpha7;
            case "8":
                return KeyCode.Alpha8;
            case "9":
                return KeyCode.Alpha9;
            case "MouseButtonLeft":
                return KeyCode.Mouse0;
            case "MouseButtonRight":
                return KeyCode.Mouse1;
            case "MouseButtonMiddle":
                return KeyCode.Mouse2;
            case "ControllerButtonA":
                return KeyCode.JoystickButton0;
            case "ControllerButtonB":
                return KeyCode.JoystickButton1;
            case "ControllerButtonX":
                return KeyCode.JoystickButton2;
            case "ControllerButtonY":
                return KeyCode.JoystickButton3;
            case "ControllerButtonLeftBumper":
                return KeyCode.JoystickButton4;
            case "ControllerButtonRightBumper":
                return KeyCode.JoystickButton5;
            case "ControllerButtonBack":
                return KeyCode.JoystickButton6;
            case "ControllerButtonHome":
                return KeyCode.JoystickButton7;
            case "ControllerButtonLeftStick":
                return KeyCode.JoystickButton8;
            case "ControllerButtonRightStick":
                return KeyCode.JoystickButton9;
            default:
                try
                {
                    return (KeyCode)Enum.Parse(typeof(KeyCode), s);
                }
                catch (Exception)
                {
                    InternalLogger.Log($"Failed to parse {s} as a valid KeyCode!", LogLevel.Error);
                    return 0;
                }
        }
    }
}