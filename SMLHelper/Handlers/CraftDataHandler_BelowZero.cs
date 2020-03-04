#if BELOWZERO
namespace SMLHelper.V2.Handlers
{
    using System.Collections.Generic;
    using Crafting;
    using Interfaces;
    using Patchers;

    /// <summary>
    /// A handler class for adding and editing crafted items.
    /// </summary>
    public partial class CraftDataHandler : ICraftDataHandler
    {

        #region BelowZero specific Static Methods

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
        /// <param name="jsonValue">The JsonValue for that TechType.</param>
        /// <seealso cref="TechData.defaults"/>
        public static void SetTechData(TechType techType, JsonValue jsonValue)
        {
            Main.SetTechData(techType, jsonValue);
        }

        /// <summary>
        /// <para>Allows you to add ingredients for a TechType crafting recipe.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose ingredient list you want to edit.</param>
        /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        public static void AddIngredients(TechType techType, ICollection<Ingredient> ingredients)
        {
            Main.AddIngredients(techType, ingredients);
        }

        /// <summary>
        /// <para>Allows you to add linked items for a TechType crafting recipe.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose ingredient list you want to edit.</param>
        /// <param name="linkedItems">The collection of linked items for that TechType</param>
        public static void AddLinkedItems(TechType techType, ICollection<TechType> linkedItems)
        {
            Main.AddLinkedItems(techType, linkedItems);
        }

        /// <summary>
        /// Safely accesses the crafting data from a modded or vanilla item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The JsonValue from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        public static RecipeData GetRecipeData(TechType techType)
        {
            return Main.GetRecipeData(techType);
        }

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The JsonValue from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        public static RecipeData GetModdedRecipeData(TechType techType)
        {
            return Main.GetModdedRecipeData(techType);
        }

        #endregion

        #region BelowZero specific implementations

        /// <summary>
        /// <para>Allows you to add or edit RecipeData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="recipeData">The TechData for that TechType.</param>
        /// <seealso cref="RecipeData"/>
        void ICraftDataHandler.SetTechData(TechType techType, RecipeData recipeData)
        {
            JsonValue currentTechType = new JsonValue
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
        /// <param name="jsonValue">The TechData for that TechType.</param>
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
            if (!CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType] = new JsonValue();

            CraftDataPatcher.CustomTechData[techType].Add(TechData.PropertyToID("ingredients"), new JsonValue(JsonValue.Type.Array));
            JsonValue ingredientslist = CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("ingredients")];
            int amount = TechData.PropertyToID("amount");
            int tech = TechData.PropertyToID("techType");
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
        /// <param name="linkedItems">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        void ICraftDataHandler.AddLinkedItems(TechType techType, ICollection<TechType> linkedItems)
        {
            if (!CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType] = new JsonValue();

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
        /// Safely accesses the crafting data from a Modded or Vanilla item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The ITechData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        RecipeData ICraftDataHandler.GetRecipeData(TechType techType)
        {
            if (CraftDataPatcher.CustomTechData.TryGetValue(techType, out JsonValue moddedTechData))
            {
                RecipeData currentRecipeData = new RecipeData();

                if (moddedTechData.TryGetValue(TechData.propertyCraftAmount, out JsonValue craftAmount))
                {
                    currentRecipeData.craftAmount = craftAmount.GetInt();
                    if (moddedTechData.GetArray(TechData.propertyIngredients, out JsonValue jsonValue, null))
                    {
                        for (int i = 0; i < jsonValue.Count; i++)
                        {
                            JsonValue jsonValue2 = jsonValue[i];
                            TechType @int = (TechType)jsonValue2.GetInt(TechData.propertyTechType, 0);
                            int int2 = jsonValue2.GetInt(TechData.propertyAmount, 0);
                            if (@int != TechType.None && int2 > 0)
                            {
                                if (currentRecipeData.Ingredients == null)
                                {
                                    currentRecipeData.Ingredients = new List<Ingredient>();
                                }
                                currentRecipeData.Ingredients.Add(new Ingredient(@int, int2));
                            }
                        }

                        if (moddedTechData.GetArray(TechData.propertyLinkedItems, out JsonValue jsonValue3, null))
                        {
                            for (int j = 0; j < jsonValue3.Count; j++)
                            {
                                TechType int3 = (TechType)jsonValue3[j].GetInt(0);
                                if (currentRecipeData.LinkedItems == null)
                                {
                                    currentRecipeData.LinkedItems = new List<TechType>();
                                }
                                currentRecipeData.LinkedItems.Add(int3);
                            }
                        }
                        return currentRecipeData;
                    }
                }
            }

            if (!TechData.Contains(TechType.Knife))
            {
                TechData.Initialize();
            }

            if (TechData.TryGetValue(techType, out JsonValue techData))
            {
                RecipeData currentRecipeData = new RecipeData();

                if (techData.TryGetValue(TechData.propertyCraftAmount, out JsonValue craftAmount))
                {
                    currentRecipeData.craftAmount = craftAmount.GetInt();

                    if (techData.GetArray(TechData.propertyIngredients, out JsonValue jsonValue, null))
                    {
                        for (int i = 0; i < jsonValue.Count; i++)
                        {
                            JsonValue jsonValue2 = jsonValue[i];
                            TechType @int = (TechType)jsonValue2.GetInt(TechData.propertyTechType, 0);
                            int int2 = jsonValue2.GetInt(TechData.propertyAmount, 0);
                            if (@int != TechType.None && int2 > 0)
                            {
                                if (currentRecipeData.Ingredients == null)
                                {
                                    currentRecipeData.Ingredients = new List<Ingredient>();
                                }
                                currentRecipeData.Ingredients.Add(new Ingredient(@int, int2));
                            }
                        }
                        if (techData.GetArray(TechData.propertyLinkedItems, out JsonValue jsonValue3, null))
                        {
                            for (int j = 0; j < jsonValue3.Count; j++)
                            {
                                TechType int3 = (TechType)jsonValue3[j].GetInt(0);
                                if (currentRecipeData.LinkedItems == null)
                                {
                                    currentRecipeData.LinkedItems = new List<TechType>();
                                }
                                currentRecipeData.LinkedItems.Add(int3);
                            }
                        }
                        return currentRecipeData;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Safely accesses the crafting data from a Modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The RecipeData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        RecipeData ICraftDataHandler.GetModdedRecipeData(TechType techType)
        {
            if (CraftDataPatcher.CustomTechData.TryGetValue(techType, out JsonValue moddedTechData))
            {
                RecipeData currentRecipeData = new RecipeData();

                if (moddedTechData.TryGetValue(TechData.propertyCraftAmount, out JsonValue craftAmount))
                {
                    currentRecipeData.craftAmount = craftAmount.GetInt();
                    if (moddedTechData.GetArray(TechData.propertyIngredients, out JsonValue jsonValue, null))
                    {
                        for (int i = 0; i < jsonValue.Count; i++)
                        {
                            JsonValue jsonValue2 = jsonValue[i];
                            TechType @int = (TechType)jsonValue2.GetInt(TechData.propertyTechType, 0);
                            int int2 = jsonValue2.GetInt(TechData.propertyAmount, 0);
                            if (@int != TechType.None && int2 > 0)
                            {
                                if (currentRecipeData.Ingredients == null)
                                {
                                    currentRecipeData.Ingredients = new List<Ingredient>();
                                }
                                currentRecipeData.Ingredients.Add(new Ingredient(@int, int2));
                            }
                        }

                        if (moddedTechData.GetArray(TechData.propertyLinkedItems, out JsonValue jsonValue3, null))
                        {
                            for (int j = 0; j < jsonValue3.Count; j++)
                            {
                                TechType int3 = (TechType)jsonValue3[j].GetInt(0);
                                if (currentRecipeData.LinkedItems == null)
                                {
                                    currentRecipeData.LinkedItems = new List<TechType>();
                                }
                                currentRecipeData.LinkedItems.Add(int3);
                            }
                        }
                        return currentRecipeData;
                    }
                }
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
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("equipmentType")] = new JsonValue((int)equipmentType);
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue
                {
                    {
                        TechData.PropertyToID("equipmentType") ,
                        new JsonValue((int)equipmentType)
                    }
                };
        }

        /// <summary>
        /// <para>Allows you to edit QuickSlotType for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose QuickSlotType you want to edit.</param>
        /// <param name="slotType">The QuickSlotType for that TechType.</param>
        void ICraftDataHandler.SetQuickSlotType(TechType techType, QuickSlotType slotType)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("slotType")] = new JsonValue((int)slotType);
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("slotType") ,
                        new JsonValue((int)slotType)
                    }
                };
        }

        /// <summary>
        /// <para>Allows you to edit harvest output, i.e. what TechType you get when you "harvest" a TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose harvest output you want to edit.</param>
        /// <param name="harvestOutput">The harvest output for that TechType.</param>
        void ICraftDataHandler.SetHarvestOutput(TechType techType, TechType harvestOutput)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("harvestOutput")] = new JsonValue((int)harvestOutput);
            else
            CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                {
                        TechData.PropertyToID("harvestOutput") ,
                        new JsonValue((int)harvestOutput)
                }
            };
        }

        /// <summary>
        /// <para>Allows you to edit how TechTypes are harvested.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose HarvestType you want to edit.</param>
        /// <param name="harvestType">The HarvestType for that TechType.</param>
        void ICraftDataHandler.SetHarvestType(TechType techType, HarvestType harvestType)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("harvestType")] = new JsonValue((int)harvestType);
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("harvestType") ,
                        new JsonValue((int)harvestType)
                    }
                };
        }

        /// <summary>
        /// <para>Allows you to edit how much additional slices/seeds are given upon last knife hit.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose final cut bonus you want to edit.</param>
        /// <param name="bonus">The number of additional slices/seeds you'll receive on last cut.</param>
        void ICraftDataHandler.SetHarvestFinalCutBonus(TechType techType, int bonus)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("harvestFinalCutBonus")] = new JsonValue(bonus);
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("harvestFinalCutBonus") ,
                        new JsonValue((int)bonus)
                    }
                };

        }

        /// <summary>
        /// <para>Allows you to edit item sizes for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose item size you want to edit.</param>
        /// <param name="size">The item size for that TechType.</param>
        void ICraftDataHandler.SetItemSize(TechType techType, Vector2int size)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
            {
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("x")] = new JsonValue(size.x);
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("y")] = new JsonValue(size.y);
            }
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("x") ,
                        new JsonValue((int)size.x)
                    },
                    {
                        TechData.PropertyToID("y") ,
                        new JsonValue((int)size.y)
                    }
                };

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
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
            {
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("x")] = new JsonValue(x);
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("y")] = new JsonValue(y);
            }
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("x") ,
                        new JsonValue((int)x)
                    },
                    {
                        TechData.PropertyToID("y") ,
                        new JsonValue((int)y)
                    }
                };
        }

        /// <summary>
        /// <para>Allows you to edit crafting times for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose crafting time you want to edit.</param>
        /// <param name="time">The crafting time, in seconds, for that TechType.</param>
        void ICraftDataHandler.SetCraftingTime(TechType techType, float time)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("craftTime")] = new JsonValue(time);
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("craftTime") ,
                        new JsonValue((int)time)
                    }
                };
        }

        /// <summary>
        /// <para>Allows you to edit the cooked creature list, i.e. associate the unedible TechType to the cooked TechType.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="uncooked">The TechType whose cooked creature counterpart to edit.</param>
        /// <param name="cooked">The cooked creature counterpart for that TechType.</param>
        void ICraftDataHandler.SetCookedVariant(TechType uncooked, TechType cooked)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(uncooked))
                CraftDataPatcher.CustomTechData[uncooked][TechData.PropertyToID("processed")] = new JsonValue((int)cooked);
            else
                CraftDataPatcher.CustomTechData[uncooked] = new JsonValue {
                    {
                        TechData.PropertyToID("processed") ,
                        new JsonValue((int)cooked)
                    }
                };
        }

        /// <summary>
        /// <para>Allows you to edit inventory background colors for TechTypes.</para>
        /// </summary>
        /// <param name="techType">The TechType whose BackgroundType you want to edit.</param>
        /// <param name="backgroundColor">The background color for that TechType.</param>
        /// <seealso cref="CraftData.BackgroundType"/>
        void ICraftDataHandler.SetBackgroundType(TechType techType, CraftData.BackgroundType backgroundColor)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("backgroundType")] = new JsonValue((int)backgroundColor);
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("backgroundType") ,
                        new JsonValue((int)backgroundColor)
                    }
                };
        }

        /// <summary>
        /// Allows you to add items to the buildable list.
        /// </summary>
        /// <param name="techType">The TechType which you want to add to the buildable list.</param>
        void ICraftDataHandler.AddBuildable(TechType techType)
        {
            if (CraftDataPatcher.CustomTechData.ContainsKey(techType))
                CraftDataPatcher.CustomTechData[techType][TechData.PropertyToID("buildable")] = new JsonValue(true);
            else
                CraftDataPatcher.CustomTechData[techType] = new JsonValue {
                    {
                        TechData.PropertyToID("buildable") ,
                        new JsonValue(true)
                    }
                };
        }

        #endregion
    }
}
#endif
