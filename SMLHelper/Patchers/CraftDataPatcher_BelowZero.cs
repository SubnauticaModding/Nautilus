#if BELOWZERO
namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using Harmony;
    using SMLHelper.V2.Assets;

    internal partial class CraftDataPatcher
    {
#region Internal Fields

        internal static IDictionary<TechType, JsonValue> CustomTechData = new SelfCheckingDictionary<TechType, JsonValue>("CustomTechData", AsStringFunction);
        internal static int TechDataAltered = 0;
        internal static int PrefabAltered = 0;

        #endregion

        internal static void AddToCustomTechData(TechType techType, JsonValue techData)
        {
            CustomTechData.Add(techType, techData);
        }

        private static void PatchForBelowZero(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(TechData), nameof(TechData.Initialize)),
               postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(AddCustomTechDataToOriginalDictionary))));
            harmony.Patch(AccessTools.Method(typeof(TechData), nameof(TechData.TryGetValue)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(TechDataCachePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache)),
               postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataPrefabIDCachePostfix))));
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetPrefabForTechType)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataGetPrefabPrefix))));
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetPrefabForTechTypeAsync)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataGetPrefabPrefix))));
        }


        private static void CraftDataGetPrefabPrefix()
        {
            if (ModPrefab.PrefabCount > 0)
            {
                ModPrefab prefab = ModPrefab.Prefabs.FirstOrFallback(null);
                if (!CraftData.techMapping.ContainsKey(prefab.TechType) || !CraftData.entClassTechTable.ContainsKey(prefab.ClassID))
                    CraftDataPrefabIDCachePostfix();
            }
        }

        private static void CraftDataPrefabIDCachePostfix()
        {
            Dictionary<TechType, string> techMapping = CraftData.techMapping;
            Dictionary<string, TechType> entClassTechTable = CraftData.entClassTechTable;
            foreach (ModPrefab prefab in ModPrefab.Prefabs)
            {
                techMapping[prefab.TechType] = prefab.ClassID;
                entClassTechTable[prefab.ClassID] = prefab.TechType;
                PrefabAltered++;
            }
        }
        private static void TechDataCachePrefix()
        {
            if(CustomTechData.Count > 0 && TechDataAltered != CustomTechData.Count)
                AddCustomTechDataToOriginalDictionary();
        }

        private static void AddCustomTechDataToOriginalDictionary()
        {
            short added = 0;
            short replaced = 0;
            foreach (TechType techType in CustomTechData.Keys)
            {
                bool techDataExists = TechData.entries.ContainsKey(techType);
                if (techDataExists && TechData.entries[techType] != CustomTechData[techType])
                {
                    if (TechData.TryGetValue(techType, out JsonValue originalData))
                    {
                        foreach (int key in CustomTechData[techType].Keys)
                        {
                            TechData.entries[techType][key] = CustomTechData[techType][key];
                        }

                        Logger.Log($"{techType} TechType already existed in the CraftData.techData dictionary. Original value was replaced.", LogLevel.Warn);
                        replaced++;
                        TechDataAltered++;
                    }
                }
                else if (!techDataExists)
                {
                    TechData.Add(techType, CustomTechData[techType]);
                    added++;
                    TechDataAltered++;
                }
            }

            if (added > 0)
                Logger.Log($"Added {added} new entries to the CraftData.techData dictionary.", LogLevel.Info);
            if (replaced > 0)
                Logger.Log($"Replaced {replaced} existing entries to the CraftData.techData dictionary.", LogLevel.Info);
        }
    }
}
#endif
