using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SMLHelper.Assets.Gadgets;
using SMLHelper.Assets.PrefabTemplates;
using SMLHelper.Handlers;
using UnityEngine;

namespace SMLHelper.Assets;

/// <summary>
/// Specifies the contract for a custom prefab.
/// </summary>
public interface ICustomPrefab
{
    /// <summary>
    /// The prefab info for this custom prefab.
    /// </summary>
    PrefabInfo Info { get; set; }
    
    /// <summary>
    /// Function which constructs a game object as this prefab.
    /// </summary>
    Func<TaskResult<GameObject>, IEnumerator> Prefab { get; }
    
    /// <summary>
    /// Adds a gadget for this custom prefab.
    /// </summary>
    /// <param name="gadget">The gadget to add</param>
    void AddGadget(Gadget gadget);
    
    /// <summary>
    /// Adds a post register action that will be called on post register event.
    /// </summary>
    /// <param name="postRegisterCallback">The action that will be called.</param>
    void AddPostRegister(Action postRegisterCallback);
}

/// <summary>
/// Represents a class with everything needed to make a custom prefab work.
/// </summary>
public class CustomPrefab : ICustomPrefab
{
    private readonly Dictionary<Type, Gadget> _gadgets = new();
    private readonly List<Action> _postRegister = new();

    private bool _registered;

    /// <inheritdoc/>
    public required PrefabInfo Info { get; set; }
    
    /// <inheritdoc/>
    public Func<TaskResult<GameObject>, IEnumerator> Prefab { get; private set; }

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
    public void AddPostRegister(Action postRegisterCallback)
    {
        _postRegister.Add(postRegisterCallback);
    }

    /// <summary>
    /// Sets a function as the game object constructor of this custom prefab.
    /// </summary>
    /// <param name="prefab">The Function to set.</param>
    public void SetPrefab(Func<IOut<GameObject>, IEnumerator> prefab) => Prefab = prefab;

    /// <summary>
    /// Sets a prefab template as the game object constructor of this custom prefab.
    /// </summary>
    /// <param name="prefabTemplate">The prefab template object to set.</param>
    public void SetPrefab(PrefabTemplate prefabTemplate) => Prefab = prefabTemplate.GetPrefabAsync;

    /// <summary>
    /// Registers this custom prefab into the game.
    /// </summary>
    public void Register()
    {
        if (_registered)
            return;
        
        PrefabHandler.Prefabs.RegisterPrefab(this);

        foreach (var reg in _postRegister)
        {
            reg?.Invoke();
        }

        foreach (var gadget in _gadgets)
        {
            gadget.Value.Build();
        }

        _registered = true;
    }
}