namespace SMLHelper.V2.Utility.JsonConverters
{
    using System;
    using Oculus.Newtonsoft.Json;
    using UnityEngine;

    internal class KeyCodeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var keyCode = (KeyCode)value;
            writer.WriteValue(KeyCodeUtils.KeyCodeToString(keyCode));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value;
            return KeyCodeUtils.StringToKeyCode(s);
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(KeyCode);
    }
}
