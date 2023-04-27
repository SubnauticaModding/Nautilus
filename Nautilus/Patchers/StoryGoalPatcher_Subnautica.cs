using System.Collections.Generic;
using HarmonyLib;
using Nautilus.MonoBehaviours;
using Story;
using Nautilus.Utility;

namespace Nautilus.Patchers;

#if SUBNAUTICA
internal static class StoryGoalPatcher
{
    internal static readonly List<ItemGoal> ItemGoals = new();
    internal static readonly List<BiomeGoal> BiomeGoals = new();
    internal static readonly List<LocationGoal> LocationGoals = new();
    internal static readonly List<CompoundGoal> CompoundGoals = new();
    internal static readonly List<OnGoalUnlock> OnGoalUnlocks = new();

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(StoryGoalPatcher));
        SaveUtils.RegisterOnQuitEvent(() => LocationGoals.ForEach(x => x.timeRangeEntered = -1f));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StoryGoalManager), nameof(StoryGoalManager.Awake))]
    private static void StoryGoalManagerAwakePostfix(StoryGoalManager __instance)
    {
        __instance.gameObject.EnsureComponent<CustomStoryGoalManager>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemGoalTracker), nameof(ItemGoalTracker.Start))]
    private static void ItemGoalTrackerStartPostfix(ItemGoalTracker __instance)
    {
        foreach (var itemGoal in ItemGoals)
        {
            var techType = itemGoal.techType;
            __instance.goals.GetOrAddNew(techType).Add(itemGoal);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BiomeGoalTracker), nameof(BiomeGoalTracker.Start))]
    private static void BiomeGoalTrackerStartPostfix(BiomeGoalTracker __instance)
    {
        foreach (var biomeGoal in BiomeGoals)
        {
            __instance.goals.Add(biomeGoal);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocationGoalTracker), nameof(LocationGoalTracker.Start))]
    private static void LocationGoalTrackerStartPostfix(LocationGoalTracker __instance)
    {
        foreach (var locationGoal in LocationGoals)
        {
            __instance.goals.Add(locationGoal);
        }
    }

    // must be a prefix because we want to add all these goals BEFORE NotifyGoalComplete is called at the end of the method
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CompoundGoalTracker), nameof(CompoundGoalTracker.Initialize))]
    private static void CompoundGoalTrackerInitializePrefix(CompoundGoalTracker __instance, HashSet<string> completedGoals)
    {
        foreach (var compoundGoal in CompoundGoals)
        {
            if (!completedGoals.Contains(compoundGoal.key))
            {
                __instance.goals.Add(compoundGoal);
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OnGoalUnlockTracker), nameof(OnGoalUnlockTracker.Initialize))]
    private static void OnGoalUnlockTrackerInitializePostfix(OnGoalUnlockTracker __instance, HashSet<string> completedGoals)
    {
        foreach (var onGoalUnlock in OnGoalUnlocks)
        {
            if (!completedGoals.Contains(onGoalUnlock.goal))
            {
                __instance.goalUnlocks[onGoalUnlock.goal] = onGoalUnlock;
            }
        }
    }
}
#endif