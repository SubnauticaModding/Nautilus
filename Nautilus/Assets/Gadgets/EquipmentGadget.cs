using Nautilus.Handlers;
using Nautilus.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Nautilus.Assets.Gadgets;

/// <summary>
/// Represents an equipment gadget
/// </summary>
public class EquipmentGadget : Gadget
{
    /// <summary>
    /// The type of equipment slot this item can fit into.
    /// </summary>
    public required EquipmentType EquipmentType { get; set; }

    /// <summary>
    /// The way the game should treat this item as when in a quick slot.
    /// </summary>
    public QuickSlotType QuickSlotType { get; set; }



    /// <summary>
    /// Constructs an equipment gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    public EquipmentGadget(ICustomPrefab prefab) : base(prefab) { }

    /// <summary>
    /// Constructs an equipment gadget.
    /// </summary>
    /// <param name="prefab">The custom prefab to operate on.</param>
    /// <param name="equipmentType">The type of equipment slot this item can fit into.</param>
    [SetsRequiredMembers]
    public EquipmentGadget(ICustomPrefab prefab, EquipmentType equipmentType) : base(prefab)
    {
        EquipmentType = equipmentType;
    }



    /// <summary>
    /// Sets the way the game should treat this item as when in a quick slot.
    /// </summary>
    /// <param name="quickSlotType">The quick slot type</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public EquipmentGadget WithQuickSlotType(QuickSlotType quickSlotType)
    {
        QuickSlotType = quickSlotType;
        return this;
    }



    /// <summary>
    /// Sets the type of upgrade module slot this item can fit into.
    /// </summary>
    /// If you plan to use anything that is not a module (Tank, Chip), except ExosuitArm,<br/>
    /// <returns>An instance to the created <see cref="UpgradeModuleGadget"/> to continue the equipment settings on.</returns>
    public UpgradeModuleGadget SetUpgradeModule()
    {
        ICustomPrefab customPrefab = prefab;
        if (!customPrefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
        {
            moduleGadget = new UpgradeModuleGadget(customPrefab);
        }
        customPrefab.TryAddGadget(moduleGadget);

        return moduleGadget;
    }



    /// <inheritdoc/>
    protected internal override void Build()
    {
        if (prefab.Info.TechType is TechType.None)
        {
            InternalLogger.Error($"Prefab '{prefab.Info}' does not contain a TechType. Skipping {nameof(EquipmentGadget)} build.");
            return;
        }

        CraftDataHandler.SetEquipmentType(prefab.Info.TechType, EquipmentType);
        CraftDataHandler.SetQuickSlotType(prefab.Info.TechType, QuickSlotType);
    }
}