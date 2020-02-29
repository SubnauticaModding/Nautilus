#if SUBNAUTICA
namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Handlers;
    using System;
    using System.Collections.Generic;

    internal partial class CraftDataPatcher
    {
        internal static IDictionary<TechType, ITechData> CustomTechData = new SelfCheckingDictionary<TechType, ITechData>("CustomTechData", AsStringFunction);
        internal static IDictionary<TechType, TechType> CustomHarvestOutputList = new SelfCheckingDictionary<TechType, TechType>("CustomHarvestOutputList", AsStringFunction);
        internal static int HarvestOutputAltered = 0;
        internal static IDictionary<TechType, HarvestType> CustomHarvestTypeList = new SelfCheckingDictionary<TechType, HarvestType>("CustomHarvestTypeList", AsStringFunction);
        internal static int HarvestTypeAltered = 0;
        internal static IDictionary<TechType, int> CustomFinalCutBonusList = new SelfCheckingDictionary<TechType, int>("CustomFinalCutBonusList", TechTypeExtensions.sTechTypeComparer, AsStringFunction);
        internal static int FinalCutBonusAltered = 0;
        internal static IDictionary<TechType, Vector2int> CustomItemSizes = new SelfCheckingDictionary<TechType, Vector2int>("CustomItemSizes", AsStringFunction);
        internal static int ItemSizesAltered = 0;
        internal static IDictionary<TechType, EquipmentType> CustomEquipmentTypes = new SelfCheckingDictionary<TechType, EquipmentType>("CustomEquipmentTypes", AsStringFunction);
        internal static int EquipmentTypesAltered = 0;
        internal static IDictionary<TechType, QuickSlotType> CustomSlotTypes = new SelfCheckingDictionary<TechType, QuickSlotType>("CustomSlotTypes", AsStringFunction);
        internal static int SlotTypesAltered = 0;
        internal static IDictionary<TechType, float> CustomCraftingTimes = new SelfCheckingDictionary<TechType, float>("CustomCraftingTimes", AsStringFunction);
        internal static int CraftingTimesAltered = 0;
        internal static IDictionary<TechType, TechType> CustomCookedCreatureList = new SelfCheckingDictionary<TechType, TechType>("CustomCookedCreatureList", AsStringFunction);
        internal static int CookedCreatureAltered = 0;
        internal static IDictionary<TechType, CraftData.BackgroundType> CustomBackgroundTypes = new SelfCheckingDictionary<TechType, CraftData.BackgroundType>("CustomBackgroundTypes", TechTypeExtensions.sTechTypeComparer, AsStringFunction);
        internal static int BackgroundTypesAltered = 0;
        internal static List<TechType> CustomBuildables = new List<TechType>();
        internal static int BuildablesAltered = 0;
        internal static int PrefabAltered = 0;

        internal static void AddToCustomTechData(TechType techType, ITechData techData)
        {
            CustomTechData.Add(techType, techData);
        }

        private static void PatchForSubnautica(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache)),
               postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(PatchModPrefabs))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.Get)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(NeedsPatchingCheckPrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetHarvestOutputData)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetHarvestOutputPrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetHarvestTypeFromTech)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetHarvestTypePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetHarvestFinalCutBonus)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetFinalCutBonusPrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetItemSize)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetItemSizePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetEquipmentType)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetEquipmentTypePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetQuickSlotType)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetSlotTypePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetCraftTime)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetCraftTimePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetCookedData)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetCookedCreaturePrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetBackgroundType)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetBackgroundTypesPrefix))));

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.IsBuildableTech)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetBuildablePrefix))));

        }

        private static void PatchModPrefabs()
        {
            Dictionary<TechType, string> techMapping = CraftData.techMapping;
            Dictionary<string, TechType> entClassTechTable = CraftData.entClassTechTable;
            foreach (ModPrefab prefab in ModPrefab.Prefabs)
            {
                techMapping[prefab.TechType] = prefab.ClassID;
                entClassTechTable[prefab.ClassID] = prefab.TechType;
            }
        }

        private static void GetHarvestOutputPrefix(TechType techType)
        {
            if (CraftData.harvestOutputList.ContainsKey(techType) && CustomHarvestOutputList.ContainsKey(techType))
            {
                if(CraftData.harvestOutputList[techType] != CustomHarvestOutputList[techType])
                    CraftData.harvestOutputList[techType] = CustomHarvestOutputList[techType];
            }
            else if (CustomHarvestOutputList.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.harvestOutputList, CustomHarvestOutputList);
            }
        }

        private static void GetHarvestTypePrefix(TechType techType)
        {
            if (CraftData.harvestTypeList.ContainsKey(techType) && CustomHarvestTypeList.ContainsKey(techType))
            {
                if (CraftData.harvestTypeList[techType] != CustomHarvestTypeList[techType])
                    CraftData.harvestTypeList[techType] = CustomHarvestTypeList[techType];
            }
            else if (CustomHarvestTypeList.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.harvestTypeList, CustomHarvestTypeList);
            }
        }

        private static void GetFinalCutBonusPrefix(TechType techType)
        {
            if (CraftData.harvestFinalCutBonusList.ContainsKey(techType) && CustomFinalCutBonusList.ContainsKey(techType))
            {
                if (CraftData.harvestFinalCutBonusList[techType] != CustomFinalCutBonusList[techType])
                    CraftData.harvestFinalCutBonusList[techType] = CustomFinalCutBonusList[techType];
            }
            else if (CustomFinalCutBonusList.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.harvestFinalCutBonusList, CustomFinalCutBonusList);
            }
        }

        private static void GetItemSizePrefix(TechType techType)
        {
            if (CraftData.itemSizes.ContainsKey(techType) && CustomItemSizes.ContainsKey(techType))
            {
                if ((CraftData.itemSizes[techType].x != CustomItemSizes[techType].x || CraftData.itemSizes[techType].y != CustomItemSizes[techType].y))
                    CraftData.itemSizes[techType] = CustomItemSizes[techType];
            }
            else if(CustomItemSizes.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.itemSizes, CustomItemSizes);
            }

        }

        private static void GetEquipmentTypePrefix(TechType techType)
        {
            if (CraftData.equipmentTypes.ContainsKey(techType) && CustomEquipmentTypes.ContainsKey(techType))
            {
                if (CraftData.equipmentTypes[techType] != CustomEquipmentTypes[techType])
                    CraftData.equipmentTypes[techType] = CustomEquipmentTypes[techType];
            }
            else if (CustomEquipmentTypes.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.equipmentTypes, CustomEquipmentTypes);
            }
        }

        private static void GetSlotTypePrefix(TechType techType)
        {
            if (CraftData.slotTypes.ContainsKey(techType) && CustomSlotTypes.ContainsKey(techType))
            {
                if (CraftData.slotTypes[techType] != CustomSlotTypes[techType])
                    CraftData.slotTypes[techType] = CustomSlotTypes[techType];
            }
            else if (CustomSlotTypes.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.slotTypes, CustomSlotTypes);
            }
        }

        private static void GetCraftTimePrefix(TechType techType)
        {
            if (CraftData.craftingTimes.ContainsKey(techType) && CustomCraftingTimes.ContainsKey(techType))
            {
                if (CraftData.craftingTimes[techType] != CustomCraftingTimes[techType])
                    CraftData.craftingTimes[techType] = CustomCraftingTimes[techType];
            }
            else if (CustomCraftingTimes.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.craftingTimes, CustomCraftingTimes);
            }
        }

        private static void GetCookedCreaturePrefix(TechType techType)
        {
            if (CraftData.cookedCreatureList.ContainsKey(techType) && CustomCookedCreatureList.ContainsKey(techType))
            {
                if (CraftData.cookedCreatureList[techType] != CustomCookedCreatureList[techType])
                    CraftData.cookedCreatureList[techType] = CustomCookedCreatureList[techType];
            }
            else if (CustomCookedCreatureList.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.cookedCreatureList, CustomCookedCreatureList);
            }
        }

        private static void GetBackgroundTypesPrefix(TechType techType)
        {
            if (CraftData.backgroundTypes.ContainsKey(techType) && CustomBackgroundTypes.ContainsKey(techType))
            {
                if (CraftData.backgroundTypes[techType] != CustomBackgroundTypes[techType])
                    CraftData.backgroundTypes[techType] = CustomBackgroundTypes[techType];
            }
            else if (CustomBackgroundTypes.ContainsKey(techType))
            {
                PatchUtils.PatchDictionary(CraftData.backgroundTypes, CustomBackgroundTypes);
            }
        }

        private static void GetBuildablePrefix(TechType recipe)
        {
            if (CustomBuildables.Contains(recipe) && !CraftData.buildables.Contains(recipe))
                PatchUtils.PatchList(CraftData.buildables, CustomBuildables);
        }

        private static void NeedsPatchingCheckPrefix(TechType techType)
        {
            bool techExists = CraftData.techData.TryGetValue(techType, out CraftData.TechData techData) && CustomTechData.ContainsKey(techType);

            bool sameData = false;
            if (techExists)
            {
                ITechData smlTechData = CustomTechData[techType];

                sameData = smlTechData.craftAmount == techData.craftAmount &&
                    smlTechData.ingredientCount == techData.ingredientCount &&
                    smlTechData.linkedItemCount == techData.linkedItemCount;

                if (sameData)
                    for (int i = 0; i < smlTechData.ingredientCount; i++)
                    {
                        if (smlTechData.GetIngredient(i).techType != techData.GetIngredient(i).techType)
                        {
                            sameData = false;
                            break;
                        }
                        if (smlTechData.GetIngredient(i).amount != techData.GetIngredient(i).amount)
                        {
                            sameData = false;
                            break;
                        }
                    }

                if (sameData)
                    for (int i = 0; i < smlTechData.linkedItemCount; i++)
                    {
                        if (smlTechData.GetLinkedItem(i) != techData.GetLinkedItem(i))
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
            foreach (TechType techType in CustomTechData.Keys)
            {
                bool techExists = CraftData.techData.TryGetValue(techType, out CraftData.TechData techData);
                bool sameData = false;
                ITechData smlTechData = CustomTechData[techType];

                if (techExists)
                {

                    sameData = smlTechData.craftAmount == techData.craftAmount &&
                        smlTechData.ingredientCount == techData.ingredientCount &&
                        smlTechData.linkedItemCount == techData.linkedItemCount;

                    if (sameData)
                        for (int i = 0; i < smlTechData.ingredientCount; i++)
                        {
                            if (smlTechData.GetIngredient(i).techType != techData.GetIngredient(i).techType)
                            {
                                sameData = false;
                                break;
                            }
                            if (smlTechData.GetIngredient(i).amount != techData.GetIngredient(i).amount)
                            {
                                sameData = false;
                                break;
                            }
                        }

                    if (sameData)
                        for (int i = 0; i < smlTechData.linkedItemCount; i++)
                        {
                            if (smlTechData.GetLinkedItem(i) != techData.GetLinkedItem(i))
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
                        _craftAmount = smlTechData.craftAmount
                    };

                    var ingredientsList = new CraftData.Ingredients();

                    if (smlTechData.ingredientCount > 0)
                    {
                        for (int i = 0; i < smlTechData.ingredientCount; i++)
                        {
                            IIngredient smlIngredient = smlTechData.GetIngredient(i);

                            var ingredient = new CraftData.Ingredient(smlIngredient.techType, smlIngredient.amount);
                            ingredientsList.Add(smlIngredient.techType, smlIngredient.amount);
                        }
                        techDataInstance._ingredients = ingredientsList;
                    }

                    if (smlTechData.linkedItemCount > 0)
                    {
                        var linkedItems = new List<TechType>();
                        for (int l = 0; l < smlTechData.linkedItemCount; l++)
                        {
                            linkedItems.Add(smlTechData.GetLinkedItem(l));
                        }
                        techDataInstance._linkedItems = linkedItems;
                    }

                    if (techExists)
                    {
                        CraftData.techData.Remove(techType);
                        Logger.Log($"{techType} TechType already existed in the CraftData.techData dictionary. Original value was replaced.", LogLevel.Warn);
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
                Logger.Log($"Added {added} new entries to the CraftData.techData dictionary.", LogLevel.Info);

            if (replaced > 0)
                Logger.Log($"Replaced {replaced} existing entries to the CraftData.techData dictionary.", LogLevel.Info);
        }

    }
}
#endif