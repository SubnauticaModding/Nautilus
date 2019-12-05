namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using System;
    using UWE;

    internal class WorldEntityDatabasePatcher
    {
        internal static SelfCheckingDictionary<string, WorldEntityInfo> CustomWorldEntityInfos = new SelfCheckingDictionary<string, WorldEntityInfo>("CustomWorldEntityInfo");

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(WorldEntityDatabase), "TryGetInfo"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(WorldEntityDatabasePatcher), "Prefix")));
        }

        private static bool Prefix(string classId, ref WorldEntityInfo info, ref bool __result)
        {
            foreach(var entry in CustomWorldEntityInfos)
            {
                if(entry.Key == classId)
                {
                    __result = true;
                    info = entry.Value;
                    return false;
                }
            }

            return true;
        }
    }
}
