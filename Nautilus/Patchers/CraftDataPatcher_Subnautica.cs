using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Utility;

#if SUBNAUTICA
namespace Nautilus.Patchers;

internal partial class CraftDataPatcher
{
    internal static IDictionary<TechType, ITechData> CustomRecipeData = new SelfCheckingDictionary<TechType, ITechData>("CustomRecipeData", AsStringFunction);
    internal static IDictionary<TechType, TechType> CustomHarvestOutputList = new SelfCheckingDictionary<TechType, TechType>("CustomHarvestOutputList", AsStringFunction);
    internal static IDictionary<TechType, HarvestType> CustomHarvestTypeList = new SelfCheckingDictionary<TechType, HarvestType>("CustomHarvestTypeList", AsStringFunction);
    internal static IDictionary<TechType, int> CustomFinalCutBonusList = new SelfCheckingDictionary<TechType, int>("CustomFinalCutBonusList", TechTypeExtensions.sTechTypeComparer, AsStringFunction);
    internal static IDictionary<TechType, Vector2int> CustomItemSizes = new SelfCheckingDictionary<TechType, Vector2int>("CustomItemSizes", AsStringFunction);
    internal static IDictionary<TechType, EquipmentType> CustomEquipmentTypes = new SelfCheckingDictionary<TechType, EquipmentType>("CustomEquipmentTypes", AsStringFunction);
    internal static IDictionary<TechType, QuickSlotType> CustomSlotTypes = new SelfCheckingDictionary<TechType, QuickSlotType>("CustomSlotTypes", AsStringFunction);
    internal static IDictionary<TechType, float> CustomCraftingTimes = new SelfCheckingDictionary<TechType, float>("CustomCraftingTimes", AsStringFunction);
    internal static IDictionary<TechType, TechType> CustomCookedCreatureList = new SelfCheckingDictionary<TechType, TechType>("CustomCookedCreatureList", AsStringFunction);
    internal static IDictionary<TechType, CraftData.BackgroundType> CustomBackgroundTypes = new SelfCheckingDictionary<TechType, CraftData.BackgroundType>("CustomBackgroundTypes", TechTypeExtensions.sTechTypeComparer, AsStringFunction);
    internal static IDictionary<TechType, string> CustomEatingSounds = new SelfCheckingDictionary<TechType, string>("CustomEatingSounds", AsStringFunction);
    internal static List<TechType> CustomBuildables = new List<TechType>();

    internal static void AddToCustomRecipeData(TechType techType, ITechData techData)
    {
        CustomRecipeData.Add(techType, techData);
    }

    private static void PatchForSubnautica(Harmony harmony)
    {
        harmony.PatchAll(typeof(CraftDataPatcher));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetHarvestOutputData))]
    private static void GetHarvestOutputPrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomHarvestOutputList, CraftData.harvestOutputList);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetHarvestTypeFromTech))]
    private static void GetHarvestTypePrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomHarvestTypeList, CraftData.harvestTypeList);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetHarvestFinalCutBonus))]
    private static void GetFinalCutBonusPrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomFinalCutBonusList, CraftData.harvestFinalCutBonusList);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetItemSize))]
    private static void GetItemSizePrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomItemSizes, CraftData.itemSizes);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetEquipmentType))]
    private static void GetEquipmentTypePrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomEquipmentTypes, CraftData.equipmentTypes);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetQuickSlotType))]
    private static void GetSlotTypePrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomSlotTypes, CraftData.slotTypes);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetCraftTime))]
    private static void GetCraftTimePrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomCraftingTimes, CraftData.craftingTimes);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetCookedData))]
    private static void GetCookedCreaturePrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomCookedCreatureList, CraftData.cookedCreatureList);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetBackgroundType))]
    private static void GetBackgroundTypesPrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomBackgroundTypes, CraftData.backgroundTypes);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetUseEatSound))]
    private static void GetUseEatSoundPrefix(TechType techType)
    {
        DictionaryPrefix(techType, CustomEatingSounds, CraftData.useEatSound);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetAllItemActions))]
    private static void GetAllItemActionsPostfix(InventoryItem item, ref ItemAction __result)
    {
        if (item != null && item.item != null && SurvivalPatcher.InventoryUseables.Contains(item.item.GetTechType()))
            __result |= ItemAction.Use;
    }

    private static void DictionaryPrefix<T>(TechType techType, IDictionary<TechType, T> customCollection, IDictionary<TechType, T> craftDataCollection)
    {
        if (customCollection.TryGetValue(techType, out T custom))
        {
            if (craftDataCollection.TryGetValue(techType, out T gameVal))
            {
                if (!custom.Equals(gameVal))
                    craftDataCollection[techType] = custom;
            }
            else
            {
                PatchUtils.PatchDictionary(craftDataCollection, customCollection);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.IsBuildableTech))]
    private static void GetBuildablePrefix(TechType recipe)
    {
        if (CustomBuildables.Contains(recipe) && !CraftData.buildables.Contains(recipe))
            PatchUtils.PatchList(CraftData.buildables, CustomBuildables);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.Get))]
    private static void NeedsPatchingCheckPrefix(TechType techType)
    {
        bool techExists = CraftData.techData.TryGetValue(techType, out CraftData.TechData techData);

        bool sameData = false;
        if (techExists && CustomRecipeData.TryGetValue(techType, out ITechData customTechData))
        {
            sameData = customTechData.craftAmount == techData.craftAmount &&
                customTechData.ingredientCount == techData.ingredientCount &&
                customTechData.linkedItemCount == techData.linkedItemCount;

            if (sameData)
                for (int i = 0; i < customTechData.ingredientCount; i++)
                {
                    if (customTechData.GetIngredient(i).techType != techData.GetIngredient(i).techType)
                    {
                        sameData = false;
                        break;
                    }
                    if (customTechData.GetIngredient(i).amount != techData.GetIngredient(i).amount)
                    {
                        sameData = false;
                        break;
                    }
                }

            if (sameData)
                for (int i = 0; i < customTechData.linkedItemCount; i++)
                {
                    if (customTechData.GetLinkedItem(i) != techData.GetLinkedItem(i))
                    {
                        sameData = false;
                        break;
                    }
                }
        }
        if (!techExists || !sameData)
        {
            PatchCustomTechData();
        }
    }


    private static void PatchCustomTechData()
    {
        short added = 0;
        short replaced = 0;
        foreach (TechType techType in CustomRecipeData.Keys)
        {
            bool techExists = CraftData.techData.TryGetValue(techType, out CraftData.TechData techData);
            ITechData customTechData = CustomRecipeData[techType];
            bool sameData = false;

            if (techExists && customTechData != null)
            {

                sameData = customTechData.craftAmount == techData.craftAmount &&
                    customTechData.ingredientCount == techData.ingredientCount &&
                    customTechData.linkedItemCount == techData.linkedItemCount;

                if (sameData)
                    for (int i = 0; i < customTechData.ingredientCount; i++)
                    {
                        if (customTechData.GetIngredient(i).techType != techData.GetIngredient(i).techType)
                        {
                            sameData = false;
                            break;
                        }
                        if (customTechData.GetIngredient(i).amount != techData.GetIngredient(i).amount)
                        {
                            sameData = false;
                            break;
                        }
                    }

                if (sameData)
                    for (int i = 0; i < customTechData.linkedItemCount; i++)
                    {
                        if (customTechData.GetLinkedItem(i) != techData.GetLinkedItem(i))
                        {
                            sameData = false;
                            break;
                        }
                    }
            }

            if (!techExists || !sameData)
            {
                var techDataInstance = new CraftData.TechData
                {
                    _techType = techType,
                    _craftAmount = customTechData?.craftAmount ?? 0
                };

                var ingredientsList = new CraftData.Ingredients();

                if (customTechData?.ingredientCount > 0)
                {
                    for (int i = 0; i < customTechData.ingredientCount; i++)
                    {
                        IIngredient customIngredient = customTechData.GetIngredient(i);

                        var ingredient = new CraftData.Ingredient(customIngredient.techType, customIngredient.amount);
                        ingredientsList.Add(customIngredient.techType, customIngredient.amount);
                    }
                    techDataInstance._ingredients = ingredientsList;
                }

                if (customTechData?.linkedItemCount > 0)
                {
                    var linkedItems = new List<TechType>();
                    for (int l = 0; l < customTechData.linkedItemCount; l++)
                    {
                        linkedItems.Add(customTechData.GetLinkedItem(l));
                    }
                    techDataInstance._linkedItems = linkedItems;
                }

                if (techExists)
                {
                    CraftData.techData.Remove(techType);
                    InternalLogger.Log($"{techType} TechType already existed in the CraftData.techData dictionary. Original value was replaced.", LogLevel.Warning);
                    replaced++;
                }
                else
                {
                    added++;
                }
                CraftData.techData.Add(techType, techDataInstance);
            }
        }

        if (added > 0)
            InternalLogger.Log($"Added {added} new entries to the CraftData.techData dictionary.", LogLevel.Debug);

        if (replaced > 0)
            InternalLogger.Log($"Replaced {replaced} existing entries to the CraftData.techData dictionary.", LogLevel.Debug);
    }

    
}
#endif