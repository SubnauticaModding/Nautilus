using System;
using System.Collections;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UWE;

namespace Nautilus.Assets.PrefabTemplates;

/// <summary>
/// Represents a creature egg prefab template.
/// </summary>
public class EggTemplate : PrefabTemplate
{
    private readonly Func<TaskResult<GameObject>, IEnumerator> _modelFactory;
    
    /// <summary>
    /// The Undiscovered Techtype of this egg.
    /// </summary>
    /// <returns>The undiscovered TechType if the <see cref="SetUndiscoveredTechType"/> was invoked; otherwise <see cref="TechType.None"/>.</returns>
    public TechType UndiscoveredTechType { get; private set; }

    /// <summary>
    /// the creature that's going to hatch from this egg.
    /// </summary>
    public TechType HatchingCreature { get; set; }

    /// <summary>
    /// amount of in-game days this egg will take to hatch the <seealso cref="HatchingCreature"/>.
    /// </summary>
    public float HatchingTime { get; set; }

    /// <summary>
    /// Mass of the egg by KG. defaulted to 100.
    /// </summary>
    public float Mass { get; set; } = 100f;

    /// <summary>
    /// Health of the egg. defaulted to 60.
    /// </summary>
    public float MaxHealth { get; set; } = 60f;
    
    /// <summary>
    /// Determines how close you have to be to the egg for it to appear. Defaults to <see cref="LargeWorldEntity.CellLevel.Medium"/>.
    /// </summary>
    public LargeWorldEntity.CellLevel CellLevel { get; set; } = LargeWorldEntity.CellLevel.Medium;
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more.
    /// </summary>
    public Action<GameObject> ModifyPrefab { get; set; }

    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more asynchronously.
    /// </summary>
    public Func<GameObject, IEnumerator> ModifyPrefabAsync { get; set; }
    
    /// <summary>
    /// Creates a <see cref="EggTemplate"/> instance and sets the specified asset bundle prefab as the base model.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="assetBundleTemplate">The asset bundle prefab to set as the model.</param>
    public EggTemplate(PrefabInfo info, AssetBundleTemplate assetBundleTemplate) : base(info)
    {
        _modelFactory = assetBundleTemplate.GetPrefabAsync;
    }
    
    /// <summary>
    /// Creates a <see cref="EggTemplate"/> instance and sets the specified game object as the base model.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="model">The game object to set as the model.</param>
    public EggTemplate(PrefabInfo info, GameObject model) : base(info)
    {
        _modelFactory = obj => SyncPrefab(obj, model);
    }
    
    /// <summary>
    /// Creates a <see cref="EggTemplate"/> instance and sets a tech type as the base model.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="techType">The tech type to clone and use as the base.</param>
    public EggTemplate(PrefabInfo info, TechType techType) : base(info)
    {
        _modelFactory = new CloneTemplate(info, techType).GetPrefabAsync;
    }

    /// <summary>
    /// Creates a <see cref="EggTemplate"/> instance and sets a class ID as the base model.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="classId">The class ID to clone and use as the base.</param>
    public EggTemplate(PrefabInfo info, string classId) : base(info)
    {
        _modelFactory = new CloneTemplate(info, classId).GetPrefabAsync;
    }

    /// <summary>
    /// the creature that's going to hatch from this egg.
    /// </summary>
    /// <param name="hatchingCreature">The tech type for the hatching creature.</param>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate WithHatchingCreature(TechType hatchingCreature)
    {
        HatchingCreature = hatchingCreature;

        return this;
    }
    
    /// <summary>
    /// amount of in-game days this egg will take to hatch the <seealso cref="HatchingCreature"/>.
    /// </summary>
    /// <param name="hatchingTime">The amount of days.</param>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate WithHatchingTime(float hatchingTime)
    {
        HatchingTime = hatchingTime;

        return this;
    }

    /// <summary>
    /// Mass of the egg by KG.
    /// </summary>
    /// <param name="mass">The mass.</param>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate WithMass(float mass)
    {
        Mass = mass;

        return this;
    }
    
    /// <summary>
    /// Health of the egg.
    /// </summary>
    /// <param name="maxHealth">The health.</param>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate WithMaxHealth(float maxHealth)
    {
        MaxHealth = maxHealth;

        return this;
    }

    /// <summary>
    /// Determines how close you have to be to the egg for it to appear.
    /// </summary>
    /// <param name="cellLevel">The cell level.</param>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate WithCellLevel(LargeWorldEntity.CellLevel cellLevel)
    {
        CellLevel = cellLevel;

        return this;
    }

    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more.
    /// </summary>
    /// <param name="modifyPrefabAction">The callback.</param>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate OnModifyPrefab(Action<GameObject> modifyPrefabAction)
    {
        ModifyPrefab = modifyPrefabAction;

        return this;
    }
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more asynchronously.
    /// </summary>
    /// <param name="modifyPrefabAsyncAction">The callback.</param>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate OnModifyPrefabAsync(Func<GameObject, IEnumerator> modifyPrefabAsyncAction)
    {
        ModifyPrefabAsync = modifyPrefabAsyncAction;

        return this;
    }

    /// <summary>
    /// Makes this egg have an unidentified egg tech type before hatching. Once it hatches, it will receive the main egg tech type.
    /// </summary>
    /// <returns>A reference to this instance after the operation is completed.</returns>
    public EggTemplate SetUndiscoveredTechType()
    {
        UndiscoveredTechType = EnumHandler.AddEntry<TechType>(info.ClassID + "Undiscovered")
            .WithPdaInfo("Creature Egg", "An unidentified egg.")
            .WithIcon(SpriteManager.Get(info.TechType))
#if SUBNAUTICA
            .WithSizeInInventory(CraftData.GetItemSize(info.TechType));
#else
            .WithSizeInInventory(TechData.GetItemSize(info.TechType));
#endif

        return this;
    }
    
    /// <summary>
    /// Gets the appropriate egg prefab.
    /// </summary>
    /// <param name="gameObject">The egg prefab is is set to this argument.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    public override IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject)
    {
        var obj = gameObject.Get();
        if (obj)
        {
            yield return ProcessEgg(obj);
            yield break;
        }

        if (_modelFactory is { })
        {
            var task = new TaskResult<GameObject>();
            yield return _modelFactory.Invoke(task);
            obj = task.Get();
        }
        if (obj == null)
        {
            InternalLogger.Error($$"""
                                   No game object was passed to the {{nameof(EggTemplate)}}.
                                   Please use one of the constructor overloads that take a game object or pass a game object directly to the GetPrefabAsync method.
                                   """);
            yield break;
        }
        yield return ProcessEgg(obj);
            
        gameObject.Set(obj);
    }
        
    private IEnumerator ProcessEgg(GameObject obj)
    {
        obj.EnsureComponent<TechTag>().type = info.TechType;
        obj.EnsureComponent<PrefabIdentifier>().ClassId = info.ClassID;
        obj.EnsureComponent<LargeWorldEntity>().cellLevel = CellLevel;

        var skyApplier = obj.EnsureComponent<SkyApplier>();
        skyApplier.anchorSky = Skies.Auto;
        skyApplier.emissiveFromPower = false;
        skyApplier.dynamic = false;
        skyApplier.renderers = obj.GetAllComponentsInChildren<Renderer>();

        obj.EnsureComponent<Pickupable>();

        var rb = obj.EnsureComponent<Rigidbody>();
        rb.mass = Mass;
        rb.isKinematic = true;
        rb.useGravity = false;

        var wf = obj.EnsureComponent<WorldForces>();
        wf.useRigidbody = rb;

        var liveMixin = obj.EnsureComponent<LiveMixin>();
        liveMixin.data = ScriptableObject.CreateInstance<LiveMixinData>();
        liveMixin.data.destroyOnDeath = true;
        liveMixin.data.maxHealth = MaxHealth;
        liveMixin.health = MaxHealth;

        var creatureEgg = obj.EnsureComponent<CreatureEgg>();
#if SUBNAUTICA
        creatureEgg.animator = obj.GetComponentInChildren<Animator>() ?? obj.EnsureComponent<Animator>();
#else
        creatureEgg.animators = obj.GetComponentsInChildren<Animator>();
#endif
        creatureEgg.creatureType = HatchingCreature;
        PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(HatchingCreature), out var filename);
        creatureEgg.creaturePrefab = new AssetReferenceGameObject(filename).ForceValid();
        creatureEgg.daysBeforeHatching = HatchingTime;
        if (UndiscoveredTechType != TechType.None)
            creatureEgg.overrideEggType = UndiscoveredTechType;

        obj.EnsureComponent<WaterParkItem>();
        
        MaterialUtils.ApplySNShaders(obj);
        
        ModifyPrefab?.Invoke(obj);
        if (ModifyPrefabAsync is { })
        {
            yield return ModifyPrefabAsync(obj);
        }
    }
    
    private IEnumerator SyncPrefab(IOut<GameObject> obj, GameObject prefab)
    {
        obj.Set(prefab);
        yield break;
    }
}