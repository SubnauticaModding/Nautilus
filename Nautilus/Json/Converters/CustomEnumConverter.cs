using System;
using Newtonsoft.Json;

namespace Nautilus.Json.Converters;

/// <summary>
/// An Enum json converter that supports custom enum conversions.
/// </summary>
public class CustomEnumConverter : JsonConverter
{
    /// <summary>
    /// A method that determines when this converter should process.
    /// </summary>
    /// <param name="objectType">the current object type</param>
    /// <returns></returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsEnum;
    }

    /// <summary>
    /// A method that tells Newtonsoft how to Serialize the current object.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var stringifyValue = Enum.GetName(value.GetType(), value);
        serializer.Serialize(writer, stringifyValue);
    }

    /// <summary>
    /// A method that tells Newtonsoft how to Deserialize and read the current object.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var enumValue = (string)serializer.Deserialize(reader, typeof(string));

            return Enum.Parse(objectType, enumValue!);
        }

        if (reader.TokenType != JsonToken.Null)
        {
            var enumValue = serializer.Deserialize(reader, objectType.GetEnumUnderlyingType());
            return Enum.ToObject(objectType, enumValue!);
        }

        throw JsonSerializationException.Create(reader, $"Error converting value {reader.Value} to type {objectType}");
    }
}