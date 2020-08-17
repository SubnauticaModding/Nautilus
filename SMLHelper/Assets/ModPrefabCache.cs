namespace SMLHelper.V2.Assets
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Logger = V2.Logger;

    /// <summary> TODO </summary>
    public static class ModPrefabCache
    {
        private const float cleanDelay = 1.0f; // delay in secs before attempt to remove prefab from cache

        // list of prefabs for removing (Item1 - time of addition, Item2 - prefab gameobject)
        private readonly static List<Tuple<float, GameObject>> prefabs = new List<Tuple<float, GameObject>>();

        private static GameObject root; // active root object with CacheCleaner component
        private static GameObject prefabRoot; // inactive child object, parent for added prefabs

        private class CacheCleaner : MonoBehaviour
        {
            public void Update()
            {
                for (int i = prefabs.Count - 1; i >= 0; i--)
                {
                    if (Time.time < prefabs[i].Item1 + cleanDelay || Builder.prefab == prefabs[i].Item2)
                        continue;

                    Logger.Debug($"ModPrefabCache: removing prefab {prefabs[i].Item2}");

                    Destroy(prefabs[i].Item2);
                    prefabs.RemoveAt(i);
                }
            }
        }

        private static void Init()
        {
            if (root)
                return;

            root = new GameObject("SMLHelper.PrefabCache", typeof(SceneCleanerPreserve), typeof(CacheCleaner));
            UnityEngine.Object.DontDestroyOnLoad(root);

            prefabRoot = new GameObject("PrefabRoot");
            prefabRoot.transform.parent = root.transform;
            prefabRoot.SetActive(false);
        }

        /// <summary> TODO </summary>
        public static void AddPrefab(GameObject prefab, bool autoremove = true)
        {
            Init();

            prefab.transform.parent = prefabRoot.transform;

            if (autoremove)
                prefabs.Add(Tuple.Create(Time.time, prefab));

            Logger.Debug($"ModPrefabCache: adding prefab {prefab}");
        }
    }
}
