namespace SMLHelper.V2.Handlers
{
    using System.Collections.Generic;
    using Crafting;
    using Interfaces;
    using Patchers;

    /// <summary>
    /// A handler class for adding and editing crafted items.
    /// </summary>
    public class CraftDataHandler : ICraftDataHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static ICraftDataHandler Main { get; } = new CraftDataHandler();

        private CraftDataHandler()
        {
            // Hides constructor
        }

        #region Static Methods

#if BELOWZERO

        /// <summary>
        /// <para>Allows you to edit recipes, i.e. RecipeData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="recipeData">The TechData for that TechType.</param>
        /// <seealso cref="RecipeData"/>
        public static void SetTechData(TechType techType, RecipeData recipeData)
        {
            Main.SetTechData(techType, recipeData);
        }

        /// <summary>
        /// <para>Allows you to edit recipes, i.e. TechData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="techData">The TechData for that TechType.</param>
        /// <seealso cref="TechData.defaults"/>
        public static void SetTechData(TechType techType, JsonValue jsonValue)
        {
            Main.SetTechData(techType, jsonValue);
        }

        /// <summary>
        /// <para>Allows you to edit recipes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        public static void AddIngredients(TechType techType, ICollection<Ingredient> ingredients)
        {
            Main.AddIngredients(techType, ingredients);
        }

        /// <summary>
        /// <para>Allows you to edit Linked Items for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        public static void AddLinkedItems(TechType techType, ICollection<TechType> linkedItems)
        {
            Main.AddLinkedItems(techType, linkedItems);
        }

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The ITechData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        public static JsonValue GetModdedTechData(TechType techType)
        {
            return Main.GetModdedTechData(techType);
        }
#elif SUBNAUTICA
        
        /// <summary>
        /// <para>Allows you to edit recipes, i.e. TechData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="techData">The TechData for that TechType.</param>
        /// <seealso cref="TechData"/>
        public static void SetTechData(TechType techType, ITechData techData)
        {
            Main.SetTechData(techType, techData);
        }

        /// <summary>
        /// <para>Allows you to edit recipes, i.e. TechData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="techData">The TechData for that TechType.</param>
        /// <seealso cref="TechData"/>
        public static void SetTechData(TechType techType, TechData techData)
        {
            Main.SetTechData(techType, techData);
        }

#endif

        /// <summary>
        /// <para>Allows you to edit EquipmentTypes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose EqiupmentType you want to edit.</param>
        /// <param name="equipmentType">The EquipmentType for that TechType.</param>
        public static void SetEquipmentType(TechType techType, EquipmentType equipmentType)
        {
            Main.SetEquipmentType(techType, equipmentType);
        }

        /// <summary>
        /// <para>Allows you to edit QuickSlotType for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose QuickSlotType you want to edit.</param>
        /// <param name="slotType">The QuickSlotType for that TechType.</param>
        public static void SetQuickSlotType(TechType techType, QuickSlotType slotType)
        {
            Main.SetQuickSlotType(techType, slotType);
        }

        /// <summary>
        /// <para>Allows you to edit harvest output, i.e. what TechType you get when you "harvest" a TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose harvest output you want to edit.</param>
        /// <param name="harvestOutput">The harvest output for that TechType.</param>
        public static void SetHarvestOutput(TechType techType, TechType harvestOutput)
        {
            Main.SetHarvestOutput(techType, harvestOutput);
        }

        /// <summary>
        /// <para>Allows you to edit how TechTypes are harvested.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose HarvestType you want to edit.</param>
        /// <param name="harvestType">The HarvestType for that TechType.</param>
        public static void SetHarvestType(TechType techType, HarvestType harvestType)
        {
            Main.SetHarvestType(techType, harvestType);
        }

        /// <summary>
        /// <para>Allows you to edit how much additional slices/seeds are given upon last knife hit.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose final cut bonus you want to edit.</param>
        /// <param name="bonus">The number of additional slices/seeds you'll receive on last cut.</param>
        public static void SetHarvestFinalCutBonus(TechType techType, int bonus)
        {
            Main.SetHarvestFinalCutBonus(techType, bonus);
        }

        /// <summary>
        /// <para>Allows you to edit item sizes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose item size you want to edit.</param>
        /// <param name="size">The item size for that TechType.</param>
        public static void SetItemSize(TechType techType, Vector2int size)
        {
            Main.SetItemSize(techType, size);
        }

        /// <summary>
        /// <para>Allows you to edit item sizes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose item size you want to edit.</param>
        /// <param name="x">The width of the item</param>
        /// <param name="y">The height of the item</param>
        public static void SetItemSize(TechType techType, int x, int y)
        {
            Main.SetItemSize(techType, x, y);
        }

        /// <summary>
        /// <para>Allows you to edit crafting times for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose crafting time you want to edit.</param>
        /// <param name="time">The crafting time, in seconds, for that TechType.</param>
        public static void SetCraftingTime(TechType techType, float time)
        {
            Main.SetCraftingTime(techType, time);
        }

        /// <summary>
        /// <para>Allows you to edit the cooked creature list, i.e. associate the unedible TechType to the cooked TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="uncooked">The TechType whose cooked creature counterpart to edit.</param>
        /// <param name="cooked">The cooked creature counterpart for that TechType.</param>
        public static void SetCookedVariant(TechType uncooked, TechType cooked)
        {
            Main.SetCookedVariant(uncooked, cooked);
        }

        /// <summary>
        /// <para>Allows you to edit inventory background colors for TechTypes.</para>
        /// </summary>
        /// <param name="techType">The TechType whose BackgroundType you want to edit.</param>
        /// <param name="backgroundColor">The background color for that TechType.</param>
        /// <seealso cref="CraftData.BackgroundType"/>
        public static void SetBackgroundType(TechType techType, CraftData.BackgroundType backgroundColor)
        {
            Main.SetBackgroundType(techType, backgroundColor);
        }

        /// <summary>
        /// Allows you to add items to the buildable list.
        /// </summary>
        /// <param name="techType">The TechType which you want to add to the buildable list.</param>
        public static void AddBuildable(TechType techType)
        {
            Main.AddBuildable(techType);
        }

        /// <summary>
        /// Allows you to add items to the game's internal grouping system.
        /// Required if you want to make buildable items show up in the Habitat Builder.
        /// </summary>
        /// <param name="group">The TechGroup you want to add your TechType to.</param>
        /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
        /// <param name="techType">The TechType you want to add.</param>
        public static void AddToGroup(TechGroup group, TechCategory category, TechType techType)
        {
            Main.AddToGroup(group, category, techType, TechType.None);
        }

        /// <summary>
        /// Allows you to add items to the game's internal grouping system.
        /// Required if you want to make buildable items show up in the Habitat Builder.
        /// </summary>
        /// <param name="group">The TechGroup you want to add your TechType to.</param>
        /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
        /// <param name="techType">The TechType you want to add.</param>
        /// <param name="after">Added TechType will be added after this TechType, for sorting purposes.</param>
        public static void AddToGroup(TechGroup group, TechCategory category, TechType techType, TechType after)
        {
            Main.AddToGroup(group, category, techType, after);
        }

        /// <summary>
        /// Allows you to remove an existing TechType from the game's internal group system.
        /// </summary>
        /// <param name="group">The TechGroup in which the TechType is located.</param>
        /// <param name="category">The TechCategory in which the TechType is located.</param>
        /// <param name="techType">The TechType which you want to remove.</param>
        public static void RemoveFromGroup(TechGroup group, TechCategory category, TechType techType)
        {
            Main.RemoveFromGroup(group, category, techType);
        }

        #endregion

        #region Interface Methods

#if BELOWZERO

        /// <summary>
        /// <para>Allows you to add or edit RecipeData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="recipeData">The TechData for that TechType.</param>
        /// <seealso cref="RecipeData"/>
        void ICraftDataHandler.SetTechData(TechType techType, RecipeData recipeData)
        {
            var currentTechType = new JsonValue();
            currentTechType = new JsonValue
            {
                { TechData.PropertyToID("techType"), new JsonValue((int)techType) },
                { TechData.PropertyToID("craftAmount"), new JsonValue(recipeData.craftAmount) }
            };
            CraftDataPatcher.CustomTechData[techType] = currentTechType;
            if (recipeData.ingredientCount > 0)
            {
                CraftDataHandler.AddIngredients(techType, recipeData.Ingredients);
            }
            if (recipeData.linkedItemCount > 0)
            {
                CraftDataHandler.AddLinkedItems(techType, recipeData.LinkedItems);
            }

        }

        /// <summary>
        /// <para>Allows you to add or edit TechData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="recipeData">The TechData for that TechType.</param>
        /// <seealso cref="TechData.defaults"/>
        void ICraftDataHandler.SetTechData(TechType techType, JsonValue jsonValue)
        {
            if (new JsonValue((int)techType) != jsonValue[TechData.PropertyToID("techType")])
            {
                jsonValue.Add(TechData.PropertyToID("techType"), new JsonValue((int)techType));
            }
            CraftDataPatcher.CustomTechData[techType] = jsonValue;
        }

        /// <summary>
        /// <para>Allows you to edit recipes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        void ICraftDataHandler.AddIngredients(TechType techType, ICollection<Ingredient> ingredients)
        {
            CraftDataPatcher.CustomTechData[techType].Add(TechData.PropertyToID("ingredients"), new JsonValue(JsonValue.Type.Array));
            JsonValue ingredientslist = CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("ingredients")];
            int amount = TechData.PropertyToID("amount");
            int tech = TechData.PropertyToID("techType");
            int count = ingredients.Count;
            int current = 0;


            foreach (Ingredient i in ingredients)
            {
                ingredientslist.Add(new JsonValue(current));
                ingredientslist[current] = new JsonValue(JsonValue.Type.Object)
                {
                    { amount, new JsonValue(i.amount) },
                    { tech, new JsonValue((int)i.techType) }
                };
                current++;
            }
        }

        /// <summary>
        /// <para>Allows you to edit Linked Items for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        void ICraftDataHandler.AddLinkedItems(TechType techType, ICollection<TechType> linkedItems)
        {
            CraftDataPatcher.CustomTechData[techType].Add(TechData.PropertyToID("linkedItems"), new JsonValue(JsonValue.Type.Array));
            JsonValue linkedItemslist = CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("linkedItems")];
            int amount = TechData.PropertyToID("amount");
            int tech = TechData.PropertyToID("techType");
            int count = linkedItems.Count;
            int current = 0;


            foreach (TechType i in linkedItems)
            {
                linkedItemslist.Add(new JsonValue(current));
                linkedItemslist[current] = new JsonValue((int)i);
                current++;
            }
        }

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The ITechData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        JsonValue ICraftDataHandler.GetModdedTechData(TechType techType)
        {
            if (CraftDataPatcher.CustomTechData.TryGetValue(techType, out JsonValue moddedTechData))
            {
                return moddedTechData;
            }

            return null;
        }

        /// <summary>
        /// <para>Allows you to edit EquipmentTypes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose EqiupmentType you want to edit.</param>
        /// <param name="equipmentType">The EquipmentType for that TechType.</param>
        void ICraftDataHandler.SetEquipmentType(TechType techType, EquipmentType equipmentType)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("equipmentType")] = new JsonValue((int)equipmentType);
        }

        /// <summary>
        /// <para>Allows you to edit QuickSlotType for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose QuickSlotType you want to edit.</param>
        /// <param name="slotType">The QuickSlotType for that TechType.</param>
        void ICraftDataHandler.SetQuickSlotType(TechType techType, QuickSlotType slotType)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("slotType")] = new JsonValue((int)slotType);
        }

        /// <summary>
        /// <para>Allows you to edit harvest output, i.e. what TechType you get when you "harvest" a TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose harvest output you want to edit.</param>
        /// <param name="harvestOutput">The harvest output for that TechType.</param>
        void ICraftDataHandler.SetHarvestOutput(TechType techType, TechType harvestOutput)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("harvestOutput")] = new JsonValue((int)harvestOutput);
        }

        /// <summary>
        /// <para>Allows you to edit how TechTypes are harvested.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose HarvestType you want to edit.</param>
        /// <param name="harvestType">The HarvestType for that TechType.</param>
        void ICraftDataHandler.SetHarvestType(TechType techType, HarvestType harvestType)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("harvestType")] = new JsonValue((int)harvestType);
        }

        /// <summary>
        /// <para>Allows you to edit how much additional slices/seeds are given upon last knife hit.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose final cut bonus you want to edit.</param>
        /// <param name="bonus">The number of additional slices/seeds you'll receive on last cut.</param>
        void ICraftDataHandler.SetHarvestFinalCutBonus(TechType techType, int bonus)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("harvestFinalCutBonus")] = new JsonValue(bonus);
        }

        /// <summary>
        /// <para>Allows you to edit item sizes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose item size you want to edit.</param>
        /// <param name="size">The item size for that TechType.</param>
        void ICraftDataHandler.SetItemSize(TechType techType, Vector2int size)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("x")] = new JsonValue(size.x);
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("y")] = new JsonValue(size.y);
        }

        /// <summary>
        /// <para>Allows you to edit item sizes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose item size you want to edit.</param>
        /// <param name="x">The width of the item</param>
        /// <param name="y">The height of the item</param>
        void ICraftDataHandler.SetItemSize(TechType techType, int x, int y)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("x")] = new JsonValue(x);
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("y")] = new JsonValue(y);
        }

        /// <summary>
        /// <para>Allows you to edit crafting times for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose crafting time you want to edit.</param>
        /// <param name="time">The crafting time, in seconds, for that TechType.</param>
        void ICraftDataHandler.SetCraftingTime(TechType techType, float time)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("craftTime")] = new JsonValue(time);
        }

        /// <summary>
        /// <para>Allows you to edit the cooked creature list, i.e. associate the unedible TechType to the cooked TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="uncooked">The TechType whose cooked creature counterpart to edit.</param>
        /// <param name="cooked">The cooked creature counterpart for that TechType.</param>
        void ICraftDataHandler.SetCookedVariant(TechType uncooked, TechType cooked)
        {
            CraftDataPatcher.CustomTechData[uncooked][TechData.PropertyToID("processed")] = new JsonValue((int)cooked);
        }

        /// <summary>
        /// <para>Allows you to edit inventory background colors for TechTypes.</para>
        /// </summary>
        /// <param name="techType">The TechType whose BackgroundType you want to edit.</param>
        /// <param name="backgroundColor">The background color for that TechType.</param>
        /// <seealso cref="CraftData.BackgroundType"/>
        void ICraftDataHandler.SetBackgroundType(TechType techType, CraftData.BackgroundType backgroundColor)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("backgroundType")] = new JsonValue((int)backgroundColor);
        }

        /// <summary>
        /// Allows you to add items to the buildable list.
        /// </summary>
        /// <param name="techType">The TechType which you want to add to the buildable list.</param>
        void ICraftDataHandler.AddBuildable(TechType techType)
        {
            CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("buildable")] = new JsonValue(true);
        }

#elif SUBNAUTICA

        /// <summary>
        /// <para>Allows you to edit recipes, i.e. TechData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="techData">The TechData for that TechType.</param>
        /// <seealso cref="TechData"/>
        void ICraftDataHandler.SetTechData(TechType techType, ITechData techData)
        {
            CraftDataPatcher.CustomTechData[techType] = techData;
        }

        /// <summary>
        /// <para>Allows you to edit recipes, i.e. TechData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="techData">The TechData for that TechType.</param>
        /// <seealso cref="TechData"/>
        void ICraftDataHandler.SetTechData(TechType techType, TechData techData)
        {
            CraftDataPatcher.CustomTechData[techType] = techData;
        }

        
        /// <summary>
        /// <para>Allows you to edit EquipmentTypes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose EqiupmentType you want to edit.</param>
        /// <param name="equipmentType">The EquipmentType for that TechType.</param>
        void ICraftDataHandler.SetEquipmentType(TechType techType, EquipmentType equipmentType)
        {
            CraftDataPatcher.CustomEquipmentTypes[techType] = equipmentType;
        }

        /// <summary>
        /// <para>Allows you to edit QuickSlotType for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose QuickSlotType you want to edit.</param>
        /// <param name="slotType">The QuickSlotType for that TechType.</param>
        void ICraftDataHandler.SetQuickSlotType(TechType techType, QuickSlotType slotType)
        {
            CraftDataPatcher.CustomSlotTypes[techType] = slotType;
        }

        /// <summary>
        /// <para>Allows you to edit harvest output, i.e. what TechType you get when you "harvest" a TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose harvest output you want to edit.</param>
        /// <param name="harvestOutput">The harvest output for that TechType.</param>
        void ICraftDataHandler.SetHarvestOutput(TechType techType, TechType harvestOutput)
        {
            CraftDataPatcher.CustomHarvestOutputList[techType] = harvestOutput;
        }

        /// <summary>
        /// <para>Allows you to edit how TechTypes are harvested.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose HarvestType you want to edit.</param>
        /// <param name="harvestType">The HarvestType for that TechType.</param>
        void ICraftDataHandler.SetHarvestType(TechType techType, HarvestType harvestType)
        {
            CraftDataPatcher.CustomHarvestTypeList[techType] = harvestType;
        }

        /// <summary>
        /// <para>Allows you to edit how much additional slices/seeds are given upon last knife hit.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose final cut bonus you want to edit.</param>
        /// <param name="bonus">The number of additional slices/seeds you'll receive on last cut.</param>
        void ICraftDataHandler.SetHarvestFinalCutBonus(TechType techType, int bonus)
        {
            CraftDataPatcher.CustomFinalCutBonusList[techType] = bonus;
        }

        /// <summary>
        /// <para>Allows you to edit item sizes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose item size you want to edit.</param>
        /// <param name="size">The item size for that TechType.</param>
        void ICraftDataHandler.SetItemSize(TechType techType, Vector2int size)
        {
            CraftDataPatcher.CustomItemSizes[techType] = size;
        }

        /// <summary>
        /// <para>Allows you to edit item sizes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose item size you want to edit.</param>
        /// <param name="x">The width of the item</param>
        /// <param name="y">The height of the item</param>
        void ICraftDataHandler.SetItemSize(TechType techType, int x, int y)
        {
            CraftDataPatcher.CustomItemSizes[techType] = new Vector2int(x, y);
        }

        /// <summary>
        /// <para>Allows you to edit crafting times for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose crafting time you want to edit.</param>
        /// <param name="time">The crafting time, in seconds, for that TechType.</param>
        void ICraftDataHandler.SetCraftingTime(TechType techType, float time)
        {
            CraftDataPatcher.CustomCraftingTimes[techType] = time;
        }

        /// <summary>
        /// <para>Allows you to edit the cooked creature list, i.e. associate the unedible TechType to the cooked TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="uncooked">The TechType whose cooked creature counterpart to edit.</param>
        /// <param name="cooked">The cooked creature counterpart for that TechType.</param>
        void ICraftDataHandler.SetCookedVariant(TechType uncooked, TechType cooked)
        {
            CraftDataPatcher.CustomCookedCreatureList[uncooked] = cooked;
        }

        /// <summary>
        /// <para>Allows you to edit inventory background colors for TechTypes.</para>
        /// </summary>
        /// <param name="techType">The TechType whose BackgroundType you want to edit.</param>
        /// <param name="backgroundColor">The background color for that TechType.</param>
        /// <seealso cref="CraftData.BackgroundType"/>
        void ICraftDataHandler.SetBackgroundType(TechType techType, CraftData.BackgroundType backgroundColor)
        {
            CraftDataPatcher.CustomBackgroundTypes[techType] = backgroundColor;
        }

        /// <summary>
        /// Allows you to add items to the buildable list.
        /// </summary>
        /// <param name="techType">The TechType which you want to add to the buildable list.</param>
        void ICraftDataHandler.AddBuildable(TechType techType)
        {
            CraftDataPatcher.CustomBuildables.Add(techType);
        }

#endif

        /// <summary>
        /// Allows you to add items to the game's internal grouping system.
        /// Required if you want to make buildable items show up in the Habitat Builder.
        /// </summary>
        /// <param name="group">The TechGroup you want to add your TechType to.</param>
        /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
        /// <param name="techType">The TechType you want to add.</param>
        void ICraftDataHandler.AddToGroup(TechGroup group, TechCategory category, TechType techType)
        {
            CraftDataPatcher.AddToCustomGroup(group, category, techType, TechType.None);
        }

        /// <summary>
        /// Allows you to add items to the game's internal grouping system.
        /// Required if you want to make buildable items show up in the Habitat Builder.
        /// </summary>
        /// <param name="group">The TechGroup you want to add your TechType to.</param>
        /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
        /// <param name="techType">The TechType you want to add.</param>
        /// <param name="after">Added TechType will be added after this TechType, for sorting purposes.</param>
        void ICraftDataHandler.AddToGroup(TechGroup group, TechCategory category, TechType techType, TechType after)
        {
            CraftDataPatcher.AddToCustomGroup(group, category, techType, after);
        }

        /// <summary>
        /// Allows you to remove an existing TechType from the game's internal group system.
        /// </summary>
        /// <param name="group">The TechGroup in which the TechType is located.</param>
        /// <param name="category">The TechCategory in which the TechType is located.</param>
        /// <param name="techType">The TechType which you want to remove.</param>
        void ICraftDataHandler.RemoveFromGroup(TechGroup group, TechCategory category, TechType techType)
        {
            CraftDataPatcher.RemoveFromCustomGroup(group, category, techType);
        }

        #endregion
    }
}
