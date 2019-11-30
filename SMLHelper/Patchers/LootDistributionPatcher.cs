namespace SMLHelper.V2.Patchers
{
    using Harmony;

    internal class LootDistributionPatcher
    {
        internal static SelfCheckingDictionary<string, LootDistributionData.SrcData> CustomSrcData = new SelfCheckingDictionary<string, LootDistributionData.SrcData>("CustomSrcData");

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(LootDistributionData), "Initialize"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(LootDistributionPatcher), "InitializePostfix")));

            Logger.Log("LootDistributionPatcher is done.", LogLevel.Debug);
        }

        private static void InitializePostfix(LootDistributionData __instance)
        {
            foreach(var entry in CustomSrcData)
            {
                if(__instance.srcDistribution.ContainsKey(entry.Key))
                {
                    // TODO:    Add some sort of functionality that
                    //          allows editing of existing entries.
                }
                else
                {
                    __instance.srcDistribution.Add(entry);
                }
            }
        }
    }
}
