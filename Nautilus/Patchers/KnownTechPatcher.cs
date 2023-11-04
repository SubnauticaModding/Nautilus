namespace Nautilus.Patchers;

using System;
using System.Collections.Generic;
using HarmonyLib;
using Handlers;
using Utility;

internal class KnownTechPatcher
{
    private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

    internal static HashSet<TechType> UnlockedAtStart = new();
    internal static HashSet<TechType> LockedWithNoUnlocks = new();
    internal static HashSet<TechType> HardLocked = new();
    internal static HashSet<TechType> RemovalTechs = new();
    internal static Dictionary<string, List<TechType>> DefaultRemovalTechs = new();
    internal static IDictionary<TechType, KnownTech.AnalysisTech> AnalysisTech = new SelfCheckingDictionary<TechType, KnownTech.AnalysisTech>("AnalysisTech", AsStringFunction);
    internal static IDictionary<TechType, List<TechType>> BlueprintRequirements = new SelfCheckingDictionary<TechType, List<TechType>>("BlueprintRequirements", AsStringFunction);
    internal static IDictionary<TechType, KnownTech.CompoundTech> CompoundTech = new SelfCheckingDictionary<TechType, KnownTech.CompoundTech>("CompoundTech", AsStringFunction);
    internal static IDictionary<TechType, List<TechType>> RemoveFromSpecificTechs = new SelfCheckingDictionary<TechType, List<TechType>>("RemoveFromSpecificTechs", AsStringFunction);

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
        if (!KnownTech.initialized)
        {
            return;
        }

        var knownTech = KnownTech.knownTech;
        var knownCompound = KnownTech.knownCompoundTech;
        var pdaData = UnityEngine.Object.Instantiate(Player.main.pdaData);
        KnownTech.Initialize(pdaData);
        KnownTech.compoundTech = KnownTech.ValidateCompoundTech(pdaData.compoundTech);
        KnownTech.analysisTech = KnownTech.ValidateAnalysisTech(pdaData.analysisTech);
        KnownTech.knownTech.AddRange(knownTech);
        knownCompound.ForEach(x => KnownTech.knownCompoundTech.Add(x.Key, x.Value));
        KnownTech.AddRange(pdaData.defaultTech, false);
    }

    private static void InitializePrefix(PDAData data)
    {
        foreach (var unlockedTech in UnlockedAtStart)
        {
            if (!data.defaultTech.Contains(unlockedTech))
            {
                data.defaultTech.Add(unlockedTech);
                InternalLogger.Debug($"Setting {unlockedTech.AsString()} to be unlocked at start.");
            }
        }

        foreach (var removalTechsByMod in DefaultRemovalTechs)
        {
            foreach (var removalTech in removalTechsByMod.Value)
            {
                if (data.defaultTech.Remove(removalTech))
                {
                    InternalLogger.Debug($"{removalTechsByMod.Key} removed {removalTech.AsString()} from unlocking at start.");
                }
            }
        }

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
                InternalLogger.Error($"TechType '{blueprintRequirements.Key.AsString()}' does not have an analysis tech. Cancelling requirement addition for TechTypes '{blueprintRequirements.Value.Join()}'.");
                continue;
            }
            
            InternalLogger.Debug($"Adding TechTypes to be unlocked by {blueprintRequirements.Key}: {blueprintRequirements.Value.Join((techType) => techType.AsString())}");
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
        
        foreach (var analysisTech in data.analysisTech)
        {
            foreach (var removalTech in RemovalTechs)
            {
                if (analysisTech.techType == removalTech)
                {
                    continue;
                }
                
                if (analysisTech.unlockTechTypes.Remove(removalTech))
                {
                    InternalLogger.Debug($"RemovalTechs: Removed unlockTechType '{removalTech}' from '{analysisTech.techType}' AnalysisTech.");
                }
            }

            if (RemoveFromSpecificTechs.TryGetValue(analysisTech.techType, out var techsToRemove))
            {
                foreach (var removalTech in techsToRemove)
                {
                    if (analysisTech.unlockTechTypes.Remove(removalTech))
                    {
                        InternalLogger.Debug($"RemoveFromSpecificTechs: Removed unlockTechType '{removalTech}' from '{analysisTech.techType}' AnalysisTech.");
                    }
                }
            }
        }
    }

    private static void GetAllUnlockablesPostfix(HashSet<TechType> __result)
    {
        var filtered = CraftData.FilterAllowed(LockedWithNoUnlocks);
        __result.AddRange(filtered);
        __result.RemoveRange(HardLocked);
    }
}