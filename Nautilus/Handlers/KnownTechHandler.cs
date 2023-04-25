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
    /// <summary>
    /// Allows you to unlock a TechType on game start.
    /// </summary>
    /// <param name="techType"></param>
    public static void UnlockOnStart(TechType techType)
    {
        KnownTechPatcher.UnlockedAtStart.Add(techType);
    }

    internal static void AddAnalysisTech(KnownTech.AnalysisTech analysisTech)
    {
        if (analysisTech.techType != TechType.None)
        {
            if (KnownTechPatcher.AnalysisTech.TryGetValue(analysisTech.techType, out KnownTech.AnalysisTech existingEntry))
            {
                existingEntry.unlockMessage = existingEntry.unlockMessage ?? analysisTech.unlockMessage;
                existingEntry.unlockSound = existingEntry.unlockSound ?? analysisTech.unlockSound;
                existingEntry.unlockPopup = existingEntry.unlockPopup ?? analysisTech.unlockPopup;
                existingEntry.unlockTechTypes.AddRange(analysisTech.unlockTechTypes);
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
        
        if (uGUI.isMainLevel)
            KnownTechPatcher.InitializePostfix();
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
        
        if (uGUI.isMainLevel)
            KnownTechPatcher.InitializePostfix();
    }

    internal static void RemoveAnalysisSpecific(TechType targetTechType, List<TechType> techTypes)
    {
        foreach (TechType techType in techTypes)
        {
            if (KnownTechPatcher.RemoveFromSpecificTechs.TryGetValue(techType, out List<TechType> types))
            {
                if (!types.Contains(targetTechType))
                {
                    types.Add(targetTechType);
                }
            }
            else
            {
                KnownTechPatcher.RemoveFromSpecificTechs[techType] = new List<TechType>() { targetTechType };
            }
        }
        
        if (uGUI.isMainLevel)
            KnownTechPatcher.InitializePostfix();
    }

    internal static void RemoveAnalysisTechEntry(TechType targetTechType)
    {
        foreach (KnownTech.AnalysisTech tech in KnownTechPatcher.AnalysisTech.Values)
        {
            if (tech.unlockTechTypes.Contains(targetTechType))
            {
                InternalLogger.Debug($"Removed {targetTechType.AsString()} from {tech.techType.AsString()} unlocks that was added by another mod!");
                tech.unlockTechTypes.Remove(targetTechType);
            }
        }

        if (KnownTechPatcher.CompoundTech.TryGetValue(targetTechType, out KnownTech.CompoundTech types))
        {
            InternalLogger.Debug($"Removed Compound Unlock for {targetTechType.AsString()} that was added by another mod!");
            KnownTechPatcher.CompoundTech.Remove(targetTechType);
        }

        if (KnownTechPatcher.UnlockedAtStart.Contains(targetTechType))
        {
            InternalLogger.Debug($"Removed UnlockedAtStart for {targetTechType.AsString()} that was added by another mod!");
            KnownTechPatcher.UnlockedAtStart.Remove(targetTechType);
        }

        if (!KnownTechPatcher.RemovalTechs.Contains(targetTechType))
        {
            KnownTechPatcher.RemovalTechs.Add(targetTechType);
        }
        
        if (uGUI.isMainLevel)
            KnownTechPatcher.InitializePostfix();
    }


    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
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
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string unlockMessage)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, unlockMessage);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
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
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
    /// <param name="unlockSound">The sound that plays when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string unlockMessage, FMODAsset unlockSound)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, unlockMessage, unlockSound, null);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
    /// <param name="unlockSprite">The sprite that shows up when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string unlockMessage, Sprite unlockSprite)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, unlockMessage, null, unlockSprite);
    }

    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
    /// added to the existing AnalysisTech entry unlocks.
    /// </summary>
    /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
    /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
    /// <param name="unlockSound">The sound that plays when you unlock the blueprint.</param>
    /// <param name="unlockSprite">The sprite that shows up when you unlock the blueprint.</param>
    public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, FMODAsset unlockSound, Sprite unlockSprite)
    {
        AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", unlockSound, unlockSprite);
    }
        
    /// <summary>
    /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
    /// If there is already an existing AnalysisTech entry for a TechType, all the TechTypes in <see cref="KnownTech.AnalysisTech.unlockTechTypes"/> will be
    /// added to the existing AnalysisTech entry unlocks.
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
    /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAnalysisTechEntryFromSpecific"/> or <see cref="RemoveAllCurrentAnalysisTechEntry"/>
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
    public static void RemoveAnalysisTechEntryFromSpecific(TechType targetTechType, List<TechType> techTypes)
    {
        RemoveAnalysisSpecific(targetTechType, techTypes);
    }

    /// <summary>
    /// Allows you to remove all unlock entries from a <see cref="TechType"/> to be able to disable or change it to a new unlock.
    /// ***Note: This is patch time specific so the LAST mod to call this on a techtype will be the only one to control what unlocks said type after its use.***
    /// </summary>
    /// <param name="targetTechType">Target <see cref="TechType"/> to remove the unlocks for.</param>
    public static void RemoveAllCurrentAnalysisTechEntry(TechType targetTechType)
    {
        RemoveAnalysisTechEntry(targetTechType);
    }
}