using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace SMLHelper.V2.Interfaces
{
    /// <summary>
    /// a Handler interface that handles and registers Coordinated (<see cref="Vector3"/> spawns).
    /// </summary>
    public interface ICoordinatedSpawnHandler
    {
        /// <summary>
        /// Registers a Coordinated Spawn
        /// </summary>
        /// <param name="spawnInfo">the SpawnInfo to spawn</param>
        void RegisterCoordinatedSpawn(SpawnInfo spawnInfo);
        
        /// <summary>
        /// registers Many Coordinated Spawns.
        /// </summary>
        /// <param name="spawnInfos">The SpawnInfos to spawn.</param>
        void RegisterCoordinatedSpawns(List<SpawnInfo> spawnInfos);
        
        /// <summary>
        /// Registers Multiple Coordinated spawns for one single passed TechType
        /// </summary>
        /// <param name="techTypeToSpawn">The TechType to spawn</param>
        /// <param name="coordinatesToSpawnTo">the coordinates the <see cref="TechType"/> should spawn to</param>
        void RegisterCoordinatedSpawnsForOneTechType(TechType techTypeToSpawn, List<Vector3> coordinatesToSpawnTo);
		
        /// <summary>
        /// Registers Multiple Coordinated spawns with rotations for one single passed TechType
        /// </summary>
        /// <param name="techTypeToSpawn">The TechType to spawn</param>
        /// <param name="coordinatesToSpawnTo">the coordinates(Key) and the rotations(Value) the <see cref="TechType"/> should spawn to</param>
        void RegisterCoordinatedSpawnsForOneTechType(TechType techTypeToSpawn, Dictionary<Vector3, Vector3> coordinatesAndRotationsToSpawnTo);
    }
}