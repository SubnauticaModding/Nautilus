namespace Nautilus.Extensions;

using Nautilus.Crafting;
using Nautilus.Utility;
using System;
using System.Collections.Generic;

/// <summary>
/// Contains extensions that are specific to the <see cref="RecipeData"/> class.
/// </summary>
public static class RecipeExtensions
{
    /// <summary>
    /// Converts the Games JsonValue data into Nautilus RecipeData.
    /// </summary>
    /// <param name="techData"></param>
    public static RecipeData ConvertToRecipeData(this JsonValue techData)
    {
        try
        {
            RecipeData currentRecipeData = new()
            {
                craftAmount = techData.GetInt(TechData.propertyCraftAmount, out int craftAmount, 0) ? craftAmount : TechData.defaultCraftAmount
            };

            if (techData.GetArray(TechData.propertyIngredients, out JsonValue jsonValue, null))
            {
                for (int i = 0; i < jsonValue.Count; i++)
                {
                    JsonValue jsonValue2 = jsonValue[i];
                    TechType techType = (TechType) jsonValue2.GetInt(TechData.propertyTechType, 0);
                    int int2 = jsonValue2.GetInt(TechData.propertyAmount, 0);
                    if (techType != TechType.None && int2 > 0)
                    {
                        if (currentRecipeData.Ingredients == null)
                        {
                            currentRecipeData.Ingredients = new List<Ingredient>();
                        }
                        currentRecipeData.Ingredients.Add(new Ingredient(techType, int2));
                    }
                }
            }

            if (techData.GetArray(TechData.propertyLinkedItems, out JsonValue jsonValue3, null))
            {
                for (int j = 0; j < jsonValue3.Count; j++)
                {
                    TechType techType1 = (TechType) jsonValue3[j].GetInt(0);
                    if (currentRecipeData.LinkedItems == null)
                    {
                        currentRecipeData.LinkedItems = new List<TechType>();
                    }
                    currentRecipeData.LinkedItems.Add(techType1);
                }
            }
            return currentRecipeData;
        }
        catch (Exception e)
        {
            InternalLogger.Error($"Error converting TechData to RecipeData: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Converts the Nautilus RecipeData into the Games JsonValue data.
    /// </summary>
    /// <param name="recipeData"></param>
    /// <param name="techType"></param>
    /// <returns><see cref="JsonValue"/> or null</returns>
    public static JsonValue ConvertToJsonValue(this RecipeData recipeData, TechType techType)
    {
        try
        {
            JsonValue jsonValue = new JsonValue
            {
                { TechData.PropertyToID("techType"), new JsonValue((int)techType) },
                { TechData.PropertyToID("craftAmount"), new JsonValue(recipeData.craftAmount) }
            };

            if (recipeData.ingredientCount > 0)
            {
                jsonValue[TechData.PropertyToID("ingredients")] = new JsonValue(JsonValue.Type.Array);
                JsonValue ingredientslist = jsonValue[TechData.PropertyToID("ingredients")];

                int amount = TechData.PropertyToID("amount");
                int tech = TechData.PropertyToID("techType");
                int current = 0;

                foreach (Ingredient i in recipeData.Ingredients)
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

            if (recipeData.linkedItemCount > 0)
            {
                jsonValue[TechData.PropertyToID("linkedItems")] = new JsonValue(JsonValue.Type.Array);
                JsonValue linkedItems = jsonValue[TechData.PropertyToID("linkedItems")];

                int current = 0;

                foreach (TechType techType1 in recipeData.LinkedItems)
                {
                    linkedItems.Add(new JsonValue(current));
                    linkedItems[current] = new JsonValue((int)techType1);
                    current++;
                }
            }

            return jsonValue;
        }
        catch (Exception e)
        {
            InternalLogger.Error($"Error converting RecipeData to JsonValue: {e.Message}");
            return null;
        }
    }
}
