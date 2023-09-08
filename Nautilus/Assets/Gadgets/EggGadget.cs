using Nautilus.Extensions;
using Nautilus.Patchers;
using Nautilus.Utility;

namespace Nautilus.Assets.Gadgets;

/// <summary>
/// Represents a creature egg gadget.
/// </summary>
public class EggGadget : Gadget
{
    /// <summary>
    /// The total amount of normal ACU floors required for the egg to be dropped in the ACU. If this is set to 0, the egg will not be accepted in the normal ACU. Defaulted to 1.
    /// </summary>
    public int RequiredAcuSize { get; set; } = 1;

    /// <summary>
    /// The total amount of Large ACU floors required for the egg to be dropped in. If this is set to 0, the egg will not be accepted in the large ACU. Defaulted to 1.
    /// </summary>
    public int RequiredLargeAcuSize { get; set; } = 1;

    /// <summary>
    /// makes the egg immune to the Lost River's Acidic Brine.
    /// </summary>
    public bool AcidImmune { get; set; } = true;

    /// <summary>
    /// Constructs a Creature egg gadget instance.
    /// </summary>
    /// <param name="prefab">The custom prefab to operate on.</param>
    /// <param name="requiredAcuSize">The total amount of ACU floors required for the egg to be dropped in the ACU. This value is shared between the normal and large ACU.</param>
    public EggGadget(ICustomPrefab prefab, int requiredAcuSize = 1) : base(prefab)
    {
        RequiredAcuSize = requiredAcuSize;
        RequiredLargeAcuSize = requiredAcuSize;
    }

    /// <summary>
    /// The total amount of normal ACU floors required for the egg to be dropped in the ACU.
    /// </summary>
    /// <param name="requiredAcuSize">The ACU stacks value. If this is set to 0, the egg will not be accepted in the normal ACU.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public EggGadget WithRequiredAcuSize(int requiredAcuSize)
    {
        RequiredAcuSize = requiredAcuSize;

        return this;
    }

    /// <summary>
    /// The total amount of Large ACU floors required for the egg to be dropped in. 
    /// </summary>
    /// <param name="requiredLargeAcuSize">The large ACU stacks value. If this is set to 0, the egg will not be accepted in the large ACU.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public EggGadget WithRequiredLargeAcuSize(int requiredLargeAcuSize)
    {
        RequiredLargeAcuSize = requiredLargeAcuSize;

        return this;
    }
    
    /// <summary>
    ///  makes the egg immune to the Lost River's Acidic Brine.
    /// </summary>
    /// <param name="isAcidImmune">Should this item be acid immune?</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public EggGadget SetAcidImmune(bool isAcidImmune)
    {
        AcidImmune = isAcidImmune;

        return this;
    }

    /// <inheritdoc/>
    protected internal override void Build()
    {
        if (prefab.Info.TechType is TechType.None)
        {
            InternalLogger.Error($"Prefab '{prefab.Info}' does not contain a TechType. Skipping {nameof(EggGadget)} build.");
            return;
        }
        
        if (AcidImmune)
            DamageSystem.acidImmune.Add(prefab.Info.TechType);

        WaterParkPatcher.requiredAcuSize[prefab.Info.TechType] = this;
    }
}