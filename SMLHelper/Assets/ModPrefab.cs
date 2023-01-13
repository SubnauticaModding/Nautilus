namespace SMLHelper.Assets
{
    using System.Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using SMLHelper.Utility;

    /// <summary>
    /// The abstract class to inherit when you want to add new PreFabs into the game.
    /// </summary>
    public abstract class ModPrefab: ModPrefabRoot
    {
        /// <summary>
        /// The class identifier used for the <see cref="PrefabIdentifier" /> component whenever applicable.
        /// </summary>
        public string ClassID { get; protected set; }

        /// <summary>
        /// Name of the prefab file.
        /// </summary>
        public string PrefabFileName { get; protected set; }

        /// <summary>
        /// The <see cref="TechType"/> of the corresponding item.
        /// Used for <see cref="TechTag" />, and <see cref="Constructable" /> components whenever applicable.
        /// </summary>
        public TechType TechType { get; protected set; }

        public override PrefabInfo PrefabInfo { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModPrefab" /> class.
        /// </summary>
        /// <param name="classId">The class identifier used for the <see cref="PrefabIdentifier" /> component whenever applicable.</param>
        /// <param name="prefabFileName">Name of the prefab file.</param>
        /// <param name="techType">The techtype of the corresponding item. 
        /// Used for the <see cref="TechTag" /> and <see cref="Constructable" /> components whenever applicable.
        /// Can also be set later in the constructor if it is not yet provided.</param>
        protected ModPrefab(string classId, string prefabFileName, TechType techType = TechType.None)
        {
            this.ClassID = classId;
            this.PrefabFileName = prefabFileName;
            this.TechType = techType;
            this.PrefabInfo = new PrefabInfo() { ClassID = classId, PrefabPath = prefabFileName, TechType = techType };

            Mod = ReflectionHelper.CallingAssemblyByStackTrace();
        }
    }
}
