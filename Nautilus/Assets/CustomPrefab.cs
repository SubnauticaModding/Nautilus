using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Assets;

/// <summary>
/// A delegate for prefab construction used by Nautilus to create game objects dynamically.
/// </summary>
public delegate IEnumerator PrefabFactoryAsync(TaskResult<GameObject> gameObject);

/// <summary>
/// Delegate used for Nautilus's prefab post processing event to modify the processed prefab via a dynamic method.
/// </summary>
public delegate IEnumerator PrefabPostProcessorAsync(GameObject gameObject);

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
    /// Function which constructs a game object for this prefab.
    /// </summary>
    PrefabFactoryAsync Prefab { get; }

    /// <summary>
    /// Function that will be executed after the Nautilus's basic processing for <see cref="Prefab"/> has been completed.
    /// </summary>
    PrefabPostProcessorAsync OnPrefabPostProcess { get; }
    
    /// <summary>
    /// Adds a gadget to this custom prefab.
    /// </summary>
    /// <param name="gadget">The gadget to add</param>
    /// <typeparam name="TGadget">Type of the gadget.</typeparam>
    /// <returns>A reference to the added gadget.</returns>
    TGadget AddGadget<TGadget>(TGadget gadget) where TGadget : Gadget;

    /// <summary>
    /// Gets the gadget instance associated with the specified gadget type attached to this custom prefab.
    /// </summary>
    /// <param name="gadgetType">The type of the gadget to get.</param>
    /// <returns>The gadget instance if found, otherwise; <c>null</c>.</returns>
    Gadget GetGadget(Type gadgetType);

    /// <summary>
    /// Gets the gadget instance associated with the specified gadget type attached to this custom prefab.
    /// </summary>
    /// <typeparam name="TGadget">The type of the gadget to get.</typeparam>
    /// <returns>The gadget instance if found, otherwise; <c>null</c>.</returns>
    TGadget GetGadget<TGadget>() where TGadget : Gadget;

    /// <summary>
    /// Gets the gadget instance associated with the specified gadget type attached to this custom prefab.
    /// </summary>
    /// <param name="gadget">The instance of the gadget found associated with the type. If not found, this will be <c>null</c> instead.</param>
    /// <typeparam name="TGadget">The type of the gadget to get.</typeparam>
    /// <returns><see langword="true"/> if the gadget associated with type was found, otherwise; <see langword="false"/>.</returns>
    bool TryGetGadget<TGadget>(out TGadget gadget) where TGadget : Gadget;

    /// <summary>
    /// Removes the gadget with the specified type.
    /// </summary>
    /// <param name="gadget">The type of the gadget to remove.</param>
    /// <returns><see langword="true"/> if the gadget was successfully found and removed, otherwise; <see langword="false"/>.<br/>
    /// This method returns <see langword="false"/> if the gadget type was not found in this custom prefab.</returns>
    bool RemoveGadget(Type gadget);

    /// <summary>
    /// Removes the gadget with the specified type.
    /// </summary>
    /// <typeparam name="TGadget">The type of the gadget to remove.</typeparam>
    /// <returns><see langword="true"/> if the gadget was successfully found and removed, otherwise; <see langword="false"/>.<br/>
    /// This method returns <see langword="false"/> if the gadget type was not found in this custom prefab.</returns>
    bool RemoveGadget<TGadget>() where TGadget : Gadget;
    
    /// <summary>
    /// Adds an action that will be called during the registration of the prefab.
    /// </summary>
    /// <param name="onRegisterCallback">The action that will be called.</param>
    void AddOnRegister(Action onRegisterCallback);
    
    /// <summary>
    /// Adds an action that will be called when this prefab has performed an unregister operation.
    /// </summary>
    /// <param name="onUnregisterCallback">The action that will be called.</param>
    void AddOnUnregister(Action onUnregisterCallback);
}

/// <summary>
/// Represents a class with everything needed to make a custom prefab work.
/// </summary>
public class CustomPrefab : ICustomPrefab
{
    private readonly Dictionary<Type, Gadget> _gadgets = new();
    private readonly List<Action> _onRegister = new();
    private readonly List<Action> _onUnregister = new();

    private bool _registered;

    /// <inheritdoc/>
    public required PrefabInfo Info { get; set; }
    
    /// <inheritdoc/>
    public PrefabFactoryAsync Prefab { get; protected set; }

    /// <inheritdoc/>
    public PrefabPostProcessorAsync OnPrefabPostProcess { get; protected set; }

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

    /// <summary>
    /// Constructs a custom prefab object with the <see cref="Info"/> assigned appropriately.
    /// </summary>
    /// <param name="classId">The class identifier used for the PrefabIdentifier component whenever applicable.</param>
    /// <param name="displayName">The display name for this item.</param>
    /// <param name="description">The description for this item.</param>
    [SetsRequiredMembers]
    public CustomPrefab(string classId, string displayName, string description)
    {
        Info = PrefabInfo.WithTechType(classId, displayName, description);
    }
    

#if SUBNAUTICA
    /// <summary>
    /// Constructs a custom prefab object with the <see cref="Info"/> assigned appropriately.
    /// </summary>
    /// <param name="classId">The class identifier used for the PrefabIdentifier component whenever applicable.</param>
    /// <param name="displayName">The display name for this item.</param>
    /// <param name="description">The description for this item.</param>
    /// <param name="icon">The icon for this item.</param>
    [SetsRequiredMembers]
    public CustomPrefab(string classId, string displayName, string description, Atlas.Sprite icon) : this(classId, displayName, description)
    {
        Info.WithIcon(icon);
    }
#endif
    
    /// <summary>
    /// Constructs a custom prefab object with the <see cref="Info"/> assigned appropriately.
    /// </summary>
    /// <param name="classId">The class identifier used for the PrefabIdentifier component whenever applicable.</param>
    /// <param name="displayName">The display name for this item.</param>
    /// <param name="description">The description for this item.</param>
    /// <param name="icon">The icon for this item.</param>
    [SetsRequiredMembers]
    public CustomPrefab(string classId, string displayName, string description, Sprite icon) : this(classId, displayName, description)
    {
        Info.WithIcon(icon);
    }
    
    /// <inheritdoc/>
    public TGadget AddGadget<TGadget>(TGadget gadget) where TGadget : Gadget
    {
        _gadgets[gadget.GetType()] = gadget;
        return gadget;
    }

    /// <inheritdoc/>
    public Gadget GetGadget(Type gadgetType)
    {
        return _gadgets.TryGetValue(gadgetType, out var gadget) ? gadget : null;
    }

    /// <inheritdoc/>
    public TGadget GetGadget<TGadget>() where TGadget : Gadget
    {
        return GetGadget(typeof(TGadget)) as TGadget;
    }

    /// <inheritdoc/>
    public bool TryGetGadget<TGadget>(out TGadget gadget) where TGadget : Gadget
    {
        var result = _gadgets.TryGetValue(typeof(TGadget), out var g);
        gadget = (TGadget)g;
        return result;
    }

    /// <inheritdoc/>
    public bool RemoveGadget(Type gadget)
    {
        return _gadgets.Remove(gadget);
    }

    /// <inheritdoc/>
    public bool RemoveGadget<TGadget>() where TGadget : Gadget
    {
        return _gadgets.Remove(typeof(TGadget));
    }

    /// <inheritdoc/>
    public void AddOnRegister(Action onRegisterCallback)
    {
        _onRegister.Add(onRegisterCallback);
    }

    /// <inheritdoc/>
    public void AddOnUnregister(Action onUnregisterCallback)
    {
        _onUnregister.Add(onUnregisterCallback);
    }

    /// <summary>
    /// Sets a function as the game object constructor of this custom prefab. This is an asynchronous version.
    /// </summary>
    /// <param name="prefabAsync">The function to set.</param>
    public void SetGameObject(Func<IOut<GameObject>, IEnumerator> prefabAsync) => Prefab = obj => prefabAsync(obj);

    /// <summary>
    /// Sets a prefab template as the game object constructor of this custom prefab.
    /// </summary>
    /// <param name="prefabTemplate">The prefab template object to set.</param>
    public void SetGameObject(PrefabTemplate prefabTemplate) => Prefab = prefabTemplate.GetPrefabAsync;
    
    /// <summary>
    /// Sets a game object as the prefab of this custom prefab.
    /// </summary>
    /// <param name="prefab">The game object to set.</param>
    public void SetGameObject(GameObject prefab) => Prefab = obj => SyncPrefab(obj, prefab);
    
    /// <summary>
    /// Sets a function as the game object constructor of this custom prefab. This is a synchronous version.
    /// </summary>
    /// <param name="prefab">The function to set.</param>
    public void SetGameObject(Func<GameObject> prefab) => Prefab = obj => SyncPrefab(obj, prefab?.Invoke());

    /// <summary>
    /// Sets a post processor for the <see cref="Prefab"/>. This is an asynchronous version.
    /// </summary>
    /// <param name="postProcessorAsync">The post processor to set.</param>
    public void SetPrefabPostProcessor(Func<GameObject, IEnumerator> postProcessorAsync) => OnPrefabPostProcess = obj => postProcessorAsync(obj);

    /// <summary>
    /// Sets a post processor for the <see cref="Prefab"/>. This is a synchronous version.
    /// </summary>
    /// <param name="postProcessor">The post processor to set.</param>
    public void SetPrefabPostProcessor(Action<GameObject> postProcessor) => OnPrefabPostProcess = obj => SyncPostProcessor(obj, postProcessor);

    /// <summary>
    /// Registers this custom prefab into the game.
    /// </summary>
    public void Register()
    {
        if (_registered)
            return;
        
        // Every prefab must have a class ID and a PrefabFileName, so if they don't exist, registration should be cancelled.
        if (string.IsNullOrEmpty(Info.ClassID) || string.IsNullOrEmpty(Info.PrefabFileName))
        {
            InternalLogger.Error($"Prefab '{Info}' does not contain a class ID or a PrefabFileName. Skipping registration.");
            return;
        }

        /*
         * It is fine for some prefabs to not have a tech type (E.G: world decorators, or anything that isn't scannable),
         * so just warn it in-case people forgot to add one.
         */
        if (Info.TechType is TechType.None)
        {
            InternalLogger.Warn($"Prefab '{Info}' does not contain a TechType.");
        }

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

    /// <summary>
    /// Unregisters this custom prefab from the game.
    /// </summary>
    /// <remarks>The class ID reference will be completely erased, however, the TechType instance will remain in the game.</remarks>
    public void Unregister()
    {
        if (!_registered)
            return;

        if (string.IsNullOrEmpty(Info.ClassID) || string.IsNullOrEmpty(Info.PrefabFileName))
        {
            InternalLogger.Info($"Prefab '{Info}' is invalid or unknown. Skipping unregister operation.");
            return;
        }
        
        foreach (var unReg in _onUnregister)
        {
            unReg?.Invoke();
        }
        
        PrefabHandler.Prefabs.UnregisterPrefab(this);

        _registered = false;
    }
    
    private IEnumerator SyncPrefab(IOut<GameObject> obj, GameObject prefab)
    {
        obj.Set(prefab);
        yield break;
    }

    private IEnumerator SyncPostProcessor(GameObject prefab, Action<GameObject> postProcessor)
    {
        postProcessor?.Invoke(prefab);
        yield break;
    }
}