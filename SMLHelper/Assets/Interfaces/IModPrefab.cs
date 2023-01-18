namespace SMLHelper.Assets.Interfaces
{
    using System;
    using System.Collections;
    using UnityEngine;

    public interface IModPrefab
    {
        /// <summary>
        /// The Coroutine Method used to generate the GameObject when the game requests this item.
        /// </summary>
        public Func<IOut<GameObject>, IEnumerator> GetGameObjectAsync { get; }

    }
}
