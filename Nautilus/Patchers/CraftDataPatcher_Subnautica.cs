using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Extensions;
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
    internal static IDictionary<TechType, float> CustomMaxCharges = new SelfCheckingDictionary<TechType, float>("CustomMaxCharges", AsStringFunction);
    internal static IDictionary<TechType, float> CustomEnergyCost = new SelfCheckingDictionary<TechType, float>("CustomEnergyCosts", AsStringFunction);
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
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetQuickSlotMaxCharge))]
    private static void GetSlotMaxCharge(TechType techType)
    {
        DictionaryPrefix(techType, CustomMaxCharges, CraftData.maxCharges);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftData), nameof(CraftData.GetEnergyCost))]
    private static void GetEnergyCost(TechType techType)
    {
        DictionaryPrefix(techType, CustomEnergyCost, CraftData.energyCost);
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

    [HarmonyPatch(typeof(CraftData), nameof(CraftData.Get)), HarmonyPrefix, HarmonyPriority(Priority.First)]
    private static void GetRecipePrefix(TechType techType)
    {
        if (CustomRecipeData.TryGetValue(techType, out var customTechData) && (!CraftData.techData.TryGetValue(techType, out var current) || !customTechData.SameAs(current)))
        {
            CraftData.techData[techType] = customTechData.ConvertToTechData(techType);
        }
    }
}
#endif