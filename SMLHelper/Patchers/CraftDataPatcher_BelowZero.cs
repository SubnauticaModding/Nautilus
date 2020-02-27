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
        internal static IDictionary<TechType, JsonValue> PatchedTechData = new SelfCheckingDictionary<TechType, JsonValue>("CustomTechData", AsStringFunction);

#endregion

        internal static void AddToCustomTechData(TechType techType, JsonValue techData)
        {
            CustomTechData.Add(techType, techData);
        }

        private static void PatchForBelowZero(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(TechDataCachePrefix))));
            harmony.Patch(AccessTools.Method(typeof(TechData), nameof(TechData.Cache)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(TechDataCachePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache)),
               postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(PreparePrefabIDCachePostfix))));
            harmony.Patch(AccessTools.Method(typeof(TechData), nameof(TechData.Cache)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(PreparePrefabIDCachePostfix))));
        }

        private static void TechDataCachePrefix()
        {
            if(CustomTechData.Count > 0)
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
                    }
                }
                else if (!techDataExists)
                {
                    TechData.Add(techType, CustomTechData[techType]);
                    added++;
                }
            }

            if (added > 0)
                Logger.Log($"Added {added} new entries to the CraftData.techData dictionary.", LogLevel.Info);
            if (replaced > 0)
                Logger.Log($"Replaced {replaced} existing entries to the CraftData.techData dictionary.", LogLevel.Info);
        }

        private static void PreparePrefabIDCachePostfix()
        {
            Dictionary<TechType, string> techMapping = CraftData.techMapping;
            Dictionary<string, TechType> entClassTechTable = CraftData.entClassTechTable;
            foreach (ModPrefab prefab in ModPrefab.Prefabs)
            {
                techMapping[prefab.TechType] = prefab.ClassID;
                entClassTechTable[prefab.ClassID] = prefab.TechType;
            }
        }
    }
}
#endif
