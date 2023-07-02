using System;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Patchers;
using Nautilus.MonoBehaviours;

namespace Nautilus.Assets.Gadgets;

/// <summary>
/// Represents a vehicle module (or upgrade) gadget.
/// </summary>
public class UpgradeModuleGadget : Gadget
{
    // UNIVERSAL

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

    /// <summary>
    /// Cooldown for this module.
    /// Does not work with Toggleable items.
    /// May not work with certain vehicles.
    /// </summary>
    public double Cooldown { get; set; } = 0f;

    /// <summary>
    /// Crush depth of this upgrade.
    /// Leave to -1f to disable
    /// </summary>
    public float CrushDepth { get; set; } = -1f;

    /// <summary>
    /// Wether the depth provided should be absolute or added to the default depth of the vehicle.
    /// Default value is false.
    /// </summary>
    public bool AbsoluteDepth { get; set; } = false;


    // ON ADDED

    /// <summary>
    /// This happens when the module is added to the vehicle.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<Vehicle, int> delegateOnAdded { get; private set; }

#if BELOWZERO
    /// <summary>
    /// This happens when the module is added to the SeaTruck.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int> seatruckOnAdded { get; private set; }

    /// <summary>
    /// This happens when the module is added to the Hoverbike.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<Hoverbike, int> hoverbikeOnAdded { get; private set; }
#endif

    // ON REMOVED

    /// <summary>
    /// This happens when the module is removed from the vehicle.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<Vehicle, int> delegateOnRemoved { get; private set; }

#if BELOWZERO
    /// <summary>
    /// This happens when the module is removed from the SeaTruck.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int> seatruckOnRemoved { get; private set; }

    /// <summary>
    /// This happens when the module is removed from the Hoverbike.
    /// </summary>
    public Action<Hoverbike, int> hoverbikeOnRemoved { get; private set; }
#endif

    // ON USED

    /// <summary>
    /// This happens when the module is used.
    /// The delegate is not run when the module is a toggleable.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<Vehicle, int, float, float> delegateOnUsed { get; private set; }

#if BELOWZERO
    /// <summary>
    /// This happens when the module is used.
    /// The delegate is not run when the module is a togglable.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// <para>The first <c>float</c> represents the current quick slot charge, and the second one represents the charge scalar (a game internal).</para>
    /// </summary>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int, float, float> seatruckOnUsed { get; private set; }

    /// <summary>
    /// This happens when the module is used.
    /// The delegate is not run when the module is a togglable.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<Hoverbike, int, float, float> hoverbikeOnUsed { get; private set; }
#endif

    // ON TOGGLED

    /// <summary>
    /// This happens when the module is toggled.<br/>
    /// The boolean represents wether the module is on or off.<br/>
    /// The delegate is not executed when the module is a selectable, selectableChargeable or a chargeable.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<Vehicle, int, float, bool> delegateOnToggled { get; private set; }

#if BELOWZERO
    /// <summary>
    /// This happens when the module is toggled.<br/>
    /// The boolean represents wether the module is on or off.<br/>
    /// The delegate is not executed when the module is a selectable, selectableChargeable or a chargeable.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<SeaTruckUpgrades, SeaTruckMotor, int, float, bool> seatruckOnToggled { get; private set; }

    /// <summary>
    /// This happens when the module is toggled.<br/>
    /// The boolean represents wether the module is on or off.<br/>
    /// The delegate is not executed when the module is a selectable, selectableChargeable or a chargeable.
    /// <para>Action that is executed after Nautilus' default action (if there is) on this event.</para>
    /// </summary>
    public Action<Hoverbike, int, float, bool> hoverbikeOnToggled { get; private set; }
#endif


    /// <summary>
    /// Constructs an equipment gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    public UpgradeModuleGadget(ICustomPrefab prefab) : base(prefab) { }

    /// <summary>
    /// The maximum charge of the item.
    /// Usually used as a multiplier for vehicle modules.
    /// (Seamoth defense perimeter, Seatruck defense perimeter)
    /// <para>Example: The Seamoth defense perimeter can be charged by holding the action key to make its damage bigger.</para>
    /// </summary>
    /// <param name="maxCharge">Charge multiplier</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithMaxCharge(double maxCharge)
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
    public UpgradeModuleGadget WithEnergyCost(double energyCost)
    {
        EnergyCost = energyCost;
        return this;
    }

    /// <summary>
    /// The cooldown of the module when it is used.
    /// <para>Cooldown may not work with certain vehicles.</para>
    /// <para>Does not work with toggleable and passive items.</para>
    /// </summary>
    /// <param name="cooldown">Cooldown of the module in seconds.</param>
    /// <returns>A reference to thihs instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithCooldown(double cooldown)
    {
        Cooldown = cooldown;
        return this;
    }

    /// <summary>
    /// Sets the crush depth given by this upgrade.
    /// </summary>
    /// <param name="newCrushDepth">New crush depth, in meters.</param>
    /// <param name="absolute">Wether the provided depth should be absolute or added to the default max depth of the vehicle.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithDepthUpgrade(float newCrushDepth, bool absolute = false)
    {
        CrushDepth = newCrushDepth;
        AbsoluteDepth = absolute;
        return this;
    }


    // DELEGATES

    /// <summary>
    /// What happens when the module is added to a vehicle ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (if the module is a hull, the new hull of the vehicle is automatically updated).<br/>
    /// For removed, see also <see cref="WithOnModuleRemoved(Action{Vehicle, int})"/>.
    /// </summary>
    /// <param name="onAdded">Action that occurs when the module is added.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleAdded(Action<Vehicle, int> onAdded)
    {
        delegateOnAdded = onAdded;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// What happens when the module is added to a Seatruck ?<br/>
    /// This actions is run <b>after</b> Nautilus' default action (if the module is a hull, the new hull of the vehicle is automatically updated).<br/>
    /// For removed, see also <see cref="WithOnModuleRemoved(Action{SeaTruckUpgrades, SeaTruckMotor, int})"/>.
    /// </summary>
    /// <param name="onAdded">Action that occurs when the module is added.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleAdded(Action<SeaTruckUpgrades, SeaTruckMotor, int> onAdded)
    {
        seatruckOnAdded = onAdded;
        return this;
    }

    /// <summary>
    /// What happens when the module is added to a seatruck ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (if the module is a hull, the new hull of the vehicle is automatically updated).<br/>
    /// For removed, see also <see cref="WithOnModuleRemoved(Action{Hoverbike, int})"/>
    /// </summary>
    /// <param name="onAdded"></param>
    /// <returns></returns>
    public UpgradeModuleGadget WithOnModuleAdded(Action<Hoverbike, int> onAdded)
    {
        hoverbikeOnAdded = onAdded;
        return this;
    }
#endif

    /// <summary>
    /// What happens when the module is removed from a vehicle ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (if the module is a hull, the new hull of the vehicle is automatically updated).<br/>
    /// For added, see also <see cref="WithOnModuleAdded(Action{Vehicle, int})"/>.
    /// </summary>
    /// <param name="onRemoved">Action that occurs when the module is removed.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleRemoved(Action<Vehicle, int> onRemoved)
    {
        delegateOnRemoved = onRemoved;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// What happens when the module is removed from a Seatruck ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (if the module is a hull, the new hull of the vehicle is automatically updated).<br/>
    /// For added, see also <see cref="WithOnModuleAdded(Action{SeaTruckUpgrades, SeaTruckMotor, int})"/>.
    /// </summary>
    /// <param name="onRemoved">Action that occurs when the module is removed.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleRemoved(Action<SeaTruckUpgrades, SeaTruckMotor, int> onRemoved)
    {
        seatruckOnRemoved = onRemoved;
        return this;
    }

    /// <summary>
    /// What happens when the module is removed from a Hoverbike ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (if the module is a hull, the new hull of the vehicle is automatically updated).<br/>
    /// For added, see also <see cref="WithOnModuleAdded(Action{Hoverbike, int})"/>.
    /// </summary>
    /// <param name="onRemoved">Action that occurs when the module is removed.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleRemoved(Action<Hoverbike, int> onRemoved)
    {
        hoverbikeOnRemoved = onRemoved;
        return this;
    }
#endif

    /// <summary>
    /// What happens when the module is used ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (cooldown and energy consumption are automatically set).<br/>
    /// For toggle, see also <see cref="WithOnModuleToggled(Action{Vehicle, int, float, bool})"/>.
    /// </summary>
    /// <param name="onUsed">Action that occurs when the module is used.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleUsed(Action<Vehicle, int, float, float> onUsed)
    {
        delegateOnUsed = onUsed;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// What happens when the module is used ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (cooldown and energy consumption are automatically set).<br/>
    /// For toggle, see also <see cref="WithOnModuleToggled(Action{SeaTruckUpgrades, SeaTruckMotor, int, float, bool})"/>.
    /// <para>The first <c>float</c> represents the current quick slot charge, and the second one represents the charge scalar (a game internal).</para>
    /// </summary>
    /// <param name="onUsed">Action that occurs when the module is used.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleUsed(Action<SeaTruckUpgrades, SeaTruckMotor, int, float, float> onUsed)
    {
        seatruckOnUsed = onUsed;
        return this;
    }

    /// <summary>
    /// What happens when the module is used ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (cooldown and energy consumption are automatically set).<br/>
    /// For toggle, see also <see cref="WithOnModuleToggled(Action{Hoverbike, int, float, bool})"/>.
    /// </summary>
    /// <param name="onUsed">Action that occurs when the module is used.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleUsed(Action<Hoverbike, int, float, float> onUsed)
    {
        hoverbikeOnUsed = onUsed;
        return this;
    }
#endif

    /// <summary>
    /// What happens when the module is toggled ?<br/>
    /// This actions is run <b>after</b> Nautilus' default action (energy consumption is automatically set).<br/>
    /// For use, see also <see cref="WithOnModuleUsed(Action{Vehicle, int, float, float})"/>.
    /// </summary>
    /// <param name="onToggled">Action that occurs <b>when the module turns on and when it turns off</b>.<br/>
    /// The boolean determines wether it is added or removed (added=true).</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleToggled(Action<Vehicle, int, float, bool> onToggled)
    {
        delegateOnToggled = onToggled;
        return this;
    }

#if BELOWZERO
    /// <summary>
    /// What happens when the module is toggled ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (energy consumption is automatically set)<br/>
    /// For use, see also <see cref="WithOnModuleUsed(Action{SeaTruckUpgrades, SeaTruckMotor, int, float, float})"/>.
    /// </summary>
    /// <param name="onToggled">Action that occurs <b>when the module turns on and when it turns off</b>.<br/>
    /// The boolean determines wether it is added or removed (added=true).</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UpgradeModuleGadget WithOnModuleToggled(Action<SeaTruckUpgrades, SeaTruckMotor, int, float, bool> onToggled)
    {
        seatruckOnToggled = onToggled;
        return this;
    }

    /// <summary>
    /// What happens when the module is toggled ?<br/>
    /// This action is run <b>after</b> Nautilus' default action (energy consumption is automatically set)<br/>
    /// For use, see also <see cref="WithOnModuleUsed(Action{Hoverbike, int, float, float})"/>
    /// </summary>
    /// <param name="onToggled"></param>
    /// <returns></returns>
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
