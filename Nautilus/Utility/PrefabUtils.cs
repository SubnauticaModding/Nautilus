using System;
using System.Collections.Generic;
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
    /// Default placement flags. Includes <see cref="Ground"/> and <see cref="Inside"/>
    /// </summary>
    Default = Ground | Inside,
        
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
    Rotatable = 1 << 7,
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
    /// <param name="model"><para>The child GameObject that holds all the renderers that are used for the ghost model.
    /// If assigned, this parameter will control the <see cref="Constructable.model"/> field. This field MUST BE ASSIGNED A VALUE to avoid errors when building!</para>
    /// <para>This should be a child of <paramref name="prefab"/>, and NOT the root. If it is the same value as <paramref name="prefab"/>, you have done something wrong!</para></param>
    /// <returns>A reference to the added <see cref="Constructable"/> instance.</returns>
    public static Constructable AddConstructable(GameObject prefab, TechType techType, ConstructableFlags constructableFlags, GameObject model = null)
    {
        return AddConstructable<Constructable>(prefab, techType, constructableFlags, model);
    }
    
    /// <summary>
    /// Adds and configures the <see cref="Constructable"/> component or a derived type on the specified prefab.
    /// </summary>
    /// <param name="prefab">The prefab to operate on.</param>
    /// <param name="techType">The tech type associated with the specified prefab.</param>
    /// <param name="constructableFlags">A bitmask comprised of one or more <see cref="ConstructableFlags"/> that specify how the prefab should be treated during placement.</param>
    /// <param name="model"><para>The child GameObject that holds all the renderers that are used for the ghost model.
    /// If assigned, this parameter will control the <see cref="Constructable.model"/> field. This field MUST BE ASSIGNED A VALUE to avoid errors when building!</para>
    /// <para>This should be a child of <paramref name="prefab"/>, and NOT the root. If it is the same value as <paramref name="prefab"/>, you have done something wrong!</para></param>
    /// <returns>A reference to the added <see cref="Constructable"/> instance.</returns>
    public static T AddConstructable<T>(GameObject prefab, TechType techType, ConstructableFlags constructableFlags, GameObject model = null) where T : Constructable
    {
        if (techType is TechType.None)
        {
            InternalLogger.Error($"TechType is required for constructable and cannot be null. Skipping {nameof(AddConstructable)}.");
            return null;
        }

        var constructable = prefab.EnsureComponent<T>();
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
        
        // dirty workaround for when the Ground flag wasn't actually functional but prefabs that used it still were allowed on ground since this is enabled
        // by default on the Constructable component. I hate how enum flags don't get highlighted in IDEs when they're not used/implemented which led
        // to this issue in the first place.
        constructable.allowedOnGround = constructableFlags == ConstructableFlags.None || constructableFlags.HasFlag(ConstructableFlags.Ground);
        
        constructable.rotationEnabled = constructableFlags.HasFlag(ConstructableFlags.Rotatable);

        if (model != null)
            constructable.model = model;

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
    /// <param name="posOffset">The offset of the model when being crafted (in METERS). This is generally around zero, but the y value may be adjusted up or down a few millimeters to fix clipping/floating issues.</param>
    /// <param name="scaleFactor">The relative scale of the model. Generally is 1x for most items.</param>
    /// <param name="eulerOffset">Rotational offset.</param>
    /// <returns>A reference to the added <see cref="VFXFabricating"/> instance.</returns>
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

    /// <summary>
    /// <para>Adds the <see cref="StorageContainer"/> component to the given prefab, for basic use cases with lockers and such.</para>
    /// <para>Due to how this component needs to be initialized, this method will disable the object and re-enable it after the component is added (assuming it was already active). This all happens within the same frame and will not be seen.</para>
    /// </summary>
    /// <param name="prefabRoot">The prefab that the component is added onto. This does not necessarily NEED to be the "prefab root". You can set it to a
    /// child collider if you want a smaller area of interaction or to have multiple storage containers on one prefab.</param>
    /// <param name="storageRootName">The name of the object that internally holds all of the items.</param>
    /// <param name="storageRootClassId">A unique string for the <see cref="ChildObjectIdentifier"/> component.</param>
    /// <param name="width">The width of this container's face.</param>
    /// <param name="height">The height of this container's interface.</param>
    /// <param name="preventDeconstructionIfNotEmpty">If true, you cannot destroy this prefab unless all of its storage containers are empty.</param>
    /// <returns>A reference to the added <see cref="StorageContainer"/> instance.</returns>
    public static StorageContainer AddStorageContainer(GameObject prefabRoot, string storageRootName, string storageRootClassId, int width, int height, bool preventDeconstructionIfNotEmpty = true)
    {
        return AddStorageContainer<StorageContainer>(prefabRoot, storageRootName, storageRootClassId, width, height,
            preventDeconstructionIfNotEmpty);
    }
    
    /// <summary>
    /// <para>Adds a component of the type <see cref="StorageContainer"/> or a derived class to the given prefab, for basic use cases with lockers and such.</para>
    /// <para>Due to how this component needs to be initialized, this method will disable the object and re-enable it after the component is added (assuming it was already active). This all happens within the same frame and will not be seen.</para>
    /// </summary>
    /// <param name="prefabRoot">The prefab that the component is added onto. This does not necessarily NEED to be the "prefab root". You can set it to a
    /// child collider if you want a smaller area of interaction or to have multiple storage containers on one prefab.</param>
    /// <param name="storageRootName">The name of the object that internally holds all of the items.</param>
    /// <param name="storageRootClassId">A unique string for the <see cref="ChildObjectIdentifier"/> component.</param>
    /// <param name="width">The width of this container's face.</param>
    /// <param name="height">The height of this container's interface.</param>
    /// <param name="preventDeconstructionIfNotEmpty">If true, you cannot destroy this prefab unless all of its storage containers are empty.</param>
    /// <returns>A reference to the added <see cref="StorageContainer"/> instance.</returns>
    public static T AddStorageContainer<T>(GameObject prefabRoot, string storageRootName, string storageRootClassId,
            int width, int height, bool preventDeconstructionIfNotEmpty = true) where T : StorageContainer
    {
        var wasActive = prefabRoot.activeSelf;

        if (wasActive) prefabRoot.SetActive(false);

        var storageRoot = new GameObject(storageRootName);
        storageRoot.transform.SetParent(prefabRoot.transform, false);

        var childObjectIdentifier = storageRoot.AddComponent<ChildObjectIdentifier>();
        childObjectIdentifier.ClassId = storageRootClassId;
    
        var container = prefabRoot.AddComponent<T>();
        container.prefabRoot = prefabRoot;
        container.width = width;
        container.height = height;
        container.storageRoot = childObjectIdentifier;
        container.preventDeconstructionIfNotEmpty = preventDeconstructionIfNotEmpty;

        if (wasActive) prefabRoot.SetActive(true);

        return container;
    }

    private static FMODAsset _soundPowerUp = AudioUtils.GetFmodAsset("event:/tools/battery_insert", "{4ec9a6fe-0256-4f4f-8f42-6df726266063}");

    private static FMODAsset _soundPowerDown = AudioUtils.GetFmodAsset("event:/tools/battery_die", "{14490ac5-73e8-47ce-b7f9-26ac8cef9467}");

    /// <summary>
    /// <para>Adds the <see cref="EnergyMixin"/> component to an object that is expected to have a slot for one battery or other power source.</para>
    /// <para>Due to how this component needs to be initialized, this method will disable the object and re-enable it after the component is added (assuming it was already active). This all happens within the same frame and will not be seen.</para>
    /// </summary>
    /// <param name="prefabRoot">The root of the prefab object, where the component is added.</param>
    /// <param name="storageRootClassId">A unique string for the <see cref="ChildObjectIdentifier"/> component.</param>
    /// <param name="defaultBattery">The TechType of the battery that is added by default. If there should be no default, set this value to <see cref="TechType.None"/>.</param>
    /// <param name="compatibleBatteries">The list of all compatible batteries. By default is typically <see cref="TechType.Battery"/> and <see cref="TechType.PrecursorIonBattery"/>. Must not be null!</param>
    /// <param name="batteryModels">If assigned a value, allows different models to appear with different battery TechTypes. Also consider <see cref="EnergyMixin.controlledObjects"/> for a more basic version of this that is not TechType-dependent.</param>
    /// <param name="storageRootName">The name of the object that internally holds all of the batteries.</param>
    /// <returns>A reference to the added <see cref="EnergyMixin"/> instance.</returns>
    public static EnergyMixin AddEnergyMixin(GameObject prefabRoot, string storageRootClassId, TechType defaultBattery, List<TechType> compatibleBatteries, EnergyMixin.BatteryModels[] batteryModels = null, string storageRootName = "BatterySlot")
    {
        var wasActive = prefabRoot.activeSelf;

        if (wasActive) prefabRoot.SetActive(false);

        var batterySlot = new GameObject(storageRootName);
        batterySlot.transform.SetParent(prefabRoot.transform, false);
        var childObjectIdentifier = batterySlot.AddComponent<ChildObjectIdentifier>();
        childObjectIdentifier.ClassId = storageRootClassId;

        var em = prefabRoot.AddComponent<EnergyMixin>();
        em.storageRoot = childObjectIdentifier;
        em.defaultBattery = defaultBattery;
        em.compatibleBatteries = compatibleBatteries;
        em.batteryModels = batteryModels ?? Array.Empty<EnergyMixin.BatteryModels>();
        em.controlledObjects = Array.Empty<GameObject>();
        em.soundPowerUp = _soundPowerUp;
        em.soundPowerDown = _soundPowerDown;

        if (wasActive) prefabRoot.SetActive(true);

        return em;
    }
    
    /// <summary>
    /// Adds the <see cref="ResourceTracker"/> component to the passed game object to allow scanning via the Scanner Room.
    /// </summary>
    /// <param name="gameObject">the game object to add the resource tracker component to.</param>
    /// <param name="categoryTechType">TechType of the category in which this object will be displayed under in the Scanner Room.</param>
    /// <returns>A reference to the added <see cref="ResourceTracker"/> instance.</returns>
    public static ResourceTracker AddResourceTracker(GameObject gameObject, TechType categoryTechType)
    {
        var tt = CraftData.GetTechType(gameObject);

        if (tt == TechType.None)
        {
            InternalLogger.Error("TechType to get for AddResourceTracker is null");
            return null;
        }
        
        var resourceTracker = gameObject.EnsureComponent<ResourceTracker>();
        resourceTracker.techType = tt;
        resourceTracker.overrideTechType = categoryTechType;
        resourceTracker.rb = gameObject.GetComponent<Rigidbody>();
        resourceTracker.prefabIdentifier = gameObject.GetComponent<PrefabIdentifier>();
        resourceTracker.pickupable = gameObject.GetComponent<Pickupable>();

        return resourceTracker;
    }

    /// <summary>
    /// Adds the World Forces component to the prefab, which is required for proper physics handling.
    /// </summary>
    /// <param name="prefab">The prefab to modify.</param>
    /// <param name="mass">The mass of the new rigidbody (only if a rigidbody is being added).
    /// If the prefab already has a Rigidbody, this has NO EFFECT.</param>
    /// <param name="underwaterGravity">The underwater gravity in m/s/s.</param>
    /// <param name="underwaterDrag">The underwater drag coefficient (using Unity's arbitrary unit for drag).</param>
    /// <param name="isKinematic">If true, the Rigidbody will be kinematic when spawned and therefore immovable.
    /// Note that if the player picks up an item and drops it, its kinematic state will be reset to false.</param>
    /// <returns>A reference to the newly added (or previously existing) <see cref="WorldForces"/> component.</returns>
    public static WorldForces AddWorldForces(GameObject prefab, float mass, float underwaterGravity = 1f, float underwaterDrag = 1f, bool isKinematic = false)
    {
        if (!prefab.TryGetComponent<Rigidbody>(out var rb))
        {
            rb = prefab.AddComponent<Rigidbody>();
            rb.mass = mass;
        }
        rb.useGravity = false;
        rb.isKinematic = isKinematic;
        var wf = prefab.EnsureComponent<WorldForces>();
        wf.useRigidbody = rb;
        wf.underwaterGravity = underwaterGravity;
        wf.underwaterDrag = underwaterDrag;
        return wf;
    }
}