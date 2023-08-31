using System.Collections;
using UnityEngine;

namespace Nautilus.Assets.PrefabTemplates;

/// <summary>
/// Represents the contract for a prefab template. 
/// </summary>
public abstract class PrefabTemplate
{
    /// <summary>
    /// The prefab info to operate on.
    /// </summary>
    protected readonly PrefabInfo info;

    /// <summary>
    /// Constructs a prefab template.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    public PrefabTemplate(PrefabInfo info)
    {
        this.info = info;
    }
    
    /// <summary>
    /// Gets a prefab of this template type. The returned object can be accessed via <see cref="TaskResult{T}.Get()"/>. 
    /// </summary>
    /// <param name="gameObject">The prefab computation result is set into this argument.<br/>
    /// If the provided task result already has a game object set to it, it will try to set the necessary components first. Otherwise; sets a default implementation of this entity type.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    public abstract IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject);

    /// <summary>
    /// Use this method to make changes to the prefab after the Nautilus' prefab processing is completed. Can be used to override or add more features to a prefab once it's settled. 
    /// </summary>
    /// <param name="prefab">The prefab to process.</param>
    /// <returns>A coroutine operation.</returns>
    public virtual IEnumerator OnPrefabPostProcessor(GameObject prefab)
    {
        yield break;
    }
}