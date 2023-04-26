using System;
using System.Collections.Generic;
using Story;
using UnityEngine;

namespace Nautilus.MonoBehaviours;

internal class CustomStoryGoalManager : MonoBehaviour, IStoryGoalListener
{
    public static CustomStoryGoalManager Instance { get; private set; }
    
    internal static readonly Dictionary<string, List<Action>> StoryGoalCustomEvents = new();

    private ItemGoalTracker _itemGoalTracker;
    private BiomeGoalTracker _biomeGoalTracker;
    private LocationGoalTracker _locationGoalTracker;
    private CompoundGoalTracker _compoundGoalTracker;
    private OnGoalUnlockTracker _onGoalUnlockTracker;
    
    public void AddImmediately<T>(T storyGoal) where T : StoryGoal
    {
        // goals that are already completed have no effect
        if (StoryGoalManager.main.IsGoalComplete(storyGoal.key))
            return;
        
        switch (storyGoal)
        {
            case ItemGoal itemGoal:
                TrackItemGoal(itemGoal);
                break;
            case BiomeGoal biomeGoal:
                TrackBiomeGoal(biomeGoal);
                break;
            case LocationGoal locationGoal:
                TrackLocationGoal(locationGoal);
                break;
            case CompoundGoal compoundGoal:
                TrackCompoundGoal(compoundGoal);
                break;
        }
    }

    public void AddImmediately(OnGoalUnlock onGoalUnlock)
    {
        // make sure to unlock new blueprints and achievements for completed goals
        if (StoryGoalManager.main.IsGoalComplete(onGoalUnlock.goal))
        {
            onGoalUnlock.TriggerBlueprints();
            onGoalUnlock.TriggerAchievements();
            return;
        }

        TrackOnGoalUnlock(onGoalUnlock);
    }

    private void Awake()
    {
        Instance = this;
        _itemGoalTracker = StoryGoalManager.main.itemGoalTracker;
        _biomeGoalTracker = StoryGoalManager.main.biomeGoalTracker;
        _locationGoalTracker = StoryGoalManager.main.locationGoalTracker;
        _compoundGoalTracker = StoryGoalManager.main.compoundGoalTracker;
        _onGoalUnlockTracker = StoryGoalManager.main.onGoalUnlockTracker;
    }

    void IStoryGoalListener.NotifyGoalComplete(string key)
    {
        foreach (var customEvent in StoryGoalCustomEvents)
        {
            if (key == customEvent.Key)
                customEvent.Value.ForEach(x => x?.Invoke());
        }
    }

    // allows the IStoryGoalListener.NotifyGoalComplete method to be called
    private void OnEnable()
    {
        if (StoryGoalManager.main)
        {
            StoryGoalManager.main.AddListener(this);
        }
    }

    // removes the listener when the object is removed, to ensure there are no invalid listeners registered
    private void OnDisable()
    {
        if (StoryGoalManager.main)
        {
            StoryGoalManager.main.RemoveListener(this);
        }
    }
    
    private void TrackItemGoal(ItemGoal itemGoal)
    {
        _itemGoalTracker.goals.GetOrAddNew(itemGoal.techType).Add(itemGoal);
    }
    
    private void TrackBiomeGoal(BiomeGoal biomeGoal)
    {
        _biomeGoalTracker.goals.Add(biomeGoal);
    }
    
    private void TrackLocationGoal(LocationGoal locationGoal)
    {
        _locationGoalTracker.goals.Add(locationGoal);
    }
    
    private void TrackCompoundGoal(CompoundGoal compoundGoal)
    {        
        _compoundGoalTracker.goals.Add(compoundGoal);
    }
    
    private void TrackOnGoalUnlock(OnGoalUnlock onGoalUnlock)
    {
        _onGoalUnlockTracker.goalUnlocks[onGoalUnlock.goal] = onGoalUnlock;
    }
}