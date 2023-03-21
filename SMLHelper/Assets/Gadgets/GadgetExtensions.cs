using System.IO;
using Newtonsoft.Json;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
using SMLHelper.Json.Converters;
using SMLHelper.Utility;
using UnityEngine;
using UWE;

namespace SMLHelper.Assets.Gadgets;

/// <summary>
/// Represents extension methods for the <see cref="Gadget"/> class.
/// </summary>
public static class GadgetExtensions
{
    /// <summary>
    /// Adds recipe to this custom prefab.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add recipe to.</param>
    /// <param name="recipeData">The recipe to add.</param>
    /// <returns>An instance to the created <see cref="CraftingGadget"/> to continue the recipe settings on.</returns>
    public static CraftingGadget SetRecipe(this ICustomPrefab customPrefab, RecipeData recipeData)
    {
        if (!customPrefab.TryGetGadget(out CraftingGadget craftingGadget))
        { 
            craftingGadget = new CraftingGadget(customPrefab, recipeData);
        }

        craftingGadget.RecipeData = recipeData;
        
        customPrefab.AddGadget(craftingGadget);
        
        return craftingGadget;
    }

    /// <summary>
    /// Adds recipe from a json file to this custom prefab.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add recipe to.</param>
    /// <param name="filePath">The path to the recipe json file. A string with valid recipe data json is also acceptable.</param>
    /// <returns>An instance to the created <see cref="CraftingGadget"/> to continue the recipe settings on.</returns>
    public static CraftingGadget SetRecipeFromJson(this ICustomPrefab customPrefab, string filePath)
    {
        RecipeData recipeData;
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);
            recipeData = JsonConvert.DeserializeObject<RecipeData>(content, new CustomEnumConverter());
        }
        else
        {
            recipeData = JsonConvert.DeserializeObject<RecipeData>(filePath, new CustomEnumConverter());
        }
        
        if (recipeData is null)
        {
            InternalLogger.Error($"File '{filePath} is not a valid RecipeData json file. Skipping recipe addition.'");
            return null;
        }

        if (!customPrefab.TryGetGadget(out CraftingGadget craftingGadget))
        { 
            craftingGadget = new CraftingGadget(customPrefab, recipeData);
        }

        craftingGadget.RecipeData = recipeData;
        
        customPrefab.AddGadget(craftingGadget);
        
        return craftingGadget;
    }

    /// <summary>
    /// Adds unlocks to this custom prefab.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add unlocks to.</param>
    /// <param name="requiredForUnlock">The blueprint to set as a requirement.</param>
    /// <param name="fragmentsToScan">Amount of <paramref name="requiredForUnlock"/> that must be scanned to unlock this item.</param>
    /// <returns>An instance to the created <see cref="ScanningGadget"/> to continue the scanning settings on.</returns>
    public static ScanningGadget SetUnlock(this ICustomPrefab customPrefab, TechType requiredForUnlock, int fragmentsToScan = 1)
    {
        if (!customPrefab.TryGetGadget(out ScanningGadget scanningGadget))
        {
            scanningGadget = new ScanningGadget(customPrefab, requiredForUnlock, fragmentsToScan);
        }
        
        scanningGadget.RequiredForUnlock = requiredForUnlock;
        scanningGadget.FragmentsToScan = fragmentsToScan;
        
        customPrefab.AddGadget(scanningGadget);

        return scanningGadget;
    }

    /// <summary>
    /// Sets the type of equipment slot this item can fit into.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to set equipment slot for.</param>
    /// <param name="equipmentType">The type of equipment slot this item can fit into.</param>
    /// <returns>An instance to the created <see cref="EquipmentGadget"/> to continue the equipment settings on.</returns>
    public static EquipmentGadget SetEquipment(this ICustomPrefab customPrefab, EquipmentType equipmentType)
    {
        if (!customPrefab.TryGetGadget(out EquipmentGadget equipmentGadget))
        {
            equipmentGadget = new EquipmentGadget(customPrefab, equipmentType);
        }

        equipmentGadget.EquipmentType = equipmentType;
        
        customPrefab.AddGadget(equipmentGadget);

        return equipmentGadget;
    }

    /// <summary>
    /// Creates a craft tree for this prefab. The created craft tree is immediately returned after this method is executed. 
    /// </summary>
    /// <param name="customPrefab">The custom prefab to set equipment slot for.</param>
    /// <param name="treeType">The created custom craft tree type.</param>
    /// <returns>An instance to the created <see cref="FabricatorGadget"/> to continue the fabricator settings on.</returns>
    public static FabricatorGadget CreateFabricator(this ICustomPrefab customPrefab, out CraftTree.Type treeType)
    {
        if (!customPrefab.TryGetGadget(out FabricatorGadget fabricatorGadget))
        {
            fabricatorGadget = new FabricatorGadget(customPrefab);
        }

        treeType = fabricatorGadget.CraftTreeType;
        
        customPrefab.AddGadget(fabricatorGadget);

        return fabricatorGadget;
    }
    
    /// <summary>
    /// Adds coordinated spawns for this custom prefab.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add coordinated spawns for.</param>
    /// <param name="spawnLocations">The spawn locations to spawn in.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static ICustomPrefab SetSpawns(this ICustomPrefab customPrefab, params SpawnLocation[] spawnLocations)
    {
        customPrefab.AddOnRegister(() =>
        {
            foreach ((Vector3 position, Vector3 eulerAngles) in spawnLocations)
            {
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(customPrefab.Info.ClassID, position, eulerAngles));
            }
        });

        return customPrefab;
    }
    
    /// <summary>
    /// Adds biome spawns for this custom prefab with default <see cref="WorldEntityInfo"/> values.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add biome spawns for.</param>
    /// <param name="biomesToSpawnIn">The biomes to spawn in.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static ICustomPrefab SetSpawns(this ICustomPrefab customPrefab, params LootDistributionData.BiomeData[] biomesToSpawnIn)
    {
        var entityInfo = new WorldEntityInfo
        {
            classId = customPrefab.Info.ClassID,
            techType = customPrefab.Info.TechType,
            localScale = Vector3.one,
            cellLevel = LargeWorldEntity.CellLevel.Medium,
            slotType = EntitySlot.Type.Medium
        };

        return SetSpawns(customPrefab, entityInfo, biomesToSpawnIn);
    }

    /// <summary>
    /// Adds biome spawns for this custom prefab.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add biome spawns for.</param>
    /// <param name="entityInfo">Data on how the biome spawner should treat this object as.</param>
    /// <param name="biomesToSpawnIn">The biomes to spawn in.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static ICustomPrefab SetSpawns(this ICustomPrefab customPrefab, WorldEntityInfo entityInfo,
        params LootDistributionData.BiomeData[] biomesToSpawnIn)
    {
        customPrefab.AddOnRegister(() =>
        {
            LootDistributionHandler.AddLootDistributionData(customPrefab.Info.ClassID,
                customPrefab.Info.PrefabFileName, biomesToSpawnIn, entityInfo);
        });

        return customPrefab;
    }
}