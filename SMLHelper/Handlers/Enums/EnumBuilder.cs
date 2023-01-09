using System;
using BepInEx.Logging;
using SMLHelper.Utility;

// ReSharper disable once CheckNamespace
namespace SMLHelper.Handlers;

/// <summary>
/// Represents a custom enum object. This class cannot be inherited
/// </summary>
/// <typeparam name="TEnum">Type of the enum.</typeparam>
public sealed class EnumBuilder<TEnum> where TEnum : Enum
{
    internal static EnumCacheManager<TEnum> CacheManager { get; } = (EnumCacheManager<TEnum>)EnumCacheProvider.EnsureManager<TEnum>();
    
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
    
    internal static EnumBuilder<TEnum> CreateInstance(string name)
    {
        var builder = new EnumBuilder<TEnum>();
        builder.AddEntry(name);
        return builder;
    }

    private TEnum AddEntry(string name)
    {
        return EnumHandler.TryGetModdedEnum(name, out _enumValue) ? _enumValue : AddEnum(name);
    }

    private TEnum AddEnum(string name)
    {
        // enum name with parent type and no char qualifiers
        var enumName = typeof(TEnum).DeclaringType is { } d
            ? $"{d.Name}{typeof(TEnum).Name}"
            : $"{typeof(TEnum).Name}";
        
        EnumTypeCache cache = CacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
        {
            Name = name,
            Index = CacheManager.GetNextAvailableIndex()
        };

         _enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));

        CacheManager.Add(_enumValue, cache.Index, cache.Name);

        InternalLogger.Log($"Successfully added {enumName}: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);

        return _enumValue;
    }

    internal static void Initialize()
    {
        // enum name with parent type and no char qualifiers
        var enumName = typeof(TEnum).DeclaringType is { } d
            ? $"{d.Name}{typeof(TEnum).Name}"
            : $"{typeof(TEnum).Name}";
        
        SaveUtils.RegisterOnSaveEvent(() => CacheManager.SaveCache());
        InternalLogger.Log($"{enumName}Patcher is initialized.", LogLevel.Debug);
    }
}