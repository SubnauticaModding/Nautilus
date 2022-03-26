namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;
    using System.Collections.Generic;
    using UnityEngine;
    using Logger = V2.Logger;

    /// <summary>
    /// A handler class for configuring custom unlocking conditions for item blueprints.
    /// </summary>
    public class KnownTechHandler : IKnownTechHandler
    {
        private static readonly KnownTechHandler singleton = new KnownTechHandler();

        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IKnownTechHandler Main => singleton;

        private KnownTechHandler()
        {
            // Hides constructor
        }

        /// <summary>
        /// Allows you to unlock a TechType on game start.
        /// </summary>
        /// <param name="techType"></param>
        public static void UnlockOnStart(TechType techType)
        {
            Main.UnlockOnStart(techType);
        }

        /// <summary>
        /// Allows you to unlock a TechType on game start.
        /// </summary>
        /// <param name="techType"></param>
        void IKnownTechHandler.UnlockOnStart(TechType techType)
        {
            KnownTechPatcher.UnlockedAtStart.Add(techType);
        }

        internal void AddAnalysisTech(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string UnlockMessage = "NotificationBlueprintUnlocked", FMODAsset UnlockSound = null, UnityEngine.Sprite UnlockSprite = null)
        {
            if (techTypeToBeAnalysed != TechType.None)
            {
                if (KnownTechPatcher.AnalysisTech.TryGetValue(techTypeToBeAnalysed, out KnownTech.AnalysisTech existingEntry))
                {
                    existingEntry.unlockMessage = existingEntry.unlockMessage ?? UnlockMessage;
                    existingEntry.unlockSound = existingEntry.unlockSound ?? UnlockSound;
                    existingEntry.unlockPopup = existingEntry.unlockPopup ?? UnlockSprite;
                    existingEntry.unlockTechTypes.AddRange(techTypesToUnlock);
                }
                else
                {
                    KnownTechPatcher.AnalysisTech.Add(techTypeToBeAnalysed, new KnownTech.AnalysisTech()
                    {
                        techType = techTypeToBeAnalysed,
                        unlockMessage = UnlockMessage,
                        unlockSound = UnlockSound,
                        unlockPopup = UnlockSprite,
                        unlockTechTypes = new List<TechType>(techTypesToUnlock)
                    });
                }
            }
            else
            {
                Logger.Error("Cannot Add Unlock to TechType.None!");
            }
        }

        internal void AddCompoundUnlock(TechType techType, List<TechType> compoundTechsForUnlock)
        {
            if (techType == TechType.None)
            {
                Logger.Error("Cannot Add Unlock to TechType.None!");
                return;
            }

            if (compoundTechsForUnlock.Contains(techType))
            {
                Logger.Error("Cannot Add Compound Unlock that contains itself!");
                return;
            }


            if (KnownTechPatcher.CompoundTech.TryGetValue(techType, out KnownTech.CompoundTech compoundTech))
            {
                Logger.Debug($"Compound Unlock already found for {techType.AsString()}, Overwriting.");
                compoundTech.dependencies = compoundTechsForUnlock;
            }
            else
            {
                Logger.Debug($"Adding Compound Unlock for {techType.AsString()}");
                KnownTechPatcher.CompoundTech.Add(techType, new KnownTech.CompoundTech() { techType = techType, dependencies = compoundTechsForUnlock });
            }
        }

        internal void RemoveAnalysisSpecific(TechType targetTechType, List<TechType> techTypes)
        {
            foreach (TechType techType in techTypes)
            {
                if (KnownTechPatcher.RemoveFromSpecificTechs.TryGetValue(techType, out var types))
                {
                    if (!types.Contains(targetTechType))
                        types.Add(targetTechType);
                }
                else
                {
                    KnownTechPatcher.RemoveFromSpecificTechs[techType] = new List<TechType>() { targetTechType };
                }
            }
        }

        internal void RemoveAnalysisTechEntry(TechType targetTechType)
        {
            foreach (KnownTech.AnalysisTech tech in KnownTechPatcher.AnalysisTech.Values)
            {
                if (tech.unlockTechTypes.Contains(targetTechType))
                {
                    Logger.Debug($"Removed {targetTechType.AsString()} from {tech.techType.AsString()} unlocks that was added by another mod!");
                    tech.unlockTechTypes.Remove(targetTechType);
                }
            }

            if (KnownTechPatcher.CompoundTech.TryGetValue(targetTechType, out var types))
            {
                Logger.Debug($"Removed Compound Unlock for {targetTechType.AsString()} that was added by another mod!");
                KnownTechPatcher.CompoundTech.Remove(targetTechType);
            }

            if (KnownTechPatcher.UnlockedAtStart.Contains(targetTechType))
            {
                Logger.Debug($"Removed UnlockedAtStart for {targetTechType.AsString()} that was added by another mod!");
                KnownTechPatcher.UnlockedAtStart.Remove(targetTechType);
            }

            if (!KnownTechPatcher.RemovalTechs.Contains(targetTechType))
                KnownTechPatcher.RemovalTechs.Add(targetTechType);
        }

        #region StaticMethods

        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        /// <param name="UnlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
        /// <param name="UnlockSound">The sound that plays when you unlock the blueprint.</param>
        /// <param name="UnlockSprite">The sprite that shows up when you unlock the blueprint.</param>
        public static void SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string UnlockMessage = "NotificationBlueprintUnlocked", FMODAsset UnlockSound = null, UnityEngine.Sprite UnlockSprite = null)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, UnlockMessage, UnlockSound, UnlockSprite);
        }


        /// <summary>
        /// Allows you to set up a custom Compound Unlock requiring multiple techtypes to be unlocked before 1 is.
        /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="RemoveAnalysisTechEntryFromSpecific"/> or <see cref="RemoveAllCurrentAnalysisTechEntry"/>
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="compoundTechsForUnlock"></param>
        public static void SetCompoundUnlock(TechType techType, List<TechType> compoundTechsForUnlock)
        {
            singleton.AddCompoundUnlock(techType, compoundTechsForUnlock);
        }


        /// <summary>
        /// Allows you to remove unlock entries for a <see cref="TechType"/> from specific entries.
        /// </summary>
        /// <param name="targetTechType">Target <see cref="TechType"/> to remove the unlocks for.</param>
        /// <param name="techTypes">List of <see cref="TechType"/> to remove the targetTechType from.</param>
        public static void RemoveAnalysisTechEntryFromSpecific(TechType targetTechType, List<TechType> techTypes)
        {
            singleton.RemoveAnalysisSpecific(targetTechType, techTypes);
        }

        /// <summary>
        /// Allows you to remove all unlock entries from a <see cref="TechType"/> to be able to disable or change it to a new unlock.
        /// ***Note: This is patch time specific so the LAST mod to call this on a techtype will be the only one to control what unlocks said type after its use.***
        /// </summary>
        /// <param name="targetTechType">Target <see cref="TechType"/> to remove the unlocks for.</param>
        public static void RemoveAllCurrentAnalysisTechEntry(TechType targetTechType)
        {
            singleton.RemoveAnalysisTechEntry(targetTechType);
        }

        #endregion

        #region InterfaceMethods
        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        void IKnownTechHandler.SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock);
        }

        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        /// <param name="UnlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
        void IKnownTechHandler.SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string UnlockMessage)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, UnlockMessage);
        }

        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        /// <param name="UnlockSound">The sound that plays when you unlock the blueprint.</param>
        void IKnownTechHandler.SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, FMODAsset UnlockSound)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", UnlockSound);
        }

        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        /// <param name="UnlockSprite">The sprite that shows up when you unlock the blueprint.</param>
        void IKnownTechHandler.SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, Sprite UnlockSprite)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", null, UnlockSprite);
        }

        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        /// <param name="UnlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
        /// <param name="UnlockSound">The sound that plays when you unlock the blueprint.</param>
        void IKnownTechHandler.SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string UnlockMessage, FMODAsset UnlockSound)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, UnlockMessage, UnlockSound, null);
        }

        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        /// <param name="UnlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
        /// <param name="UnlockSprite">The sprite that shows up when you unlock the blueprint.</param>
        void IKnownTechHandler.SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, string UnlockMessage, Sprite UnlockSprite)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, UnlockMessage, null, UnlockSprite);
        }

        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="techTypeToBeAnalysed">This TechType is the criteria for all of the "unlock TechTypes"; when this TechType is unlocked, so are all the ones in that list</param>
        /// <param name="techTypesToUnlock">The TechTypes that will be unlocked when "techTypeToSet" is unlocked.</param>
        /// <param name="UnlockSound">The sound that plays when you unlock the blueprint.</param>
        /// <param name="UnlockSprite">The sprite that shows up when you unlock the blueprint.</param>
        void IKnownTechHandler.SetAnalysisTechEntry(TechType techTypeToBeAnalysed, IEnumerable<TechType> techTypesToUnlock, FMODAsset UnlockSound, Sprite UnlockSprite)
        {
            singleton.AddAnalysisTech(techTypeToBeAnalysed, techTypesToUnlock, "NotificationBlueprintUnlocked", UnlockSound, UnlockSprite);
        }

        /// <summary>
        /// Allows you to set up a custom Compound Unlock requiring multiple techtypes to be unlocked before 1 is.
        /// ***Note: This will not remove any original unlock and if you need to do so you should use <see cref="IKnownTechHandler.RemoveAnalysisTechEntryFromSpecific"/> or <see cref="IKnownTechHandler.RemoveAllCurrentAnalysisTechEntry"/>
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="compoundTechsForUnlock"></param>
        void IKnownTechHandler.SetCompoundUnlock(TechType techType, List<TechType> compoundTechsForUnlock)
        {
            singleton.AddCompoundUnlock(techType, compoundTechsForUnlock);
        }


        /// <summary>
        /// Allows you to remove unlock entries for a <see cref="TechType"/> from specific entries.
        /// </summary>
        /// <param name="targetTechType">Target <see cref="TechType"/> to remove the unlocks for.</param>
        /// <param name="techTypes">List of <see cref="TechType"/> to remove the targetTechType from.</param>
        void IKnownTechHandler.RemoveAnalysisTechEntryFromSpecific(TechType targetTechType, List<TechType> techTypes)
        {
            singleton.RemoveAnalysisSpecific(targetTechType, techTypes);
        }

        /// <summary>
        /// Allows you to remove all unlock entries from a <see cref="TechType"/> to be able to disable or change it to a new unlock.
        /// ***Note: This is patch time specific so the LAST mod to call this on a techtype will be the only one to control what unlocks said type after its use.***
        /// </summary>
        /// <param name="targetTechType">Target <see cref="TechType"/> to remove the unlocks for.</param>
        void IKnownTechHandler.RemoveAllCurrentAnalysisTechEntry(TechType targetTechType)
        {
            singleton.RemoveAnalysisTechEntry(targetTechType);
        }
        #endregion
    }
}
