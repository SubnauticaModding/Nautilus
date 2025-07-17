using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Patchers;

internal partial class CraftDataPatcher
{
    #region Internal Fields

    private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

    #endregion

    #region Group Handling

    internal static bool ModPrefabsPatched;

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

    #region Patching

    internal static void Patch(Harmony harmony)
    {
#if SUBNAUTICA
        PatchForSubnautica(harmony);
#elif BELOWZERO
            PatchForBelowZero(harmony);
#endif
        harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataPrefabIDCachePrefix))),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataPrefabIDCachePostfix))));

        InternalLogger.Log("CraftDataPatcher is done.", LogLevel.Debug);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetTechType), new Type[] { typeof(GameObject), typeof(GameObject) }, argumentVariations: new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out })]
    private static void CraftDataGetTechTypePrefix(GameObject obj, out GameObject go, ref TechType __result)
    {
        CraftData.PreparePrefabIDCache();
        Transform transform = obj.transform;
        TechTag techTag = null;
        PrefabIdentifier prefabIdentifier = null;

        while(transform != null && !transform.TryGetComponent(out prefabIdentifier) && !transform.TryGetComponent(out techTag))
        {
            transform = transform.parent;
        }

        if(prefabIdentifier != null)
        {
            go = prefabIdentifier.gameObject;
            __result = CraftData.entClassTechTable.GetOrDefault(prefabIdentifier.ClassId, TechType.None);
            return;
        }

        if(techTag != null)
        {
            go = techTag.gameObject;
            __result = techTag.type;
            return;
        }

        go = null;
        __result = TechType.None;
        return;
    }
    
    [HarmonyPatch(typeof(uGUI_CraftingMenu.Node), nameof(uGUI_CraftingMenu.Node.GetEnumerator), MethodType.Enumerator)]
    [HarmonyPrefix]
    public static bool OrderNodes(uGUI_CraftingMenu.Node __instance, ref IEnumerator<uGUI_CraftingMenu.Node> __result)
    {
        //The number of nodes to space out each tab node by
        //Also equal to the width of the crafting grid
        //In other words, if the grid is 3 wide (a 3x3 grid, roughly)
        //We space them out every 3 nodes
        //Meaning the craft nodes are always on the leftmost side
        var offset = Mathf.CeilToInt(Mathf.Sqrt(__instance.childCount));
        var tabs = __instance.nodes.Where(node => node is uGUI_CraftingMenu.Node craftMenuNode && craftMenuNode.action == TreeAction.Expand).ToList();

        if (tabs.Count == 0)
        {
            // Run original
            return true;
        }

        var craftNodes = __instance.nodes.Where(node => !tabs.Contains(node)).ToList();

        var sortedNodes = new List<uGUI_CraftingMenu.Node>();

        var tabsUsed = 0;    

        for (var i = 0; i < __instance.childCount; i++)
        {
            if(i % offset == 0 && tabs.Count > tabsUsed)
            {
                sortedNodes.Add((uGUI_CraftingMenu.Node)tabs[tabsUsed]);
                tabsUsed++;
                continue;
            }
            sortedNodes.Add((uGUI_CraftingMenu.Node)craftNodes[i - tabsUsed]);
        }

        __result = sortedNodes.GetEnumerator();
        // Skip original
        return false;
    }

    private static bool NeedsPatching = true;

    private static void CraftDataPrefabIDCachePrefix()
    {
        NeedsPatching = CraftData.cacheInitialized;
    }

    private static void CraftDataPrefabIDCachePostfix()
    {
        if(!NeedsPatching && ModPrefabsPatched)
            return;

        Dictionary<TechType, string> techMapping = CraftData.techMapping;
        Dictionary<string, TechType> entClassTechTable = CraftData.entClassTechTable;
        foreach (var prefab in PrefabHandler.Prefabs)
        {
            if (prefab.Key.TechType is TechType.None)
                continue;
                
            techMapping[prefab.Key.TechType] = prefab.Key.ClassID;
            entClassTechTable[prefab.Key.ClassID] = prefab.Key.TechType;
        }
        ModPrefabsPatched = true;
    }
    #endregion
}