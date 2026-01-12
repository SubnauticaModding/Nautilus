using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Nautilus.Extensions;

internal static class TypeExtensions
{
    private static readonly List<string> _builtinTypeAliases = new()
    {
        "void",
        null,   // all other types
        "DBNull",
        "bool",
        "char",
        "sbyte",
        "byte",
        "short",
        "ushort",
        "int",
        "uint",
        "long",
        "ulong",
        "float",
        "double",
        "decimal",
        null,   // DateTime?
        null,   // ???
        "string"
    };

    /// <param name="type"></param>
    extension(Type type)
    {
        /// <summary>
        /// Format the given <paramref name="type"/>'s name into a more developer-friendly form.
        /// </summary>
        /// <returns></returns>
        public string GetFriendlyName()
        {
            if (type.TryUnwrapArrayType(out Type elementType))
                return GetFriendlyName(elementType) + "[]";
        
            if (type.TryUnwrapNullableType(out Type valueType))
                return GetFriendlyName(valueType) + "?";
        
            // TODO: format tuples as well
        
            if (type.IsConstructedGenericType)
                return type.Name[..type.Name.LastIndexOf('`')]
                       + $"<{type.GenericTypeArguments.Select(GetFriendlyName).Join()}>";
        
            return _builtinTypeAliases[(int) Type.GetTypeCode(type)] ?? type.Name;
        }

        /// <summary>
        /// "Unwraps" the inner <paramref name="type"/> from an array and/or nullable type.
        /// </summary>
        /// <returns>
        /// The inner type - for example, <see cref="string"/> from a <see cref="Array">string[]</see>, or <see cref="bool"/> from <see cref="Nullable{T}">bool?</see>.<br/>
        /// If the <paramref name="type"/> isn't wrapped, it is returned as-is.
        /// </returns>
        public Type GetUnderlyingType()
        {
            if (type.TryUnwrapArrayType(out Type elementType))
                type = elementType;
            if (type.TryUnwrapNullableType(out Type valueType))
                type = valueType;
            return type;
        }

        public bool TryUnwrapArrayType(out Type elementType)
        {
            // GetElementType checks if it's an array, pointer, or reference
            elementType = type.GetElementType();
            return type.IsArray // restrict to arrays only
                   && elementType != null;
        }

        public bool TryUnwrapNullableType(out Type valueType)
        {
            valueType = Nullable.GetUnderlyingType(type);
            return valueType != null;
        }
    }
}
