namespace Nautilus.Patchers;

using System;
using System.Collections.Generic;
using System.Linq;
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
    internal static Dictionary<string, HashSet<TechType>> DefaultRemovalTechs = new();
    internal static IDictionary<TechType, KnownTech.AnalysisTech> AnalysisTech = new SelfCheckingDictionary<TechType, KnownTech.AnalysisTech>("AnalysisTech", AsStringFunction);
    internal static IDictionary<TechType, HashSet<TechType>> BlueprintRequirements = new SelfCheckingDictionary<TechType, HashSet<TechType>>("BlueprintRequirements", AsStringFunction);
    internal static IDictionary<TechType, KnownTech.CompoundTech> CompoundTech = new SelfCheckingDictionary<TechType, KnownTech.CompoundTech>("CompoundTech", AsStringFunction);
    internal static IDictionary<TechType, HashSet<TechType>> RemoveFromSpecificTechs = new SelfCheckingDictionary<TechType, HashSet<TechType>>("RemoveFromSpecificTechs", AsStringFunction);

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
        knownCompound
            .Where(tech => !KnownTech.knownCompoundTech.ContainsKey(tech.Key))
            .ForEach(tech => KnownTech.knownCompoundTech.Add(tech.Key, tech.Value));
        KnownTech.AddRange(pdaData.defaultTech, false);
    }

    private static void InitializePrefix(PDAData data)
    {
        // This needs to be done first as this list was supposed to be only for game unlocks.
        // Mod unlocks requesting removal are already processed in the KnownTechHandler at the time of the request.
        // Mods trying to remove other mods unlocks must make sure to run after the original mod has already added them.
        foreach (var removalTech in RemovalTechs)
        {
            if (data.compoundTech.RemoveAll((x)=> x.techType == removalTech) > 0)
                InternalLogger.Debug($"RemovalTechs: Removed compoundTech for '{removalTech}'");

            if (data.analysisTech.RemoveAll((x) => x.techType == removalTech) > 0)
                InternalLogger.Debug($"RemovalTechs: Removed analysisTech for '{removalTech}'");

            foreach (var analysisTech in data.analysisTech)
            {
                if (analysisTech.unlockTechTypes.Remove(removalTech))
                {
                    InternalLogger.Debug($"RemovalTechs: Removed unlockTechType '{removalTech}' from '{analysisTech.techType}' AnalysisTech.");
                }
            }
        }

        // Process removals of specific game unlocks as requested by mods.
        // Mod unlocks requesting removal are already processed in the KnownTechHandler at the time of the request.
        // Mods trying to remove other mods unlocks must make sure to run after the original mod has already added them.
        foreach (var analysisTech in data.analysisTech)
        {
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

        // remove all default unlocks specified by mods.
        // Mod unlocks requesting removal are already processed in the KnownTechHandler at the time of the request.
        // Mods trying to remove other mods unlocks must make sure to run after the original mod has already added them.
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

        // Add all default unlocks set by mods.
        foreach (var unlockedTech in UnlockedAtStart)
        {
            if (!data.defaultTech.Contains(unlockedTech))
            {
                data.defaultTech.Add(unlockedTech);
                InternalLogger.Debug($"Setting {unlockedTech.AsString()} to be unlocked at start.");
            }
        }

        // Add or modify analysisTechs as requested by mods.
        foreach (var tech in AnalysisTech.Values)
        {
            var index = data.analysisTech.FindIndex(analysisTech => analysisTech.techType == tech.techType);
            if (index == -1)
            {
                InternalLogger.Debug($"Adding analysisTech for {tech.techType}");

                if (tech.unlockSound == null)
                    tech.unlockSound = KnownTechHandler.DefaultUnlockData.BlueprintUnlockSound;

                data.analysisTech.Add(tech);
            }
            else
            {
                InternalLogger.Debug($"Altering original analysisTech for {tech.techType}");
                var existingEntry = data.analysisTech[index];

                existingEntry.unlockMessage = tech.unlockMessage ?? existingEntry.unlockMessage;
                existingEntry.unlockSound = tech.unlockSound ?? existingEntry.unlockSound;
                existingEntry.unlockPopup = tech.unlockPopup ?? existingEntry.unlockPopup;
                existingEntry.unlockTechTypes.AddRange(tech.unlockTechTypes);
#if SUBNAUTICA
                existingEntry.storyGoals.AddRange(tech.storyGoals);
#endif
            }
        }

        // Set unlocks for already existing analysis techs where a AnalysisTech is not created by the mod.
        // tbh I don't know why this was added as we used to just make an analysis tech with the data given by the modder if they did not specify one.
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

        // Add or Replace CompoundTechs as requested by mods.
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
        __result.RemoveRange(HardLocked);
    }
}