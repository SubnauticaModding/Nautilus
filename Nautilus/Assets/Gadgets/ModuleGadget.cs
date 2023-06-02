using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Handlers;
using Nautilus.Utility;

namespace Nautilus.Assets.Gadgets;

/// <summary>
/// Represents a vehicle module (or upgrade) gadget
/// </summary>
public class ModuleGadget : Gadget
{
    /// <summary>
    /// The type of the equipment slot
    /// </summary>
    public required EquipmentType ModuleType { get; set; }

    /// <summary>
    /// The way the game should treat this item as when in a quick slot.
    /// </summary>
    public QuickSlotType QuickSlotType { get; set; }

    /// <summary>
    /// Max charge of this item.
    /// Should apply to modules of vehicles and to chargeable items.
    /// </summary>
    public double MaxCharge { get; set; }

    /// <summary>
    /// Energy cost of this item.
    /// Should apply to modules of vehicles.
    /// </summary>
    public double EnergyCost { get; set; }

    // TODO: Add delegates for Vehicle.OnUpgradeChanged(bool added) (added and removed), Vehicle.OnUpgradeUse(), Vehicle.OnUpgradeToggle().


    /// <summary>
    /// Constructs an equipment gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    public ModuleGadget(ICustomPrefab prefab) : base(prefab) { }

    /// <summary>
    /// Constructs an equipment gadget.
    /// </summary>
    /// <param name="prefab">The custom prefab to operate on.</param>
    /// <param name="equipmentType">The type of equipment slot this item can fit into.</param>
    [SetsRequiredMembers]
    public ModuleGadget(ICustomPrefab prefab, EquipmentType equipmentType) : base(prefab)
    {
        ModuleType = equipmentType;
    }



    /// <summary>
    /// Sets the way the game should treat this item as when in a quick slot.
    /// </summary>
    /// <param name="quickSlotType">The quick slot type</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ModuleGadget WithQuickSlotType(QuickSlotType quickSlotType)
    {
        QuickSlotType = quickSlotType;
        return this;
    }

    /// <summary>
    /// The maximum charge of the item.
    /// Usually used as a multiplier for vehicle modules.
    /// (Seamoth defense perimeter, Seatruck defense perimeter)
    /// </summary>
    /// <param name="maxCharge">Charge multiplier</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ModuleGadget WithMaxCharge(double maxCharge)
    {
        MaxCharge = maxCharge;
        return this;
    }

    /// <summary>
    /// The energy cost of the item.
    /// Usually used for vehicle modules to consume energy.
    /// (Seamoth perimeter defense, Seamoth sonar)
    /// </summary>
    /// <param name="energyCost">Energy cost</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public ModuleGadget WithEnergyCost(double energyCost)
    {
        EnergyCost = energyCost;
        return this;
    }



    /// <inheritdoc/>
    protected internal override void Build()
    {
        if (prefab.Info.TechType is TechType.None)
        {
            InternalLogger.Error($"Prefab '{prefab.Info}' does not contain a TechType. Skipping {nameof(ModuleGadget)} build.");
            return;
        }

        CraftDataHandler.SetEquipmentType(prefab.Info.TechType, ModuleType);
        CraftDataHandler.SetQuickSlotType(prefab.Info.TechType, QuickSlotType);
        CraftDataHandler.SetMaxCharge(prefab.Info.TechType, MaxCharge);
        CraftDataHandler.SetEnergyCost(prefab.Info.TechType, EnergyCost);
    }
}
