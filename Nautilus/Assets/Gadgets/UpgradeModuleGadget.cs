using System;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Patchers;
using Nautilus.MonoBehaviours;

namespace Nautilus.Assets.Gadgets;

/// <summary>
/// Represents a vehicle upgrade module gadget.
/// </summary>
public class UpgradeModuleGadget : Gadget
{
    // UNIVERSAL

    /// <summary>
    /// Defines the max charge in units of energy of this upgrade module when activated.
    /// Applicable for chargeable vehicle modules.
    /// </summary>
    public double MaxCharge { get; set; }

    /// <summary>
    /// Defines the energy cost of this upgrade module when activated. Applicable for all usable vehicle modules.
    /// </summary>
    /// <remarks>For chargeable modules, this defines the number of units of energy expended per second.</remarks>
    public double EnergyCost { get; set; }

    /// <summary>
    /// Defines the cooldown of this upgrade module after being activated. Applicable for non-toggleable vehicle modules,
    /// with some potential limitations.
    /// </summary>
    public double Cooldown { get; set; } = 0f;

    /// <summary>
    /// Defines the crush depth of this upgrade module. Default value is -1. Only used when values are above 0.
    /// </summary>
    public float CrushDepth { get; set; } = -1f;

    /// <summary>
    /// Whether the crush depth provided (if any) should be absolute or added to the default depth of the vehicle.
    /// Default value is false.
    /// </summary>
    public bool AbsoluteDepth { get; set; } = false;
    
    // ON ADDED

    /// <summary>
    /// The Action that is executed after the vehicle module is added, called after default events.
    /// </summary>
    public Action<Vehicle, int> delegateOnAdded { get; private set; }

#if BELOWZERO
    /// <summary>
    /// The Action that is executed after the vehicle module is added to a Seatruck,
    /// called after default events.
    /// </summary>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int> seatruckOnAdded { get; private set; }

    /// <summary>
    /// The Action that is executed after the vehicle module is added to a Snowfox,
    /// called after default events.
    /// </summary>
    public Action<Hoverbike, int> hoverbikeOnAdded { get; private set; }
#endif

    // ON REMOVED

    /// <summary>
    /// The Action that is executed after the vehicle module is removed.
    /// </summary>
    public Action<Vehicle, int> delegateOnRemoved { get; private set; }

#if BELOWZERO
    /// <summary>
    /// The Action that is executed after the vehicle module is removed from a Seatruck.
    /// </summary>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int> seatruckOnRemoved { get; private set; }

    /// <summary>
    /// The Action that is executed after the vehicle module is removed from a Snowfox.
    /// </summary>
    public Action<Hoverbike, int> hoverbikeOnRemoved { get; private set; }
#endif

    // ON USED

    /// <summary>
    /// The Action that is executed when the module is used. Applies to non-toggleable vehicle modules.
    /// </summary>
    public Action<Vehicle, int, float, float> delegateOnUsed { get; private set; }

#if BELOWZERO
    /// <summary>
    /// The Action that is executed when the module is used on a Seatruck. Applies to non-toggleable vehicle modules.
    /// <para>The first <c>float</c> represents the current quick slot charge, and the second one represents the charge scalar.</para>
    /// </summary>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int, float, float> seatruckOnUsed { get; private set; }

    /// <summary>
    /// The Action that is executed when the module is used on a Snowfox. Applies to non-toggleable vehicle modules.
    /// <para>The first <c>float</c> represents the current quick slot charge, and the second one represents the charge scalar.</para>
    /// </summary>
    public Action<Hoverbike, int, float, float> hoverbikeOnUsed { get; private set; }
#endif

    // ON TOGGLED

    /// <summary>
    /// The Action that is executed when the module is toggled. Only applies to toggleable vehicle modules.
    /// </summary>
    /// <remarks>
    /// The boolean represents whether the module is on or off.
    /// </remarks>
    public Action<Vehicle, int, float, bool> delegateOnToggled { get; private set; }

#if BELOWZERO
    /// <summary>
    /// The Action that is executed when the module is toggled in a Seatruck. Only applies to toggleable vehicle modules.
    /// </summary>
    /// <remarks>
    /// The boolean represents whether the module is on or off.
    /// </remarks>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int, float, bool> seatruckOnToggled { get; private set; }

    /// <summary>
    /// The Action that is executed when the module is toggled in a Snowfox. Only applies to toggleable vehicle modules.
    /// </summary>
    /// <remarks>
    /// The boolean represents whether the module is on or off.
    /// </remarks>
    public Action<Hoverbike, int, float, bool> hoverbikeOnToggled { get; private set; }
#endif
    
    /// <summary>
    /// Constructs an equipment gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    public UpgradeModuleGadget(ICustomPrefab prefab) : base(prefab) { }

    /// <summary>
    /// Sets the maximum charge of an upgrade module, in units of energy.
    /// </summary>
    /// <param name="maxCharge">The maximum charge in total units of energy that can be expended.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>
    /// This is used for the Seamoth and Seatruck Perimeter Defense Module to provide a strength modifier
    /// based on how long the player holds the key.
    /// </remarks>
    public UpgradeModuleGadget WithMaxCharge(double maxCharge)
    {
        MaxCharge = maxCharge;
        return this;
    }

    /// <summary>
    /// Sets the energy cost of this upgrade module when used.
    /// </summary>
    /// <param name="energyCost">The energy cost per use (or per second for chargeable modules).</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>
    /// For chargeable modules, this defines the number of units of energy expended per second
    /// towards the total charge.
    /// </remarks>
    public UpgradeModuleGadget WithEnergyCost(double energyCost)
    {
        EnergyCost = energyCost;
        return this;
    }

    /// <summary>
    /// Sets the cooldown of the module after it is used.
    /// </summary>
    /// <param name="cooldown">The cooldown of the module in seconds.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>Does not work with toggleable or passive items, and may not be implemented by all vehicles.</remarks>
    public UpgradeModuleGadget WithCooldown(double cooldown)
    {
        Cooldown = cooldown;
        return this;
    }

    /// <summary>
    /// Sets the crush depth given by this upgrade.
    /// </summary>
    /// <param name="newCrushDepth">The new crush depth in meters.</param>
    /// <param name="absolute">Whether the provided depth should be absolute or added to the default max depth of the vehicle.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    public UpgradeModuleGadget WithDepthUpgrade(float newCrushDepth, bool absolute = false)
    {
        CrushDepth = newCrushDepth;
        AbsoluteDepth = absolute;
        return this;
    }


    // DELEGATES

    /// <summary>
    /// Defines extra functionality when this upgrade module is added.
    /// </summary>
    /// <param name="onAdded">Action that occurs after the module is added.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    public UpgradeModuleGadget WithOnModuleAdded(Action<Vehicle, int> onAdded)
    {
        delegateOnAdded = onAdded;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// Defines extra functionality when this upgrade module is added.
    /// </summary>
    /// <param name="onAdded">Action that occurs after the module is added.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    public UpgradeModuleGadget WithOnModuleAdded(Action<SeaTruckUpgrades, SeaTruckMotor, int> onAdded)
    {
        seatruckOnAdded = onAdded;
        return this;
    }

    /// <summary>
    /// Defines extra functionality when this upgrade module is added.
    /// </summary>
    /// <param name="onAdded">Action that occurs after the module is added.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    public UpgradeModuleGadget WithOnModuleAdded(Action<Hoverbike, int> onAdded)
    {
        hoverbikeOnAdded = onAdded;
        return this;
    }
#endif

    /// <summary>
    /// Defines extra functionality when this upgrade module is removed.
    /// </summary>
    /// <param name="onRemoved">Action that occurs after the module is removed.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    public UpgradeModuleGadget WithOnModuleRemoved(Action<Vehicle, int> onRemoved)
    {
        delegateOnRemoved = onRemoved;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// Defines extra functionality when this upgrade module is removed.
    /// </summary>
    /// <param name="onRemoved">Action that occurs after the module is removed.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    public UpgradeModuleGadget WithOnModuleRemoved(Action<SeaTruckUpgrades, SeaTruckMotor, int> onRemoved)
    {
        seatruckOnRemoved = onRemoved;
        return this;
    }

    /// <summary>
    /// Defines extra functionality when this upgrade module is removed.
    /// </summary>
    /// <param name="onRemoved">Action that occurs after the module is removed.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    public UpgradeModuleGadget WithOnModuleRemoved(Action<Hoverbike, int> onRemoved)
    {
        hoverbikeOnRemoved = onRemoved;
        return this;
    }
#endif

    /// <summary>
    /// Defines extra functionality when this upgrade module is used, for non-toggleable modules.
    /// </summary>
    /// <param name="onUsed">Action that occurs when the module is used.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>The first <c>float</c> represents the current quick slot charge, and the second one represents the charge scalar.</remarks>
    public UpgradeModuleGadget WithOnModuleUsed(Action<Vehicle, int, float, float> onUsed)
    {
        delegateOnUsed = onUsed;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// Defines extra functionality when this upgrade module is used, for non-toggleable modules.
    /// </summary>
    /// <param name="onUsed">Action that occurs when the module is used.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>The first <c>float</c> represents the current quick slot charge, and the second one represents the charge scalar.</remarks>
    public UpgradeModuleGadget WithOnModuleUsed(Action<SeaTruckUpgrades, SeaTruckMotor, int, float, float> onUsed)
    {
        seatruckOnUsed = onUsed;
        return this;
    }

    /// <summary>
    /// Defines extra functionality when this upgrade module is used, for non-toggleable modules.
    /// </summary>
    /// <param name="onUsed">Action that occurs when the module is used.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>The first <c>float</c> represents the current quick slot charge, and the second one represents the charge scalar.</remarks>
    public UpgradeModuleGadget WithOnModuleUsed(Action<Hoverbike, int, float, float> onUsed)
    {
        hoverbikeOnUsed = onUsed;
        return this;
    }
#endif

    /// <summary>
    /// Defines extra functionality when this upgrade module is toggled.
    /// </summary>
    /// <param name="onToggled">Action that occurs when the module is toggled.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>The boolean represents whether it is being added or removed.</remarks>
    public UpgradeModuleGadget WithOnModuleToggled(Action<Vehicle, int, float, bool> onToggled)
    {
        delegateOnToggled = onToggled;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// Defines extra functionality when this upgrade module is toggled.
    /// </summary>
    /// <param name="onToggled">Action that occurs when the module is toggled.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>The boolean represents whether it is being added or removed.</remarks>
    public UpgradeModuleGadget WithOnModuleToggled(Action<SeaTruckUpgrades, SeaTruckMotor, int, float, bool> onToggled)
    {
        seatruckOnToggled = onToggled;
        return this;
    }

    /// <summary>
    /// Defines extra functionality when this upgrade module is toggled.
    /// </summary>
    /// <param name="onToggled">Action that occurs when the module is toggled.</param>
    /// <returns>A reference to this instance after the operation has been completed.</returns>
    /// <remarks>The boolean represents whether it is being added or removed.</remarks>
    public UpgradeModuleGadget WithOnModuleToggled(Action<Hoverbike, int, float, bool> onToggled)
    {
        hoverbikeOnToggled = onToggled;
        return this;
    }
#endif

    /// <inheritdoc/>
    protected internal override void Build()
    {
        if (prefab.Info.TechType is TechType.None)
        {
            InternalLogger.Error($"Prefab '{prefab.Info}' does not contain a TechType. Skipping {nameof(UpgradeModuleGadget)} build.");
            return;
        }

        CraftDataHandler.SetMaxCharge(prefab.Info.TechType, MaxCharge);
        CraftDataHandler.SetEnergyCost(prefab.Info.TechType, EnergyCost);

        prefab.TryGetGadget<EquipmentGadget>(out EquipmentGadget equipmentGadget);
        if (equipmentGadget is null)
            return;

        switch (equipmentGadget.EquipmentType)
        {
            case EquipmentType.VehicleModule:
                VehicleUpgradesPatcher.VehicleUpgradeModules.Add(prefab.Info.TechType, prefab);
                break;
            case EquipmentType.ExosuitModule:
                if(!AbsoluteDepth)
                {
                    Exosuit.crushDepths.Add(prefab.Info.TechType, CrushDepth);
                }
                VehicleUpgradesPatcher.ExosuitUpgradeModules.Add(prefab.Info.TechType, prefab);
                break;
#if BELOWZERO
            case EquipmentType.SeaTruckModule:
                if (!AbsoluteDepth)
                {
                    SeaTruckUpgrades.crushDepths.Add(prefab.Info.TechType, CrushDepth);
                }
                VehicleUpgradesPatcher.SeatruckUpgradeModules.Add(prefab.Info.TechType, prefab);
                break;
            case EquipmentType.HoverbikeModule:
                HoverbikeModulesSupport.CustomModules.Add(prefab.Info.TechType, prefab);
                break;
#elif SUBNAUTICA
            case EquipmentType.SeamothModule:
                VehicleUpgradesPatcher.SeamothUpgradeModules.Add(prefab.Info.TechType, prefab);
                break;
#endif
            default:
                throw new Exception($"The upgrade module type of item {prefab.Info.TechType} is not supported by the Upgrade Module Gadget. You're maybe using a type not existing on this game version ?\nEquipmentType provided: {nameof(EquipmentType)}.{nameof(equipmentGadget.EquipmentType)}");
        }
    }
}
