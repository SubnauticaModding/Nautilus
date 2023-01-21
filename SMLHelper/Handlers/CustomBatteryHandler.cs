namespace SMLHelper.Handlers
{
    using System.Collections.Generic;
    using SMLHelper.Assets;
    using SMLHelper.Assets.Interfaces;
    using SMLHelper.Patchers;
    using SMLHelper.Utility;

    /// <summary>
    /// Handler used to Register mod Batteries for patching into the games systems.
    /// </summary>
    public static class CustomBatteryHandler
    {

        /// <summary>
        /// Gets the steps to for the default fabricator that Custom Batteries are normally placed into.
        /// </summary>
        public static string[] BatteryCraftPath => CustomBatteriesPatcher.BatteryCraftPath;

        /// <summary>
        /// Gets the steps to for the default fabricator that Custom PowerCells are normally placed into.
        /// </summary>
        public static string[] PowerCellCraftPath => CustomBatteriesPatcher.PowCellCraftPath;

        /// <summary>
        /// Gets the full collection of <see cref="TechType" />s for all batteries, both vanilla and modded.
        /// </summary>
        /// <returns>
        /// The full collection of battery <see cref="TechType" />s.
        /// </returns>
        /// <seealso cref="BatteryCharger" />
        public static HashSet<TechType> GetAllBatteries()
        {
            return new HashSet<TechType>(BatteryCharger.compatibleTech);
        }

        /// <summary>
        /// Gets the full collection of <see cref="TechType" />s for all power cells, both vanilla and modded.
        /// </summary>
        /// <returns>
        /// The full collection of power cell <see cref="TechType" />s.
        /// </returns>
        /// <seealso cref="PowerCellCharger" />
        public static HashSet<TechType> GetAllPowerCells()
        {
            return new HashSet<TechType>(PowerCellCharger.compatibleTech);
        }


        /// <summary>
        /// Returns the <see cref="EquipmentType"/> associated to the provided <see cref="TechType"/>.<br/>
        /// This is intended to identify if a given <see cref="TechType"/> is a Battery, Power Cell, or something else.
        /// </summary>
        /// <param name="techType">The item techtype to check</param>
        /// <returns>
        /// <see cref="EquipmentType.BatteryCharger"/> if the TechType is a Battery,
        /// <see cref="EquipmentType.PowerCellCharger"/> if the TechType is a Power Cell,
        /// or the resulting value from the game's GetEquipmentType method.
        /// </returns>
        public static EquipmentType GetEquipmentType(TechType techType)
        {
            if(BatteryCharger.compatibleTech.Contains(techType))
            {
                return EquipmentType.BatteryCharger;
            }
            else if(PowerCellCharger.compatibleTech.Contains(techType))
            {
                return EquipmentType.PowerCellCharger;
            }
            else if(CustomBatteriesPatcher.TrackItems.Contains(techType))
            {
                if(CustomBatteriesPatcher.BatteryItems.FindIndex(x => x == techType) > -1)
                    return EquipmentType.BatteryCharger; // Batteries that do not go into chargers
                else if(CustomBatteriesPatcher.PowerCellItems.FindIndex(x => x == techType) > -1)
                    return EquipmentType.PowerCellCharger; // Power cells that do not go into chargers
            }

#if SUBNAUTICA
            return CraftData.GetEquipmentType(techType);
#elif BELOWZERO
            return TechData.GetEquipmentType(techType);
#endif
        }

        /// <summary>
        /// Adds a <see cref="ICustomPrefabAsync"/> to be registered as a valid battery.
        /// </summary>
        public static void RegisterCustomBattery(PrefabInfo prefabInfo, ICustomBattery modPrefab)
        {
            var techType = prefabInfo.TechType;

            if(techType == TechType.None)
            {
                InternalLogger.Error($"{ReflectionHelper.CallingAssemblyNameByStackTrace()} tried to register TechType.None as a Battery!");
                return;
            }

            if(!CustomBatteriesPatcher.BatteryItems.Contains(techType))
                CustomBatteriesPatcher.BatteryItems.Add(techType);
            if(!CustomBatteriesPatcher.TrackItems.Contains(techType))
                CustomBatteriesPatcher.TrackItems.Add(techType);

            CustomBatteriesPatcher.BatteryModels[techType] = modPrefab;

            if(!BatteryCharger.compatibleTech.Contains(techType))
                BatteryCharger.compatibleTech.Add(techType);
        }

        /// <summary>
        /// Adds a <see cref="ICustomPrefabAsync"/> to be registered as a valid PowerCell.
        /// </summary>
        public static void RegisterCustomPowerCell(PrefabInfo prefabInfo, ICustomBattery modPrefab)
        {
            var techType = prefabInfo.TechType;
            if(techType == TechType.None)
            {
                InternalLogger.Error($"{ReflectionHelper.CallingAssemblyNameByStackTrace()} tried to register TechType.None as a PowerCell!");
                return;
            }

            if(!CustomBatteriesPatcher.PowerCellItems.Contains(techType))
                CustomBatteriesPatcher.PowerCellItems.Add(techType);
            if(!CustomBatteriesPatcher.TrackItems.Contains(techType))
                CustomBatteriesPatcher.TrackItems.Add(techType);

            CustomBatteriesPatcher.PowerCellModels[techType] = modPrefab;

            if(!PowerCellCharger.compatibleTech.Contains(techType))
                PowerCellCharger.compatibleTech.Add(techType);
        }
    }
}
