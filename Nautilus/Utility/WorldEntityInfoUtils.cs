using Nautilus.Assets;
using UnityEngine;
using UWE;

namespace Nautilus.Utility;

/// <summary>
/// Utilities related to creating a <see cref="WorldEntityInfo"/> instance.
/// </summary>
public static class WorldEntityInfoUtils
{
    /// <summary>
    /// Creates a new instance of the <see cref="WorldEntityInfo"/> class based on the given parameters.
    /// </summary>
    /// <param name="classId">The Class ID of the prefab.</param>
    /// <param name="techType">The TechType of the prefab.</param>
    /// <param name="cellLevel">The cell level of the prefab (should match whatever is used in the <see cref="LargeWorldEntity"/> component).</param>
    /// <param name="slotType">The slot type of the prefab.</param>
    /// <param name="zUp">If true, the prefab will use its Z axis as the "up" direction. By default is false, and instead uses its Y axis to determine which way is up.</param>
    /// <param name="localScale">The local scale of the prefab when spawned. If left at <see langword="default"/>, aka (0, 0, 0) or <see cref="Vector3.zero"/>, will automatically resolve to <see cref="Vector3.one"/>.</param>
    /// <returns>The created <see cref="WorldEntityInfo"/>.</returns>
    public static WorldEntityInfo Create(string classId, TechType techType, LargeWorldEntity.CellLevel cellLevel, EntitySlot.Type slotType, bool zUp = false, Vector3 localScale = default)
    {
        return new WorldEntityInfo()
        {
            classId = classId,
            techType = techType,
            cellLevel = cellLevel,
            slotType = slotType,
            prefabZUp = zUp,
            localScale = localScale == default ? Vector3.one : localScale
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="WorldEntityInfo"/> class based on the given parameters.
    /// </summary>
    /// <param name="prefabInfo">The object that holds the Class ID and TechType.</param>
    /// <param name="cellLevel">The cell level of the prefab (should match whatever is used in the <see cref="LargeWorldEntity"/> component).</param>
    /// <param name="slotType">The slot type of the prefab.</param>
    /// <param name="zUp">If true, the prefab will use its Z axis as the "up" direction. By default is false, and instead uses its Y axis to determine which way is up.</param>
    /// <param name="localScale">The local scale of the prefab when spawned. If left at <see langword="default"/>, aka (0, 0, 0) or <see cref="Vector3.zero"/>, will automatically resolve to <see cref="Vector3.one"/>.</param>
    /// <returns>The created <see cref="WorldEntityInfo"/>.</returns>
    public static WorldEntityInfo Create(PrefabInfo prefabInfo, LargeWorldEntity.CellLevel cellLevel, EntitySlot.Type slotType, bool zUp = false, Vector3 localScale = default)
    {
        return new WorldEntityInfo()
        {
            classId = prefabInfo.ClassID,
            techType = prefabInfo.TechType,
            cellLevel = cellLevel,
            slotType = slotType,
            prefabZUp = zUp,
            localScale = localScale == default ? Vector3.one : localScale
        };
    }
}
