using System;
using System.Collections;
using Nautilus.MonoBehaviours;
using UnityEngine;

namespace Nautilus.Assets.PrefabTemplates;

/// <summary>
/// A template for Atmosphere Volumes, which are basic invisible triggers for mini-biomes. Atmosphere volumes can affect fog, music, ambient sounds and even the player's swim speed.
/// </summary>
public class AtmosphereVolumeTemplate : PrefabTemplate
{
    private const int AtmosphereVolumesLayer = 21;

    /// <summary>
    /// The shape of this atmosphere volume.
    /// </summary>
    public VolumeShape Shape { get; set; }
    /// <summary>
    /// The biome type used by this atmosphere volume.
    /// </summary>
    public string OverrideBiome { get; set; }
    /// <summary>
    /// The priority of this atmosphere volume. Atmosphere volumes with higher priorities override those with lower priorities. The default priority is 10.
    /// </summary>
    public int Priority { get; set; }
    /// <summary>
    /// Whether this atmosphere volume can be entered while inside a vehicle or not. For unknown reasons, this is NOT true for base game volumes. However, in this template, it is true by default.
    /// </summary>
    public bool CanEnterWhileInsideVehicle { get; set; } = true;

    /// <summary>
    /// Determines the loading distance of this atmosphere volume prefab. Default value is <see cref="LargeWorldEntity.CellLevel.Far"/>. Although vanilla prefabs always use Batch for this, this does not work with our custom systems.
    /// </summary>
    public LargeWorldEntity.CellLevel CellLevel { get; set; }
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more.
    /// </summary>
    public System.Action<GameObject> ModifyPrefab { get; set; }
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more asynchronously.
    /// </summary>
    public System.Func<GameObject, IEnumerator> ModifyPrefabAsync { get; set; }

    /// <summary>
    /// Creates a new prefab template for an asset bundle.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="shape">The shape of this atmosphere volume.</param>
    /// <param name="overrideBiome">The biome type used by this atmosphere volume.</param>
    /// <param name="priority">The priority of this atmosphere volume. Atmosphere volumes with higher priorities override those with lower priorities.</param>
    /// <param name="cellLevel">Determines the loading distance of this atmosphere volume prefab. Although vanilla prefabs always use Batch for this, this does not work with our custom systems.</param>
    public AtmosphereVolumeTemplate(PrefabInfo info, VolumeShape shape, string overrideBiome, int priority = 10, LargeWorldEntity.CellLevel cellLevel = LargeWorldEntity.CellLevel.Far) : base(info)
    {
        Shape = shape;
        OverrideBiome = overrideBiome;
        Priority = priority;
        CellLevel = cellLevel;
    }

    /// <summary>
    /// Creates an atmosphere volume prefab.
    /// </summary>
    public override IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject)
    {
        var prefab = new GameObject(info.ClassID);
        prefab.SetActive(false);
        prefab.layer = AtmosphereVolumesLayer;
        
        Collider collider = Shape switch
        {
            VolumeShape.Sphere => prefab.AddComponent<SphereCollider>(),
            VolumeShape.Cube => prefab.AddComponent<BoxCollider>(),
            VolumeShape.Capsule => prefab.AddComponent<CapsuleCollider>(),
            _ => throw new NotImplementedException()
        };
        collider.isTrigger = true;
        
        prefab.AddComponent<PrefabIdentifier>().ClassId = info.ClassID;
        prefab.AddComponent<LargeWorldEntity>().cellLevel = CellLevel;

        var atmosphereVolume = prefab.AddComponent<AtmosphereVolume>();
        atmosphereVolume.overrideBiome = OverrideBiome;
        atmosphereVolume.priority = Priority;

        if (CanEnterWhileInsideVehicle)
        {
            prefab.AddComponent<AtmosphereVolumeTriggerFix>().atmosphereVolume = atmosphereVolume;
        }
        
        ModifyPrefab?.Invoke(prefab);
        if (ModifyPrefabAsync is { })
            yield return ModifyPrefabAsync(prefab);

        gameObject.Set(prefab);
    }

    /// <summary>
    /// The shape of an atmosphere volume's trigger.
    /// </summary>
    public enum VolumeShape
    {
        /// <summary>
        /// Sphere with default radius 0.5m (diameter 1m).
        /// </summary>
        Sphere,
        /// <summary>
        /// Cube with default dimensions of 1x1x1m.
        /// </summary>
        Cube,
        /// <summary>
        /// Capsule with default radius of 0.5m and height of 2m.
        /// </summary>
        Capsule
    }
}