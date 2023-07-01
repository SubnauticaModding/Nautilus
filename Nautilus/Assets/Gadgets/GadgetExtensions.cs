using System.IO;
using System.Runtime.CompilerServices;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Json.Converters;
using Nautilus.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace Nautilus.Assets.Gadgets;

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
            return customPrefab.AddGadget(new CraftingGadget(customPrefab, recipeData));

        craftingGadget.RecipeData = recipeData;
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
            return customPrefab.AddGadget(new CraftingGadget(customPrefab, recipeData));

        craftingGadget.RecipeData = recipeData;
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
            return customPrefab.AddGadget(new ScanningGadget(customPrefab, requiredForUnlock, fragmentsToScan));

        scanningGadget.RequiredForUnlock = requiredForUnlock;
        scanningGadget.FragmentsToScan = fragmentsToScan;
        return scanningGadget;
    }

    /// <summary>
    /// Adds this item into a blueprint category to appear in.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add unlocks to.</param>
    /// <param name="group">The main group in the PDA blueprints where this item appears</param>
    /// <param name="category">The category within the group in the PDA blueprints where this item appears.</param>
    /// <returns>An instance to the created <see cref="ScanningGadget"/> to continue the scanning settings on.</returns>
    /// <remarks>If the specified <paramref name="group"/> is a tech group that is present in the <see cref="uGUI_BuilderMenu.groups"/> list, this item will automatically
    /// become buildable. To avoid this, or make this item a buildable manually, use the <see cref="ScanningGadget.SetBuildable"/> method.</remarks>
    public static ScanningGadget SetPdaGroupCategory(this ICustomPrefab customPrefab, TechGroup group, TechCategory category)
    {
        if (!customPrefab.TryGetGadget(out ScanningGadget scanningGadget))
            scanningGadget = customPrefab.AddGadget(new ScanningGadget(customPrefab, TechType.None));

        scanningGadget.WithPdaGroupCategory(group, category);
        return scanningGadget;
    }

    /// <summary>
    /// Adds this item into a blueprint category to appear in.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add unlocks to.</param>
    /// <param name="group">The main group in the PDA blueprints where this item appears.</param>
    /// <param name="category">The category within the group in the PDA blueprints where this item appears.</param>
    /// <param name="target">It will be added after this target item or at the end if not found.</param>
    /// <returns>An instance to the created <see cref="ScanningGadget"/> to continue the scanning settings on.</returns>
    /// <remarks>If the specified <paramref name="group"/> is a tech group that is present in the <see cref="uGUI_BuilderMenu.groups"/> list, this item will automatically
    /// become buildable. To avoid this, or make this item a buildable manually, use the <see cref="ScanningGadget.SetBuildable"/> method.</remarks>
    public static ScanningGadget SetPdaGroupCategoryAfter(this ICustomPrefab customPrefab, TechGroup group, TechCategory category, TechType target)
    {
        if (!customPrefab.TryGetGadget(out ScanningGadget scanningGadget))
            scanningGadget = customPrefab.AddGadget(new ScanningGadget(customPrefab, TechType.None));

        return scanningGadget.WithPdaGroupCategoryAfter(group, category, target);
    }

    /// <summary>
    /// Adds this item into a blueprint category to appear in.
    /// </summary>
    /// <param name="customPrefab">The custom prefab to add unlocks to.</param>
    /// <param name="group">The main group in the PDA blueprints where this item appears.</param>
    /// <param name="category">The category within the group in the PDA blueprints where this item appears.</param>
    /// <param name="target">It will be inserted before this target item or at the beginning if not found.</param>
    /// <returns>An instance to the created <see cref="ScanningGadget"/> to continue the scanning settings on.</returns>
    /// <remarks>If the specified <paramref name="group"/> is a tech group that is present in the <see cref="uGUI_BuilderMenu.groups"/> list, this item will automatically
    /// become buildable. To avoid this, or make this item a buildable manually, use the <see cref="ScanningGadget.SetBuildable"/> method.</remarks>
    public static ScanningGadget SetPdaGroupCategoryBefore(this ICustomPrefab customPrefab, TechGroup group, TechCategory category, TechType target)
    {
        if (!customPrefab.TryGetGadget(out ScanningGadget scanningGadget))
            scanningGadget = customPrefab.AddGadget(new ScanningGadget(customPrefab, TechType.None));

        return scanningGadget.WithPdaGroupCategoryBefore(group, category, target);
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
            return customPrefab.AddGadget(new EquipmentGadget(customPrefab, equipmentType));

        equipmentGadget.EquipmentType = equipmentType;
        return equipmentGadget;
    }

    /// <summary>
    /// Sets this item as a vehicle upgrade module. Cyclops upgrades are not supported by this function.
    /// <para>If you're using this function, please do not use <see cref="SetEquipment(ICustomPrefab, EquipmentType)"/>,<br/>
    /// it would interefere with this and possibly make the game crash or cause the mod to not work.</para>
    /// </summary>
    /// <param name="customPrefab">The custom prefab to set vehicle upgrade for.</param>
    /// <param name="equipmentType">The type of equipment slot this item can fit into. Preferably use something related to vehicles.</param>
    /// <param name="slotType">The quick slot type</param>
    /// <returns>An instance to the created <see cref="UpgradeModuleGadget"/> to continue the upgrade settings on.</returns>
    public static UpgradeModuleGadget SetVehicleUpgradeModule(this ICustomPrefab customPrefab, EquipmentType equipmentType = EquipmentType.VehicleModule, QuickSlotType slotType = QuickSlotType.Passive)
    {
        if(customPrefab.TryGetGadget(out UpgradeModuleGadget upgradeModuleGadget))
        {
            customPrefab.SetEquipment(equipmentType).WithQuickSlotType(slotType);
            return upgradeModuleGadget;
        }
        else
        {
            customPrefab.SetEquipment(equipmentType).WithQuickSlotType(slotType);
            upgradeModuleGadget = new UpgradeModuleGadget(customPrefab);
            customPrefab.TryAddGadget(upgradeModuleGadget);
            return upgradeModuleGadget;
        }
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
            customPrefab.AddGadget(fabricatorGadget);
        }

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