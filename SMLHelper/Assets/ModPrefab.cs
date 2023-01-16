namespace SMLHelper.Assets
{
    using System.Reflection;
    using System.Collections;
    using UnityEngine;
    using SMLHelper.Utility;
    using System;
    using SMLHelper.Assets.Interfaces;

    /// <summary>
    /// The class to inherit when you want to add new PreFabs into the game.
    /// </summary>
    public abstract class ModPrefab: IModPrefab
    {
        /// <summary>
        /// The Assembly of the mod that added this prefab.
        /// </summary>
        public Assembly Mod { get; protected set; }

        /// <summary>
        /// The class identifier used for the <see cref="PrefabIdentifier" /> component whenever applicable.
        /// </summary>
        public string ClassID { get; set; }
        /// <summary>
        /// Name of the prefab file.
        /// </summary>
        public string PrefabFileName { get; set; }

        /// <summary>
        /// The <see cref="TechType"/> of the corresponding item.
        /// Used for <see cref="TechTag" />, and <see cref="Constructable" /> components whenever applicable.
        /// </summary>
        public TechType TechType { get; set; }

        /// <summary>
        /// The PrefabInfo associated with this ModPrefab.
        /// </summary>
        public PrefabInfo PrefabInfo { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModPrefab" /> class.
        /// </summary>
        /// <param name="classID">The class identifier used for the <see cref="PrefabIdentifier" /> component whenever applicable.</param>
        /// <param name="prefabFileName">Name of the prefab file.</param>
        /// <param name="techType">The techtype of the corresponding item. 
        /// Used for the <see cref="TechTag" /> and <see cref="Constructable" /> components whenever applicable.
        /// Can also be set later in the constructor if it is not yet provided.</param>
        protected ModPrefab(string classID, string prefabFileName, TechType techType = TechType.None)
        {
            if(string.IsNullOrWhiteSpace(classID))
                throw new ArgumentNullException("classID cannot be null or empty spaces.");

            this.ClassID = classID;
            this.PrefabFileName = prefabFileName;
            this.TechType = techType;
            this.Mod = ReflectionHelper.CallingAssemblyByStackTrace();
            this.PrefabInfo = new PrefabInfo(this);
        }

        #region GameObjectProcessing

        /// <summary>
        /// Caches the prefab, then sets its TechType and ClassID to a default set of values applicable to most mods.<br/>
        /// FOR ADVANCED MODDING ONLY. Do not override unless you know exactly what you are doing.
        /// </summary>
        /// <param name="go"></param>
        public virtual void ProcessPrefab(GameObject go)
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
