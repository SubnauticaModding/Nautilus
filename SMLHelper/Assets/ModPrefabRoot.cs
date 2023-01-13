namespace SMLHelper.Assets
{
    using System.Collections;
    using System.Reflection;
    using UnityEngine;


    /// <summary>
    /// The root class to hold the most basic mod prefab systems.
    /// </summary>
    public abstract class ModPrefabRoot
    {
        /// <summary>
        /// The Assembly of the mod that added this prefab.
        /// </summary>
        public Assembly Mod { get; protected set; }

        public abstract PrefabInfo PrefabInfo { get; protected set; }

        #region GameObjectProcessing

        internal GameObject GetGameObjectInternal()
        {
            GameObject go = GetGameObject();
            if(go == null)
            {
                return null;
            }

            ProcessPrefab(go);
            return go;
        }

        internal IEnumerator GetGameObjectInternalAsync(IOut<GameObject> gameObject)
        {
            TaskResult<GameObject> taskResult = new();
            yield return GetGameObjectAsync(taskResult);

            GameObject go = taskResult.Get();
            if(go == null)
            {
                yield break;
            }

            ProcessPrefab(go);
            gameObject.Set(go);
        }

        /// <summary>
        /// Caches the prefab, then sets its TechType and ClassID to a default set of values applicable to most mods.<br/>
        /// FOR ADVANCED MODDING ONLY. Do not override unless you know exactly what you are doing.
        /// </summary>
        /// <param name="go"></param>
        protected virtual void ProcessPrefab(GameObject go)
        {
            if(go.activeInHierarchy) // inactive prefabs don't need to be removed by cache
            {
                ModPrefabCache.AddPrefab(go);
            }

            go.name = this.PrefabInfo.ClassID;
            var tech = PrefabInfo.TechType;

            if(tech != TechType.None)
            {
                if(go.GetComponent<TechTag>() is { } tag)
                {
                    tag.type = tech;
                }

                if(go.GetComponent<Constructable>() is { } cs)
                {
                    cs.techType = tech;
                }
            }

            if(go.GetComponent<PrefabIdentifier>() is { } pid)
            {
                pid.ClassId = PrefabInfo.ClassID;
            }
        }


        /// <summary>
        /// Gets the prefab game object. Set up your prefab components here.
        /// The <see cref="TechType"/> and ClassID are already handled.
        /// </summary>
        /// <returns>The game object to be instantiated into a new in-game entity.</returns>
        public virtual GameObject GetGameObject()
        {
            return null;
        }

        /// <summary>
        /// Gets the prefab game object asynchronously. Set up your prefab components here.
        /// The <see cref="TechType"/> and ClassID are already handled.
        /// </summary>
        /// <param name="gameObject"> The game object to be instantiated into a new in-game entity. </param>
        public virtual IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            return null;
        }

        #endregion

    }
}
