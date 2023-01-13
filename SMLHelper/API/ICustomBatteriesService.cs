namespace SMLHelper.API;

using System;
using System.Collections.Generic;
using Assets;

/// <summary>
/// A simple interface that defines the services that CustomBatteries can provide to external mods.
/// </summary>
public interface ICustomBatteriesService
{
    /// <summary>
    /// Gets the full collection of <see cref="TechType"/>s for all batteries, both vanilla and modded.
    /// </summary>
    /// <returns>
    /// The full collection of battery <see cref="TechType"/>s.
    /// </returns>
    /// <seealso cref="BatteryCharger"/>
    HashSet<TechType> GetAllBatteries();

    /// <summary>
    /// Gets the full collection of <see cref="TechType"/>s for all power cells, both vanilla and modded.
    /// </summary>
    /// <returns>
    /// The full collection of power cell <see cref="TechType"/>s.
    /// </returns>
    /// <seealso cref="PowerCellCharger"/>
    HashSet<TechType> GetAllPowerCells();

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
    EquipmentType GetEquipmentType(TechType techType);
}