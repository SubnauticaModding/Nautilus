using Nautilus.Utility;
using System.Collections.Generic;
using Nautilus.Extensions;
using Nautilus.Handlers;
using UnityEngine;

namespace Nautilus.Assets;

/// <summary>
/// Class used by the prefab system to store GameObjects.
/// Objects in the cache are inactive because they are placed within an inactive parent object.
/// </summary>
public static class ModPrefabCache
{
    internal static HashSet<string> RunningPrefabs = new();
    
    private static ModPrefabCacheInstance _cacheInstance;

    /// <summary> Adds the given prefab to the cache. </summary>
    /// <param name="prefab"> The prefab object that is disabled and cached.</param>
    public static void AddPrefab(GameObject prefab)
    {
        EnsureCacheExists();

        _cacheInstance.EnterPrefabIntoCache(prefab);
    }

    /// <summary>
    /// Determines if a prefab is already cached, searching by class id.
    /// </summary>
    /// <param name="classId">The class id to search for.</param>
    /// <returns>True if a prefab by the given <paramref name="classId"/> exists in the cache, otherwise false.</returns>
    public static bool IsPrefabCached(string classId)
    {
        return _cacheInstance != null && _cacheInstance.Entries.ContainsKey(classId);
    }

    /// <summary>
    /// Any prefab with the matching <paramref name="classId"/> will be removed from the cache.
    /// </summary>
    /// <param name="classId">The class id of the prefab that will be removed.</param>
    /// <remarks>This operation is extremely dangerous on custom prefabs that are directly registering an asset bundle prefab as it may make the prefab unusable in the current session.<br/>Avoid using this method unless you know what you're doing.</remarks>
    public static void RemovePrefabFromCache(string classId)
    {
        if (_cacheInstance == null)
        {
            InternalLogger.Debug($"Removed '{classId}' from prefab cache.");
            ModPrefabCacheInstance.BannedPrefabs.Add(classId);
            return;
        }

        _cacheInstance.RemoveCachedPrefab(classId);
    }

    /// <summary>
    /// Attempts to fetch a prefab from the cache by its <paramref name="classId"/>. The <paramref name="prefab"/> out parameter is set to the prefab, if any was found.
    /// </summary>
    /// <param name="classId">The class id of the prefab we are searching for.</param>
    /// <param name="prefab">The prefab that may or may not be found.</param>
    /// <returns>True if the prefab was found in the cache, otherwise false.</returns>
    public static bool TryGetPrefabFromCache(string classId, out GameObject prefab)
    {
        if(_cacheInstance == null)
        {
            prefab = null;
            return false;
        }

        return _cacheInstance.Entries.TryGetValue(classId, out prefab) && prefab != null;
    }

    private static void EnsureCacheExists()
    {
        if(_cacheInstance != null)
            return;
        _cacheInstance = new GameObject("Nautilus.PrefabCache").AddComponent<ModPrefabCacheInstance>();
    }
}

internal class ModPrefabCacheInstance: MonoBehaviour
{
    public Dictionary<string, GameObject> Entries { get; } = new();
    
    // Prefabs that are banned from getting cached
    internal static readonly HashSet<string> BannedPrefabs = new();

    private Transform _prefabRoot;

    private void Awake()
    {
        _prefabRoot = new GameObject("PrefabRoot").transform;
        _prefabRoot.parent = transform;
        _prefabRoot.gameObject.SetActive(false);

        gameObject.AddComponent<SceneCleanerPreserve>();
        DontDestroyOnLoad(gameObject);
        
        SaveUtils.RegisterOnQuitEvent(ModPrefabCache.RunningPrefabs.Clear);
        SaveUtils.RegisterOnQuitEvent(RemoveFakePrefabs);
    }

    public void EnterPrefabIntoCache(GameObject prefab)
    {
        var prefabIdentifier = prefab.GetComponent<PrefabIdentifier>();

        if (prefabIdentifier == null)
        {
            InternalLogger.Warn($"ModPrefabCache: prefab {prefab.name} is missing a PrefabIdentifier component! Unable to add to cache.");
            return;
        }

        if (BannedPrefabs.Contains(prefabIdentifier.classId))
        {
            return;
        }

        if (!Entries.ContainsKey(prefabIdentifier.classId))
        {
            Entries.Add(prefabIdentifier.classId, prefab);
            InternalLogger.Debug($"ModPrefabCache: added prefab {prefab}");
            // Proper prefabs can never exist in the scene, so parenting them is dangerous and pointless. 
            if (prefab.IsPrefab())
            {
                InternalLogger.Debug($"Game Object: {prefab} is a proper prefab. Skipping parenting for cache.");
            }
            else
            {
                prefab.transform.parent = _prefabRoot;
                ResetIds(prefab);
                prefab.SetActive(true);
            }
        }
        else // This should never happen
        {
            InternalLogger.Warn($"ModPrefabCache: prefab {prefabIdentifier.classId} already existed in cache!");
        }   
    }

    public void RemoveCachedPrefab(string classId)
    {
        BannedPrefabs.Add(classId);

        if (!Entries.TryGetValue(classId, out var prefab))
        {
            return;
        }
        
        if (!prefab)
        {
            InternalLogger.Debug($"ModPrefabCache: Prefab for '{classId}' is null; removing entry.");
            Entries.Remove(classId);
            return;
        }
            
        if (!prefab.IsPrefab())
        {
            Destroy(prefab);
        }
            
        Entries.Remove(classId);
        InternalLogger.Debug($"ModPrefabCache: removing prefab '{classId}'");
    }

    private void RemoveFakePrefabs()
    {
        foreach (var prefab in new Dictionary<string, GameObject>(Entries))
        {
            if (prefab.Value.Exists() is null)
            {
                Entries.Remove(prefab.Key);
                continue;
            }

            if (prefab.Value.IsPrefab())
            {
                continue;
            }
            
            Destroy(prefab.Value);
            Entries.Remove(prefab.Key);
        }
    }

    private void ResetIds(GameObject prefab)
    {
        var uniqueIds = prefab.GetAllComponentsInChildren<UniqueIdentifier>();

        foreach (var uniqueId in uniqueIds)
        {
            if (string.IsNullOrEmpty(uniqueId.id))
            {
                continue;
            }
            
            UniqueIdentifier.identifiers.Remove(uniqueId.id);
            uniqueId.id = null;
        }
    }
}