using System.Collections.Generic;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Patchers;
using UnityEngine;

namespace SMLHelper.V2.Handlers
{
    /// <summary>
    /// a Handler that handles and registers Coordinated (<see cref="Vector3"/> spawns).
    /// </summary>
    public class CoordinatedSpawnsHandler : ICoordinatedSpawnHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static ICoordinatedSpawnHandler Main { get; } = new CoordinatedSpawnsHandler();
        
        private CoordinatedSpawnsHandler()
        {
            // Hide Constructor
        }

        #region Interface Implementations
        /// <summary>
        /// Registers Multiple Coordinated spawns for one single passed TechType
        /// </summary>
        /// <param name="techTypeToSpawn">The TechType to spawn</param>
        /// <param name="coordinatesToSpawnTo">the coordinates the <see cref="TechType"/> should spawn to</param>
        void ICoordinatedSpawnHandler.RegisterCoordinatedSpawnsForOneTechType(TechType techTypeToSpawn, List<Vector3> coordinatesToSpawnTo)
        {
            var spawnInfos = new List<SpawnInfo>();
            foreach (var coordinate in coordinatesToSpawnTo)
            {
                spawnInfos.Add(new SpawnInfo(techTypeToSpawn, coordinate));
            }
            LargeWorldStreamerPatcher.spawnInfos.AddRange(spawnInfos);
        }

        /// <summary>
        /// Registers a Coordinated Spawn
        /// </summary>
        /// <param name="spawnInfo">the SpawnInfo to spawn</param>
        void ICoordinatedSpawnHandler.RegisterCoordinatedSpawn(SpawnInfo spawnInfo)
        {
            LargeWorldStreamerPatcher.spawnInfos.Add(spawnInfo);
        }

        /// <summary>
        /// registers Many Coordinated Spawns.
        /// </summary>
        /// <param name="spawnInfos">The SpawnInfos to spawn.</param>
        void ICoordinatedSpawnHandler.RegisterCoordinatedSpawns(List<SpawnInfo> spawnInfos)
        {
            LargeWorldStreamerPatcher.spawnInfos.AddRange(spawnInfos);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Registers a Coordinated Spawn
        /// </summary>
        /// <param name="spawnInfo">the SpawnInfo to spawn</param>
        public static void RegisterCoordinatedSpawn(SpawnInfo spawnInfo)
        {
            Main.RegisterCoordinatedSpawn(spawnInfo);
        }

        /// <summary>
        /// registers Many Coordinated Spawns.
        /// </summary>
        /// <param name="spawnInfos">The SpawnInfo to spawn.</param>
        public static void RegisterCoordinatedSpawns(List<SpawnInfo> spawnInfos)
        {
            Main.RegisterCoordinatedSpawns(spawnInfos);
        }

        /// <summary>
        /// Registers Multiple Coordinated spawns for one single passed TechType
        /// </summary>
        /// <param name="techTypeToSpawn">The TechType to spawn</param>
        /// <param name="coordinatesToSpawnTo">the coordinates the <see cref="TechType"/> should spawn to</param>
        public static void RegisterCoordinatedSpawnsForOneTechType(TechType techTypeToSpawn, List<Vector3> coordinatesToSpawnTo)
        {
            Main.RegisterCoordinatedSpawnsForOneTechType(techTypeToSpawn, coordinatesToSpawnTo);
        }

        #endregion
    }
    
    #region SpawnInfo
    /// <summary>
    /// a basic class that provides enough info for the <see cref="CoordinatedSpawnsHandler"/> System to function.
    /// </summary>
    public class SpawnInfo
    {
        internal readonly TechType techType;
        internal readonly string classId;
        internal readonly Vector3 spawnPosition;
        internal readonly Quaternion rotation;

        internal SpawnType spawnType;

        /// <summary>
        /// Initializes a new <see cref="SpawnInfo"/>.
        /// </summary>
        /// <param name="techType">TechType to spawn.</param>
        /// <param name="spawnPosition">Position to spawn into.</param>
        public SpawnInfo(TechType techType, Vector3 spawnPosition)
        {
            this.techType = techType;
            this.spawnPosition = spawnPosition;
            this.rotation = Quaternion.identity;
            spawnType = SpawnType.TechType;
        }

        /// <summary>
        /// Initializes a new <see cref="SpawnInfo"/>.
        /// </summary>
        /// <param name="classId">ClassID to spawn.</param>
        /// <param name="spawnPosition">Position to spawn into.</param>
        public SpawnInfo(string classId, Vector3 spawnPosition)
        {
            this.classId = classId;
            this.spawnPosition = spawnPosition;
            this.rotation = Quaternion.identity;
            spawnType = SpawnType.ClassId;
        }

        /// <summary>
        /// Initializes a new <see cref="SpawnInfo"/>.
        /// </summary>
        /// <param name="techType">TechType to spawn.</param>
        /// <param name="spawnPosition">Position to spawn into.</param>
        /// <param name="rotation">Rotation to spawn at.</param>
        public SpawnInfo(TechType techType, Vector3 spawnPosition, Quaternion rotation)
        {
            this.techType = techType;
            this.spawnPosition = spawnPosition;
            this.rotation = rotation;
            spawnType = SpawnType.TechType;
        }

        /// <summary>
        /// Initializes a new <see cref="SpawnInfo"/>.
        /// </summary>
        /// <param name="classId">ClassID to spawn.</param>
        /// <param name="spawnPosition">Position to spawn into.</param>
        /// <param name="rotation">Rotation to spawn at.</param>
        public SpawnInfo(string classId, Vector3 spawnPosition, Quaternion rotation)
        {
            this.classId = classId;
            this.spawnPosition = spawnPosition;
            this.rotation = rotation;
            spawnType = SpawnType.ClassId;
        }

        /// <summary>
        /// Initializes a new <see cref="SpawnInfo"/>.
        /// </summary>
        /// <param name="techType">TechType to spawn.</param>
        /// <param name="spawnPosition">Position to spawn into.</param>
        /// <param name="rotation">Rotation to spawn at.</param>
        public SpawnInfo(TechType techType, Vector3 spawnPosition, Vector3 rotation)
        {
            this.techType = techType;
            this.spawnPosition = spawnPosition;
            this.rotation = Quaternion.Euler(rotation);
            spawnType = SpawnType.TechType;
        }

        /// <summary>
        /// Initializes a new <see cref="SpawnInfo"/>.
        /// </summary>
        /// <param name="classId">ClassID to spawn.</param>
        /// <param name="spawnPosition">Position to spawn into.</param>
        /// <param name="rotation">Rotation to spawn at.</param>
        public SpawnInfo(string classId, Vector3 spawnPosition, Vector3 rotation)
        {
            this.classId = classId;
            this.spawnPosition = spawnPosition;
            this.rotation = Quaternion.Euler(rotation);
            spawnType = SpawnType.ClassId;
        }

        internal enum SpawnType
        {
            ClassId, 
            TechType
        }
    }
    #endregion
}