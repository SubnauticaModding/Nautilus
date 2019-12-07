namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;
    using System.Collections.Generic;
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
        /// </summary>
        /// <param name="data"></param>
        /// <param name="classId"></param>
        public static void AddCustomLootDistData(LootDistributionData.SrcData data, string classId)
        {
            Main.AddCustomLootDistData(data, classId);
        }

        /// <summary>
        /// Edits Loot Distribution data for existing prefabs, for e.g. original game prefabs.
        /// </summary>
        /// <param name="classID">The ClassID of the prefab. If unsure, use CraftData.GetClassIdForTechType.</param>
        /// <param name="biome">The biome, in which this prefab is spawning, or being set to spawn in.</param>
        /// <param name="probability">The probability for which this prefab should spawn for. Between 0 and 1.</param>
        /// <param name="count">The count of the prefab. Usually just 1.</param>
        public static void EditLootBiomeData(string classID, BiomeType biome, float probability, int count)
        {
            Main.EditLootBiomeData(classID, biome, probability, count);
        }

        void ILootDistributionHandler.AddCustomLootDistData(LootDistributionData.SrcData data, string classId)
        {
            LootDistributionPatcher.CustomSrcData.Add(classId, data);
        }

        void ILootDistributionHandler.EditLootBiomeData(string classid, BiomeType biome, float probability, int count)
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
