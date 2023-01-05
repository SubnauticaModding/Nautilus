namespace SMLHelper.Handlers
{
    using SMLHelper.Patchers.EnumPatching;
    using SMLHelper.Utility;

    /// <summary>
    /// A handler class for everything related to creating new TechGroups.
    /// </summary>
    public static class TechGroupHandler
    {
        /// <summary>
        /// Adds a new <see cref="TechGroup" /> into the game.
        /// </summary>
        /// <param name="techGroupName">The name of the TechGroup. Should not contain special characters.</param>
        /// <param name="displayName">The display name of the TechGroup. Can be anything.</param>
        /// <returns>
        /// The new <see cref="TechGroup" /> that is created.
        /// </returns>
        public static TechGroup AddTechGroup(string techGroupName, string displayName)
        {
            TechGroup techGroup = TechGroupPatcher.AddTechGroup(techGroupName);
            LanguageHandler.SetLanguageLine("Group" + techGroupName, displayName);
            return techGroup;
        }

        /// <summary>
        /// Safely looks for a modded group from another mod in the SMLHelper TechGroupCache.
        /// </summary>
        /// <param name="techGroupString">The string used to define the techgroup.</param>
        /// <returns>
        ///   <c>True</c> if the item was found; Otherwise <c>false</c>.
        /// </returns>
        public static bool ModdedTechGroupExists(string techGroupString)
        {
            return TechGroupPatcher.cacheManager.RequestCacheForTypeName(techGroupString, false) != null;
        }

        /// <summary>
        /// Safely looks for a modded item from another mod in the SMLHelper TechGroupCache and outputs its <see cref="TechGroup" /> value when found.
        /// </summary>
        /// <param name="techGroupString">The string used to define the techgroup.</param>
        /// <param name="modTechGroup">The TechGroup enum value of the modded. Defaults to <see cref="TechGroup.Uncategorized" /> when the item was not found.</param>
        /// <returns>
        ///   <c>True</c> if the item was found; Otherwise <c>false</c>.
        /// </returns>
        public static bool TryGetModdedTechGroup(string techGroupString, out TechGroup modTechGroup)
        {
            EnumTypeCache cache = TechGroupPatcher.cacheManager.RequestCacheForTypeName(techGroupString, false);

            if(cache != null) // Item Found
            {
                modTechGroup = (TechGroup)cache.Index;
                return true;
            }
            else // Mod not present or not yet loaded
            {
                modTechGroup = TechGroup.Uncategorized;
                return false;
            }
        }
    }
}
