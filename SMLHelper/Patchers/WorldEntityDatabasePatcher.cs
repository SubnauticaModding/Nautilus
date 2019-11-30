namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using UWE;

    internal class WorldEntityDatabasePatcher
    {
        internal static SelfCheckingDictionary<string, WorldEntityInfo> CustomWorldEntityInfos = new SelfCheckingDictionary<string, WorldEntityInfo>("CustomWorldEntityInfo");

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Constructor(typeof(WorldEntityDatabase)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(WorldEntityDatabasePatcher), "CtorPostfix")));
        }

        private static void CtorPostfix()
        {
            foreach(var customInfo in CustomWorldEntityInfos)
            {
                if(WorldEntityDatabase.main.infos.ContainsKey(customInfo.Key))
                {
                    // TODO:    Allow some sort of functionality
                    //          that allows editing of existing entries
                }
                else
                {
                    WorldEntityDatabase.main.infos.Add(customInfo);
                }
            }
        }
    }
}
