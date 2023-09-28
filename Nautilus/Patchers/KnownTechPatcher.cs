using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class KnownTechPatcher
{
    private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

    internal static HashSet<TechType> UnlockedAtStart = new();
    internal static HashSet<TechType> LockedWithNoUnlocks = new();
    internal static IDictionary<TechType, KnownTech.AnalysisTech> AnalysisTech = new SelfCheckingDictionary<TechType, KnownTech.AnalysisTech>("AnalysisTech", AsStringFunction);
    internal static IDictionary<TechType, List<TechType>> BlueprintRequirements = new SelfCheckingDictionary<TechType, List<TechType>>("BlueprintRequirements", AsStringFunction);
    internal static IDictionary<TechType, KnownTech.CompoundTech> CompoundTech = new SelfCheckingDictionary<TechType, KnownTech.CompoundTech>("CompoundTech", AsStringFunction);
    internal static IDictionary<TechType, List<TechType>> RemoveFromSpecificTechs = new SelfCheckingDictionary<TechType, List<TechType>>("RemoveFromSpecificTechs", AsStringFunction);
    internal static List<TechType> RemovalTechs = new();

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(KnownTech), nameof(KnownTech.Initialize)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(KnownTechPatcher), nameof(InitializePrefix))));
        
        harmony.Patch(AccessTools.Method(typeof(KnownTech), nameof(KnownTech.GetAllUnlockables)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(KnownTechPatcher), nameof(GetAllUnlockablesPostfix))));
    }

    internal static void Reinitialize()
    {
        // At this point the KnownTech stuff haven't been initialized yet, so no need to re-patch.
        if (!Player.main)
        {
            return;
        }

        var pdaData = Player.main.pdaData;
        InitializePrefix(pdaData);
        KnownTech.AddRange(pdaData.defaultTech, false);
        KnownTech.compoundTech = KnownTech.ValidateCompoundTech(pdaData.compoundTech);
        KnownTech.analysisTech = KnownTech.ValidateAnalysisTech(pdaData.analysisTech);
    }

    private static void InitializePrefix(PDAData data)
    {
        data.defaultTech.AddRange(UnlockedAtStart);

        foreach (var tech in AnalysisTech.Values)
        {
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

            if (tech.unlockSound == null)
            {
                tech.unlockSound = KnownTechHandler.DefaultUnlockData.BlueprintUnlockSound;
            }
        }

        foreach (var blueprintRequirements in BlueprintRequirements)
        {
            var index = data.analysisTech.FindIndex(tech => tech.techType == blueprintRequirements.Key);
            if (index == -1)
            {
                InternalLogger.Error($"TechType '{blueprintRequirements.Key.AsString()}' does not have an analysis tech. Cancelling requirement addition.");
                continue;
            }
            
            data.analysisTech[index].unlockTechTypes.AddRange(blueprintRequirements.Value);
        }

        foreach (var tech in CompoundTech.Values)
        {
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