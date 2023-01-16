namespace SMLHelper.API;

using System;
using System.Collections.Generic;
using System.Reflection;
using Assets;

/// <summary>
/// An API service class that handles requests for CustomBatteries from external mods.
/// </summary>
/// <seealso cref="ICustomBatteriesService" />

public class CustomBatteriesService : ICustomBatteriesService
{
    /// <summary>
    /// Gets the full collection of <see cref="TechType" />s for all batteries, both vanilla and modded.
    /// </summary>
    /// <returns>
    /// The full collection of battery <see cref="TechType" />s.
    /// </returns>
    /// <seealso cref="BatteryCharger" />
    public HashSet<TechType> GetAllBatteries()
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
    public HashSet<TechType> GetAllPowerCells()
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
    public EquipmentType GetEquipmentType(TechType techType)
    {
        if (BatteryCharger.compatibleTech.Contains(techType))
        {
            return EquipmentType.BatteryCharger;
        }
        else if (PowerCellCharger.compatibleTech.Contains(techType))
        {
            return EquipmentType.PowerCellCharger;
        }
        else if (CbDatabase.TrackItems.Contains(techType))
        {
            if (CbDatabase.BatteryItems.FindIndex(x => x == techType) > -1)
                return EquipmentType.BatteryCharger; // Batteries that do not go into chargers
            else if (CbDatabase.PowerCellItems.FindIndex(x => x == techType) > -1)
                return EquipmentType.PowerCellCharger; // Power cells that do not go into chargers
        }

#if SUBNAUTICA
        return CraftData.GetEquipmentType(techType);
#elif BELOWZERO
            return TechData.GetEquipmentType(techType);
#endif
    }
}