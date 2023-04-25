using System.Collections.Generic;
using HarmonyLib;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class PDAEncyclopediaPatcher
{
    internal static readonly SelfCheckingDictionary<string, PDAEncyclopedia.EntryData> CustomEntryData = new("CustomEntryData");

    internal static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Initialize)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(PDAEncyclopediaPatcher), nameof(PDAEncyclopediaPatcher.InitializePostfix))));
    }

    internal static void InitializePostfix()
    {
        Dictionary<string, PDAEncyclopedia.EntryData> mapping = PDAEncyclopedia.mapping;

        // Add custom entry data
        foreach(KeyValuePair<string, PDAEncyclopedia.EntryData> customEntry in CustomEntryData)
        {
            if (!mapping.ContainsKey(customEntry.Key))
            {
                mapping.Add(customEntry.Key, customEntry.Value);
                InternalLogger.Debug($"Adding PDAEncyclopedia EntryData for Key Value: {customEntry.Key}.");
            }
            else
            {
                mapping[customEntry.Key] = customEntry.Value;
                InternalLogger.Debug($"PDAEncyclopedia already Contains EntryData for Key Value: {customEntry.Key}, Overwriting Original.");
            }
        }
    }
}