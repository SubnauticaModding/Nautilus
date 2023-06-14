using System;
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

    private TEnum _enumValue;

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
    /// <param name="enumBuilder">The Enum Builder</param>
    /// <returns>The enum object equivalent to this instance.</returns>
    public static implicit operator TEnum(EnumBuilder<TEnum> enumBuilder)
    {
        return enumBuilder.Value;
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
        if(builder.TryAddEnum(name, addedBy, out TEnum enumValue))
        {
            ProcessExtraActions(builder);
            return builder;
        }

        InternalLogger.Announce($"{name} already exists in {nameof(TEnum)}!", LogLevel.Error, true);
        return null;
    }

    private static void ProcessExtraActions(EnumBuilder<TEnum> builder)
    {
        switch(builder)
        {
            case EnumBuilder<TechType> techTypeBuilder:
                var techType = techTypeBuilder.Value;
                var name = techType.ToString();
                var intKey = ((int)techType).ToString();
                TechTypeExtensions.stringsNormal[techType] = name;
                TechTypeExtensions.stringsLowercase[techType] = name.ToLowerInvariant();
                TechTypeExtensions.techTypesNormal[name] = techType;
                TechTypeExtensions.techTypesIgnoreCase[name] = techType;
                TechTypeExtensions.techTypeKeys[techType] = intKey;
                TechTypeExtensions.keyTechTypes[intKey] = techType;
                break;
        }
    }

    private bool TryAddEnum(string name, Assembly addedBy, out TEnum enumValue)
    {                
        if(CacheManager.RequestCacheForTypeName(name, false, true) != null)
        {
            enumValue = default;
            return false;
        }

        if(Enum.GetNames(typeof(TEnum)).Any((x)=> x.ToLowerInvariant() == name.ToLowerInvariant()))
        {
            enumValue = default;
            return false;
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

        return true;
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
}