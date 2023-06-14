using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Utility;

namespace Nautilus.Patchers;

// Special thanks to Gorillazilla9 for sharing this method of fragment count patching
// https://github.com/Gorillazilla9/SubnauticaFragReqBoost/blob/master/PDAScannerPatcher.cs
internal class PDAPatcher
{
    internal static readonly SelfCheckingDictionary<TechType, int> FragmentCount = new("CustomFragmentCount");
    internal static readonly SelfCheckingDictionary<TechType, float> FragmentScanTime = new("CustomFragmentScanTime");
    internal static readonly SelfCheckingDictionary<TechType, PDAScanner.EntryData> CustomEntryData = new("CustomEntryData");

    private static readonly Dictionary<TechType, PDAScanner.EntryData> BlueprintToFragment = new();

    internal static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(PDAScanner), nameof(PDAScanner.Initialize)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(PDAPatcher), nameof(PDAPatcher.InitializePostfix))));

        InternalLogger.Log($"PDAPatcher is done.", LogLevel.Debug);
    }

    internal static void InitializePostfix()
    {
        BlueprintToFragment.Clear();

        // Direct access to private fields made possible by https://github.com/CabbageCrow/AssemblyPublicizer/
        // See README.md for details.
        Dictionary<TechType, PDAScanner.EntryData> mapping = PDAScanner.mapping;

        // Populate BlueprintToFragment for reverse lookup
        foreach(KeyValuePair<TechType, PDAScanner.EntryData> entry in mapping)
        {
            TechType blueprintTechType = entry.Value.blueprint;

            BlueprintToFragment[blueprintTechType] = entry.Value;
        }

        // Add custom entry data
        foreach(KeyValuePair<TechType, PDAScanner.EntryData> customEntry in CustomEntryData)
        {
            if (!mapping.ContainsKey(customEntry.Key))
            {
                PDAScanner.mapping.Add(customEntry.Key, customEntry.Value);
                InternalLogger.Debug($"Adding PDAScanner EntryData for TechType: {customEntry.Key.AsString()}");
            }
            else
            {
                mapping[customEntry.Key] = customEntry.Value;
                InternalLogger.Debug($"PDAScanner already Contains EntryData for TechType: {customEntry.Key.AsString()}, Overwriting Original.");
            }
        }

        // Update fragment totals
        foreach(KeyValuePair<TechType, int> fragmentEntry in FragmentCount)
        {
            if(mapping.TryGetValue(fragmentEntry.Key, out PDAScanner.EntryData entry)) // Lookup by techtype of fragment
            {
                entry.totalFragments = fragmentEntry.Value;
            }
            else if(BlueprintToFragment.TryGetValue(fragmentEntry.Key, out PDAScanner.EntryData entryData)) // Lookup by blueprint techtype
            {
                entryData.totalFragments = fragmentEntry.Value;
            }
            else
            {
                InternalLogger.Log($"Warning: TechType {fragmentEntry.Key} not known in PDAScanner.EntryData", LogLevel.Warning);
            }
        }

        // Update scan times
        foreach(KeyValuePair<TechType, float> fragmentEntry in FragmentScanTime)
        {
            if(mapping.TryGetValue(fragmentEntry.Key, out PDAScanner.EntryData entry)) // Lookup by techtype of fragment
            {
                entry.scanTime = fragmentEntry.Value;
            }
            else if(BlueprintToFragment.TryGetValue(fragmentEntry.Key, out PDAScanner.EntryData entryData)) // Lookup by blueprint techtype
            {
                entryData.scanTime = fragmentEntry.Value;
            }
            else
            {
                InternalLogger.Log($"Warning: TechType {fragmentEntry.Key} not known in PDAScanner.EntryData", LogLevel.Warning);
            }
        }
    }
}