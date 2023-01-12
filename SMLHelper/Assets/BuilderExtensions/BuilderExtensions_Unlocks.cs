namespace SMLHelper.Assets
{
    using System.Collections.Generic;
    using Handlers;
    using Utility;

    /// <summary>
    /// Extensions for the ModPrefab class to set things up without having to use Inheritance based prefabs.
    /// </summary>
    public static partial class BuilderExtensions
    {
        /// <summary>
        /// Allows you to define which TechTypes are unlocked when a certain TechType is unlocked, i.e., "analysed".
        /// If there is already an exisitng AnalysisTech entry for a TechType, all the TechTypes in "techTypesToUnlock" will be
        /// added to the existing AnalysisTech entry unlocks.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="unlockTech">When this TechType is unlocked, so is this prefab</param>
        /// <param name="unlockMessage">The message that shows up on the right when the blueprint is unlocked. </param>
        public static ModPrefabBuilder SetUnlockTech(this ModPrefabBuilder modPrefabBuilder, TechType unlockTech, string unlockMessage)
        {
            var techType = modPrefabBuilder.ModPrefab.TechType;
            if(techType == TechType.None)
            {
                InternalLogger.Error($"Cannot set UnlockTech for {modPrefabBuilder.ModPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }
            bool defaultMessage = string.IsNullOrWhiteSpace(unlockMessage);
            string DiscoverMessageResolved = defaultMessage ? "NotificationBlueprintUnlocked" : $"{techType.AsString()}_DiscoverMessage";
            if(!defaultMessage)
                LanguageHandler.SetLanguageLine(DiscoverMessageResolved, unlockMessage);

            KnownTechHandler.SetAnalysisTechEntry(unlockTech, new TechType[1] { techType }, DiscoverMessageResolved);
            return modPrefabBuilder;
        }

        /// <summary>
        /// Allows you to set up a custom Compound Unlock requiring multiple techtypes to be unlocked before 1 is.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="techsForUnlock">List of <see cref="TechType"/> to needed to unlock.</param>
        public static ModPrefabBuilder SetCompoundUnlock(this ModPrefabBuilder modPrefabBuilder, List<TechType> techsForUnlock)
        {
            var techType = modPrefabBuilder.ModPrefab.TechType;
            if(techType == TechType.None)
            {
                InternalLogger.Error($"Cannot set CompoundUnlock for {modPrefabBuilder.ModPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            if(techsForUnlock == null || techsForUnlock.Count < 2)
                return modPrefabBuilder;

            KnownTechHandler.SetCompoundUnlock(techType, techsForUnlock);
            return modPrefabBuilder;
        }


        /// <summary>
        /// Adds custom Encyclopedia entry for this prefab.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="entry">The <see cref="PDAEncyclopedia.EntryData"/> entry.</param>
        public static ModPrefabBuilder SetEncyclopediaEntry(this ModPrefabBuilder modPrefabBuilder, PDAEncyclopedia.EntryData entry)
        {
            if(entry != null)
                PDAHandler.AddEncyclopediaEntry(entry);

            return modPrefabBuilder;
        }


        /// <summary>
        /// Adds in a custom <see cref="PDAScanner.EntryData"/>.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="entry">The <see cref="PDAScanner.EntryData"/> of the entry. Must be populated when passed in.</param>
        public static ModPrefabBuilder SetScannerEntry(this ModPrefabBuilder modPrefabBuilder, PDAScanner.EntryData entry)
        {
            if(entry != null)
                PDAHandler.AddCustomScannerEntry(entry);

            return modPrefabBuilder;
        }
    }
}
