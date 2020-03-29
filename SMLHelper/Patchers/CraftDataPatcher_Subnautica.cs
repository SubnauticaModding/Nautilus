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
            DictionaryPrefix(techType, CustomHarvestOutputList, CraftData.harvestOutputList);
        }

        private static void GetHarvestTypePrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomHarvestTypeList, CraftData.harvestTypeList);
        }

        private static void GetFinalCutBonusPrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomFinalCutBonusList, CraftData.harvestFinalCutBonusList);
        }

        private static void GetItemSizePrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomItemSizes, CraftData.itemSizes);
        }

        private static void GetEquipmentTypePrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomEquipmentTypes, CraftData.equipmentTypes);
        }

        private static void GetSlotTypePrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomSlotTypes, CraftData.slotTypes);
        }

        private static void GetCraftTimePrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomCraftingTimes, CraftData.craftingTimes);
        }

        private static void GetCookedCreaturePrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomCookedCreatureList, CraftData.cookedCreatureList);
        }

        private static void GetBackgroundTypesPrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomBackgroundTypes, CraftData.backgroundTypes);
        }

        private static void GetUseEatSoundPrefix(TechType techType)
        {
            DictionaryPrefix(techType, CustomEatingSounds, CraftData.useEatSound);
        }

        private static void DictionaryPrefix<T>(TechType techType, IDictionary<TechType, T> smlCollection, IDictionary<TechType, T> craftDataCollection)
        {
            if (smlCollection.TryGetValue(techType, out T sml))
            {
                if (craftDataCollection.TryGetValue(techType, out T gameVal))
                {
                    if (!sml.Equals(gameVal))
                        craftDataCollection[techType] = sml;
                }
                else
                {
                    PatchUtils.PatchDictionary(craftDataCollection, smlCollection);
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