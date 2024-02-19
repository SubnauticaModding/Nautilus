using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nautilus.Patchers;
using Nautilus.Utility;
using ProtoBuf;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class responsible for implementing Protobuf serialization to new custom types.
/// </summary>
public static class ProtobufSerializerHandler
{
    internal static Dictionary<int, SerializerEntry> SerializerEntries;

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
    /// Automatically generates a serialize method and a deserialize method to be used in .
    /// </summary>
    /// <typeparam name="T">Serialized type</typeparam>
    public static (Action<T, int, ProtoWriter>, Func<T, ProtoReader, T>) GenerateSerializeAndDeserializeMethods<T>() where T : new()
    {
        // Extract the fields and to be serialized (marked with SubnauticaSerialized)
        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
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

        // Generate serialized and deserialize methods using the default serializing and deserializing utilities
        // provided by the internal methods TrySerializeAuxiliaryType and TryDeserializeAuxiliaryType from TypeModel
        Action<T, int, ProtoWriter> serialize = new((instance, objectId, writer) =>
        {
            foreach (KeyValuePair<int, FieldInfo> pair in serializedFieldsByTag.OrderBy(entry => entry.Key))
            {
                int tag = pair.Key;
                FieldInfo field = pair.Value;

                object value = field.GetValue(instance);
                writer.model.TrySerializeAuxiliaryType(writer, field.FieldType, DataFormat.Default, tag, value, false);
            }
        });

        Func<T, ProtoReader, T> deserialize = new((instance, reader) =>
        {
            object value = null;
            foreach (KeyValuePair<int, FieldInfo> pair in serializedFieldsByTag.OrderBy(entry => entry.Key))
            {
                int tag = pair.Key;
                FieldInfo field = pair.Value;
                if (reader.model.TryDeserializeAuxiliaryType(reader, DataFormat.Default, tag, field.FieldType, ref value, true, true, false, false))
                {
                    field.SetValue(instance, value);
                }
            }
            
            return instance;
        });


        return (serialize, deserialize);
    }

    /// <summary>
    /// Marks the fields to be detectable by <see cref="GenerateSerializeAndDeserializeMethods{T}"/>.
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
