
namespace SMLHelper.V2.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using ProtoBuf;
    using ProtoBuf.Meta;

    /// <summary>
    /// Allows saving mod components directly into game files
    /// </summary>
    public static class SMLProtobufSerializer
    {
        private static readonly Dictionary<Type, int> knownTypes = ProtobufSerializerPrecompiled.knownTypes;
        private static readonly RuntimeTypeModel model = TypeModel.Create();

        /// <summary>
        /// Save an object to a stream
        /// </summary>
        /// <param name="stream">Stream for the object to be saved to</param>
        /// <param name="source">Object to be saved</param>
        public static void Serialize(Stream stream, object source)
        {
            model.SerializeWithLengthPrefix(stream, source, source.GetType(), PrefixStyle.Base128, 0);
        }

        /// <summary>
        /// Checks if a type can be serialized
        /// </summary>
        /// <param name="type">Type to check for serializability</param>
        /// <returns>true if type is registered</returns>
        public static bool CanSerialize(Type type)
        {
            return model.CanSerialize(type);
        }

        /// <summary>
        /// Registers Type to be serialized with SMLProtobufSerializer
        /// </summary>
        /// <param name="type">Type to be serialized</param>
        public static void RegisterType(Type type)
        {
            model.Add(type, true);
        }

        /// <summary>
        /// Registers all types from an assembly to be serialized with SMLProtobufSerializer
        /// </summary>
        /// <param name="assembly">Assembly to register types from</param>
        public static void RegisterTypes(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                bool hasUweProtobuf = (type.GetCustomAttributes(typeof(ProtoContractAttribute), false).Length > 0);

                if (hasUweProtobuf)
                {
                    model.Add(type, true);
                    knownTypes[type] = int.MaxValue; // UWE precompiled is going to pass everything that we add to us
                }
            }
        }

        /// <summary>
        /// Load an Object from a Stream
        /// </summary>
        /// <param name="stream">Stream to load the object from</param>
        /// <param name="target">Object output</param>
        /// <param name="type">Type of the object to load</param>
        public static void Deserialize(Stream stream, object target, Type type)
        {
            model.DeserializeWithLengthPrefix(stream, target, type, PrefixStyle.Base128, 0);
        }
    }
}
