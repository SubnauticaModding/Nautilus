using System;
using UnityEngine;
#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
namespace SMLHelper.V2.Json.Converters
{
    /// <summary>
    /// A Vector3 json converter that simplifies the Vector3 to only x,y,z serialization.
    /// </summary>
    public class Vector3Converter : JsonConverter
    {
        /// <summary>
        /// A method that determines when this converter should process.
        /// </summary>
        /// <param name="objectType">the current object type</param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }

        /// <summary>
        /// A method that tells Newtonsoft how to Serialize the current object.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector3 = (Vector3)value;

            serializer.Serialize(writer, new Vector3Json(vector3.x, vector3.y, vector3.z));
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
            var v = (Vector3Json)serializer.Deserialize(reader, typeof(Vector3Json));

            return new Vector3(v.x, v.y, v.z);
        }
    }

    internal record Vector3Json(float x, float y, float z);
}
