namespace SMLHelper.V2.Handlers
{
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Interfaces;
    using SMLHelper.V2.Patchers.EnumPatching;
    using SMLHelper.V2.Utility;

    /// <summary>
    /// A handler class for everything related to creating new Equipments.
    /// </summary>
    public class EquipmentHandler : IEquipmentHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IEquipmentHandler Main { get; } = new EquipmentHandler();

        private EquipmentHandler()
        {
            // Hide constructor
        }

        /// <summary>
        /// Adds a new <see cref="Equipment" /> into the game.
        /// </summary>
        /// <param name="equipmentName">The name of the Equipment. Should not contain special characters.</param>
        /// <param name="displayName">The display name of the Equipment. Can be anything.</param>
        /// <returns>
        /// The new <see cref="Equipment" /> that is created.
        /// </returns>
        public EquipmentType RegisterNewEquipmentType(string equipmentName, string displayName)
        {
            EquipmentType equipment = EquipmentTypePatcher.AddEquipmentType(equipmentName);
            LanguageHandler.SetLanguageLine(equipmentName, displayName);
            return equipment;
        }

        /// <summary>
        /// Safely looks for a modded group from another mod in the SMLHelper EquipmentCache.
        /// </summary>
        /// <param name="equipmentString">The string used to define the techgroup.</param>
        /// <returns>
        ///   <c>True</c> if the item was found; Otherwise <c>false</c>.
        /// </returns>
        public bool ModdedEquipmentExists(string equipmentString)
        {
            EnumTypeCache cache = EquipmentTypePatcher.cacheManager.RequestCacheForTypeName(equipmentString, false);
            if (cache != null) // Item Found
            {
                return true;
            }
            else // Mod not present or not yet loaded
            {
                return false;
            }
        }

        /// <summary>
        /// Safely looks for a modded item from another mod in the SMLHelper EquipmentCache and outputs its <see cref="Equipment" /> value when found.
        /// </summary>
        /// <param name="equipmentString">The string used to define the techgroup.</param>
        /// <param name="modEquipment">The Equipment enum value of the modded. Defaults to <see cref="EquipmentType.None" /> when the item was not found.</param>
        /// <returns>
        ///   <c>True</c> if the item was found; Otherwise <c>false</c>.
        /// </returns>
        public bool TryGetModdedEquipmentType(string equipmentString, out EquipmentType modEquipment)
        {
            EnumTypeCache cache = EquipmentTypePatcher.cacheManager.RequestCacheForTypeName(equipmentString, false);
            if (cache != null) // Item Found
            {
                modEquipment = (EquipmentType)cache.Index;
                return true;
            }
            else // Mod not present or not yet loaded
            {
                modEquipment = EquipmentType.None;
                return false;
            }
        }

    }
}
