using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Patchers;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for adding and editing crafted items.
/// </summary>
public static class CraftDataHandler
{
    /// <summary>
    /// <para>Allows you to add or edit RecipeData for TechTypes.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose RecipeData you want to edit.</param>
    /// <param name="recipeData">The RecipeData for that TechType.</param>
    /// <seealso cref="RecipeData"/>
    public static void SetRecipeData(TechType techType, RecipeData recipeData)
    {
        if (CraftDataPatcher.CustomRecipeData.TryGetValue(techType, out JsonValue jsonValue))
        {
            jsonValue[TechData.PropertyToID("techType")] = new JsonValue((int)techType);
            jsonValue[TechData.PropertyToID("craftAmount")] = new JsonValue(recipeData.craftAmount);
        }
        else
        {
            jsonValue = new JsonValue
            {
                { TechData.PropertyToID("techType"), new JsonValue((int)techType) },
                { TechData.PropertyToID("craftAmount"), new JsonValue(recipeData.craftAmount) }
            };

            CraftDataPatcher.CustomRecipeData.Add(techType, jsonValue);
        }

        if (recipeData.ingredientCount > 0)
        {
            SetIngredients(techType, recipeData.Ingredients);
        }
        if (recipeData.linkedItemCount > 0)
        {
            SetLinkedItems(techType, recipeData.LinkedItems);
        }
    }

    /// <summary>
    /// <para>Allows you to edit recipes for TechTypes.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose RecipeData you want to edit.</param>
    /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
    /// <seealso cref="Ingredient"/>
    public static void SetIngredients(TechType techType, ICollection<Ingredient> ingredients)
    {
        if (!CraftDataPatcher.CustomRecipeData.TryGetValue(techType, out JsonValue jsonValue))
        {
            jsonValue = new JsonValue();
            CraftDataPatcher.CustomRecipeData.Add(techType, jsonValue);
        }

        if (!jsonValue.Contains(TechData.PropertyToID("ingredients")))
        {
            jsonValue.Add(TechData.PropertyToID("ingredients"), new JsonValue(JsonValue.Type.Array));
        }
        else
        {
            jsonValue[TechData.PropertyToID("ingredients")] = new JsonValue(JsonValue.Type.Array);
        }

        JsonValue ingredientslist = jsonValue[TechData.PropertyToID("ingredients")];

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

        if (Player.main)
        {
            TechData.cachedIngredients[techType] = ingredients.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// <para>Allows you to edit Linked Items for TechTypes.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose RecipeData you want to edit.</param>
    /// <param name="linkedItems">The collection of Ingredients for that TechType.</param>
    /// <seealso cref="Ingredient"/>
    public static void SetLinkedItems(TechType techType, ICollection<TechType> linkedItems)
    {
        if (!CraftDataPatcher.CustomRecipeData.TryGetValue(techType, out JsonValue jsonValue))
        {
            CraftDataPatcher.CustomRecipeData.Add(techType, new JsonValue());
            jsonValue = CraftDataPatcher.CustomRecipeData[techType];
        }

        if (!jsonValue.Contains(TechData.PropertyToID("linkedItems")))
        {
            jsonValue.Add(TechData.PropertyToID("linkedItems"), new JsonValue(JsonValue.Type.Array));
        }
        else
        {
            jsonValue[TechData.PropertyToID("linkedItems")] = new JsonValue(JsonValue.Type.Array);
        }

        JsonValue linkedItemslist = jsonValue[TechData.PropertyToID("linkedItems")];

        int current = 0;

        foreach (TechType i in linkedItems)
        {
            linkedItemslist.Add(new JsonValue(current));
            linkedItemslist[current] = new JsonValue((int)i);
            current++;
        }
        
        if (Player.main)
        {
            TechData.cachedLinkedItems[techType] = linkedItems.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Safely accesses the crafting data from a Modded or Vanilla item.<para/>
    /// WARNING: This method is highly dependent on mod load order. 
    /// Make sure your mod is loading after the mod whose RecipeData you are trying to access.
    /// </summary>
    /// <param name="techType">The TechType whose RecipeData you want to access.</param>
    /// <returns>The RecipeData from the item if it exists; Otherwise, returns <c>null</c>.</returns>
    public static RecipeData GetRecipeData(TechType techType)
    {
        RecipeData moddedRecipeData = GetModdedRecipeData(techType);

        if (moddedRecipeData != null)
        {
            return moddedRecipeData;
        }

        if (!TechData.Contains(TechType.Knife))
        {
            TechData.Initialize();
        }

        if (TechData.TryGetValue(techType, out JsonValue techData))
        {
            return techData?.ConvertToRecipeData();
        }

        return null;
    }

    /// <summary>
    /// Converts the Games JsonValue data into Nautilus RecipeData.
    /// </summary>
    /// <param name="techData"></param>
    public static RecipeData ConvertToRecipeData(JsonValue techData)
    {
        return techData?.ConvertToRecipeData();
    }

    /// <summary>
    /// Safely accesses the crafting data from a Modded item.<para/>
    /// WARNING: This method is highly dependent on mod load order. 
    /// Make sure your mod is loading after the mod whose RecipeData you are trying to access.
    /// </summary>
    /// <param name="techType">The TechType whose RecipeData you want to access.</param>
    /// <returns>The RecipeData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
    public static RecipeData GetModdedRecipeData(TechType techType)
    {
        return CraftDataPatcher.CustomRecipeData.TryGetValue(techType, out JsonValue techData) ? techData.ConvertToRecipeData() : null;
    }

    /// <summary>
    /// <para>Allows you to edit EquipmentTypes for TechTypes.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose EqiupmentType you want to edit.</param>
    /// <param name="equipmentType">The EquipmentType for that TechType.</param>
    public static void SetEquipmentType(TechType techType, EquipmentType equipmentType)
    {
        AddJsonProperty(techType, "equipmentType", new JsonValue((int)equipmentType));
    }

    /// <summary>
    /// <para>Allows you to edit QuickSlotType for TechTypes. Can be used for existing TechTypes too.</para>
    /// <para>Careful: This has to be called after <see cref="SetRecipeData(TechType, RecipeData)"/>.</para>
    /// </summary>
    /// <param name="techType">The TechType whose QuickSlotType you want to edit.</param>
    /// <param name="slotType">The QuickSlotType for that TechType.</param>
    public static void SetQuickSlotType(TechType techType, QuickSlotType slotType)
    {
        AddJsonProperty(techType, "slotType", new JsonValue((int)slotType));
    }

    /// <summary>
    /// <para>Allows you to edit MaxCharge for TechTypes. Can be used for existing TechTypes too.</para>
    /// <para>Careful: This has to be called after <see cref="SetRecipeData(TechType, RecipeData)"/>.</para>
    /// </summary>
    /// <param name="techType">The TechType whose MaxCharge you want to edit.</param>
    /// <param name="maxCharge">The MaxCharge for that TechType.</param>
    public static void SetMaxCharge(TechType techType, double maxCharge)
    {
        AddJsonProperty(techType, "maxCharge", new JsonValue(maxCharge));
    }

    /// <summary>
    /// <para>Allows you to edit EnergyCost for TechTypes. Can be used for existing TechTypes too.</para>
    /// <para>Careful: This has to be called after <see cref="SetRecipeData(TechType, RecipeData)"/>.</para>
    /// </summary>
    /// <param name="techType">The TechType wose EnergyCost you want to edit</param>
    /// <param name="energyCost">The EnergyCost for that TechType.</param>
    public static void SetEnergyCost(TechType techType, double energyCost)
    {
        AddJsonProperty(techType, "energyCost", new JsonValue(energyCost));
    }

    /// <summary>
    /// <para>Allows you to edit harvest output, i.e. what TechType you get when you "harvest" a TechType.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose harvest output you want to edit.</param>
    /// <param name="harvestOutput">The harvest output for that TechType.</param>
    public static void SetHarvestOutput(TechType techType, TechType harvestOutput)
    {
        AddJsonProperty(techType, "harvestOutput", new JsonValue((int)harvestOutput));
    }

    /// <summary>
    /// <para>Allows you to edit how TechTypes are harvested.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose HarvestType you want to edit.</param>
    /// <param name="harvestType">The HarvestType for that TechType.</param>
    public static void SetHarvestType(TechType techType, HarvestType harvestType)
    {
        AddJsonProperty(techType, "harvestType", new JsonValue((int)harvestType));
    }

    /// <summary>
    /// <para>Allows you to edit how much additional slices/seeds are given upon last knife hit.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose final cut bonus you want to edit.</param>
    /// <param name="bonus">The number of additional slices/seeds you'll receive on last cut.</param>
    public static void SetHarvestFinalCutBonus(TechType techType, int bonus)
    {
        AddJsonProperty(techType, "harvestFinalCutBonus", new JsonValue(bonus));
    }

    /// <summary>
    /// <para>Allows you to edit item sizes for TechTypes.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose item size you want to edit.</param>
    /// <param name="size">The item size for that TechType.</param>
    public static void SetItemSize(TechType techType, Vector2int size)
    {
        SetItemSize(techType, size.x, size.y);
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
        JsonValue jsonValue = new()
        {
            {
                TechData.propertyX,
                new JsonValue(x)
            },
            {
                TechData.propertyY,
                new JsonValue(y)
            }
        };
        AddJsonProperty(techType, "itemSize", jsonValue);
    }

    /// <summary>
    /// <para>Allows you to edit crafting times for TechTypes.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="techType">The TechType whose crafting time you want to edit.</param>
    /// <param name="time">The crafting time, in seconds, for that TechType.</param>
    public static void SetCraftingTime(TechType techType, float time)
    {
        AddJsonProperty(techType, "craftTime", new JsonValue(time));
    }

    /// <summary>
    /// <para>Allows you to edit the cooked creature list, i.e. associate the unedible TechType to the cooked TechType.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="uncooked">The TechType whose cooked creature counterpart to edit.</param>
    /// <param name="cooked">The cooked creature counterpart for that TechType.</param>
    public static void SetCookedVariant(TechType uncooked, TechType cooked)
    {
        AddJsonProperty(uncooked, "processed", new JsonValue((int)cooked));
    }

    /// <summary>
    /// <para>Allows you to edit the Cold Resistance of a TechType.</para>
    /// <para>Can be used for existing TechTypes too.</para>
    /// </summary>
    /// <param name="uncooked">The TechType whose Cold Resistance to edit.</param>
    /// <param name="resistance">The Cold Resistance for that TechType.</param>
    public static void SetColdResistance(TechType uncooked, int resistance)
    {
        AddJsonProperty(uncooked, "coldResistance", new JsonValue((int)resistance));
    }

    /// <summary>
    /// <para>Allows you to edit inventory background colors for TechTypes.</para>
    /// </summary>
    /// <param name="techType">The TechType whose BackgroundType you want to edit.</param>
    /// <param name="backgroundColor">The background color for that TechType.</param>
    /// <seealso cref="CraftData.BackgroundType"/>
    public static void SetBackgroundType(TechType techType, CraftData.BackgroundType backgroundColor)
    {
        AddJsonProperty(techType, "backgroundType", new JsonValue((int)backgroundColor));
    }

    /// <summary>
    /// Allows you to add items to the buildable list.
    /// </summary>
    /// <param name="techType">The TechType which you want to add to the buildable list.</param>
    public static void AddBuildable(TechType techType)
    {
        AddJsonProperty(techType, "buildable", new JsonValue(true));
    }

    /// <summary>
    /// Sets the maximum charge.
    /// </summary>
    /// <param name="techType">The TechType whose MaxCharge you want to edit.</param>
    /// <param name="maxCharge">The maximum charge.</param>
    public static void SetMaxCharge(TechType techType, float maxCharge)
    {
        AddJsonProperty(techType, "maxCharge", new JsonValue((double)maxCharge));
    }

    /// <summary>
    /// Sets the energy cost.
    /// </summary>
    /// <param name="techType">The TechType whose EnergyCost you want to edit.</param>
    /// <param name="energyCost">The energy cost.</param>
    public static void SetEnergyCost(TechType techType, float energyCost)
    {
        AddJsonProperty(techType, "energyCost", new JsonValue((double)energyCost));
    }

#if SUBNAUTICA
    /// <summary>
    /// Sets the eating sound for the provided TechType.
    /// </summary>
    /// <param name="consumable">The item being consumed during <see cref="Survival.Eat(UnityEngine.GameObject)"/>.</param>
    /// <param name="soundPath">
    /// The sound path.
    /// <para>
    /// Valid values are
    /// - "event:/player/drink"
    /// - "event:/player/drink_stillsuit"
    /// - "event:/player/use_first_aid"
    /// - "event:/player/eat" (default)
    /// </para>
    /// </param>
    public static void SetEatingSound(TechType consumable, string soundPath)
    {
        AddJsonProperty(consumable, "soundUse", new JsonValue(soundPath));
    }
    /// <summary>
    /// Sets the pickup sound for the provided TechType.
    /// </summary>
    /// <param name="consumable">The item to add the new pickup sound to.</param>
    /// <param name="soundPath">
    /// The sound path. A list of all sound paths can be viewed
    /// <a href="https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-FMODEvents.txt">on this page</a>.
    /// <para>
    /// The default sound is "event:/loot/pickup_default".
    /// </para>
    /// </param>
    public static void SetPickupSound(TechType consumable, string soundPath)
    {
        AddJsonProperty(consumable, "soundPickup", new JsonValue(soundPath));
    }
    /// <summary>
    /// Sets the drop sound for the provided TechType.
    /// </summary>
    /// <param name="consumable">The item to add the new drop sound to.</param>
    /// <param name="soundPath">
    /// The sound path. A list of all sound paths can be viewed
    /// <a href="https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-FMODEvents.txt">on this page</a>.
    /// <para>
    /// The default sound is "event:/tools/pda/drop_item".
    /// </para>
    /// </param>
    public static void SetDropSound(TechType consumable, string soundPath)
    {
        AddJsonProperty(consumable, "soundDrop", new JsonValue(soundPath));
    }
#else
    /// <summary>
    /// Sets the type of the sound.
    /// </summary>
    /// <param name="techType">Type of the tech.</param>
    /// <param name="soundType">Type of the sound.</param>
    public static void SetSoundType(TechType techType, TechData.SoundType soundType)
    {
        AddJsonProperty(techType, "soundType", new JsonValue((int)soundType));
    }
#endif

    private static void AddJsonProperty(TechType techType, string key, JsonValue newValue)
    {
        if (CraftDataPatcher.CustomRecipeData.TryGetValue(techType, out JsonValue techData))
        {
            techData[TechData.PropertyToID(key)] = newValue;
        }
        else
        {
            CraftDataPatcher.CustomRecipeData[techType] = new JsonValue
            {
                {
                    TechData.PropertyToID("techType"),
                    new JsonValue((int)techType)
                },
                {
                    TechData.PropertyToID(key),
                    newValue
                }
            };
        }
    }
    

    /// <summary>
    /// Allows you to add items to the game's internal grouping system.
    /// Required if you want to make buildable items show up in the Habitat Builder or show in the Blueprints Tab of the PDA.
    /// </summary>
    /// <param name="group">The TechGroup you want to add your TechType to.</param>
    /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
    /// <param name="techType">The TechType you want to add.</param>
    public static void AddToGroup(TechGroup group, TechCategory category, TechType techType)
    {
        CraftDataPatcher.AddToGroup(group, category, techType, TechType.None, true);
    }

    /// <summary>
    /// Allows you to add items to the game's internal grouping system.
    /// Required if you want to make buildable items show up in the Habitat Builder or show in the Blueprints Tab of the PDA.
    /// </summary>
    /// <param name="group">The TechGroup you want to add your TechType to.</param>
    /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
    /// <param name="techType">The TechType you want to add.</param>
    /// <param name="target">The icon in the blueprints tab of the PDA will be added next to this item or at the end/beginning if not found.</param>
    /// <param name="after">Whether to append after (true) or insert before (false) the target, for sorting purposes.</param>
    public static void AddToGroup(TechGroup group, TechCategory category, TechType techType, TechType target = TechType.None, bool after = true)
    {
        CraftDataPatcher.AddToGroup(group, category, techType, target, after);
    }

    /// <summary>
    /// Allows you to remove an existing TechType from the game's internal group system.
    /// </summary>
    /// <param name="group">The TechGroup in which the TechType is located.</param>
    /// <param name="category">The TechCategory in which the TechType is located.</param>
    /// <param name="techType">The TechType which you want to remove.</param>
    public static void RemoveFromGroup(TechGroup group, TechCategory category, TechType techType)
    {
        CraftDataPatcher.RemoveFromGroup(group, category, techType);
    }
}