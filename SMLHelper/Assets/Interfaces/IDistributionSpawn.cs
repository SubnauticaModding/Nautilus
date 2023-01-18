namespace SMLHelper.Assets.Interfaces
{
    using System.Collections.Generic;
    using UWE;

    public interface IDistributionSpawn
    {
        /// <summary>
        /// Returns the List of BiomeData that handles what Biomes this prefab will spawn, how probable it is to spawn there and how many per spawn.
        /// </summary>
        public List<LootDistributionData.BiomeData> BiomesToSpawnIn { get; }

        /// <summary>
        /// Returns <see cref="WorldEntityInfo"/> of this object to use to register it for spawning.
        /// </summary>
        public WorldEntityInfo EntityInfo { get; }
    }
}
