using HarmonyLib;

namespace SMLHelper.V2.Patchers
{
    internal class PDALogPatcher
    {
        internal static readonly SelfCheckingDictionary<string, PDALog.EntryData> CustomEntryData = new("CustomEntryData");

        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PDALog), nameof(PDALog.Initialize)), 
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PDALogPatcher), nameof(InitializePostfix))));
        }

        private static void InitializePostfix()
        {
            var mapping = PDALog.mapping;

            foreach (var entryData in CustomEntryData)
            {
                mapping[entryData.Key] = entryData.Value;
            }
        }
    }
}