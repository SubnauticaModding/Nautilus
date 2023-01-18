namespace SMLHelper.Assets.Interfaces
{
    using System;
    using UnityEngine;

    public interface IProcessPrefabOverride
    {
        /// <summary>
        /// Caches the prefab, then sets its TechType and ClassID to a default set of values applicable to most mods.<br/>
        /// FOR ADVANCED MODDING ONLY. Do not override unless you know exactly what you are doing.
        /// </summary>
        public Action<GameObject> ProcessPrefab { get; }
    }
}
