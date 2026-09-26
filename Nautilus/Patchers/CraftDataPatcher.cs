using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Patchers;

internal class CraftDataPatcher
{
    internal static readonly IDictionary<TechType, JsonValue> CustomRecipeData 
        = new SelfCheckingDictionary<TechType, JsonValue>("CustomTechData", t => t.AsString());

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(CraftDataPatcher));

        InternalLogger.Log("CraftDataPatcher is done.", LogLevel.Debug);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TechData), nameof(TechData.TryGetValue))]
    private static void CheckPatchRequired(TechType techType)
    {
        if (CustomRecipeData.TryGetValue(techType, out JsonValue customTechData))
        {
            if (!TechData.entries.TryGetValue(techType, out JsonValue techData) ||
                customTechData != techData)
            {
                AddCustomTechDataToOriginalDictionary();
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TechData), nameof(TechData.Cache))]
    private static void AddCustomTechDataToOriginalDictionary()
    {
        List<TechType> added = new();
        List<TechType> updated = new();
        foreach (KeyValuePair<TechType, JsonValue> customRecipe in CustomRecipeData)
        {
            JsonValue jsonValue = customRecipe.Value;
            TechType techType = customRecipe.Key;
            if (TechData.entries.TryGetValue(techType, out JsonValue techData))
            {
                if (techData != jsonValue)
                {
                    updated.Add(techType);
                    
                    foreach (int key in jsonValue.Keys)
                    {
                        if (jsonValue[key] is null or { propertyType: JsonValue.Type.None })
                        {
                            techData.Remove(key);
                            continue;
                        }
                        
                        techData[key] = jsonValue[key];
                    }
                }
            }
            else
            {
                TechData.entries.Add(techType, jsonValue);
                added.Add(techType);
            }
        }

        for (int i = 0; i < updated.Count; i++)
        {
            TechType updatedTechData = updated[i];
            CustomRecipeData[updatedTechData] = TechData.entries[updatedTechData];
        }

        if (added.Count > 0)
        {
            InternalLogger.Log($"Added {added.Count} new entries to the TechData.entries dictionary.", LogLevel.Info);
            LogEntries("Added the following TechTypes", added);
        }

        if (updated.Count > 0)
        {
            InternalLogger.Log($"Updated {updated.Count} existing entries to the TechData.entries dictionary.", LogLevel.Info);
            LogEntries("Updated the following TechTypes", updated);
        }
    }

    private static void LogEntries(string log, List<TechType> updated)
    {
        StringBuilder builder = new();
        for (int i = 0; i < updated.Count; i++)
        {
            builder.AppendLine($"{updated[i]}");
        }

        InternalLogger.Log($"{log}:{Environment.NewLine}{builder}", LogLevel.Debug);
    }

    #region Group Handling

    internal static void AddToGroup(TechGroup group, TechCategory category, TechType techType, TechType target, bool after)
    {
        if (!CraftData.groups.TryGetValue(group, out Dictionary<TechCategory, List<TechType>> techGroup))
        {
            // Should never happen, but doesn't hurt to add it.
            InternalLogger.Log("Invalid TechGroup!", LogLevel.Error);
            return;
        }

        if (!techGroup.TryGetValue(category, out List<TechType> techCategory))
        {
            InternalLogger.Log($"{group} does not contain {category} as a registered group. Please ensure to register your TechCategory to the TechGroup using the TechCategoryHandler before using the combination.", LogLevel.Error);
            return;
        }

        techCategory.Remove(techType);

        int index = techCategory.IndexOf(target);

        if (index == -1) // Not found
        {
            techCategory.Insert(after ? techCategory.Count : 0, techType);
            InternalLogger.Log($"{(after ? "Add" : "Insert")}ed \"{techType:G}\" {(after ? "" : "in")}to groups under \"{group:G}->{category:G}\"", LogLevel.Debug);
        }
        else
        {
            techCategory.Insert(index + (after ? 1 : 0), techType);
            InternalLogger.Log($"{(after ? "Add" : "Insert")}ed \"{techType:G}\" {(after ? "" : "in")}to groups under \"{group:G}->{category:G}\" {(after ? "after" : "before")} \"{target:G}\"", LogLevel.Debug);
        }
    }

    internal static void RemoveFromGroup(TechGroup group, TechCategory category, TechType techType)
    {
        if (CraftData.groups.TryGetValue(group, out var techGroup)
            && techGroup.TryGetValue(category, out var techCategory)
            && techCategory.Remove(techType))
        {
            InternalLogger.Log($"Successfully Removed \"{techType:G}\" from groups under \"{group:G}->{category:G}\"", LogLevel.Debug);
        }
    }

    #endregion

    #region Cache Patching
    
    internal static bool ModPrefabsPatched;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache))]
    private static void CraftDataPrefabIDCachePrefix()
    {
        if (!CraftData.cacheInitialized) ModPrefabsPatched = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache))]
    private static void CraftDataPrefabIDCachePostfix()
    {
        if(ModPrefabsPatched) return;
        
        foreach (var prefab in PrefabHandler.Prefabs)
        {
            if (prefab.Key.TechType is TechType.None) continue;
            
            CraftData.techMapping[prefab.Key.TechType] = prefab.Key.ClassID;
            CraftData.entClassTechTable[prefab.Key.ClassID] = prefab.Key.TechType;
        }
        ModPrefabsPatched = true;
    }
    
    #endregion
}