namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;
    using SMLHelper.V2.Assets;
    using System.Collections.Generic;
    using System.Linq;
    using UWE;

    /// <summary>
    /// A handler that manages Loot Distribution.
    /// </summary>
    public class LootDistributionHandler : ILootDistributionHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static ILootDistributionHandler Main { get; } = new LootDistributionHandler();

        private LootDistributionHandler() { } // Hides constructor

        /// <summary>
        /// Adds in a custom entry into the Loot Distribution of the game.
        /// You must also add the <see cref="WorldEntityInfo"/> into the <see cref="WorldEntityDatabase"/> using <see cref="WorldEntityDatabaseHandler"/>.
        /// </summary>
        /// <param name="data">The <see cref="LootDistributionData.SrcData"/> that contains data related to the spawning of a prefab, also contains the path to the prefab.</param>
        /// <param name="classId">The classId of the prefab.</param>
        /// <param name="info">The WorldEntityInfo of the prefab. For more information on how to set this up, see <see cref="WorldEntityDatabaseHandler"/>.</param>
        public static void AddLootDistributionData(string classId, LootDistributionData.SrcData data, WorldEntityInfo info = null)
        {
            Main.AddLootDistributionData(data, classId);

            if (info != null)
                WorldEntityDatabaseHandler.AddCustomInfo(classId, info);
        }

        /// <summary>
        /// Adds in a custom entry into the Loot Distribution of the game.
        /// You must also add the <see cref="WorldEntityInfo"/> into the <see cref="WorldEntityDatabase"/> using <see cref="WorldEntityDatabaseHandler"/>.
        /// </summary>
        /// <param name="classId">The classId of the prefab.</param>
        /// <param name="prefabPath">The prefab path of the prefab.</param>
        /// <param name="biomeDistribution">The <see cref="LootDistributionData.BiomeData"/> dictating how the prefab should spawn in the world.</param>
        /// <param name="info">The WorldEntityInfo of the prefab. For more information on how to set this up, see <see cref="WorldEntityDatabaseHandler"/>.</param>
        public static void AddLootDistributionData(string classId, string prefabPath, IEnumerable<LootDistributionData.BiomeData> biomeDistribution, WorldEntityInfo info = null)
        {
            Main.AddLootDistributionData(new LootDistributionData.SrcData()
            {
                distribution = biomeDistribution.ToList(),
                prefabPath = prefabPath
            }, classId);

            if (info != null)
                WorldEntityDatabaseHandler.AddCustomInfo(classId, info);
        }

        /// <summary>
        /// Adds in a custom entry into the Loot Distribution of the game.
        /// You must also add the <see cref="WorldEntityInfo"/> into the <see cref="WorldEntityDatabase"/> using <see cref="WorldEntityDatabaseHandler"/>.
        /// </summary>
        /// <param name="prefab">The custom prefab which you want to spawn naturally in the game.</param>
        /// <param name="biomeDistribution">The <see cref="LootDistributionData.BiomeData"/> dictating how the prefab should spawn in the world.</param>
        /// <param name="info">The WorldEntityInfo of the prefab. For more information on how to set this up, see <see cref="WorldEntityDatabaseHandler"/>.</param>
        public static void AddLootDistributionData(ModPrefab prefab, IEnumerable<LootDistributionData.BiomeData> biomeDistribution, WorldEntityInfo info)
        {
            AddLootDistributionData(prefab.ClassID, prefab.PrefabFileName, biomeDistribution);

            if (info != null)
                WorldEntityDatabaseHandler.AddCustomInfo(prefab.ClassID, info);
        }

        /// <summary>
        /// Edits Loot Distribution data for existing prefabs, for e.g. original game prefabs.
        /// </summary>
        /// <param name="classID">The ClassID of the prefab. If unsure, use CraftData.GetClassIdForTechType.</param>
        /// <param name="biome">The biome, in which this prefab is spawning, or being set to spawn in.</param>
        /// <param name="probability">The probability for which this prefab should spawn for.</param>
        /// <param name="count">The count of the prefab. Usually just 1.</param>
        public static void EditLootDistributionData(string classID, BiomeType biome, float probability, int count)
        {
            Main.EditLootDistributionData(classID, biome, probability, count);
        }

        /// <summary>
        /// Edits Loot Distribution data for existing prefabs, for e.g. original game prefabs.
        /// </summary>
        /// <param name="classID">The ClassID of the prefab. If unsure, use CraftData.GetClassIdForTechType.</param>
        /// <param name="biomeDistribution">The list of <see cref="LootDistributionData.BiomeData"/> that contains information about how/when it should spawn in biomes.</param>
        public static void EditLootDistributionData(string classID, IEnumerable<LootDistributionData.BiomeData> biomeDistribution)
        {
            foreach(var distribution in biomeDistribution)
            {
                Main.EditLootDistributionData(classID, distribution.biome, distribution.probability, distribution.count);
            }
        }

        void ILootDistributionHandler.AddLootDistributionData(LootDistributionData.SrcData data, string classId)
        {
            LootDistributionPatcher.CustomSrcData.Add(classId, data);
        }

        void ILootDistributionHandler.EditLootDistributionData(string classid, BiomeType biome, float probability, int count)
        {
            LootDistributionData.SrcData srcData;
            if(!LootDistributionPatcher.CustomSrcData.TryGetValue(classid, out srcData))
            {
                LootDistributionPatcher.CustomSrcData[classid] = (srcData = new LootDistributionData.SrcData());

                List<LootDistributionData.BiomeData> biomeDistribution = new List<LootDistributionData.BiomeData>();
                biomeDistribution.Add(new LootDistributionData.BiomeData()
                {
                    biome = biome,
                    probability = probability,
                    count = count
                });

                srcData.distribution = biomeDistribution;

                return;
            }

            for(int i = 0; i < srcData.distribution.Count; i++)
            {
                LootDistributionData.BiomeData distribution = srcData.distribution[i];

                if(distribution.biome == biome)
                {
                    distribution.count = count;
                    distribution.probability = probability;

                    return;
                }
            }

            // If we reached this point, that means the srcData is present, but the biome in the distribution is not.
            // Lets add it manually.
            srcData.distribution.Add(new LootDistributionData.BiomeData()
            {
                biome = biome,
                probability = probability,
                count = count
            });
        }
    }
}
