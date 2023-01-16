namespace SMLHelper.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SMLHelper.API;
    using SMLHelper.Assets;
    using SMLHelper.Utility;

    /// <summary>
    /// Handler used to Register mod Batteries for patching into the games systems.
    /// </summary>
    public static class CustomBatteryHandler
    {
        /// <summary>
        /// Gets the steps to for the default fabricator that Custom Batteries are normally placed into.
        /// </summary>
        public static string[] BatteryCraftPath => CbDatabase.BatteryCraftPath;

        /// <summary>
        /// Gets the steps to for the default fabricator that Custom PowerCells are normally placed into.
        /// </summary>
        public static string[] PowerCellCraftPath => CbDatabase.PowCellCraftPath;

        /// <summary>
        /// Adds a <see cref="TechType"/> to be registered as a valid battery.
        /// </summary>
        /// <param name="techType">The techtype to register.</param>
        /// <param name="customModelData">Any custom model data to use for the battery when placed in tools and chargers</param>
        public static void RegisterCustomBattery(TechType techType, CBModelData customModelData = null)
        {
            if(techType == TechType.None)
            {
                InternalLogger.Error($"{ReflectionHelper.CallingAssemblyNameByStackTrace()} tried to register TechType.None as a Battery!");
                return;
            }

            if(!CbDatabase.BatteryItems.Contains(techType))
                CbDatabase.BatteryItems.Add(techType);
            if(!CbDatabase.TrackItems.Contains(techType))
                CbDatabase.TrackItems.Add(techType);

            CbDatabase.BatteryModels[techType] = customModelData;
        }

        /// <summary>
        /// Adds a <see cref="TechType"/> to be registered as a valid PowerCell.
        /// </summary>
        /// <param name="techType">The techtype to register.</param>
        /// <param name="customModelData">Any custom model data to use for the PowerCell when placed in tools, vehicles and chargers</param>
        public static void RegisterCustomPowerCell(TechType techType, CBModelData customModelData = null)
        {
            if(techType == TechType.None)
            {
                InternalLogger.Error($"{ReflectionHelper.CallingAssemblyNameByStackTrace()} tried to register TechType.None as a PowerCell!");
                return;
            }

            if(!CbDatabase.PowerCellItems.Contains(techType))
                CbDatabase.PowerCellItems.Add(techType);
            if(!CbDatabase.TrackItems.Contains(techType))
                CbDatabase.TrackItems.Add(techType);

            CbDatabase.PowerCellModels[techType] = customModelData;
        }
    }
}
