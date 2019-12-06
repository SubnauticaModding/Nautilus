namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using System;
    using System.Collections.Generic;

    internal class PDAEncyclopediaPatcher
    {
        internal static SelfCheckingDictionary<string, PDAEncyclopedia.EntryData> CustomEntryData = new SelfCheckingDictionary<string, PDAEncyclopedia.EntryData>("CustomEntryData");

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PDAEncyclopedia), "Initialize"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PDAEncyclopediaPatcher), "InitializePostfix")));
        }

        private static void InitializePostfix()
        {
            Dictionary<string, PDAEncyclopedia.EntryData> mapping = PDAEncyclopedia.mapping;

            // Add custom entry data
            foreach (var entry in CustomEntryData)
            {
                if (!mapping.ContainsKey(entry.Key))
                {
                    mapping.Add(entry.Key, entry.Value);
                }
            }
        }
    }
}
