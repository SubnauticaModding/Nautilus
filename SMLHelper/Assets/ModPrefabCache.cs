namespace SMLHelper.Assets;

using SMLHelper.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class used by <see cref="PrefabInfo"/> to store game objects that used as prefabs.
/// Game objects in cache are inactive and will not be on scene.
/// </summary>
public static class ModPrefabCache
{
    //Stored prefabs and their destruction timers. Keyed by ClassID.
    internal readonly static Dictionary<string, Tuple<bool, GameObject>> CachedPrefabs = new();

    private static GameObject root; // active root object with CacheCleaner component
    private static GameObject prefabRoot; // inactive child object, parent for added prefabs

    private class CacheCleaner : MonoBehaviour
    {
        private float lastClean = 0f;

        public void Update()
        {
            lastClean += Time.deltaTime;

            if(lastClean >= 5)
            {
                foreach(var pair in CachedPrefabs)
                {
                    if(!pair.Value.Item1 || Builder.prefab == pair.Value.Item2)
                    {
                        continue;
                    }

                    InternalLogger.Debug($"ModPrefabCache: removing prefab {pair.Value.Item2}");
                    Destroy(pair.Value.Item2);
                    CachedPrefabs.Remove(pair.Key);
                    lastClean = 0f;
                    break;
                }
            }
        }
    }

    private static void Init()
    {
        if (root != null)
        {
            return;
        }

        root = new GameObject("SMLHelper.PrefabCache", typeof(SceneCleanerPreserve), typeof(CacheCleaner));
        UnityEngine.Object.DontDestroyOnLoad(root);
        root.EnsureComponent<SceneCleanerPreserve>();

        prefabRoot = new GameObject("PrefabRoot");
        prefabRoot.transform.parent = root.transform;
        prefabRoot.SetActive(false);
    }

    /// <summary> Add prefab to cache </summary>
    /// <param name="prefab"> Prefab to add. </param>
    /// <param name="autoremove">
    /// Is prefab needed to be removed from cache after use.
    /// Prefabs without autoremove flag can be safely deleted by <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)" />
    /// </param>
    public static void AddPrefab(GameObject prefab, bool autoremove = true)
    {
        Init();
        prefab.transform.parent = prefabRoot.transform;

        AddPrefabInternal(prefab, autoremove);
    }

    /// <summary> Add prefab copy to cache (instatiated copy will not run 'Awake') </summary>
    /// <param name="prefab"> Prefab to copy and add. </param>
    /// <param name="autoremove">
    /// Is prefab copy needed to be removed from cache after use.
    /// Prefabs without autoremove flag can be safely deleted by <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)" />
    /// </param>
    /// <returns> Prefab copy </returns>
    public static GameObject AddPrefabCopy(GameObject prefab, bool autoremove = true)
    {
        Init();
        GameObject prefabCopy = UnityEngine.Object.Instantiate(prefab, prefabRoot.transform);

        AddPrefabInternal(prefabCopy, autoremove);
        return prefabCopy;
    }

    private static void AddPrefabInternal(GameObject prefab, bool autoremove)
    {
        PrefabIdentifier identifier = prefab.GetComponent<PrefabIdentifier>();
        if(identifier == null)
        {
            InternalLogger.Warn($"ModPrefabCache: prefab is missing a PrefabIdentifier! Unable to add to cache.");
            return;
        }
        if(!CachedPrefabs.ContainsKey(identifier.classId))
        {
            CachedPrefabs.Add(identifier.classId ,Tuple.Create(autoremove, prefab));
            InternalLogger.Debug($"ModPrefabCache: adding prefab {prefab}");
        }
        else
        {
            InternalLogger.Warn($"ModPrefabCache: prefab {identifier.classId} already existed in cache!");
        }
    }
}