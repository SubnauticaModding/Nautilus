using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal static class EnumPatcher
{
    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(EnumPatcher));

        InternalLogger.Log("EnumPatcher is done.", LogLevel.Debug);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
    private static void Postfix_GetValues(Type enumType, ref Array __result)
    {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager))
        {
            __result = GetValues(enumType, manager, __result);
        }
    }
    

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetNames))]
    private static void Postfix_GetNames(Type enumType, ref Array __result)
    {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager))
        {
            __result = GetNames(manager, __result);
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetName))]
    private static bool Prefix_GetName(Type enumType, object value, ref string __result)
    {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager) && manager.TryGetValue(value, out var name))
        {
            __result = name;
            return false;
        }

        return true;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.IsDefined))]
    private static bool Prefix_IsDefined(Type enumType, object value, ref bool __result)
    {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager) && IsDefined(manager, value))
        {
            __result = true;
            return false;
        }

        return true;
    }

    private static bool IsDefined(IEnumCache cacheManager, object value)
    {
        return cacheManager.ContainsKey(value);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.Parse), new[] { typeof(Type), typeof(string), typeof(bool) })]
    private static bool Prefix_Parse(Type enumType, string value, bool ignoreCase, ref object __result)
    {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager) && manager.TryParse(value, out __result))
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.ToString), new Type[] { })]
    private static bool Prefix_ToString(Enum __instance, ref string __result)
    {
        if (EnumCacheProvider.TryGetManager(__instance.GetType(), out var manager) &&
            manager.TryGetValue(__instance, out __result))
        {
            return false;
        }

        return true;
    }
    
    private static Array GetValues(Type enumType, IEnumCache cacheManager, Array __result)
    {
        Type genericListType = typeof(List<>).MakeGenericType(enumType);
        IList list = (IList)Activator.CreateInstance(genericListType);
        foreach (var type in __result)
        {
            list.Add(type);
        }
        foreach(var type2 in cacheManager.ModdedKeys)
        {
            list.Add(type2);
        }

        Array array = Array.CreateInstance(enumType, list.Count);
        list.CopyTo(array, 0);

        return array;
    }

    private static Array GetNames(IEnumCache cacheManager, Array __result)
    {
        var list = new List<string>();
        foreach (string type in __result)
        {
            list.Add(type);
        }

        foreach (var type in cacheManager.ModdedKeys)
        {
            if (cacheManager.TryGetValue(type, out string name))
                list.Add(name);
        }

        return list.ToArray();
    }
}