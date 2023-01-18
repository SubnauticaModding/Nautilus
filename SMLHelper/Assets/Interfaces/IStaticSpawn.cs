namespace SMLHelper.Assets.Interfaces
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A lightweight class used to specify the position of a Coordinated Spawn and optionally set its rotation.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="eulerAngles"></param>
    public record SpawnLocation(Vector3 position, Vector3 eulerAngles = default);

    public interface IStaticSpawn
    {
        /// <summary>
        /// Returns the list of <see cref="SpawnLocation"/>s that specify the prefab's Coordinated Spawns.<br/>
        /// By default this will be null.
        /// </summary>
        public List<SpawnLocation> CoordinatedSpawns { get; }

    }
}
