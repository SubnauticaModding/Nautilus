using Newtonsoft.Json;
using SMLHelper.Assets.Interfaces;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
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
        var craftingGadget = new CraftingGadget(customPrefab, recipeData);
        customPrefab.AddGadget(craftingGadget);
        
        return craftingGadget;
    }

    /// <summary>
    /// Adds recipe from a json file to this custom prefab.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add recipe to.</param>
    /// <param name="filePath">The path to the recipe json file.</param>
    /// <returns>An instance to the created <see cref="CraftingGadget"/> to continue the recipe settings on.</returns>
    public static CraftingGadget SetRecipeFromJson(this ICustomPrefab customPrefab, string filePath)
    {
        var recipeData = JsonConvert.DeserializeObject<RecipeData>(filePath);
        var craftingGadget = new CraftingGadget(customPrefab, recipeData);
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
        var scanningGadget = new ScanningGadget(customPrefab, requiredForUnlock, fragmentsToScan);
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
        var equipmentGadget = new EquipmentGadget(customPrefab, equipmentType);
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
        var fabricatorGadget = new FabricatorGadget(customPrefab);
        customPrefab.AddGadget(fabricatorGadget);

        treeType = fabricatorGadget.CraftTreeType;

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
        customPrefab.AddPostRegister(() =>
        {
            foreach ((Vector3 position, Vector3 eulerAngles) in spawnLocations)
            {
                CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(customPrefab.Info.ClassID, position, eulerAngles));
            }
        });

        return customPrefab;
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
        customPrefab.AddPostRegister(() =>
        {
            LootDistributionHandler.AddLootDistributionData(customPrefab.Info.ClassID,
                customPrefab.Info.PrefabFileName, biomesToSpawnIn, entityInfo);
        });

        return customPrefab;
    }
}