using System;
using HarmonyLib;
using SMLHelper.Utility;

// ReSharper disable once CheckNamespace
namespace SMLHelper.Handlers;

/// <summary>
/// Class responsible to resolve anything related to adding custom enum objects.
/// </summary>
public static class EnumHandler
{
    /// <summary>
    /// Adds a new custom enum object instance.
    /// </summary>
    /// <param name="name">The name for this instance. Must be unique and not contain any special characters.<br/>
    /// </param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns>A reference to the created custom enum object.</returns>
    public static EnumBuilder<TEnum> AddEntry<TEnum>(string name) where TEnum : Enum
    {
        var builder = EnumBuilder<TEnum>.CreateInstance(name);
        return builder;
    }

    /// <summary>
    /// Safely looks for a custom enum object from another mod and outputs the instance when found.
    /// </summary>
    /// <param name="name">The name of the custom enum object.</param>
    /// <param name="moddedEnum">The custom enum object value.</param>
    /// <typeparam name="TEnum">Type of the enum to search for.</typeparam>
    /// <returns><see langword="true"/> if the object was found; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// There's no guarantee in which order the mods are loaded; if two mods are loaded simultaneously, they might not be visible to each other.
    /// </remarks>
    public static bool TryGetModdedEnum<TEnum>(string name, out TEnum moddedEnum) where TEnum : Enum
    {
        moddedEnum = default;
        
        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out var manager))
            return false;

        var cache = manager.RequestCacheForTypeName(name, false);

        if (cache != null) // Item Found
        {
            moddedEnum = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
            return true;
        }

        // Mod not present or not yet loaded
        return false;
    }
    
    /// <summary>
    /// Safely looks for a custom enum object from another mod.
    /// </summary>
    /// <param name="name">The name of the custom enum object.</param>
    /// <typeparam name="TEnum">Type of the enum to search for.</typeparam>
    /// <returns><see langword="true"/> if the object was found; otherwise <see langword="false"/>.</returns>
    public static bool ModdedEnumExists<TEnum>(string name) where TEnum : Enum
    {
        return TryGetModdedEnum<TEnum>(name, out _);
    }

    /// <summary>
    /// Uses reflection to initialize all used custom enum types dynamically.
    /// </summary>
    internal static void InitializeAll()
    {
        var enumBuilderType = typeof(EnumBuilder<>);
        foreach (var enumType in EnumCacheProvider.CacheManagers.Keys)
        {
            var genericType = enumBuilderType.MakeGenericType(enumType);
            AccessTools.Method(genericType, nameof(EnumBuilder<Enum>.Initialize)).Invoke(null, new object[] {});
        }
    }
    
}
