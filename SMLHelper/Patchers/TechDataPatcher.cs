namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Assets;
    using Harmony;


    internal class TechDataPatcher
    {
        #region Internal Fields

        private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

        internal static IDictionary<TechType, JsonValue> CustomTechData = new SelfCheckingDictionary<TechType, JsonValue>("CustomTechData", AsStringFunction);

        #endregion



        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(CraftData), "PreparePrefabIDCache"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(TechDataPatcher), "TechDataCachePostfix")));
            harmony.Patch(AccessTools.Method(typeof(TechData), "Cache"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(TechDataPatcher), "TechDataCachePostfix")));

            Logger.Log("CraftDataPatcher is done.", LogLevel.Debug);
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
                    Console.WriteLine(TechData.entries[techType]);
                    TechData.entries[techType] = CustomTechData[techType];
                    Logger.Log($"{techType} TechType already existed in the CraftData.techData dictionary. Original value was replaced.", LogLevel.Warn);
                    replaced++;
                    Console.WriteLine("Replaced Item: " + techType + " " + TechData.Contains(techType));
                }
                else if (!techDataExists)
                {
                    TechData.Add(techType, CustomTechData[techType]);
                    added++;
                    Console.WriteLine("Added Item: " + techType + " " + TechData.Contains(techType));
                }
            }
            if(added>0)
                Logger.Log($"Added {added} new entries to the CraftData.techData dictionary.", LogLevel.Info);
            if(replaced>0)
                Logger.Log($"Replaced {replaced} existing entries to the CraftData.techData dictionary.", LogLevel.Info);
        }
    }
}
