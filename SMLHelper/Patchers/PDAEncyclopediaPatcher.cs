namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using Harmony;
    using Abstract;

    internal class PDAEncyclopediaPatcher : IPatch
    {
        internal static readonly SelfCheckingDictionary<string, PDAEncyclopedia.EntryData> CustomEntryData = new SelfCheckingDictionary<string, PDAEncyclopedia.EntryData>("CustomEntryData");

        public void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Initialize)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PDAEncyclopediaPatcher), nameof(PDAEncyclopediaPatcher.InitializePostfix))));
        }

        private static void InitializePostfix()
        {
            Dictionary<string, PDAEncyclopedia.EntryData> mapping = PDAEncyclopedia.mapping;

            // Add custom entry data
            foreach (KeyValuePair<string, PDAEncyclopedia.EntryData> entry in CustomEntryData)
            {
                if (!mapping.ContainsKey(entry.Key))
                {
                    mapping.Add(entry.Key, entry.Value);
                }
            }
        }
    }
}
