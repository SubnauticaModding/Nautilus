// ReSharper disable once CheckNamespace
namespace SMLHelper.Handlers;

using SMLHelper.Utility;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Extensions to interact more with custom enum objects.
/// </summary>
public static partial class EnumExtensions
{

    private static Dictionary<Type, IList> vanillaTypes = new();

    /// <summary>
    /// Checks if a TEnum is from from the game and not added by any mod using SMLHelper.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsVanillaTechType<TEnum>(this TEnum type) where TEnum : Enum
    {
        if(!EnumCacheProvider.TryGetManager(typeof(TEnum), out var manager))
        {
            // We have not added any values to this enum so it must be Vanilla or someone not using SMLHelper.
            return true;
        }
        if(!vanillaTypes.TryGetValue(typeof(TEnum), out IList vanillaTypeList) || vanillaTypeList.Count == 0)
        {
            var array = (TEnum[])Enum.GetValues(typeof(TEnum));
            var allTechTypes = new List<TEnum>(array);
            allTechTypes.RemoveAll(tt => manager.ModdedKeys.Contains(tt));
            vanillaTypeList = allTechTypes;
            vanillaTypes[typeof(TEnum)] = vanillaTypeList;
        }
        return vanillaTypeList.Contains(type);
    }

}