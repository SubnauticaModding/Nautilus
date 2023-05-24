using System.Collections.Generic;
using UnityEngine;

namespace Nautilus.Assets;

/// <summary>
/// Class used by the prefab system to store GameObjects.
/// Objects in the cache are inactive because they are placed within an inactive parent object.
/// </summary>
public static class ModPrefabCache
{
    private static ModPrefabCacheInstance _cacheInstance;

    internal record struct Entry(string ClassId, GameObject Prefab);

    /// <summary> Adds the given prefab to the cache. </summary>
    /// <param name="prefab"> The prefab object that is disabled and cached. </param>
    public static void AddPrefab(GameObject prefab)
    {
        EnsureCacheExists();
    }

    /// <summary>
    /// Returns
    /// </summary>
    /// <param name="classId"></param>
    /// <returns></returns>
    public static bool IsPrefabCached(string classId)
    {
        if (_cacheInstance == null)
            return false;
        return _cacheInstance.Entries.ContainsKey(classId);
    }

    /// <summary>
    /// Returns true if the prefab matching the given <paramref name="classId"/> is found in the cache, where it then assigns the <paramref name="prefab"/> out parameter.
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static bool TryGetPrefabFromCache(string classId, out GameObject prefab)
    {
        EnsureCacheExists();

        if (_cacheInstance.Entries.TryGetValue(classId, out var found))
        {
            prefab = found.Prefab;
            return prefab != null;
        }

        prefab = null;
        return false;
    }

    private static void EnsureCacheExists()
    {
        if (_cacheInstance != null)
            return;
        _cacheInstance = new GameObject("Nautilus.PrefabCache").AddComponent<ModPrefabCacheInstance>();
    }
}
internal class ModPrefabCacheInstance : MonoBehaviour
{
    public Dictionary<string, ModPrefabCache.Entry> Entries { get; } = new Dictionary<string, ModPrefabCache.Entry>();

    private Transform _prefabRoot;

    private void Awake()
    {
        _prefabRoot = new GameObject("PrefabRoot").transform;
        _prefabRoot.parent = transform;
        _prefabRoot.gameObject.SetActive(false);
    }
}