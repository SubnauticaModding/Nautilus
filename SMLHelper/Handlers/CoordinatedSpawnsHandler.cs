using System;
using System.Collections.Generic;
using UnityEngine;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;

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

        /// <summary>
        /// Registers Multiple Coordinated spawns with rotations for one single passed TechType
        /// </summary>
        /// <param name="techTypeToSpawn">The TechType to spawn</param>
        /// <param name="coordinatesAndRotationsToSpawnTo">the coordinates(Key) and the rotations(Value) the <see cref="TechType"/> should spawn to</param>
        void ICoordinatedSpawnHandler.RegisterCoordinatedSpawnsForOneTechType(TechType techTypeToSpawn, Dictionary<Vector3, Vector3> coordinatesAndRotationsToSpawnTo)
        {
            var spawnInfos = new List<SpawnInfo>();
            foreach (var kvp in coordinatesAndRotationsToSpawnTo)
            {
                spawnInfos.Add(new SpawnInfo(techTypeToSpawn, kvp.Key, kvp.Value));
            }
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

        /// <summary>
        /// Registers Multiple Coordinated spawns with rotations for one single passed TechType
        /// </summary>
        /// <param name="techTypeToSpawn">The TechType to spawn</param>
        /// <param name="coordinatesAndRotationsToSpawnTo">the coordinates(Key) and the rotations(Value) the <see cref="TechType"/> should spawn to</param>
        public static void RegisterCoordinatedSpawnsForOneTechType(TechType techTypeToSpawn, Dictionary<Vector3, Vector3> coordinatesAndRotationsToSpawnTo)
        {
            Main.RegisterCoordinatedSpawnsForOneTechType(techTypeToSpawn, coordinatesAndRotationsToSpawnTo);
        }

        #endregion
    }

    #region SpawnInfo
    /// <summary>
    /// a basic class that provides enough info for the <see cref="CoordinatedSpawnsHandler"/> System to function.
    /// </summary>
    public class SpawnInfo : IEquatable<SpawnInfo>
    {
        [JsonProperty]
        internal TechType techType { get; }
        [JsonProperty]
        internal string classId { get; }
        [JsonProperty]
        internal Vector3 spawnPosition { get; }
        [JsonProperty]
        internal Quaternion rotation { get; }
        [JsonProperty]
        internal SpawnType spawnType { get; }

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

        [JsonConstructor]
        internal SpawnInfo(string classId, TechType techType, Vector3 spawnPosition, Quaternion rotation)
        {
            this.classId = classId;
            this.techType = techType;
            this.spawnPosition = spawnPosition;
            this.rotation = rotation;
            spawnType = this.techType == TechType.None ? SpawnType.ClassId : SpawnType.TechType;
        }

        /// <summary>
        /// Indicates whether the current <see cref="SpawnInfo"/> is equal to another.
        /// </summary>
        /// <param name="other">The other <see cref="SpawnInfo"/>.</param>
        /// <returns><see langword="true"/> if the current <see cref="SpawnInfo"/> is equal to the <paramref name="other"/> parameter;
        /// otherwise <see langword="false"/>.</returns>
        public bool Equals(SpawnInfo other)
        {
            return other.techType == techType
                && other.classId == classId
                && other.spawnPosition == spawnPosition
                && other.rotation == rotation
                && other.spawnType == spawnType;
        }

        internal enum SpawnType
        {
            ClassId,
            TechType
        }
    }
    #endregion
}
