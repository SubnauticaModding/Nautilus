namespace SMLHelper.Assets
{
    using System.Collections.Generic;
    using Handlers;
    using Utility;
    using Crafting;

#if SUBNAUTICA
    using RecipeData = Crafting.TechData;
#endif

    /// <summary>
    /// Extensions for the ModPrefab class to set things up without having to use Inheritance based prefabs.
    /// </summary>
    public static partial class BuilderExtensions
    {
        /// <summary>
        /// Sets the recipe value of this <see cref="ModPrefab"/> using its <see cref="ModPrefab.TechType"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="count">How many of this gets crafted at a time.</param>
        /// <param name="ingredients">The ingredients cost.</param>
        /// <param name="linkedItems">Any other TechTypes that should be gived to the player when this one is crafted. (like how you get gloves and helm with the suits)</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetRecipe(this ModPrefabBuilder modPrefabBuilder, int count, List<Ingredient> ingredients, List<TechType> linkedItems)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set Recipe for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            return modPrefabBuilder.SetRecipe(new RecipeData() { craftAmount = count, Ingredients = ingredients, LinkedItems = linkedItems });
        }

        /// <summary>
        /// Sets the recipe value of this <see cref="ModPrefab"/> using its <see cref="ModPrefab.TechType"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="recipe">The recipe information you want to add to this ModPrefab</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetRecipe(this ModPrefabBuilder modPrefabBuilder, RecipeData recipe)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set Recipe for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            CraftDataHandler.SetTechData(modPrefab.TechType, recipe);
            return modPrefabBuilder;
        }

        /// <summary>
        /// Registers the Inventory size for this <see cref="ModPrefab"/> using its <see cref="ModPrefab.TechType"/> and the provided <see cref="Vector2int"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="x">The horizontal size you want to register to this ModPrefab's TechType</param>
        /// <param name="y">The vertical size you want to register to this ModPrefab's TechType</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetInventorySize(this ModPrefabBuilder modPrefabBuilder, int x, int y)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set InventorySize for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            CraftDataHandler.SetItemSize(modPrefab.TechType, x, y);
            return modPrefabBuilder;
        }

        /// <summary>
        /// Registers the Inventory size for this <see cref="ModPrefab"/> using its <see cref="ModPrefab.TechType"/> and the provided <see cref="Vector2int"/>. 
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="size">The size you want to register to this ModPrefab's TechType</param>
        /// <returns>The original Modprefab so these can be called in sequence.</returns>
        public static ModPrefabBuilder SetInventorySize(this ModPrefabBuilder modPrefabBuilder, Vector2int size)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set InventorySize for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            CraftDataHandler.SetItemSize(modPrefab.TechType, size);
            return modPrefabBuilder;
        }


        /// <summary>
        /// Allows you to add items to the game's internal grouping system.
        /// Required if you want to make buildable items show up in the Habitat Builder or show in the Blueprints Tab of the PDA.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="group">The TechGroup you want to add your TechType to.</param>
        /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
        public static ModPrefabBuilder SetTechCategory(this ModPrefabBuilder modPrefabBuilder, TechGroup group, TechCategory category)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set TechCategory for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            if(group == TechGroup.Uncategorized)
                return modPrefabBuilder;

            List<TechCategory> categories = new();
            CraftData.GetBuilderCategories(group, categories);
            if(!categories.Contains(category))
            {
                InternalLogger.Error($"Failed to add {modPrefab.TechType} to {group}/{category} as it is not a registered combination.");
                return modPrefabBuilder;
            }

            CraftDataHandler.AddToGroup(group, category, modPrefab.TechType);
            return modPrefabBuilder;
        }


        /// <summary>
        /// Adds a new crafting node to the specified crafting tree, at the provided tab location or the root if no steps provided.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="fabricatorType">The target craft tree to edit.</param>
        /// <param name="stepsToFabricatorTab">
        /// <para>The steps to the target tab.</para>
        /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
        /// <para>Do not include "root" in this path.</para>
        /// </param>        
        public static ModPrefabBuilder SetCraftingNode(this ModPrefabBuilder modPrefabBuilder, CraftTree.Type fabricatorType, string[] stepsToFabricatorTab = null)
        {
            if(fabricatorType == CraftTree.Type.None)
                return modPrefabBuilder;

            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set Crafting Node for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            if(stepsToFabricatorTab == null || stepsToFabricatorTab.Length == 0)
            {
                CraftTreeHandler.AddCraftingNode(fabricatorType, modPrefab.TechType);
            }
            else
            {
                CraftTreeHandler.AddCraftingNode(fabricatorType, modPrefab.TechType, stepsToFabricatorTab);
            }

            return modPrefabBuilder;
        }

        /// <summary>
        /// <para>Allows you to edit crafting times for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="craftingTime">The crafting time, in seconds, for that TechType.</param>
        public static ModPrefabBuilder SetCraftingTime(this ModPrefabBuilder modPrefabBuilder, float craftingTime)
        {
            if(craftingTime <= 0f)
                return modPrefabBuilder;

            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set crafting time for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            CraftDataHandler.SetCraftingTime(modPrefab.TechType, craftingTime);
            return modPrefabBuilder;
        }

        /// <summary>
        /// <para>Allows you to edit EquipmentTypes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="equipmentType">The EquipmentType for that TechType.</param>
        public static ModPrefabBuilder SetEquipmentType(this ModPrefabBuilder modPrefabBuilder, EquipmentType equipmentType)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set equipment type for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            CraftDataHandler.SetEquipmentType(modPrefab.TechType, equipmentType);
            return modPrefabBuilder;
        }

        /// <summary>
        /// <para>Allows you to edit QuickSlotType for TechTypes. Can be used for existing TechTypes too.</para>
        /// <para>Careful: This has to be called after <see cref="SetRecipe(ModPrefabBuilder, int, List{Ingredient}, List{TechType})"/> and <see cref="SetRecipe(ModPrefabBuilder, RecipeData)"/>.</para>
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="quickSlotType">The QuickSlotType for that TechType.</param>
        public static ModPrefabBuilder SetQuickSlotType(this ModPrefabBuilder modPrefabBuilder, QuickSlotType quickSlotType)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set quick slot type for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            CraftDataHandler.SetQuickSlotType(modPrefab.TechType, quickSlotType);
            return modPrefabBuilder;
        }

        /// <summary>
        /// Allows you to add items to the buildable list.
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        public static ModPrefabBuilder SetBuildableFlag(this ModPrefabBuilder modPrefabBuilder)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set buildable flag for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            CraftDataHandler.AddBuildable(modPrefab.TechType);
            return modPrefabBuilder;
        }
    }
}
