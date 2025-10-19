using System;
using System.Reflection;
using System.Reflection.Emit;
using Nautilus.Utility;

// ReSharper disable once CheckNamespace
namespace Nautilus.Handlers;

/// <summary>
/// Delegate used when a custom enum is registered via <see cref="EnumHandler.AddEntry{TEnum}(string,System.Reflection.Assembly)"/>.
/// </summary>
/// <typeparam name="TEnum">The type of the enum to process.</typeparam>
public delegate void OnEnumRegistered<TEnum>(EnumBuilder<TEnum> builder) where TEnum : Enum;

/// <summary>
/// Class responsible to resolve anything related to adding custom enum objects.
/// </summary>
public static class EnumHandler
{
    internal static class Events<TEnum> where TEnum : Enum
    {
        public static event OnEnumRegistered<TEnum> OnEnumRegistered;

        public static void NotifyOnEnumRegistered(EnumBuilder<TEnum> builder)
        {
            OnEnumRegistered?.Invoke(builder);
        }
    }
    
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
        var builder =  EnumBuilder<TEnum>.CreateInstance(name, ownerAssembly);
        if (builder is not null)
        {
            Events<TEnum>.NotifyOnEnumRegistered(builder);
        }

        return builder;
    }

    /// <summary>
    /// Adds a new custom enum object instance.
    /// </summary>
    /// <param name="name">The name for this instance. Must be unique and not contain any special characters.<br/>
    /// </param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns>A reference to the created custom enum object or if the name is already in use it will return null.</returns>
    public static EnumBuilder<TEnum> AddEntry<TEnum>(string name) where TEnum : Enum
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        callingAssembly = callingAssembly == Assembly.GetExecutingAssembly()
            ? ReflectionHelper.CallingAssemblyByStackTrace()
            : callingAssembly;
        
        var builder = AddEntry<TEnum>(name, callingAssembly);

        return builder;
    }

    /// <summary>
    /// Adds a new custom enum object instance.
    /// </summary>
    /// <param name="name">The name for this instance. Must be unique and not contain any special characters.<br/>
    /// </param>
    /// <param name="ownerAssembly">The owner of this TechType instance.</param>
    /// <param name="builder">The reference to the created custom enum object.</param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns><see langword="true"/>if successful otherwise; <see langword="false"/>.</returns>
    public static bool TryAddEntry<TEnum>(string name, Assembly ownerAssembly, out EnumBuilder<TEnum> builder) where TEnum : Enum
    {
        return (builder = AddEntry<TEnum>(name, ownerAssembly)) != null;
    }

    /// <summary>
    /// Adds a new custom enum object instance.
    /// </summary>
    /// <param name="name">The name for this instance. Must be unique and not contain any special characters.<br/>
    /// </param>
    /// <param name="builder">The reference to the created custom enum object.</param>
    /// <typeparam name="TEnum">Type of the enum to add an entry for.</typeparam>
    /// <returns><see langword="true"/>if successful; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the object was found; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Make sure to set a [BepInDependency("otherModGUID", BepInDependency.DependencyFlags.SoftDependency)] on your plugin to ensure theirs loads first.
    /// </remarks>
    public static bool TryGetValue<TEnum>(string name, out TEnum enumValue) where TEnum : Enum
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
    public static bool TryGetOwnerAssembly<TEnum>(TEnum modEnumValue, out Assembly addedBy) where TEnum : Enum
    {
        addedBy = null;
        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache manager))
            return false;

        if (manager.TypesAddedBy.TryGetValue(modEnumValue.ToString(), out addedBy)) // Assembly Found
            return true;

        // Was not added by a mod using Nautilus.
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
    public static bool TryGetValue<TEnum>(string name, out TEnum enumValue, out Assembly addedBy) where TEnum : Enum
    {
        enumValue = default;
        addedBy = null;

        if(!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache manager))
            return false;

        var cache = manager.RequestCacheForTypeName(name, false, true);

        if(cache != null) // Item Found
        {
            enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
            addedBy = manager.TypesAddedBy[enumValue.ToString()];
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
        return TryGetValue<TEnum>(name, out _);
    }

    /// <summary>
    /// Adds a callback that's executed every time a custom enum of a certain type is created.
    /// </summary>
    /// <param name="callback">The callback that will be executed.</param>
    /// <typeparam name="TEnum">The type of the enum this callback is for.</typeparam>
    public static void AddOnEnumRegistered<TEnum>(OnEnumRegistered<TEnum> callback) where TEnum : Enum
    {
        Events<TEnum>.OnEnumRegistered +=  callback;
    }
    
    /// <summary>
    /// Removes a callback that was previously added to the OnEnumRegistered event.
    /// </summary>
    /// <param name="callback">The callback to remove.</param>
    public static void RemoveOnEnumRegistered<TEnum>(OnEnumRegistered<TEnum> callback) where TEnum : Enum
    {
        Events<TEnum>.OnEnumRegistered -=  callback;
    }
}
