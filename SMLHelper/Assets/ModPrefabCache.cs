namespace SMLHelper.Assets
{
    using SMLHelper.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;


    /// <summary>
    /// Class that used by <see cref="PrefabInfo"/> to store game objects that used as prefabs.
    /// Also it can be used by mods directly, e.g. in <see cref="PrefabInfo.GetGameObjectAsync"/> to store prefab before returning.
    /// Game objects in cache are inactive and will not be on scene.
    /// </summary>
    public static class ModPrefabCache
    {
        private static readonly Dictionary<string, PrefabInfo> FileNameDictionary = new(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<string, PrefabInfo> ClassIdDictionary = new(StringComparer.InvariantCultureIgnoreCase);
        private static readonly List<PrefabInfo> PreFabsList = new();
        internal static bool ModPrefabsPatched = false;

        internal static void Add(PrefabInfo prefabInfo)
        {
            FileNameDictionary.Add(prefabInfo.PrefabFileName, prefabInfo);
            ClassIdDictionary.Add(prefabInfo.ClassID, prefabInfo);
            PreFabsList.Add(prefabInfo);
            ModPrefabsPatched = false;
        }

        internal static IEnumerable<PrefabInfo> Prefabs => PreFabsList;
        internal static bool TryGetFromFileName(string classId, out PrefabInfo prefab)
        {
            if(string.IsNullOrEmpty(classId))
            {
                prefab = null;
                return false;
            }
            return FileNameDictionary.TryGetValue(classId, out prefab);
        }

        internal static bool TryGetFromClassId(string classId, out PrefabInfo prefab)
        {
            if(string.IsNullOrEmpty(classId))
            {
                prefab = null;
                return false;
            }
            return ClassIdDictionary.TryGetValue(classId, out prefab);
        }

        private const float cleanDelay = 30.0f; // delay in secs before attempt to remove prefab from cache

        // list of prefabs for removing (Item1 - time of addition, Item2 - prefab gameobject)
        private readonly static List<Tuple<float, GameObject>> prefabs = new();

        private static GameObject root; // active root object with CacheCleaner component
        private static GameObject prefabRoot; // inactive child object, parent for added prefabs

        private class CacheCleaner : MonoBehaviour
        {
            public void Update()
            {
                for (int i = prefabs.Count - 1; i >= 0; i--)
                {
                    if (Time.time < prefabs[i].Item1 + cleanDelay || Builder.prefab == prefabs[i].Item2)
                    {
                        continue;
                    }

                    InternalLogger.Debug($"ModPrefabCache: removing prefab {prefabs[i].Item2}");

                    Destroy(prefabs[i].Item2);
                    prefabs.RemoveAt(i);
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
            if (autoremove)
            {
                prefabs.Add(Tuple.Create(Time.time, prefab));
            }

            InternalLogger.Debug($"ModPrefabCache: adding prefab {prefab}");
        }
    }
}
