namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

#if BELOWZERO
    internal class TechDataPatcher
    {
        private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();
        internal static IDictionary<TechType, JsonValue> CustomTechData = new SelfCheckingDictionary<TechType, JsonValue>("CustomTechData", AsStringFunction);

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(TechData), "Cache"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), "TechDataCachePostfix")));
        }

        private static void TechDataCachePostfix()
        {
            AddCustomTechDataToOriginalDictionary();
        }

        internal static void AddToCustomTechData(TechType techType, JsonValue techData)
        {
            CustomTechData.Add(techType, techData);
        }

        private static void AddCustomTechDataToOriginalDictionary()
        {
            short added = 0;
            short replaced = 0;
            foreach (TechType techType in CustomTechData.Keys)
            {
                bool techDataExists = TechData.Contains(techType);

                if (techDataExists && TechData.entries[techType] != CustomTechData[techType])
                {
                    if (TechData.TryGetValue(techType, out JsonValue originalData))
                    {
                        foreach (JsonValue newData in CustomTechData[techType])
                        {
                            TechData.entries[techType][newData.GetInt()] = CustomTechData[techType][newData.GetInt()];
                        }

                        Logger.Log($"{techType} TechType already existed in the CraftData.techData dictionary. Original value was replaced.", LogLevel.Warn);
                        replaced++;
                        Logger.Log($"Replaced Item: " + techType + " " + TechData.Contains(techType), LogLevel.Info);
                    }
                }
                else if (!techDataExists)
                {
                    TechData.Add(techType, CustomTechData[techType]);
                    added++;
                    Logger.Log($"Added Item: " + techType + " " + TechData.Contains(techType), LogLevel.Info);
                }
            }
            if (added > 0)
                Logger.Log($"Added {added} new entries to the CraftData.techData dictionary.", LogLevel.Info);
            if (replaced > 0)
                Logger.Log($"Replaced {replaced} existing entries to the CraftData.techData dictionary.", LogLevel.Info);
        }
    }
#endif
}
