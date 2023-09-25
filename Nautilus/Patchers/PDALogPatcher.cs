using HarmonyLib;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class PDALogPatcher
{
    internal static readonly SelfCheckingDictionary<string, PDALog.EntryData> CustomEntryData = new("CustomEntryData");

    internal static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(PDALog), nameof(PDALog.Initialize)), 
            postfix: new HarmonyMethod(AccessTools.Method(typeof(PDALogPatcher), nameof(InitializePostfix))));
    }

    internal static void InitializePostfix()
    {
        System.Collections.Generic.Dictionary<string, PDALog.EntryData> mapping = PDALog.mapping;

        foreach (System.Collections.Generic.KeyValuePair<string, PDALog.EntryData> entryData in CustomEntryData)
        {
            mapping[entryData.Key] = entryData.Value;
        }
    }
}