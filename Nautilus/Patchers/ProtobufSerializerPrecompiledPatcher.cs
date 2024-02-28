using System;
using System.Reflection;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using ProtoBuf;

namespace Nautilus.Patchers;

/// <summary>
/// Replaces <see cref="ProtobufSerializerPrecompiled.Serialize"/> and <see cref="ProtobufSerializerPrecompiled.Deserialize"/>
/// by a dictionary lookup instead of an if tree. Also enables mods from serializing classes using the Protobuf serializer.
/// </summary>
internal static class ProtobufSerializerPrecompiledPatcher
{
    internal static MethodInfo serializeMethodInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(nameof(ProtobufSerializerPrecompiled.Serialize));
    internal static MethodInfo deserializeMethodInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(nameof(ProtobufSerializerPrecompiled.Deserialize));

    internal static void Patch(Harmony harmony)
    {
        HarmonyMethod optimizedSerialize = new(AccessTools.Method(typeof(ProtobufSerializerPrecompiledPatcher), nameof(OptimizedSerializePrefix)));
        harmony.Patch(serializeMethodInfo, prefix: optimizedSerialize);

        HarmonyMethod optimizedDeserialize = new(AccessTools.Method(typeof(ProtobufSerializerPrecompiledPatcher), nameof(OptimizedDeserializePrefix)));
        harmony.Patch(deserializeMethodInfo, prefix: optimizedDeserialize);        
    }

    /// <summary>
    /// Prefix to replace <see cref="ProtobufSerializerPrecompiled.Serialize"/>'s if tree by a dictionary lookup
    /// </summary>
    private static bool OptimizedSerializePrefix(int num, object obj, ProtoWriter writer, ProtobufSerializerPrecompiled __instance)
    {
        if (ProtobufSerializerHandler.SerializerEntries.TryGetValue(obj.GetType(), out ProtobufSerializerHandler.SerializerEntry entry))
        {
            // Always provide __instance but it's only used for Subnautica's known types as custom types methods must be static
            entry.SerializeInfo.Invoke(__instance, new object[] { Convert.ChangeType(obj, entry.Type), num, writer });
        }
        return false;
    }

    /// <summary>
    /// Prefix to replace <see cref="ProtobufSerializerPrecompiled.Deserialize"/>'s if tree by a dictionary lookup
    /// </summary>
    private static bool OptimizedDeserializePrefix(int num, object obj, ProtoReader reader, ProtobufSerializerPrecompiled __instance, ref object __result)
    {
        if (ProtobufSerializerHandler.SerializerEntries.TryGetValue(obj.GetType(), out ProtobufSerializerHandler.SerializerEntry entry))
        {
            __result = entry.DeserializeInfo.Invoke(__instance, new object[] { Convert.ChangeType(obj, entry.Type), reader });
        }
        return false;
    }
}
