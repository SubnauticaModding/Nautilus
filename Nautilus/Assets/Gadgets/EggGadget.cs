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
    /// The total amount of ACU floors required for the egg to be dropped in the ACU. defaulted to 1.
    /// </summary>
    public int RequiredAcuSize { get; set; } = 1;

    /// <summary>
    /// makes the egg immune to the Lost River's Acidic Brine.
    /// </summary>
    public bool AcidImmune { get; set; } = true;

    /// <summary>
    /// Constructs a Creature egg gadget instance.
    /// </summary>
    /// <param name="prefab">The custom prefab to operate on.</param>
    /// <param name="requiredAcuSize">The total amount of ACU floors required for the egg to be dropped in the ACU.</param>
    public EggGadget(ICustomPrefab prefab, int requiredAcuSize = 1) : base(prefab)
    {
        RequiredAcuSize = requiredAcuSize;
    }

    /// <summary>
    /// The total amount of ACU floors required for the egg to be dropped in the ACU.
    /// </summary>
    /// <param name="requiredAcuSize">The ACU stacks value.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public EggGadget WithRequiredAcuSize(int requiredAcuSize)
    {
        RequiredAcuSize = requiredAcuSize;

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

        if (RequiredAcuSize > 1)
        {
            WaterParkPatcher.requiredAcuSize[prefab.Info.TechType] = RequiredAcuSize;
        }
    }
}