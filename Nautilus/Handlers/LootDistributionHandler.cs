using System.Collections.Generic;
using BepInEx.Logging;
using Nautilus.Assets;
using Nautilus.Patchers;
using Nautilus.Utility;
using UWE;

namespace Nautilus.Handlers;

/// <summary>
/// A handler that manages the distribution of spawned resources throughout the world. Used for fish, items, outcrops, fragments, eggs, etc...
/// </summary>
public static class LootDistributionHandler
{
    /// <summary>
    /// Adds in a custom entry into the Loot Distribution of the game.
    /// You must also add the <see cref="WorldEntityInfo"/> into the <see cref="WorldEntityDatabase"/> using <see cref="WorldEntityDatabaseHandler"/>.
    /// </summary>
    /// <param name="data">The <see cref="LootDistributionData.SrcData"/> that contains data related to the spawning of a prefab, also contains the path to the prefab.</param>
    /// <param name="classId">The classId of the prefab.</param>
    /// <param name="info">The WorldEntityInfo of the prefab. For more information on how to set this up, see <see cref="WorldEntityDatabaseHandler"/>.</param>
    public static void AddLootDistributionData(string classId, LootDistributionData.SrcData data, WorldEntityInfo info)
    {
        AddLootDistributionData(classId, data);

        WorldEntityDatabaseHandler.AddCustomInfo(classId, info);
    }

    /// <summary>
    /// Adds in a custom entry into the Loot Distribution of the game.
    /// You must also add the <see cref="WorldEntityInfo"/> into the <see cref="WorldEntityDatabase"/> using <see cref="WorldEntityDatabaseHandler"/>.
    /// </summary>
    /// <param name="classId">The classId of the prefab.</param>
    /// <param name="prefabPath">The prefab path of the prefab.</param>
    /// <param name="biomeDistribution">The <see cref="LootDistributionData.BiomeData"/> dictating how the prefab should spawn in the world.</param>
    public static void AddLootDistributionData(string classId, string prefabPath, IEnumerable<LootDistributionData.BiomeData> biomeDistribution)
    {
        AddLootDistributionData(classId, new LootDistributionData.SrcData()
        {
            distribution = new List<LootDistributionData.BiomeData>(biomeDistribution),
            prefabPath = prefabPath
        });
    }

    /// <summary>
    /// Adds in a custom entry into the Loot Distribution of the game.
    /// </summary>
    /// <param name="classId">The classId of the prefab.</param>
    /// <param name="prefabPath">The prefab path of the prefab.</param>
    /// <param name="biomeDistribution">The <see cref="LootDistributionData.BiomeData"/> dictating how the prefab should spawn in the world.</param>
    /// <param name="info">The WorldEntityInfo of the prefab. For more information on how to set this up, see <see cref="WorldEntityDatabaseHandler"/>.</param>
    public static void AddLootDistributionData(string classId, string prefabPath, IEnumerable<LootDistributionData.BiomeData> biomeDistribution, WorldEntityInfo info)
    {
        AddLootDistributionData(classId, new LootDistributionData.SrcData()
        {
            distribution = new List<LootDistributionData.BiomeData>(biomeDistribution),
            prefabPath = prefabPath
        });

        WorldEntityDatabaseHandler.AddCustomInfo(classId, info);
    }

    /// <summary>
    /// Adds in a custom entry into the Loot Distribution of the game.
    /// </summary>
    /// <param name="prefabInfo">The custom prefab which you want to spawn naturally in the game.</param>
    /// <param name="biomeDistribution">The <see cref="LootDistributionData.BiomeData"/> dictating how the prefab should spawn in the world.</param>
    /// <param name="info">The WorldEntityInfo of the prefab. For more information on how to set this up, see <see cref="WorldEntityDatabaseHandler"/>.</param>
    public static void AddLootDistributionData(PrefabInfo prefabInfo, IEnumerable<LootDistributionData.BiomeData> biomeDistribution, WorldEntityInfo info)
    {
        AddLootDistributionData(prefabInfo.ClassID, prefabInfo.PrefabFileName, biomeDistribution);

        WorldEntityDatabaseHandler.AddCustomInfo(prefabInfo.ClassID, info);
    }

    /// <summary>
    /// Adds in a custom entry into the Loot Distribution of the game.
    /// </summary>
    /// <param name="classId">The classId of the prefab.</param>
    /// <param name="data">The <see cref="LootDistributionData.SrcData"/> that contains data related to the spawning of a prefab, also contains the path to the prefab.</param>
    public static void AddLootDistributionData(string classId, LootDistributionData.SrcData data)
    {
        if (LootDistributionPatcher.CustomSrcData.ContainsKey(classId))
        {
            InternalLogger.Log($"{classId}-{data.prefabPath} already has custom distribution data. Replacing with latest.", LogLevel.Debug);
        }

        LootDistributionPatcher.CustomSrcData[classId] = data;
    }

    /// <summary>
    /// Adds in a custom entry into Loot Distribution of the game.
    /// </summary>
    /// <param name="classId">The classId of the prefab.</param>
    /// <param name="biomeDistribution">The <see cref="LootDistributionData.BiomeData"/> dictating how the prefab should spawn in the world.</param>
    public static void AddLootDistributionData(string classId, params LootDistributionData.BiomeData[] biomeDistribution)
    {
        if (!PrefabDatabase.TryGetPrefabFilename(classId, out var filename))
        {
            InternalLogger.Error($"Could not find prefab file path for class ID '{classId}'. Cancelling loot distribution addition.");
            return;
        }
            
        AddLootDistributionData(classId, filename, biomeDistribution);
    }
        
    /// <summary>
    /// Adds in a custom entry into Loot Distribution of the game.
    /// </summary>
    /// <param name="classId">The classId of the prefab.</param>
    /// <param name="info">The WorldEntityInfo of the prefab. For more information on how to set this up, see <see cref="WorldEntityDatabaseHandler"/>.</param>
    /// <param name="biomeDistribution">The <see cref="LootDistributionData.BiomeData"/> dictating how the prefab should spawn in the world.</param>
    public static void AddLootDistributionData(string classId, WorldEntityInfo info, params LootDistributionData.BiomeData[] biomeDistribution)
    {
        if (!PrefabDatabase.TryGetPrefabFilename(classId, out var filename))
        {
            InternalLogger.Error($"Could not find prefab file path for class ID '{classId}'. Cancelling loot distribution addition.");
            return;
        }
            
        AddLootDistributionData(classId, filename, biomeDistribution);
        WorldEntityDatabaseHandler.AddCustomInfo(classId, info);
    }


    /// <summary>
    /// Edits Loot Distribution data for existing prefabs.
    /// </summary>
    /// <param name="classId">The ClassID of the prefab. If unsure, use CraftData.GetClassIdForTechType.</param>
    /// <param name="biome">The <see cref="BiomeType"/>to change the data for.</param>
    /// <param name="probability">The desired probability.</param>
    /// <param name="count">The number to spawn at a time when spawning happens.</param>
    public static void EditLootDistributionData(string classId, BiomeType biome, float probability, int count)
    {
        if (!LootDistributionPatcher.CustomSrcData.TryGetValue(classId, out LootDistributionData.SrcData srcData))
        {
            LootDistributionPatcher.CustomSrcData[classId] = (srcData = new LootDistributionData.SrcData());

            List<LootDistributionData.BiomeData> biomeDistribution = new()
            {
                new LootDistributionData.BiomeData()
                {
                    biome = biome,
                    probability = probability,
                    count = count
                }
            };

            srcData.distribution = biomeDistribution;

            return;
        }

        for (int i = 0; i < srcData.distribution.Count; i++)
        {
            LootDistributionData.BiomeData distribution = srcData.distribution[i];

            if (distribution.biome == biome)
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

    /// <summary>
    /// Edits Loot Distribution data for existing prefabs, for e.g. original game prefabs.
    /// </summary>
    /// <param name="classId">The ClassID of the prefab. If unsure, use CraftData.GetClassIdForTechType.</param>
    /// <param name="biomeDistribution">The list of <see cref="LootDistributionData.BiomeData"/> that contains information about how/when it should spawn in biomes.</param>
    public static void EditLootDistributionData(string classId, IEnumerable<LootDistributionData.BiomeData> biomeDistribution)
    {
        foreach (LootDistributionData.BiomeData distribution in biomeDistribution)
        {
            EditLootDistributionData(classId, distribution.biome, distribution.probability, distribution.count);
        }
    }
}