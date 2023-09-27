using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class KnownTechPatcher
{
    private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

    internal static HashSet<TechType> UnlockedAtStart = new();
    internal static HashSet<TechType> LockedWithNoUnlocks = new();
    internal static IDictionary<TechType, KnownTech.AnalysisTech> AnalysisTech = new SelfCheckingDictionary<TechType, KnownTech.AnalysisTech>("AnalysisTech", AsStringFunction);
    internal static IDictionary<TechType, KnownTech.CompoundTech> CompoundTech = new SelfCheckingDictionary<TechType, KnownTech.CompoundTech>("CompoundTech", AsStringFunction);
    internal static IDictionary<TechType, List<TechType>> RemoveFromSpecificTechs = new SelfCheckingDictionary<TechType, List<TechType>>("RemoveFromSpecificTechs", AsStringFunction);
    internal static List<TechType> RemovalTechs = new();

    private static FMODAsset UnlockSound;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(KnownTech), nameof(KnownTech.Initialize)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(KnownTechPatcher), nameof(InitializePrefix))));
        
        harmony.Patch(AccessTools.Method(typeof(KnownTech), nameof(KnownTech.GetAllUnlockables)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(KnownTechPatcher), nameof(GetAllUnlockablesPostfix))));
    }

    internal static void InitializePrefix(PDAData data)
    {
        if (KnownTech.initialized)
        {
            return;
        }

        data.defaultTech.AddRange(KnownTechPatcher.UnlockedAtStart);

        foreach (var tech in KnownTechPatcher.AnalysisTech.Values)
        {
            data.defaultTech.Remove(tech.techType);

            var index = data.analysisTech.FindIndex(analysisTech => analysisTech.techType == tech.techType);
            if (index == -1)
            {
                InternalLogger.Debug($"Adding analysisTech for {tech.techType}");
                data.analysisTech.Add(tech);
            }
            else
            {
                InternalLogger.Debug($"Replacing analysisTech for {tech.techType}");
                data.analysisTech[index] = tech;
            }
        }

        foreach (var tech in KnownTechPatcher.CompoundTech.Values)
        {
            data.defaultTech.Remove(tech.techType);

            var index = data.compoundTech.FindIndex(compoundTech => compoundTech.techType == tech.techType);
            if (index == -1)
            {
                InternalLogger.Debug($"Adding compoundTech for {tech.techType}");
                data.compoundTech.Add(tech);
            }
            else
            {
                InternalLogger.Debug($"Replacing compoundTech for {tech.techType}");
                data.compoundTech[index] = tech;
            }
        }
    }

    private static void GetAllUnlockablesPostfix(HashSet<TechType> __result)
    {
        var filtered = CraftData.FilterAllowed(LockedWithNoUnlocks);
        __result.AddRange(filtered);
    }
}