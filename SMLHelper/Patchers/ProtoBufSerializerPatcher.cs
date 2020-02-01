namespace SMLHelper.V2.Patchers
{
    using Abstract;
    using Harmony;
    using System;
    using System.IO;
    using System.Reflection;
    using Utility;

    internal class ProtoBufSerializerPatcher : IPatch
    {
        
        public void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(typeof(ProtobufSerializer).GetMethod("Serialize", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(ProtoBufSerializerPatcher), nameof(ProtoBufSerializerPatcher.Prefix_Serialize)));
            harmony.Patch(typeof(ProtobufSerializer).GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(ProtoBufSerializerPatcher), nameof(ProtoBufSerializerPatcher.Prefix_Deserialize)));
        }

        private static bool Prefix_Serialize(Stream stream, object source, Type type)
        {
            if (SMLProtobufSerializer.CanSerialize(type))
            {
                SMLProtobufSerializer.Serialize(stream, source);
                return false;
            }
            return true;
        }

        private static bool Prefix_Deserialize(Stream stream, object target, Type type)
        {
            if (SMLProtobufSerializer.CanSerialize(type))
            {
                SMLProtobufSerializer.Deserialize(stream, target, type);
                return false;
            }
            return true;
        }
    }
}
