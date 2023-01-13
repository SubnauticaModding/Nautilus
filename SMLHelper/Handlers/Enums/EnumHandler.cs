namespace SMLHelper.Handlers;

using System;
using System.Reflection;
using SMLHelper.Utility;

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
    /// <param name="ownerAssembly">The owner of this TechType instance.</param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns>A reference to the created custom enum object or if the name is already in use it will return null</returns>
    public static EnumBuilder<TEnum> AddEntry<TEnum>(string name, Assembly ownerAssembly) where TEnum : Enum
    {
        return EnumBuilder<TEnum>.CreateInstance(name, ownerAssembly);
    }

    /// <summary>
    /// Adds a new custom enum object instance.
    /// </summary>
    /// <param name="name">The name for this instance. Must be unique and not contain any special characters.<br/>
    /// </param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns>A reference to the created custom enum object or if the name is already in use it will return null</returns>
    public static EnumBuilder<TEnum> AddEntry<TEnum>(string name) where TEnum : Enum
    {
        return AddEntry<TEnum>(name, Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Adds a new custom enum object instance.
    /// </summary>
    /// <param name="name">The name for this instance. Must be unique and not contain any special characters.<br/>
    /// </param>
    /// <param name="ownerAssembly">The owner of this TechType instance.</param>
    /// <param name="builder">The reference to the created custom enum object</param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns><see langword="true"/>if successfull otherwise <see langword="false"/></returns>
    public static bool TryAddEntry<TEnum>(string name, Assembly ownerAssembly, out EnumBuilder<TEnum> builder) where TEnum : Enum
    {
        return (builder = AddEntry<TEnum>(name, ownerAssembly)) != null;
    }

    /// <summary>
    /// Adds a new custom enum object instance.
    /// </summary>
    /// <param name="name">The name for this instance. Must be unique and not contain any special characters.<br/>
    /// </param>
    /// <param name="builder">The reference to the created custom enum object</param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns><see langword="true"/>if successfull otherwise <see langword="false"/></returns>
    public static bool TryAddEntry<TEnum>(string name, out EnumBuilder<TEnum> builder) where TEnum : Enum
    {
        return (builder = AddEntry<TEnum>(name)) != null;
    }

    /// <summary>
    /// Safely looks for a custom enum object from another mod and outputs the instance when found.
    /// </summary>
    /// <param name="name">The name of the custom enum object.</param>
    /// <param name="enumValue">The custom enum object value.</param>
    /// <typeparam name="TEnum">Type of the enum to search for.</typeparam>
    /// <returns><see langword="true"/> if the object was found; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Make sure to set a [BepInDependency("otherModGUID", BepInDependency.DependencyFlags.SoftDependency)] on your plugin to ensure theirs loads first.
    /// </remarks>
    public static bool TryGetModAddedEnumValue<TEnum>(string name, out TEnum enumValue) where TEnum : Enum
    {
        enumValue = default;
        
        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out var manager))
            return false;

        var cache = manager.RequestCacheForTypeName(name, false, true);

        if (cache != null) // Item Found
        {
            enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
            return true;
        }

        // Mod not present or not yet loaded
        return false;
    }
    
    /// <summary>
    /// Safely looks for a custom enum object from another mod and outputs the instance when found.
    /// </summary>
    /// <param name="modEnumValue">The custom enum object value.</param>
    /// <param name="addedBy">The Assembly that added the Enum value.</param>
    /// <typeparam name="TEnum">Type of the enum to search for.</typeparam>
    /// <returns><see langword="true"/> if the Assembly was found; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Make sure to set a [BepInDependency("otherModGUID", BepInDependency.DependencyFlags.SoftDependency)] on your plugin to ensure theirs loads first.
    /// </remarks>
    public static bool TryGetModAssembly<TEnum>(TEnum modEnumValue, out Assembly addedBy) where TEnum : Enum
    {
        addedBy = null;
        if(!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache manager))
            return false;

        if(manager.TypesAddedBy.TryGetValue(modEnumValue.ToString(), out addedBy)) // Assembly Found
            return true;

        // Was not added by a mod using SMLHelper.
        return false;
    }

    /// <summary>
    /// Safely looks for a custom enum object from another mod and outputs the instance when found.
    /// </summary>
    /// <param name="name">The name of the custom enum object.</param>
    /// <param name="enumValue">The custom enum object value.</param>
    /// <param name="addedBy">The Assembly that added the Enum value.</param>
    /// <typeparam name="TEnum">Type of the enum to search for.</typeparam>
    /// <returns><see langword="true"/> if the object was found; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Make sure to set a [BepInDependency("otherModGUID", BepInDependency.DependencyFlags.SoftDependency)] on your plugin to ensure theirs loads first.
    /// </remarks>
    public static bool TryGetModAddedEnumValue<TEnum>(string name, out TEnum enumValue, out Assembly addedBy) where TEnum : Enum
    {
        enumValue = default;
        addedBy = null;

        if(!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache manager))
            return false;

        var cache = manager.RequestCacheForTypeName(name, false, true);

        if(cache != null) // Item Found
        {
            addedBy = manager.TypesAddedBy[enumValue.ToString()];
            enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
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
    public static bool ModAddedEnumValueExists<TEnum>(string name) where TEnum : Enum
    {
        return TryGetModAddedEnumValue<TEnum>(name, out _);
    }    
}
