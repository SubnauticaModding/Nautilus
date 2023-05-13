using UnityEngine;

namespace Nautilus.Utility;

/// <summary>
/// Defines how a constructable can be placed.
/// </summary>
[System.Flags]
public enum ConstructableFlags
{
    /// <summary>
    /// Undefined placement.
    /// </summary>
    None,
        
    /// <summary>
    /// Can be placed on floors.
    /// </summary>
    Ground = 1,
        
    /// <summary>
    /// Can be placed on walls.
    /// </summary>
    Wall = 1 << 1,
        
    /// <summary>
    /// Can be placed on ceilings.
    /// </summary>
    Ceiling = 1 << 2,
        
    /// <summary>
    /// Can be placed in bases.
    /// </summary>
    Base = 1 << 3,
        
    /// <summary>
    /// Can be placed in Cyclops or any other submarine.
    /// </summary>
    Submarine = 1 << 4,
        
    /// <summary>
    /// Can be placed inside. Combines <see cref="Base"/> and <see cref="Submarine"/>.
    /// </summary>
    Inside = Base | Submarine,
        
    /// <summary>
    /// Can be placed outside.
    /// </summary>
    Outside = 1 << 5,
        
    /// <summary>
    /// Allowed on constructed entities such as tables, desks, shelves, etc...
    /// </summary>
    AllowedOnConstructable = 1 << 6,
        
    /// <summary>
    /// The constructable can be rotated during placement.
    /// </summary>
    Rotatable = 1 << 7
}

/// <summary>
/// A small collection of prefab related utilities.
/// </summary>
public static class PrefabUtils
{
    /// <summary>
    /// Adds and configures the following components on the <paramref name="prefab"/>.<para/>
    /// <br/>- <see cref="PrefabIdentifier"/>: Required for an object to be considered a prefab.
    /// <br/>- <see cref="TechTag"/>: Required for inventory items, crafting, scanning, etc.
    /// <br/>- <see cref="LargeWorldEntity"/>: Required for objects to persist after saving and exiting.
    /// <br/>- <see cref="SkyApplier"/>: Added if Renderers exist in the hierarchy. Applies the correct lighting onto an object.
    /// </summary>
    /// <param name="prefab">The prefab to operate on.</param>
    /// <param name="classId">The class ID associated with the specified prefab.</param>
    /// <param name="techType">Ignored if <see cref="TechType.None"/> is inputted.</param>
    /// <param name="cellLevel">Level of distance this prefab can stay visible before unloading.</param>
    public static void AddBasicComponents(GameObject prefab, string classId, TechType techType, LargeWorldEntity.CellLevel cellLevel)
    {
        prefab.EnsureComponent<PrefabIdentifier>().ClassId = classId;
            
        if (techType != TechType.None)
        {
            prefab.EnsureComponent<TechTag>().type = techType;
        }
            
        prefab.EnsureComponent<LargeWorldEntity>().cellLevel = cellLevel;
            
        var renderers = prefab.GetComponentsInChildren<Renderer>(true);
        if (renderers != null)
        {
            prefab.EnsureComponent<SkyApplier>().renderers = renderers;
        }
    }

    /// <summary>
    /// Adds and configures the <see cref="Constructable"/> component on the specified prefab.
    /// </summary>
    /// <param name="prefab">The prefab to operate on.</param>
    /// <param name="techType">The tech type associated with the specified prefab.</param>
    /// <param name="constructableFlags">A bitmask comprised of one or more <see cref="ConstructableFlags"/> that specify how the prefab should be treated during placement.</param>
    /// <returns>The added constructable component.</returns>
    public static Constructable AddConstructable(GameObject prefab, TechType techType, ConstructableFlags constructableFlags)
    {
        if (techType is TechType.None)
        {
            InternalLogger.Error($"TechType is required for constructable and cannot be null. Skipping {nameof(AddConstructable)}.");
            return null;
        }

        var constructable = prefab.EnsureComponent<Constructable>();
        constructable.controlModelState = true;
        // TODO: Add ghost material for BZ
#if SUBNAUTICA
        constructable.ghostMaterial = MaterialUtils.GhostMaterial;
#endif
        constructable.techType = techType;
        constructable.allowedInBase = constructableFlags.HasFlag(ConstructableFlags.Base);
        constructable.allowedInSub = constructableFlags.HasFlag(ConstructableFlags.Submarine);
        constructable.allowedOutside = constructableFlags.HasFlag(ConstructableFlags.Outside);
        constructable.allowedOnCeiling = constructableFlags.HasFlag(ConstructableFlags.Ceiling);
        constructable.allowedOnConstructables = constructableFlags.HasFlag(ConstructableFlags.AllowedOnConstructable);
        constructable.allowedOnWall = constructableFlags.HasFlag(ConstructableFlags.Wall);

        return constructable;
    }

    /// <summary>
    /// Adds the <see cref="VFXFabricating"/> component onto the child found by <paramref name="pathToModel"/>. 
    /// </summary>
    /// <param name="prefabRoot">The prefab object that this is applied to.</param>
    /// <param name="pathToModel">Leave as null or empty to point to the prefab root. Otherwise this is the path to the crafting model Transform, relative to the prefab's root Transform. For example, the Repair Tool's would be `welder_scaled/welder`.</param>
    /// <param name="minY">
    /// <para>The relative y position of where the ghost effect begins, in global coordinates relative to the model's center, taking the <paramref name="posOffset"/> into account.</para>
    /// <para>Typically a negative value because the bottom of an object is below its center. You may need to adjust this at runtime with Subnautica Runtime Editor to get desired results.</para></param>
    /// <param name="maxY">
    /// <para>The relative y position of where the ghost effect ends, in global coordinates relative to the model's center, taking the <paramref name="posOffset"/> into account.</para>
    /// <para>Typically a positive value because the top of an object is above its center. You may need to adjust this at runtime with Subnautica Runtime Editor to get desired results.</para></param>
    /// <param name="posOffset">The offset of the model when being crafted (in METERS). This is generally around zero, but the y value may be ajusted up or down a few millimeters to fix clipping/floating issues.</param>
    /// <param name="scaleFactor">The relative scale of the model. Generally is 1x for most items.</param>
    /// <param name="eulerOffset">Rotational offset.</param>
    /// <returns>The added component.</returns>
    public static VFXFabricating AddVFXFabricating(GameObject prefabRoot, string pathToModel, float minY, float maxY, Vector3 posOffset = default, float scaleFactor = 1f, Vector3 eulerOffset = default)
    {
        GameObject modelObject = prefabRoot;
        if (!string.IsNullOrEmpty(pathToModel))
        {
            modelObject = prefabRoot.transform.Find(pathToModel).gameObject;
        }
        VFXFabricating vfxFabricating = modelObject.AddComponent<VFXFabricating>();
        vfxFabricating.localMinY = minY;
        vfxFabricating.localMaxY = maxY;
        vfxFabricating.posOffset = posOffset;
        vfxFabricating.scaleFactor = scaleFactor;
        vfxFabricating.eulerOffset = eulerOffset;
        return vfxFabricating;
    }
}