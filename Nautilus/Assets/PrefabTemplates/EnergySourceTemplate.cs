using System;
using System.Collections;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Assets.PrefabTemplates;

/// <summary>
/// Represents an energy source template. This template is capable of returning a Battery or a Power Cell.
/// </summary>
public class EnergySourceTemplate : PrefabTemplate
{
    /// <summary>
    /// Is this energy source a Power Cell?
    /// </summary>
    public bool IsPowerCell { get; set; }

    /// <summary>
    /// Determines which model to use as the base. If <c>true</c>, this template will use the Precursor Ion Battery or Power cell.<br/>
    /// Otherwise; uses the default Battery or Power Cell models.
    /// </summary>
    public bool UseIonModelAsBase { get; set; }
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more.
    /// </summary>
    public System.Action<GameObject> ModifyPrefab { get; set; }
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more asynchronously.
    /// </summary>
    public System.Func<GameObject, IEnumerator> ModifyPrefabAsync { get; set; }

    private readonly int _energyAmount;

    /// <summary>
    /// Creates an <see cref="EnergySourceTemplate"/> instance.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="energyAmount">The amount of energy this source should have.</param>
    public EnergySourceTemplate(PrefabInfo info, int energyAmount) : base(info)
    {
        _energyAmount = energyAmount;
    }
    
    /// <summary>
    /// Gets the appropriate energy source prefab.
    /// </summary>
    /// <param name="gameObject">The energy source prefab is set into this argument.<br/>
    /// If the provided task result already has a game object, it will try to set the necessary components first.
    /// Otherwise; sets the standard Battery or Power Cell.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    public override IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject)
    {
        var obj = gameObject.Get();
        if (obj)
        {
            obj.SetActive(false);
            ApplyModifications(obj);
            yield break;
        }

        yield return CreateEnergySource(gameObject);
    }

    private IEnumerator CreateEnergySource(IOut<GameObject> gameObject)
    {
        var tt = GetReferenceType();
        var task = CraftData.GetPrefabForTechTypeAsync(tt, false);
        yield return task;

        var obj = UWE.Utils.InstantiateDeactivated(task.GetResult());

        yield return ApplyModifications(obj);
        
        gameObject.Set(obj);
    }

    private TechType GetReferenceType()
    {
        return IsPowerCell switch
        {
            false when !UseIonModelAsBase => TechType.Battery,
            true when !UseIonModelAsBase => TechType.PowerCell,
            false when UseIonModelAsBase => TechType.PrecursorIonBattery,
            true when UseIonModelAsBase => TechType.PrecursorIonPowerCell,
            _ => throw new NotSupportedException()
        };
    }

    private IEnumerator ApplyModifications(GameObject obj)
    {
        PrefabUtils.AddBasicComponents(obj, info.ClassID, info.TechType, LargeWorldEntity.CellLevel.Medium);
        var battery = obj.EnsureComponent<Battery>();
        battery._capacity = _energyAmount;
        
        ModifyPrefab?.Invoke(obj);
        if (ModifyPrefabAsync is { })
            yield return ModifyPrefabAsync.Invoke(obj);
    }
}