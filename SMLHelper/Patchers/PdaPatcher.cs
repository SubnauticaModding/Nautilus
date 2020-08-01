namespace SMLHelper.V2.Patchers
{
    using HarmonyLib;
    using System.Collections.Generic;

    // Special thanks to Gorillazilla9 for sharing this method of fragment count patching
    // https://github.com/Gorillazilla9/SubnauticaFragReqBoost/blob/master/PDAScannerPatcher.cs
    internal class PDAPatcher
    {
        internal static readonly Dictionary<TechType, int> FragmentCount = new Dictionary<TechType, int>();
        internal static readonly Dictionary<TechType, float> FragmentScanTime = new Dictionary<TechType, float>();
        internal static readonly SelfCheckingDictionary<TechType, PDAScanner.EntryData> CustomEntryData = new SelfCheckingDictionary<TechType, PDAScanner.EntryData>("CustomEntryData");

        private static readonly Dictionary<TechType, PDAScanner.EntryData> BlueprintToFragment = new Dictionary<TechType, PDAScanner.EntryData>();

        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PDAScanner), nameof(PDAScanner.Initialize)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PDAPatcher), nameof(PDAPatcher.InitializePostfix))));

            Logger.Log($"PDAPatcher is done.", LogLevel.Debug);
        }

        private static void InitializePostfix()
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
            foreach(KeyValuePair<TechType, PDAScanner.EntryData> entry in CustomEntryData)
            {
                if(!mapping.ContainsKey(entry.Key))
                {
                    PDAScanner.mapping.Add(entry.Key, entry.Value);
                }
                else
                {
                    Logger.Warn($"PDAScanner already Contains EntryData for TechType: {entry.Key.AsString()}, Unable to Overwrite.");
                }
            }

            // Update fragment totals
            foreach(KeyValuePair<TechType, int> fragmentEntry in FragmentCount)
            {
                if(mapping.ContainsKey(fragmentEntry.Key)) // Lookup by techtype of fragment
                {
                    mapping[fragmentEntry.Key].totalFragments = fragmentEntry.Value;
                }
                else if(BlueprintToFragment.TryGetValue(fragmentEntry.Key, out PDAScanner.EntryData entryData)) // Lookup by blueprint techtype
                {
                    entryData.totalFragments = fragmentEntry.Value;
                }
                else
                {
                    Logger.Log($"Warning: TechType {fragmentEntry.Key} not known in PDAScanner.EntryData", LogLevel.Warn);
                }
            }

            // Update scan times
            foreach(KeyValuePair<TechType, float> fragmentEntry in FragmentScanTime)
            {
                if(mapping.ContainsKey(fragmentEntry.Key)) // Lookup by techtype of fragment
                {
                    mapping[fragmentEntry.Key].scanTime = fragmentEntry.Value;
                }
                else if(BlueprintToFragment.TryGetValue(fragmentEntry.Key, out PDAScanner.EntryData entryData)) // Lookup by blueprint techtype
                {
                    entryData.scanTime = fragmentEntry.Value;
                }
                else
                {
                    Logger.Log($"Warning: TechType {fragmentEntry.Key} not known in PDAScanner.EntryData", LogLevel.Warn);
                }
            }
        }
    }
}
