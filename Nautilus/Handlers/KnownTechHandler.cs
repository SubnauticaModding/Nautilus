using System.Collections.Generic;
using System.Linq;
using Nautilus.Patchers;
using Nautilus.Utility;
using Story;
using UnityEngine;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for configuring custom unlocking conditions for item blueprints.
/// </summary>
public static class KnownTechHandler
{
    private static void Reinitialize()
    {
        KnownTechPatcher.Reinitialize();
    }
    
    /// <summary>
    /// Allows you to unlock a TechType on game start.
    /// </summary>
    /// <param name="techType">The TechType to unlock at start.</param>
    public static void UnlockOnStart(TechType techType)
    {
        KnownTechPatcher.UnlockedAtStart.Add(techType);

        bool removed = false;
        var removalList = new List<string>();
        KnownTechPatcher.DefaultRemovalTechs.ForEach((x) => 
        {
            if (x.Value.Remove(techType))
            {
                removed = true;
                InternalLogger.Debug($"Removed {techType.AsString()} from {x.Key}'s DefaultRemovalTechs");
                if (x.Value.Count > 0 )
                {
                    removalList.Add(x.Key);
                }
            }
        });

        foreach (string key in removalList)
            KnownTechPatcher.DefaultRemovalTechs.Remove(key);

        if (removed)
            InternalLogger.Debug($"Set {techType.AsString()} to Unlock On Start ");
        Reinitialize();
    }

    /// <summary>
    /// Unlocks the <paramref name="blueprint"/> when the <paramref name="requirement"/> tech type is unlocked.
    /// </summary>
    /// <param name="blueprint">The blueprint to unlock.</param>
    /// <param name="requirement">The tech type that will unlock the specified blueprint once unlocked.</param>
    public static void AddRequirementForUnlock(TechType blueprint, TechType requirement)
    {
        KnownTechPatcher.BlueprintRequirements.GetOrAddNew(requirement).Add(blueprint);
        Reinitialize();
    }

    /// <summary>
    /// Makes the specified tech type hard locked. Hard locking means that the tech type will not be unlocked via the unlockall command and will not be unlocked by default
    /// in creative. 
    /// </summary>
    /// <remarks>Calling this method will remove the specified item from being unlocked at start.</remarks>
    /// <param name="techType">The tech type to set as hard locked.</param>
    /// <seealso cref="UnlockOnStart"/>
    public static void SetHardLocked(TechType techType)
    {
        KnownTechPatcher.HardLocked.Add(techType);
        RemoveDefaultUnlock(techType);
        Reinitialize();
    }

    internal static void AddAnalysisTech(KnownTech.AnalysisTech analysisTech)
    {
        if (analysisTech.techType != TechType.None)
        {
            if (KnownTechPatcher.AnalysisTech.TryGetValue(analysisTech.techType, out KnownTech.AnalysisTech existingEntry))
            {
                existingEntry.unlockMessage = analysisTech.unlockMessage ?? existingEntry.unlockMessage;
                existingEntry.unlockSound = analysisTech.unlockSound ?? existingEntry.unlockSound;
                existingEntry.unlockPopup = analysisTech.unlockPopup ?? existingEntry.unlockPopup;
                existingEntry.unlockTechTypes.AddRange(analysisTech.unlockTechTypes);
#if SUBNAUTICA
                analysisTech.storyGoals ??= existingEntry.storyGoals ?? new();
#endif
            }
            else
            {
#if SUBNAUTICA
                analysisTech.storyGoals ??= new();
#endif
                
                KnownTechPatcher.AnalysisTech.Add(analysisTech.techType, analysisTech);
            }
        }
        else
        {
            InternalLogger.Error("Cannot Add Unlock to TechType.None!");
        }
        
        Reinitialize();
    }

    internal static void AddAnalysisTech(
        TechType techTypeToBeAnalysed, 
        IEnumerable<TechType> techTypesToUnlock, 
        string unlockMessage = "NotificationBlueprintUnlocked",
        FMODAsset unlockSound = null, 
        UnityEngine.Sprite unlockSprite = null
#if SUBNAUTICA
        ,List<StoryGoal> storyGoals = null
#endif
    )
    {
        AddAnalysisTech(new KnownTech.AnalysisTech
        {
            techType = techTypeToBeAnalysed,
            unlockMessage = unlockMessage,
            unlockSound = unlockSound,
            unlockPopup = unlockSprite,
            unlockTechTypes = techTypesToUnlock.ToList(),
#if SUBNAUTICA
            storyGoals = storyGoals,
#endif
        });
    }

    internal static void AddCompoundUnlock(TechType techType, List<TechType> compoundTechsForUnlock)
    {
        if (techType == TechType.None)
        {
            InternalLogger.Error("Cannot Add Unlock to TechType.None!");
            return;
        }

        if (compoundTechsForUnlock.Contains(techType))
        {
            InternalLogger.Error("Cannot Add Compound Unlock that contains itself!");
            return;
        }


        if (KnownTechPatcher.CompoundTech.TryGetValue(techType, out KnownTech.CompoundTech compoundTech))
        {
            InternalLogger.Debug($"Compound Unlock already found for {techType.AsString()}, Overwriting.");
            compoundTech.dependencies = compoundTechsForUnlock;
        }
        else
        {
            InternalLogger.Debug($"Adding Compound Unlock for {techType.AsString()}");
            KnownTechPatcher.CompoundTech.Add(techType, new KnownTech.CompoundTech() { techType = techType, dependencies = compoundTechsForUnlock });
        }
        
        Reinitialize();
    }

    internal static void RemoveAnalysisSpecific(TechType targetTechType, List<TechType> techTypes)
    {
        foreach (TechType techType in techTypes)
        {
            if (KnownTechPatcher.AnalysisTech.TryGetValue(techType, out var analysisTech) && analysisTech.unlockTechTypes.Remove(targetTechType))
                InternalLogger.Debug($"Removed unlock for {targetTechType.AsString()} from {techType} that was added by another mod.");

            if (KnownTechPatcher.RemoveFromSpecificTechs.TryGetValue(techType, out HashSet<TechType> types))
                types.Add(targetTechType);
            else
                KnownTechPatcher.RemoveFromSpecificTechs[techType] = new HashSet<TechType>() { targetTechType };
        }
        
        Reinitialize();
    }

    internal static void RemoveAnalysisTechEntry(TechType targetTechType)
    {
        if (KnownTechPatcher.AnalysisTech.Remove(targetTechType))
        {
            InternalLogger.Debug($"Removed Analysis Tech for {targetTechType.AsString()} that was added by another mod!");
        }

        foreach (KnownTech.AnalysisTech tech in KnownTechPatcher.AnalysisTech.Values)
        {
            if (tech.unlockTechTypes.Remove(targetTechType))
            {
                InternalLogger.Debug($"Removed {targetTechType.AsString()} from {tech.techType.AsString()} unlocks that was added by another mod!");
            }
        }

        if (KnownTechPatcher.CompoundTech.Remove(targetTechType))
        {
            InternalLogger.Debug($"Removed Compound Unlock for {targetTechType.AsString()} that was added by another mod!");
        }

        if (KnownTechPatcher.UnlockedAtStart.Remove(targetTechType))
        {
            InternalLogger.Debug($"Removed UnlockedAtStart for {targetTechType.AsString()} that was added by another mod!");
        }

        KnownTechPatcher.RemovalTechs.Add(targetTechType);
        
        Reinitialize();
    }


    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockMessage">The message that shows up on the right when the blueprint is unlocked. See <see cref="DefaultUnlockData"/> for a list of some valid parameters.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string unlockMessage)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, unlockMessage);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockSound">The sound that plays when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, FMODAsset unlockSound)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", unlockSound);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockSprite">The sprite that shows up when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, Sprite unlockSprite)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", null, unlockSprite);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockMessage">The message that shows up on the right when the blueprint is unlocked. See <see cref="DefaultUnlockData"/> for a list of some valid parameters.</param>
    /// <param name="unlockSound">The sound that plays when you unlock the blueprint. See <see cref="DefaultUnlockData"/> for a list of some valid parameters.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string unlockMessage, FMODAsset unlockSound)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, unlockMessage, unlockSound, null);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockMessage">The message that shows up on the right when the blueprint is unlocked. See <see cref="DefaultUnlockData"/> for a list of some valid parameters.</param>
    /// <param name="unlockSprite">The sprite that shows up when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string unlockMessage, Sprite unlockSprite)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, unlockMessage, null, unlockSprite);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockSound">The sound that plays when you unlock the blueprint. See <see cref="DefaultUnlockData"/> for a list of some valid parameters.</param>
    /// <param name="unlockSprite">The sprite that shows up when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, FMODAsset unlockSound, Sprite unlockSprite)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", unlockSound, unlockSprite);
    }
        
    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in <see cref="KnownTech.AnalysisTech.unlockTechTypes"/> will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="analysisTech">The analysis tech entry to add.</param>
    public static void SetAnalysisTechEntry(KnownTech.AnalysisTech analysisTech)
    {
        AddAnalysisTech(analysisTech);
    }
        
    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockSound">The sound that plays when you unlock the blueprint.</param>
    /// <param name="unlockSprite">The sprite that shows up when you unlock the blueprint.</param>
    /// <param name="storyGoals">The story goals that will be triggered when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, FMODAsset unlockSound, Sprite unlockSprite, List<StoryGoal> storyGoals)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", unlockSound, unlockSprite);
    }

    /// <summary>
    /// Allows you to set up a custom Compound Unlock requiring multiple techtypes to be unlocked before 1 is.
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAllCurrentAnalysisTechEntry"/> before calling this method.
    /// </summary>
    /// <param name="techType"></param>
    /// <param name="compoundTechsForUnlock"></param>
    public static void SetCompoundUnlock(TechType techType, List<TechType> compoundTechsForUnlock)
    {
        AddCompoundUnlock(techType, compoundTechsForUnlock);
    }


    /// <summary>
    /// Allows you to remove unlock entries for a <see cref="TechType"/> from specific entries.
    /// </summary>
    /// <param name="targetTechType">Target <see cref="TechType"/> to remove the unlocks for.</param>
    /// <param name="techTypes">List of <see cref="TechType"/> to remove the targetTechType from.</param>
    /// <seealso cref="RemoveAllCurrentAnalysisTechEntry"/>
    public static void RemoveAnalysisTechEntryFromSpecific(TechType targetTechType, List<TechType> techTypes)
    {
        RemoveAnalysisSpecific(targetTechType, techTypes);
    }

    /// <summary>
    /// Allows you to remove all unlock entries from a <see cref="TechType"/> to be able to disable or change it to a new unlock.
    /// </summary>
    /// <remarks>The unlock entry for analysis techs for the specified <paramref name="targetTechType"/> will not be removed. I.E: This method will not remove self-unlocks.<br/>
    /// To also target unlock entry removal for the specified tech type's analysis tech entry, use <see cref="RemoveAnalysisTechEntryFromSpecific"/> instead.</remarks>
    /// <param name="targetTechType">Target <see cref="TechType"/> to remove the unlocks for.</param>
    /// <seealso cref="RemoveAnalysisTechEntry"/>
    public static void RemoveAllCurrentAnalysisTechEntry(TechType targetTechType)
    {
        RemoveAnalysisTechEntry(targetTechType);
    }

    /// <summary>
    /// Allows you to remove a <see cref="TechType"/> from being unlocked by default.
    /// </summary>
    /// <param name="techType"></param>
    public static void RemoveDefaultUnlock(TechType techType)
    {
        var modName = ReflectionHelper.CallingAssemblyByStackTrace().GetName().Name;
        if (!KnownTechPatcher.DefaultRemovalTechs.TryGetValue(modName, out var techTypes))
            techTypes = new HashSet<TechType>();
        techTypes.Add(techType);

        KnownTechPatcher.DefaultRemovalTechs[modName] = techTypes;
        if (KnownTechPatcher.UnlockedAtStart.Remove(techType))
            InternalLogger.Debug($"Removed Default unlock for {techType} that was added by another mod.");
        Reinitialize();
    }

    /// <summary>
    /// References to generic unlock sounds and unlock messages for the Known Tech system, matching those used in the base game.
    /// </summary>
    public static class DefaultUnlockData
    {
        /// <summary>Message on unlocking new creatures; "NEW LIFEFORM DISCOVERED"</summary>
        public const string NewCreatureDiscoveredMessage = "NotificationCreatureDiscovered";
        
        /// <summary>Sound on unlocking new creatures; "NEW LIFEFORM DISCOVERED"</summary>
        public static FMODAsset NewCreatureDiscoveredSound { get; } = AudioUtils.GetFmodAsset("event:/player/new_creature");

        /// <summary>Message on unlocking new blueprints from picking up items; "NEW BLUEPRINT SYNTHESIZED FROM ALIEN RESOURCE"</summary>
        public const string BlueprintPickupMessage = "NotificationBlueprintPickup";

        /// <summary>Message on unlocking new blueprints from scanning; "NEW BLUEPRINT SYNTHESIZED"</summary>
        public const string BlueprintUnlockMessage = "NotificationBlueprintUnlocked";
        
        /// <summary>Sound on unlocking new blueprints from scanning or picking up items; "NEW BLUEPRINT SYNTHESIZED"</summary>
        public static FMODAsset BlueprintUnlockSound { get; } = AudioUtils.GetFmodAsset("event:/tools/scanner/new_blueprint");

        /// <summary>Basic sound for unlocking items. Not commonly used and typically uses <see cref="BlueprintUnlockMessage"/> for its associated message.</summary>
        public static FMODAsset BasicUnlockSound { get; } = AudioUtils.GetFmodAsset("event:/tools/scanner/new_encyclopediea");
    }
}
