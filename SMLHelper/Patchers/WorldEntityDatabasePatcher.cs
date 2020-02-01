namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using Harmony;
    using UWE;
    using Abstract;

    internal class WorldEntityDatabasePatcher : IPatch
    {
        internal static readonly SelfCheckingDictionary<string, WorldEntityInfo> CustomWorldEntityInfos = new SelfCheckingDictionary<string, WorldEntityInfo>("CustomWorldEntityInfo");

        public void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(WorldEntityDatabase), nameof(WorldEntityDatabase.TryGetInfo)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(WorldEntityDatabasePatcher), nameof(WorldEntityDatabasePatcher.Prefix))));
        }

        private static bool Prefix(string classId, ref WorldEntityInfo info, ref bool __result)
        {
            foreach (KeyValuePair<string, WorldEntityInfo> entry in CustomWorldEntityInfos)
            {
                if (entry.Key == classId)
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
