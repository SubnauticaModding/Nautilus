using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nautilus.Patchers;
using Nautilus.Utility;
using ProtoBuf;
using UnityEngine;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class responsible for implementing Protobuf serialization to new custom types.
/// </summary>
public static class ProtobufSerializerHandler
{
    internal static Dictionary<Type, SerializerEntry> SerializerEntries;

    static ProtobufSerializerHandler()
    {
        InitializeSerializerEntries(ProtobufSerializerPrecompiledPatcher.serializeMethodInfo);
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
                Type objectType = (Type) serializeMethodInstructions[i - 3].Value;
                // The ldc.i4 instruction is always 2 steps before the call instruction
                int associatedTypeId = (int) serializeMethodInstructions[i - 2].Value;
                // To get the methodName we remove the 5 first letters and we cut before the opening parenthese
                string serializeMethodName = instruction.Value.ToString()[5..].Split('(')[0];
                // The deserialize method name will be the same but with Deserialize[...] instead of Serialize[...]
                string deserializeMethodName = serializeMethodName.Replace("S", "Des");

                SerializerEntries.Add(objectType, new(objectType, associatedTypeId, serializeMethodName, deserializeMethodName));
            }
        }
    }

    /// <summary>
    /// Registers a type as serializable for Protobuf.
    /// </summary>
    /// <remarks>
    /// This is the only method from <see cref="ProtobufSerializerPrecompiledPatcher"/> which should be used by mods.
    /// </remarks>
    /// <param name="serializableType">Type of the serializable type</param>
    private static SerializerEntry RegisterSerializableType(Type serializableType)
    {
        ProtobufSerializerPrecompiled.knownTypes.Add(serializableType, 0);

        Dictionary<int, FieldInfo> serializedFieldsByTag = GetSerializedFieldsByTag(serializableType);
        SerializerEntry serializerEntry = new(serializableType, ((Delegate) Serialize).Method, ((Delegate) Deserialize).Method, serializedFieldsByTag);
        SerializerEntries.Add(serializableType, serializerEntry);
        
        InternalLogger.Info($"Registered serializable type {serializableType}");
        return serializerEntry;
    }

    /// <summary>
    /// Searches for all types marked with attribute <see cref="ProtoContractAttribute"/> in the executing assembly,
    /// and registers them as serializable by <see cref="RegisterSerializableType"/>.
    /// </summary>
    public static void RegisterAllSerializableTypesInAssembly()
    {
        List<SerializerEntry> serializerEntries = new();
        foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
        {
            foreach (Attribute attribute in type.GetCustomAttributes())
            {
                if (attribute is ProtoContractAttribute)
                {
                    serializerEntries.Add(RegisterSerializableType(type));
                    break;
                }
            }
        }

        foreach (SerializerEntry serializerEntry in serializerEntries)
        {
            // If the type inherits (whatever at which depth) a serializable class, make sure to notice it
            Type derivedType = serializerEntry.Type.BaseType;
            while (derivedType != null && derivedType != typeof(MonoBehaviour))
            {
                if (SerializerEntries.TryGetValue(derivedType, out SerializerEntry inheritedSerializerEntry))
                {
                    InternalLogger.Log($"Found derived type for {serializerEntry.Type}: {derivedType}");
                    serializerEntry.FirstInheritedSerializerEntry = inheritedSerializerEntry;
                    break;
                }
                derivedType = derivedType.BaseType;
            }
        }
    }

    /// <summary>
    /// Automatically serializes a registered type from <see cref="SerializerEntries"/> based on its type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Should only be called by <see cref="ProtobufSerializerPrecompiledPatcher.OptimizedSerializePrefix"/>
    /// </remarks>
    public static void Serialize(object instance, int objId, ProtoWriter writer)
    {
        if (SerializerEntries.TryGetValue(instance.GetType(), out SerializerEntry serializerEntry))
        {
            SerializeEntry(serializerEntry, instance, writer);
        }
    }

    private static void SerializeEntry(SerializerEntry serializerEntry, object instance, ProtoWriter writer)
    {
        if (serializerEntry.FirstInheritedSerializerEntry is SerializerEntry inheritedEntry)
        {
            if (inheritedEntry.SerializedFieldsByTag != null)
            {
                SerializeEntry(inheritedEntry, instance, writer);
            }
            else
            {
                inheritedEntry.SerializeInfo.Invoke(writer.model, new object[] { instance, inheritedEntry.TypeId, writer });
            }
        }

        foreach (KeyValuePair<int, FieldInfo> pair in serializerEntry.SerializedFieldsByTag)
        {
            int tag = pair.Key;
            FieldInfo field = pair.Value;

            object value = field.GetValue(instance);
            writer.model.TrySerializeAuxiliaryType(writer, field.FieldType, DataFormat.Default, tag, value, false);
        }
    }

    /// <summary>
    /// Automatically deserializes a registered type from <see cref="SerializerEntries"/> based on its type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Should only be called by <see cref="ProtobufSerializerPrecompiledPatcher.OptimizedDeserializePrefix"/>
    /// </remarks>
    public static object Deserialize(object instance, ProtoReader reader)
    {
        if (SerializerEntries.TryGetValue(instance.GetType(), out SerializerEntry serializerEntry))
        {
            return DeserializeEntry(serializerEntry, instance, reader);
        }
        return null;
    }

    private static object DeserializeEntry(SerializerEntry serializerEntry, object instance, ProtoReader reader)
    {
        if (serializerEntry.FirstInheritedSerializerEntry is SerializerEntry inheritedEntry)
        {
            if (inheritedEntry.SerializedFieldsByTag != null)
            {
                DeserializeEntry(inheritedEntry, instance, reader);
            }
            else
            {
                inheritedEntry.DeserializeInfo.Invoke(reader.model, new object[] { instance, reader });
            }
        }

        object value = null;
        foreach (KeyValuePair<int, FieldInfo> pair in serializerEntry.SerializedFieldsByTag)
        {
            int tag = pair.Key;
            FieldInfo field = pair.Value;
            if (reader.model.TryDeserializeAuxiliaryType(reader, DataFormat.Default, tag, field.FieldType, ref value, true, true, false, false))
            {
                field.SetValue(instance, value);
            }
        }

        return instance;
    }

    /// <summary>
    /// Automatically generates an ordered list of fields to serialize
    /// </summary>
    /// <param name="serializableType">Serialized type</param>
    private static Dictionary<int, FieldInfo> GetSerializedFieldsByTag(Type serializableType)
    {
        // Extract the fields and to be serialized (marked with SubnauticaSerialized)
        FieldInfo[] fields = serializableType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Dictionary<int, FieldInfo> serializedFieldsByTag = new();

        foreach (FieldInfo field in fields)
        {
            foreach (Attribute attribute in field.GetCustomAttributes())
            {
                if (attribute is SubnauticaSerialized subnauticaSerialized)
                {
                    serializedFieldsByTag.Add(subnauticaSerialized.tag, field);
                    break;
                }
            }
        }
        return serializedFieldsByTag;
    }

    /// <summary>
    /// Marks the fields to be detectable by <see cref="GetSerializedFieldsByTag"/>.
    /// (Encapsulates <see cref="ProtoMemberAttribute"/>)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SubnauticaSerialized : ProtoMemberAttribute
    {
        /// <inheritdoc cref="ProtoMemberAttribute(int, bool)" />
        public SubnauticaSerialized(int tag, bool forced = false) : base(tag, forced) { }
    }

    /// <summary>
    /// Data structure which holds the method data for serialization and deserialization.
    /// </summary>
    internal class SerializerEntry
    {
        internal Type Type;
        internal int TypeId;
        internal MethodInfo SerializeInfo;
        internal MethodInfo DeserializeInfo;
        /// <summary>
        /// <see cref="SerializerEntry"/> corresponding to the first inherited type which is registered to be serialized.
        /// Can be null if there's none.
        /// </summary>
        internal SerializerEntry FirstInheritedSerializerEntry;
        internal IOrderedEnumerable<KeyValuePair<int, FieldInfo>> SerializedFieldsByTag;

        /// <summary>
        /// Constructor to be commonly used when adding a new serializable type.
        /// </summary>
        /// <param name="serializeInfo">A static method ran when Subnautica serializes the <see cref="Type"/></param>
        /// <param name="deserializeInfo">A static method ran when Subnautica deserializes the <see cref="Type"/></param>
        /// <param name="serializedFieldsByTag">A dictionary containing all fields to be serialized, linked to their tag</param>
        /// <param name="type">The type to be serializable</param>
        public SerializerEntry(Type type, MethodInfo serializeInfo, MethodInfo deserializeInfo, Dictionary<int, FieldInfo> serializedFieldsByTag)
        {
            Type = type;
            SerializeInfo = serializeInfo;
            DeserializeInfo = deserializeInfo;
            SerializedFieldsByTag = serializedFieldsByTag.OrderBy(entry => entry.Key);
        }

        /// <summary>
        /// Constructor to be used for Subnautica's default known types only.
        /// </summary>
        public SerializerEntry(Type type, int typeId, string serializeMethodName, string deserializeMethodName)
        {
            Type = type;
            TypeId = typeId;
            SerializeInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(serializeMethodName);
            DeserializeInfo = ReflectionHelper.GetInstanceMethod<ProtobufSerializerPrecompiled>(deserializeMethodName);
        }
    }
}
