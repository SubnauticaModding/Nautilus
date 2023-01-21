namespace SMLHelper.Assets.Interfaces
{
    using System;
    using System.Collections;
    using UnityEngine;

#pragma warning disable 0618
    public interface ICustomPrefabAsync: IModPrefab
#pragma warning restore 0618
    {
        /// <summary>
        /// The Coroutine Method used to generate the GameObject when the game requests this item.
        /// </summary>
        public Func<IOut<GameObject>, IEnumerator> GetGameObjectAsync { get; }

    }
}
