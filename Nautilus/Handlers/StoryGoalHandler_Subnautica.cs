using System;
using Nautilus.MonoBehaviours;
using Nautilus.Patchers;
using Story;
using UnityEngine;

namespace Nautilus.Handlers;

#if SUBNAUTICA
/// <summary>
/// <para>A handler class for interacting with all of the major goal systems in Subnautica, which are essential in the game progression. Utilizes the following systems:</para>
/// <list type="bullet">
///    <item>
///        <term><see cref="ItemGoalTracker"/></term>
///        <description>Completes an <see cref="ItemGoal"/> (or multiple) when an object with the given TechType is picked up, equipped, or crafted through the Mobile Vehicle Bay.</description>
///    </item>
///    <item>
///        <term><see cref="BiomeGoalTracker"/></term>
///        <description>Completes a <see cref="BiomeGoal"/> when the player stays in a given biome for a specified period of time.</description>
///    </item>
///    <item>
///        <term><see cref="LocationGoalTracker"/></term>
///        <description>Completes a <see cref="LocationGoal"/> when the player stays within range of a certain position for a specified period of time.</description>
///    </item>
///    <item>
///        <term><see cref="CompoundGoalTracker"/></term>
///        <description>Completes a <see cref="CompoundGoal"/> when all required "precondition" goals have been completed.</description>
///    </item>
///    <item>
///        <term><see cref="OnGoalUnlockTracker"/></term>
///        <description>Handles the completion of a goal with the data defined in an <see cref="OnGoalUnlock"/> object. Defines the unlocked blueprints, signals, items, and achievements that should be unlocked.</description>
///    </item>
///    <item>
///        <term><see cref="StoryGoalCustomEventHandler"/></term>
///        <description>Handles arbitrary code that is executed after completing a goal, for use in more specific story events.</description>
///    </item>
/// </list>
/// </summary>
public static class StoryGoalHandler
{
    /// <summary>
    /// <para>Registers a goal that is completed when an object with the given TechType is picked up, equipped, or crafted through the Mobile Vehicle Bay.</para>
    /// <para><b>Important:</b> This method can be called <b>as many times as needed</b> to add different goals to the same TechType.</para>
    /// </summary>
    /// <param name="key">The unique identifier, required for all types of StoryGoals.</param>
    /// <param name="goalType">If assigned a value other than 'Story', this will determine the automatic response to being triggered. Can add a PDA log, Radio message or Databank entry.</param>
    /// <param name="delay">StoryGoal listeners will not be notified until this many seconds after the goal is completed.</param>
    /// <param name="techType">The TechType that causes this goal to trigger, when picked up, equipped or crafted through the Mobile Vehicle Bay.</param>
    /// <returns>The registered <see cref="ItemGoal"/>.</returns>
    public static ItemGoal RegisterItemGoal(string key, Story.GoalType goalType, TechType techType, float delay = 0f)
    {
        var goal = new ItemGoal() { key = key, goalType = goalType, delay = delay, techType = techType };
        StoryGoalPatcher.ItemGoals.Add(goal);

        if (CustomStoryGoalManager.Instance)
        {
            CustomStoryGoalManager.Instance.AddImmediately(goal);
        }

        return goal;
    }

    /// <summary>
    /// Registers a goal that is completed when the player stays in a given biome for a specified period of time.
    /// </summary>
    /// <param name="key">The unique identifier, required for all types of StoryGoals.</param>
    /// <param name="goalType">If assigned a value other than 'Story', this will determine the automatic response to being triggered. Can add a PDA log, Radio message or Databank entry.</param>
    /// <param name="delay">StoryGoal listeners will not be notified until this many seconds after the goal is completed.</param>
    /// <param name="biomeName">The biome that must be entered to trigger this goal.</param>
    /// <param name="minStayDuration">The minimum amount of time the player must stay in the given biome.</param>
    /// <returns>The registered <see cref="BiomeGoal"/>.</returns>
    public static BiomeGoal RegisterBiomeGoal(string key, Story.GoalType goalType, string biomeName, float minStayDuration, float delay = 0f)
    {
        var goal = new BiomeGoal() { key = key, goalType = goalType, delay = delay, biome = biomeName, minStayDuration = minStayDuration };
        StoryGoalPatcher.BiomeGoals.Add(goal);
        
        if (CustomStoryGoalManager.Instance)
        {
            CustomStoryGoalManager.Instance.AddImmediately(goal);
        }

        return goal;
    }

    /// <summary>
    /// Registers a goal that is completed when the player stays within range of a certain position for a specified period of time.
    /// </summary>
    /// <param name="key">The unique identifier, required for all types of StoryGoals.</param>
    /// <param name="goalType">If assigned a value other than 'Story', this will determine the automatic response to being triggered. Can add a PDA log, Radio message or Databank entry.</param>
    /// <param name="delay">StoryGoal listeners will not be notified until this many seconds after the goal is completed.</param>
    /// <param name="position">The center of the sphere that must be occupied.</param>
    /// <param name="range">The radius of the sphere that must be occupied.</param>
    /// <param name="minStayDuration">The minimum amount of time the player must stay for this goal to be completed.</param>
    /// <returns>The registered <see cref="LocationGoal"/>.</returns>
    public static LocationGoal RegisterLocationGoal(string key, Story.GoalType goalType, Vector3 position, float range, float minStayDuration, float delay = 0f)
    {
        var goal = new LocationGoal() { key = key, goalType = goalType, delay = delay, position = position, range = range, minStayDuration = minStayDuration };
        StoryGoalPatcher.LocationGoals.Add(goal);
        
        if (CustomStoryGoalManager.Instance)
        {
            CustomStoryGoalManager.Instance.AddImmediately(goal);
        }

        return goal;
    }

    /// <summary>
    /// Registers a goal that is completed when all required "precondition" goals have been completed
    /// </summary>
    /// <param name="key">The unique identifier, required for all types of StoryGoals.</param>
    /// <param name="goalType">If assigned a value other than 'Story', this will determine the automatic response to being triggered. Can add a PDA log, Radio message or Databank entry.</param>
    /// <param name="delay">StoryGoal listeners will not be notified until this many seconds after the goal is completed.</param>
    /// <param name="requiredGoals">The list of all goals that must be completed before this goal is marked as complete.</param>
    /// <returns>The registered <see cref="CompoundGoal"/>.</returns>
    public static CompoundGoal RegisterCompoundGoal(string key, Story.GoalType goalType, float delay, params string[] requiredGoals)
    {
        var goal = new CompoundGoal() { key = key, goalType = goalType, delay = delay, preconditions = requiredGoals };
        StoryGoalPatcher.CompoundGoals.Add(goal);
        
        if (CustomStoryGoalManager.Instance)
        {
            CustomStoryGoalManager.Instance.AddImmediately(goal);
        }

        return goal;
    }

    /// <summary>
    /// <para>Registers a new <see cref="OnGoalUnlock"/> object for an existing goal. Handles complex actions that occur with the goal's completion.</para>
    /// <para><b>Important:</b> Since these are stored in a dictionary, only <b>one</b> <see cref="OnGoalUnlock"/> object can be added for each specific goal key. Therefore, be careful when adding unlock data to base-game features.</para>
    /// </summary>
    /// <param name="goal">The goal that is associated with this action.</param>
    /// <param name="blueprints">Array of blueprints that are unlocked alongside the given goal. The class has no constructor, so make sure you assign every field properly.</param>
    /// <param name="signals">Array of signals that are unlocked alongside the given goal. The class has no constructor, so make sure you assign every field properly.</param>
    /// <param name="items">Array of items that are unlocked alongside the given goal. The class has no constructor, so make sure you assign every field properly.</param>
    /// <param name="achievements">Array of achievements that are unlocked alongside the given goal.</param>
    /// <returns>The registered <see cref="OnGoalUnlock"/> object.</returns>
    public static void RegisterOnGoalUnlockData(string goal, UnlockBlueprintData[] blueprints = null, UnlockSignalData[] signals = null, UnlockItemData[] items = null, GameAchievements.Id[] achievements = null)
    {
        var onGoalUnlock = new OnGoalUnlock()
        {
            goal = goal,
            blueprints = blueprints ?? Array.Empty<UnlockBlueprintData>(),
            signals = signals ?? Array.Empty<UnlockSignalData>(),
            items = items ?? Array.Empty<UnlockItemData>(),
            achievements = achievements ?? Array.Empty<GameAchievements.Id>()
        };
        
        StoryGoalPatcher.OnGoalUnlocks.Add(onGoalUnlock);
        
        if (CustomStoryGoalManager.Instance)
        {
            CustomStoryGoalManager.Instance.AddImmediately(onGoalUnlock);
        }
    }

    /// <summary>
    /// Registers a given <see cref="Action"/> to be performed when its associated goal is completed.
    /// </summary>
    /// <param name="key">The key of the goal that triggers the <paramref name="customEventCallback"/>.</param>
    /// <param name="customEventCallback">The method that is called when the associated goal is completed. The name of the goal will be passed as a parameter.</param>
    public static void RegisterCustomEvent(string key, Action customEventCallback)
    {
        CustomStoryGoalManager.StoryGoalCustomEvents.GetOrAddNew(key).Add(customEventCallback);
    }

    /// <summary>
    /// Unregisters a custom event.
    /// </summary>
    /// <param name="key">The key of the goal that triggers the <paramref name="customEventCallback"/>.</param>
    /// <param name="customEventCallback">The method to unregister.</param>
    public static void UnregisterCustomEvent(string key, Action customEventCallback)
    {
        if (CustomStoryGoalManager.StoryGoalCustomEvents.TryGetValue(key, out var callbacks) && callbacks is { })
        {
            callbacks.Remove(customEventCallback);
        }
    }
}
#endif