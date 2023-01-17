namespace SMLHelper.Assets.PrefabTemplates;

using System;
using System.Collections;
using SMLHelper.API;
using UnityEngine;

/// <summary>
/// Represents an energy source template. This template is capable of returning a Battery or a Power Cell.
/// </summary>
public class EnergySourceTemplate
{
    /// <summary>
    /// Is this energy source a Power Cell?
    /// </summary>
    public bool IsPowerCell { get; init; }
    
    /// <summary>
    /// Does this use the basic models or the Ion models
    /// </summary>
    public bool UseIonModel { get; init; }

    private readonly float _energyAmount;

    /// <summary>
    /// Creates an <see cref="EnergySourceTemplate"/> instance.
    /// </summary>
    /// <param name="energyAmount">The amount of energy this source can store.</param>
    public EnergySourceTemplate(float energyAmount)
    {
        _energyAmount = energyAmount;
    }

    /// <summary>
    /// Instantiates a copy of the appropriate energy source prefab and sets the capacity.
    /// </summary>
    /// <param name="gameObjectTask">The energy source prefab task. Use gameObjectTask.Get() to retrieve the loaded GameObject.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    public IEnumerator GetPrefabAsync(IOut<GameObject> gameObjectTask)
    {
        var tt = GetReferenceType();
        var task = new TaskResult<GameObject>();
        yield return CraftData.InstantiateFromPrefabAsync(tt, task);
        var obj = task.Get();

        var battery = obj.EnsureComponent<Battery>();
        battery._capacity = _energyAmount;

        gameObjectTask.Set(obj);
    }

    private TechType GetReferenceType()
    {
        return IsPowerCell switch
        {
            false when !UseIonModel => TechType.Battery,
            true when !UseIonModel => TechType.PowerCell,
            false when UseIonModel => TechType.PrecursorIonBattery,
            true when UseIonModel => TechType.PrecursorIonPowerCell,
            _ => throw new NotSupportedException()
        };
    }
}