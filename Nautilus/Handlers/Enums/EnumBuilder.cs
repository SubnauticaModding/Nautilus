using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Nautilus.Utility;

// ReSharper disable once CheckNamespace
namespace Nautilus.Handlers;

/// <summary>
/// Represents a custom enum object. This class cannot be inherited.
/// </summary>
/// <typeparam name="TEnum">Type of the enum.</typeparam>
public sealed class EnumBuilder<TEnum> where TEnum : Enum
{
    internal static EnumCacheManager<TEnum> CacheManager { get; } = (EnumCacheManager<TEnum>)EnumCacheProvider.EnsureManager<TEnum>();
    private static bool _initialized = false;
    private static readonly HashSet<string> _vanillaNamesLowerInvariant = new(typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => field.Name.ToLowerInvariant()));

    private TEnum _enumValue;

    // Colons in particular are NOT valid because they interfere with the EnumCache format
    private static readonly char[] _invalidEnumCharacters = [':', '\n'];

    /// <summary>
    /// The enum value corresponding to this builder.
    /// </summary>
    public TEnum Value => _enumValue;
    
    private EnumBuilder()
    {
        // Hide constructor
    }

    /// <summary>
    /// Converts an EnumBuilder to its corresponding enum object. 
    /// </summary>
    /// <param name="enumBuilder">The Enum Builder.</param>
    /// <returns>The enum object equivalent to this instance.</returns>
    public static implicit operator TEnum(EnumBuilder<TEnum> enumBuilder)
    {
        return enumBuilder.Value;
    }

    /// <summary>
    /// Converts an Enum object to EnumBuilder.
    /// </summary>
    /// <param name="enum">The Enum object.</param>
    /// <returns>The constructed enum builder for the specified enum object.</returns>
    public static explicit operator EnumBuilder<TEnum>(TEnum @enum)
    {
        var enumBuilder = new EnumBuilder<TEnum>
        {
            _enumValue = @enum
        };

        return enumBuilder;
    }

    /// <summary>
    /// Converts the value of this instance to a <see cref="string"/>.
    /// </summary>
    /// <returns>A string whose value is the same as this instance.</returns>
    public override string ToString()
    {
        return _enumValue.ToString();
    }
    
    internal static EnumBuilder<TEnum> CreateInstance(string name, Assembly addedBy)
    {
        var builder = new EnumBuilder<TEnum>();

        TryAddEnumValueResult valueResult = builder.TryAddEnum(name, addedBy, out TEnum enumValue);
        
        if (valueResult == TryAddEnumValueResult.Success)
        {
            return builder;
        }

        // For duplicate results, produce errors but do not throw
        if (valueResult is TryAddEnumValueResult.AlreadyExistsInVanilla or TryAddEnumValueResult.DuplicateModEnumValue)
        {
            InternalLogger.Announce($"{name} already exists in {nameof(TEnum)}! Message: {valueResult}", LogLevel.Error, true);
            return null;
        }

        // For other invalid results, throw an exception
        throw new ArgumentException($"Problem while registering enum value '{name}' in {nameof(TEnum)}: {valueResult}");
    }

    private TryAddEnumValueResult TryAddEnum(string name, Assembly addedBy, out TEnum enumValue)
    {                
        if (CacheManager.RequestCacheForTypeName(name, false, true) != null)
        {
            enumValue = default;
            return TryAddEnumValueResult.DuplicateModEnumValue;
        }

        if (_vanillaNamesLowerInvariant.Contains(name.ToLowerInvariant()))
        {
            enumValue = default;
            return TryAddEnumValueResult.AlreadyExistsInVanilla;
        }

        if (IsEnumNameInvalid(name))
        {
            enumValue = default;
            return TryAddEnumValueResult.InvalidName;
        }

        EnumTypeCache cache = CacheManager.RequestCacheForTypeName(name, addedBy: addedBy) ?? new EnumTypeCache()
        {
            Name = name,
            Index = CacheManager.GetNextAvailableIndex()
        };

        if(!_initialized)
        {
            Initialize();
        }

        enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));

        CacheManager.Add(enumValue, cache.Index, cache.Name, addedBy);

        // enum name with parent type and no char qualifiers
        var enumName = typeof(TEnum).DeclaringType is { } d
            ? $"{d.Name}{typeof(TEnum).Name}"
            : $"{typeof(TEnum).Name}";

        InternalLogger.Log($"Successfully added {enumName}: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
        _enumValue = enumValue;

        return TryAddEnumValueResult.Success;
    }

    internal void Initialize()
    {
        // enum name with parent type and no char qualifiers
        var enumName = typeof(TEnum).DeclaringType is { } d
            ? $"{d.Name}{typeof(TEnum).Name}"
            : $"{typeof(TEnum).Name}";
        
        SaveUtils.RegisterOnSaveEvent(() => CacheManager.SaveCache());
        InternalLogger.Log($"{enumName}Patcher is initialized.", LogLevel.Debug);
        _initialized = true;
    }

    // This is less restrictive than it needs to be, only for backwards compatibility
    private static bool IsEnumNameInvalid(string enumName)
    {
        if (enumName.IndexOfAny(_invalidEnumCharacters) >= 0)
        {
            return true;
        }

        return false;
    }

    private enum TryAddEnumValueResult : byte
    {
        Success,
        DuplicateModEnumValue,
        AlreadyExistsInVanilla,
        InvalidName
    }
}