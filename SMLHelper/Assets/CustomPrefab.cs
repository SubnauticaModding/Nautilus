using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SMLHelper.Assets.Gadgets;
using SMLHelper.Assets.PrefabTemplates;
using SMLHelper.Handlers;
using UnityEngine;

namespace SMLHelper.Assets;

public delegate IEnumerator PrefabFactoryAsync(TaskResult<GameObject> gameObject);

/// <summary>
/// Specifies the contract for a custom prefab.
/// </summary>
public interface ICustomPrefab
{
    /// <summary>
    /// The prefab info for this custom prefab.
    /// </summary>
    PrefabInfo Info { get; }
    
    /// <summary>
    /// Function which constructs a game object as this prefab.
    /// </summary>
    PrefabFactoryAsync Prefab { get; }
    
    /// <summary>
    /// Adds a gadget for this custom prefab.
    /// </summary>
    /// <param name="gadget">The gadget to add</param>
    void AddGadget(Gadget gadget);
    
    /// <summary>
    /// Adds an action that will be called during the registration of the prefab.
    /// </summary>
    /// <param name="onRegisterCallback">The action that will be called.</param>
    void AddOnRegister(Action onRegisterCallback);
}

/// <summary>
/// Represents a class with everything needed to make a custom prefab work.
/// </summary>
public class CustomPrefab : ICustomPrefab
{
    private readonly Dictionary<Type, Gadget> _gadgets = new();
    private readonly List<Action> _onRegister = new();

    private bool _registered;

    /// <inheritdoc/>
    public required PrefabInfo Info { get; set; }
    
    /// <inheritdoc/>
    public PrefabFactoryAsync Prefab { get; private set; }

    /// <summary>
    /// Constructs a custom prefab object.
    /// </summary>
    public CustomPrefab() { }
    
    /// <summary>
    /// Constructs a custom prefab object.
    /// </summary>
    /// <param name="info">The information this prefab will be registered as.</param>
    [SetsRequiredMembers]
    public CustomPrefab(PrefabInfo info)
    {
        Info = info;
    }
    
    /// <inheritdoc/>
    public void AddGadget(Gadget gadget)
    {
        _gadgets[gadget.GetType()] = gadget;
    }

    /// <inheritdoc/>
    public void AddOnRegister(Action onRegisterCallback)
    {
        _onRegister.Add(onRegisterCallback);
    }

    /// <summary>
    /// Sets a function as the game object constructor of this custom prefab. This is an asynchronous version.
    /// </summary>
    /// <param name="prefabAsync">The function to set.</param>
    public void SetPrefab(Func<IOut<GameObject>, IEnumerator> prefabAsync) => Prefab = obj => prefabAsync(obj);

    /// <summary>
    /// Sets a prefab template as the game object constructor of this custom prefab.
    /// </summary>
    /// <param name="prefabTemplate">The prefab template object to set.</param>
    public void SetPrefab(PrefabTemplate prefabTemplate) => Prefab = prefabTemplate.GetPrefabAsync;
    
    /// <summary>
    /// Sets a game object as the prefab of this custom prefab.
    /// </summary>
    /// <param name="prefab">The game object to set.</param>
    public void SetPrefab(GameObject prefab) => Prefab = obj => SyncPrefab(obj, prefab);
    
    /// <summary>
    /// Sets a function as the game object constructor of this custom prefab. This is a synchronous version.
    /// </summary>
    /// <param name="prefab">The function to set.</param>
    public void SetPrefab(Func<GameObject> prefab) => Prefab = obj => SyncPrefab(obj, prefab?.Invoke());

    /// <summary>
    /// Registers this custom prefab into the game.
    /// </summary>
    public void Register()
    {
        if (_registered)
            return;

        foreach (var reg in _onRegister)
        {
            reg?.Invoke();
        }

        foreach (var gadget in _gadgets)
        {
            gadget.Value.Build();
        }
        
        PrefabHandler.Prefabs.RegisterPrefab(this);

        _registered = true;
    }
    
    private IEnumerator SyncPrefab(IOut<GameObject> obj, GameObject prefab)
    {
        obj.Set(prefab);
        yield break;
    }
}