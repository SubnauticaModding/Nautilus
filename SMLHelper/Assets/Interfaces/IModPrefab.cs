namespace SMLHelper.Assets.Interfaces
{
    using System.Collections;
    using UnityEngine;

    public interface IModPrefab
    {
        /// <summary>
        /// The PrefabInfo associated with this ModPrefab.
        /// </summary>
        public PrefabInfo PrefabInfo { get; }

        /// <summary>
        /// Gets the prefab game object asynchronously. Set up your prefab components here.
        /// The <see cref="TechType"/> and ClassID are already handled.
        /// </summary>
        /// <param name="gameObject"> The game object to be instantiated into a new in-game entity. </param>
        IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject);
    }
}
