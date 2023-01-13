using System.Collections;
using UnityEngine;

namespace SMLHelper.Assets.PrefabTemplates;

/// <summary>
/// Defines a prefab template. Prefab templates are used to either get a default implementation, or set the necessary components of a specific type of entities.
/// </summary>
public interface IPrefabTemplate
{
    /// <summary>
    /// Gets a prefab of this template type. The returned object can be accessed via <see cref="TaskResult{T}.Get()"/>. 
    /// </summary>
    /// <param name="gameObject">The prefab computation result is set into this argument.<br/>
    /// If the provided task result already has a game object set to it, it will try to set the necessary components first. Otherwise; sets a default implementation of this entity type.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject);
}