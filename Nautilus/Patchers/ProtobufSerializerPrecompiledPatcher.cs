using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nautilus.Utility;
using ProtoBuf;

namespace Nautilus.Patchers;

/// <summary>
/// Replaces <see cref="ProtobufSerializerPrecompiled.Serialize"/> and <see cref="ProtobufSerializerPrecompiled.Deserialize"/>
/// by a dictionary lookup instead of an if tree. Also enables mods from serializing classes using the Protobuf serializer.
/// </summary>
internal static class ProtobufSerializerPrecompiledPatcher
{
    internal static Dictionary<int, SerializerEntry> SerializerEntries;

    internal static void Patch(Harmony harmony)
    {
        MethodInfo serializeMethodInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(nameof(ProtobufSerializerPrecompiled.Serialize));
        HarmonyMethod optimizedSerialize = new(AccessTools.Method(typeof(ProtobufSerializerPrecompiledPatcher), nameof(OptimizedSerializePrefix)));
        harmony.Patch(serializeMethodInfo, prefix: optimizedSerialize);

        MethodInfo deserializeMethodInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(nameof(ProtobufSerializerPrecompiled.Deserialize));
        HarmonyMethod optimizedDeserialize = new(AccessTools.Method(typeof(ProtobufSerializerPrecompiledPatcher), nameof(OptimizedDeserializePrefix)));
        harmony.Patch(deserializeMethodInfo, prefix: optimizedDeserialize);

        InitializeSerializerEntries(serializeMethodInfo);
    }

    /// <summary>
    /// Initialize <see cref="SerializerEntries"/> with Subnautica's known types
    /// </summary>
    private static void InitializeSerializerEntries(MethodInfo serializeMethodInfo)
    {
        SerializerEntries = new();
        
        // We loop through the IL code instructions to find the exact match between a typeId and the method it calls
        List<KeyValuePair<OpCode, object>> serializeMethodInstructions = PatchProcessor.ReadMethodBody(serializeMethodInfo).ToList();
        for (int i = 0; i < serializeMethodInstructions.Count; i++)
        {
            KeyValuePair<OpCode, object> instruction = serializeMethodInstructions[i];
            if (instruction.Key == OpCodes.Call)
            {
                // The castclass instruction is always 3 steps before the call instruction
                Type objectType = (Type)serializeMethodInstructions[i - 3].Value;
                // The ldc.i4 instruction is always 2 steps before the call instruction
                int associatedTypeId = (int)serializeMethodInstructions[i - 2].Value;
                // To get the methodName we remove the 5 first letters and we cut before the opening parenthese
                string serializeMethodName = instruction.Value.ToString()[5..].Split('(')[0];
                // The deserialize method name will be the same but with Deserialize[...] instead of Serialize[...]
                string deserializeMethodName = serializeMethodName.Replace("S", "Des");

                SerializerEntries.Add(associatedTypeId, new(objectType, serializeMethodName, deserializeMethodName));
            }
        }
    }

    /// <summary>
    /// Registers a type as serializable for Protobuf.
    /// </summary>
    /// <remarks>
    /// This is the only method from <see cref="ProtobufSerializerPrecompiledPatcher"/> which should be used by mods.
    /// </remarks>
    /// <typeparam name="T">Type of the serializable type</typeparam>
    /// <param name="typeId">
    /// An arbitrary unique type id which shouldn't be already used by Subnautica nor by any other mod.
    /// Look through <see cref="ProtobufSerializerPrecompiled"/> to ensure the id is not already taken.
    /// </param>
    /// <param name="serializeMethod">
    /// A reference to the serialize method.
    /// NB: Take inspiration from <see cref="ProtobufSerializerPrecompiled.Serialize11492366"/> to understand how make one.
    /// </param>
    /// <param name="deserializeMethod">
    /// A reference to the serialize method.
    /// NB: Take inspiration from <see cref="ProtobufSerializerPrecompiled.Deserialize11492366"/> to understand how make one.
    /// </param>
    public static void RegisterSerializableType<T>(int typeId, Action<T, int, ProtoWriter> serializeMethod, Func<T, ProtoReader, T> deserializeMethod)
    {
        if (SerializerEntries.ContainsKey(typeId))
        {
            throw new DuplicateTypeIdException(typeId, typeof(T));
        }
        ProtobufSerializerPrecompiled.knownTypes.Add(typeof(T), typeId);
        SerializerEntries.Add(typeId, new(serializeMethod.Method, deserializeMethod.Method, typeof(T)));
    }

    /// <summary>
    /// Prefix to replace <see cref="ProtobufSerializerPrecompiled.Serialize"/>'s if tree by a dictionary lookup
    /// </summary>
    private static bool OptimizedSerializePrefix(int num, object obj, ProtoWriter writer, ProtobufSerializerPrecompiled __instance)
    {
        if (SerializerEntries.TryGetValue(num, out SerializerEntry entry))
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
        if (SerializerEntries.TryGetValue(num, out SerializerEntry entry))
        {
            __result = entry.DeserializeInfo.Invoke(__instance, new object[] { Convert.ChangeType(obj, entry.Type), reader });
        }
        return false;
    }

    /// <summary>
    /// Data structure which holds the method data for serialization and deserialization.
    /// </summary>
    public class SerializerEntry
    {
        internal MethodInfo SerializeInfo;
        internal MethodInfo DeserializeInfo;
        internal Type Type;

        /// <summary>
        /// Constructor to be commonly used when adding a new serializable type.
        /// </summary>
        /// <param name="serializeInfo">A static method ran when Subnautica serializes the <see cref="Type"/></param>
        /// <param name="deserializeInfo">A static method ran when Subnautica deserializes the <see cref="Type"/></param>
        /// <param name="type">The type to be serializable</param>
        public SerializerEntry(MethodInfo serializeInfo, MethodInfo deserializeInfo, Type type)
        {
            if (!serializeInfo.IsStatic)
            {
                throw new ArgumentException($"The provided serialize method '{serializeInfo.Name}' should be static for type '{type}'");
            }
            if (!deserializeInfo.IsStatic)
            {
                throw new ArgumentException($"The provided deserialize method '{deserializeInfo.Name}' should be static for type '{type}'");

            }
            SerializeInfo = serializeInfo;
            DeserializeInfo = deserializeInfo;
            Type = type;
        }

        /// <summary>
        /// Constructor to be used for Subnautica's default known types only.
        /// </summary>
        public SerializerEntry(Type type, string serializeMethodName, string deserializeMethodName)
        {
            SerializeInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(serializeMethodName);
            DeserializeInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(deserializeMethodName);
            Type = type;
        }
    }

    /// <summary>
    /// The exception that is thrown when a <see cref="SerializerEntry"/> is attempted to be added when an existing one of the same type already exists.
    /// </summary>
    public class DuplicateTypeIdException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateTypeIdException"/> class with default properties.
        /// </summary>
        /// <param name="typeId">Type id of the already registered serializer entry.</param>
        /// <param name="type">Type of the requested</param>
        public DuplicateTypeIdException(int typeId, Type type) : base
            ($"Cannot register serializable type '{type}' with id '{typeId}' because it is already used by either Subnautica or another instance.")
        {

        }
    }
}
