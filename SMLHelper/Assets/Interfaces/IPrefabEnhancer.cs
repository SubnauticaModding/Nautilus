namespace SMLHelper.Assets
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Allows you to do more to a prefab after all of SMLHelpers Post Processing is completed on it.
    /// </summary>
    public interface IPrefabEnhancer
    {
        /// <summary>
        /// Your method to modify the prefab after it has been put through all of SMLHelpers processing.
        /// </summary>
         public Action<GameObject> EnhancePrefab { get; }
    }
}