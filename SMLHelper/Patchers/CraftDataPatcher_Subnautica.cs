#if SUBNAUTICA
namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using System.Collections.Generic;

    internal partial class CraftDataPatcher
    {
        internal static IDictionary<TechType, ITechData> CustomTechData = new SelfCheckingDictionary<TechType, ITechData>("CustomTechData", AsStringFunction);
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

        internal static void AddToCustomTechData(TechType techType, ITechData techData)
        {
            CustomTechData.Add(techType, techData);
        }

        private static void PatchForSubnautica(HarmonyInstance harmony)
        {
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

            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetUseEatSound)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetUseEatSoundPrefix))));

        }

        private static void GetHarvestOutputPrefix(TechType techType)
        {
            if (CustomHarvestOutputList.TryGetValue(techType, out TechType smlho))
            {
                if(CraftData.harvestOutputList.TryGetValue(techType, out TechType tt))
                {
                    if(smlho != tt)
                        CraftData.harvestOutputList[techType] = smlho;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.harvestOutputList, CustomHarvestOutputList);
                }
            }
        }

        private static void GetHarvestTypePrefix(TechType techType)
        {
            if (CustomHarvestTypeList.TryGetValue(techType, out HarvestType smlht))
            {
                if (CraftData.harvestTypeList.TryGetValue(techType, out HarvestType ht))
                {
                    if (ht != smlht)
                        CraftData.harvestTypeList[techType] = smlht;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.harvestTypeList, CustomHarvestTypeList);
                }
            }
        }

        private static void GetFinalCutBonusPrefix(TechType techType)
        {
            if (CustomFinalCutBonusList.TryGetValue(techType, out int smlhfcb))
            {
                if (CraftData.harvestFinalCutBonusList.TryGetValue(techType, out int hfcb))
                {
                    if (hfcb != smlhfcb)
                        CraftData.harvestFinalCutBonusList[techType] = smlhfcb;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.harvestFinalCutBonusList, CustomFinalCutBonusList);
                }
            }
        }

        private static void GetItemSizePrefix(TechType techType)
        {
            if(CustomItemSizes.TryGetValue(techType, out Vector2int smlItemSize))
            {
                if(CraftData.itemSizes.TryGetValue(techType, out Vector2int itemSize))
                {
                    if (smlItemSize.x != itemSize.x || smlItemSize.y != itemSize.y)
                        CraftData.itemSizes[techType] = smlItemSize;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.itemSizes, CustomItemSizes);
                }
            }
        }

        private static void GetEquipmentTypePrefix(TechType techType)
        {
            if (CustomEquipmentTypes.TryGetValue(techType, out EquipmentType SMLET))
            {
                if (CraftData.equipmentTypes.TryGetValue(techType, out EquipmentType ET))
                {
                    if (ET != SMLET)
                        CraftData.equipmentTypes[techType] = SMLET;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.equipmentTypes, CustomEquipmentTypes);
                }
            }
        }

        private static void GetSlotTypePrefix(TechType techType)
        {
            if (CustomSlotTypes.TryGetValue(techType, out QuickSlotType smlqst))
            {
                if (CraftData.slotTypes.TryGetValue(techType, out QuickSlotType qst))
                {
                    if (qst != smlqst)
                        CraftData.slotTypes[techType] = smlqst;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.slotTypes, CustomSlotTypes);
                }
            }
        }

        private static void GetCraftTimePrefix(TechType techType)
        {
            if (CustomCraftingTimes.TryGetValue(techType, out float smlct))
            {
                if (CraftData.craftingTimes.TryGetValue(techType, out float ct))
                {
                    if (ct != smlct)
                        CraftData.craftingTimes[techType] = smlct;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.craftingTimes, CustomCraftingTimes);
                }
            }
        }

        private static void GetCookedCreaturePrefix(TechType techType)
        {
            if (CustomCookedCreatureList.TryGetValue(techType, out TechType smltt))
            {
                if (CraftData.cookedCreatureList.TryGetValue(techType, out TechType tt))
                {
                    if (tt != smltt)
                        CraftData.cookedCreatureList[techType] = smltt;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.cookedCreatureList, CustomCookedCreatureList);
                }
            }
        }

        private static void GetBackgroundTypesPrefix(TechType techType)
        {
            if (CustomBackgroundTypes.TryGetValue(techType, out CraftData.BackgroundType smlbt))
            {
                if (CraftData.backgroundTypes.TryGetValue(techType, out CraftData.BackgroundType bt))
                {
                    if (bt != smlbt)
                        CraftData.backgroundTypes[techType] = smlbt;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.backgroundTypes, CustomBackgroundTypes);
                }
            }
        }

        private static void GetUseEatSoundPrefix(TechType techType)
        {
            if (CustomEatingSounds.TryGetValue(techType, out string smlsoundPath))
            {
                if (CraftData.useEatSound.TryGetValue(techType, out string soundPath))
                {
                    if (smlsoundPath != soundPath)
                        CraftData.useEatSound[techType] = smlsoundPath;
                }
                else
                {
                    PatchUtils.PatchDictionary(CraftData.useEatSound, CustomEatingSounds);
                }
            }
        }

        private static void GetBuildablePrefix(TechType recipe)
        {
            if (CustomBuildables.Contains(recipe) && !CraftData.buildables.Contains(recipe))
                PatchUtils.PatchList(CraftData.buildables, CustomBuildables);
        }

        private static void NeedsPatchingCheckPrefix(TechType techType)
        {
            bool techExists = CraftData.techData.TryGetValue(techType, out CraftData.TechData techData);

            bool sameData = false;
            if (techExists && CustomTechData.TryGetValue(techType, out ITechData smlTechData))
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
                ITechData smlTechData = CustomTechData[techType];
                bool sameData = false;

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