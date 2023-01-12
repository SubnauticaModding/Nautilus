namespace SMLHelper.Assets
{
    using System.Collections.Generic;
    using Handlers;
    using UnityEngine;
    using UWE;

    /// <summary>
    /// Extensions for the ModPrefab class to set things up without having to use Inheritance based prefabs.
    /// </summary>
    public static partial class BuilderExtensions
    {
        /// <summary>
        /// Registers the <see cref="WorldEntityInfo"/> for this <see cref="ModPrefab"/> and optionally adds random spawn locations using a list of <see cref="LootDistributionData.BiomeData"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="entityInfo">The <see cref="WorldEntityInfo"/> you want to register to this ModPrefab</param>
        /// <param name="biomeDistribution">Optional list of <see cref="LootDistributionData.BiomeData"/> to register random spawn locations for this prefab.</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetWorldEntityInfo(this ModPrefabBuilder modPrefabBuilder, WorldEntityInfo entityInfo, IEnumerable<LootDistributionData.BiomeData> biomeDistribution = null)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(biomeDistribution != null)
            {
                LootDistributionHandler.AddLootDistributionData(modPrefab, biomeDistribution, entityInfo);
            }
            else if(entityInfo != null)
            {
                WorldEntityDatabaseHandler.AddCustomInfo(modPrefab.ClassID, entityInfo);
            }

            return modPrefabBuilder;
        }

        /// <summary>
        /// Registers a Coordinate based Spawns for this <see cref="ModPrefab"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="spawnLocations">List of <see cref="SpawnLocation"/> to register spawn locations for this prefab.</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetStaticSpawnLocations(this ModPrefabBuilder modPrefabBuilder, IEnumerable<SpawnLocation> spawnLocations)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(spawnLocations != null)
            {
                foreach((Vector3 position, Vector3 eulerAngles) in spawnLocations)
                {
                    CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(modPrefab.ClassID, position, eulerAngles));
                }
            }
            return modPrefabBuilder;
        }
    }
}
