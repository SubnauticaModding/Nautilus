using System;
using System.Collections;
using Nautilus.Utility;
using UnityEngine;
using UWE;
using Object = UnityEngine.Object;

namespace Nautilus.Assets.PrefabTemplates;

/// <summary>
/// Represents an fabricator template. This template is capable of returning a Fabricator or a Workbench.
/// </summary>
public class FabricatorTemplate : PrefabTemplate
{
    /// <summary>
    /// Defines a list of available models for your custom fabricator.
    /// </summary>
    public enum Model
    {
        /// <summary>
        /// Use this option only if you want to provide your own custom model for your fabricator.<para/>
        /// To use this value, you must pass a non-null game object to the task result in the <see cref="FabricatorTemplate.GetPrefabAsync"/> method.
        /// </summary>
        Custom,
        
        /// <summary>
        /// The regular fabricator like the one in the life pod.
        /// </summary>
        Fabricator,

        /// <summary>
        /// The modification station that upgrades your equipment.
        /// </summary>
        Workbench,
        
#if SUBNAUTICA
        /// <summary>
        /// The style of fabricator found in the Moon Pool and the Cyclops sub.
        /// </summary>
        MoonPool,
#endif
    }
    
    /// <summary>
    /// The model this template will use. Leave it to <see cref="Model.Custom"/> if you've got a custom model.
    /// </summary>
    public Model FabricatorModel { get; set; }

    private ConstructableFlags _constructableFlags;

    /// <summary>
    /// Indicates where this fabricator can be placed.<para/>
    /// By default, this is set to the following flags: <br/>
    /// <see cref="Utility.ConstructableFlags.Inside"/> for all fabricators. <br/>
    /// <see cref="Utility.ConstructableFlags.Wall"/> for non-workbench fabricators.<br/>
    /// And <see cref="Utility.ConstructableFlags.Ground"/> and <see cref="Utility.ConstructableFlags.Rotatable"/> for workbench. 
    /// </summary>
    public ConstructableFlags ConstructableFlags 
    {
        get
        {
            if (_constructableFlags == ConstructableFlags.None)
            {
                _constructableFlags = ConstructableFlags.Inside | 
                    (FabricatorModel == Model.Workbench
                        ? ConstructableFlags.Ground | ConstructableFlags.Rotatable
                        : ConstructableFlags.Wall);
            }

            return _constructableFlags;
        }
        set => _constructableFlags = value;
    }

    /// <summary>
    /// Applies a simple color tint to the fabricator model.
    /// </summary>
    public Color? ColorTint { get; set; }

    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more.
    /// </summary>
    public System.Action<GameObject> ModifyPrefab { get; set; }
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more asynchronously.
    /// </summary>
    public System.Func<GameObject, IEnumerator> ModifyPrefabAsync { get; set; }
    
    private readonly CraftTree.Type _craftTreeType;
    
    /// <summary>
    /// Creates a <see cref="FabricatorTemplate"/> instance.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="craftTreeType">The craft tree type for this template.</param>
    public FabricatorTemplate(PrefabInfo info, CraftTree.Type craftTreeType) : base(info)
    {
        _craftTreeType = craftTreeType;
    }

    /// <summary>
    /// Gets the appropriate fabricator prefab.
    /// </summary>
    /// <param name="gameObject">The fabricator prefab is set into this argument.<br/>
    /// If the provided task result already has a game object, it will try to set the necessary components first.
    /// Otherwise; sets the standard Battery or Power Cell.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    public override IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject)
    {
        var obj = gameObject.Get();
        if (obj)
        {
            ApplyCrafterPrefab(obj);
            ModifyPrefab?.Invoke(obj);
            yield break;
        }

        yield return CreateFabricator(gameObject);
    }

    private IEnumerator CreateFabricator(IOut<GameObject> gameObject)
    {
        var task = GetReferenceTask();
        if (task is null || FabricatorModel == Model.Custom)
        {
            InternalLogger.Error($"""
            Failed retrieving the requested fabricator model. Please ensure the "{nameof(FabricatorTemplate)}.{nameof(FabricatorModel)}" property is assigned.
            If you are using a custom prefab, please assign the custom prefab to the passed task result.
            """);
            
            yield break;
        }
        
        yield return task;
        
        task.TryGetPrefab(out var prefab);
        var obj = Object.Instantiate(prefab);
        yield return ApplyCrafterPrefab(obj);
        gameObject.Set(obj);
    }

    private IPrefabRequest GetReferenceTask()
    {
        return FabricatorModel switch
        {
            Model.Fabricator => PrefabDatabase.GetPrefabAsync(CraftData.GetClassIdForTechType(TechType.Fabricator)),
            Model.Workbench => PrefabDatabase.GetPrefabAsync(CraftData.GetClassIdForTechType(TechType.Workbench)),
#if SUBNAUTICA
            Model.MoonPool => PrefabDatabase.GetPrefabForFilenameAsync("Submarine/Build/CyclopsFabricator.prefab"),
#endif
            _ => null,
        };
    }

    private IEnumerator ApplyCrafterPrefab(GameObject obj)
    {
        PrefabUtils.AddBasicComponents(obj, info.ClassID, info.TechType, LargeWorldEntity.CellLevel.Medium);
        GhostCrafter crafter;
        switch (FabricatorModel)
        {
            case Model.Fabricator:
            case Model.Custom:
                crafter = obj.EnsureComponent<Fabricator>();
                break;
#if SUBNAUTICA
            case Model.MoonPool:
                // Retrieve sub game objects
                GameObject cyclopsFabLight = obj.FindChild("fabricatorLight");
                GameObject cyclopsFabModel = obj.FindChild("submarine_fabricator_03");
                // Translate CyclopsFabricator model and light
                obj.transform.localPosition = new Vector3(cyclopsFabModel.transform.localPosition.x, // Same X position
                    cyclopsFabModel.transform.localPosition.y - 0.8f, // Push towards the wall slightly
                    cyclopsFabModel.transform.localPosition.z); // Same Z position
                obj.transform.localPosition = new Vector3(cyclopsFabLight.transform.localPosition.x, // Same X position
                    cyclopsFabLight.transform.localPosition.y - 0.8f, // Push towards the wall slightly
                    cyclopsFabLight.transform.localPosition.z); // Same Z position
                
                obj.EnsureComponent<Constructable>().model = cyclopsFabModel;
                crafter = obj.EnsureComponent<Fabricator>();
                break;
#endif
            case Model.Workbench:
                crafter = obj.EnsureComponent<Workbench>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(FabricatorModel));
        }

        crafter.craftTree = _craftTreeType;
        crafter.handOverText = $"Use {Language.main.Get(info.ClassID)}";

        PrefabUtils.AddConstructable(obj, info.TechType, ConstructableFlags);

        crafter.powerRelay = PowerSource.FindRelay(obj.transform);

        if (ColorTint.HasValue)
        {
            var renderer = obj.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.material.color = ColorTint.Value;
        }
        
        ModifyPrefab?.Invoke(obj);
        if (ModifyPrefabAsync is { })
            yield return ModifyPrefabAsync.Invoke(obj);
    }
}