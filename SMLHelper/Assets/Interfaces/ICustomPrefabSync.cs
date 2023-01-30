﻿namespace SMLHelper.Assets.Interfaces
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Only use this if you can setup your GameObject without needing to use any of the games Async Coroutines.
    /// </summary>
#pragma warning disable 0618
    public interface ICustomPrefabSync: IModPrefab
#pragma warning restore 0618
    {
        /// <summary>
        /// Gets the prefab game object. Set up your prefab components here.
        /// The <see cref="TechType"/> and ClassID are already handled.
        /// </summary>
        /// <returns>The game object to be instantiated into a new in-game entity.</returns>
        public Func<GameObject> GetGameObject { get; }
    }
}