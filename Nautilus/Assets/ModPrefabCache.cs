using Nautilus.Utility;
using System.Collections.Generic;
using Nautilus.Extensions;
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
    /// <param name="prefab"> The prefab object that is disabled and cached. </param>
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
    public static void RemovePrefabFromCache(string classId)
    {
        if(_cacheInstance == null)
            return;

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
    public Dictionary<string, GameObject> Entries { get; } = new Dictionary<string, GameObject>();

    private Transform _prefabRoot;

    private void Awake()
    {
        _prefabRoot = new GameObject("PrefabRoot").transform;
        _prefabRoot.parent = transform;
        _prefabRoot.gameObject.SetActive(false);

        gameObject.AddComponent<SceneCleanerPreserve>();
        DontDestroyOnLoad(gameObject);
        SaveUtils.RegisterOnQuitEvent(ModPrefabCache.RunningPrefabs.Clear);
    }

    public void EnterPrefabIntoCache(GameObject prefab)
    {
        var prefabIdentifier = prefab.GetComponent<PrefabIdentifier>();

        if (prefabIdentifier == null)
        {
            InternalLogger.Warn($"ModPrefabCache: prefab {prefab.name} is missing a PrefabIdentifier component! Unable to add to cache.");
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
        if(Entries.TryGetValue(classId, out var prefab))
        {
            if(!prefab.IsPrefab())
                Destroy(prefab);
            InternalLogger.Debug($"ModPrefabCache: removed prefab {classId}");
            Entries.Remove(classId);
        }
    }
}