namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using System.Collections.Generic;

    internal class PDAEncyclopediaPatcher
    {
        internal static readonly SelfCheckingDictionary<string, PDAEncyclopedia.EntryData> CustomEntryData = new SelfCheckingDictionary<string, PDAEncyclopedia.EntryData>("CustomEntryData");

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Initialize)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PDAEncyclopediaPatcher), nameof(PDAEncyclopediaPatcher.InitializePostfix))));
        }

        private static void InitializePostfix()
        {
            Dictionary<string, PDAEncyclopedia.EntryData> mapping = PDAEncyclopedia.mapping;

            // Add custom entry data
            foreach(KeyValuePair<string, PDAEncyclopedia.EntryData> entry in CustomEntryData)
            {
                if(!mapping.ContainsKey(entry.Key))
                {
                    mapping.Add(entry.Key, entry.Value);
                }
                else
                {
                    Logger.Warn($"PDAEncyclopedia already Contains EntryData for Key Value: {entry.Key}, Unable to Overwrite.");
                }
            }
        }
    }
}
