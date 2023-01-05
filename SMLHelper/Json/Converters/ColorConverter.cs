using Newtonsoft.Json;
using System;
using UnityEngine;

namespace SMLHelper.Json.Converters
{
    internal class ColorConverter : JsonConverter
    {
        /// <summary>
        /// A method that determines when this converter should process.
        /// </summary>
        /// <param name="objectType">the current object type</param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        /// <summary>
        /// A method that tells Newtonsoft how to Serialize the current object.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Color color = (Color)value;
            serializer.Serialize(writer, (ColorJson)color);
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
            return (Color)serializer.Deserialize<ColorJson>(reader);
        }
    }

    internal record ColorJson(float R, float G, float B, float A)
    {
        public static explicit operator Color(ColorJson c) => new(c.R, c.G, c.B, c.A);
        public static explicit operator ColorJson(Color c) => new(c.r, c.g, c.b, c.a);
    }
}