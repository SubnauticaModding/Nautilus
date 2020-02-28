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
        internal static int TechDataAltered = 0;
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
               postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(AddCustomDataToOriginalDictionaries))));
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache)),
               postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataPrefabIDCachePostfix))));
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.Get)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataGetPrefix))));
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetPrefabForTechType)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(CraftDataGetPrefabPrefix))));
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetHarvestOutputData)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetHarvestOutputPrefix))));
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.GetHarvestTypeExpensive)),
               prefix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(GetHarvestTypePrefix))));
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
            
            IngameMenuHandler.Main.RegisterOnQuitEvent(() => ResetAltered());

        }

        private static void ResetAltered()
        {
            TechDataAltered = 0;
            HarvestOutputAltered = 0;
            HarvestTypeAltered = 0;
            FinalCutBonusAltered = 0;
            ItemSizesAltered = 0;
            EquipmentTypesAltered = 0;
            SlotTypesAltered = 0;
            CraftingTimesAltered = 0;
            CookedCreatureAltered = 0;
            BackgroundTypesAltered = 0;
            BuildablesAltered = 0;
            PrefabAltered = 0;
        }

        private static void CraftDataGetPrefabPrefix()
        {
            if (ModPrefab.PrefabCount > 0 && PrefabAltered < ModPrefab.PrefabCount)
                CraftDataPrefabIDCachePostfix();
        }

        private static void CraftDataPrefabIDCachePostfix()
        {
            Dictionary<TechType, string> techMapping = CraftData.techMapping;
            Dictionary<string, TechType> entClassTechTable = CraftData.entClassTechTable;
            foreach (ModPrefab prefab in ModPrefab.Prefabs)
            {
                techMapping[prefab.TechType] = prefab.ClassID;
                entClassTechTable[prefab.ClassID] = prefab.TechType;
                PrefabAltered++;
            }
        }

        private static void CraftDataGetPrefix()
        {
            if (TechDataAltered < CustomTechData.Keys.Count)
                AddCustomDataToOriginalDictionaries();
        }

        private static void GetHarvestOutputPrefix()
        {
            if (CustomHarvestOutputList.Count > 0 && HarvestOutputAltered < CustomHarvestOutputList.Count)
            {
                PatchUtils.PatchDictionary(CraftData.harvestOutputList, CustomHarvestOutputList);
                HarvestOutputAltered = CustomHarvestOutputList.Count;
            }
        }

        private static void GetHarvestTypePrefix()
        {
            if (CustomHarvestTypeList.Count > 0 && HarvestTypeAltered < CustomHarvestTypeList.Count)
            {
                PatchUtils.PatchDictionary(CraftData.harvestTypeList, CustomHarvestTypeList);
                HarvestTypeAltered = CustomHarvestTypeList.Count;
            }
        }

        private static void GetFinalCutBonusPrefix()
        {
            if (CustomFinalCutBonusList.Count > 0 && FinalCutBonusAltered < CustomFinalCutBonusList.Count)
            {
                PatchUtils.PatchDictionary(CraftData.harvestFinalCutBonusList, CustomFinalCutBonusList);
                FinalCutBonusAltered = CustomFinalCutBonusList.Count;
            }
        }

        private static void GetItemSizePrefix()
        {
            if (CustomItemSizes.Count > 0 && ItemSizesAltered < CustomItemSizes.Count)
            {
                PatchUtils.PatchDictionary(CraftData.itemSizes, CustomItemSizes);
                ItemSizesAltered = CustomItemSizes.Count;
            }
        }

        private static void GetEquipmentTypePrefix()
        {
            if (CustomEquipmentTypes.Count > 0 && EquipmentTypesAltered < CustomEquipmentTypes.Count)
            {
                PatchUtils.PatchDictionary(CraftData.equipmentTypes, CustomEquipmentTypes);
                EquipmentTypesAltered = CustomEquipmentTypes.Count;
            }
        }

        private static void GetSlotTypePrefix()
        {
            if (CustomSlotTypes.Count > 0 && SlotTypesAltered < CustomSlotTypes.Count)
            {
                PatchUtils.PatchDictionary(CraftData.slotTypes, CustomSlotTypes);
                SlotTypesAltered = CustomSlotTypes.Count;
            }
        }

        private static void GetCraftTimePrefix()
        {
            if (CustomCraftingTimes.Count > 0 && CraftingTimesAltered < CustomCraftingTimes.Count)
            {
                PatchUtils.PatchDictionary(CraftData.craftingTimes, CustomCraftingTimes);
                CraftingTimesAltered = CustomCraftingTimes.Count;
            }
        }

        private static void GetCookedCreaturePrefix()
        {
            if (CustomCookedCreatureList.Count > 0 && CookedCreatureAltered < CustomCookedCreatureList.Count)
            {
                PatchUtils.PatchDictionary(CraftData.cookedCreatureList, CustomCookedCreatureList);
                CookedCreatureAltered = CustomCookedCreatureList.Count;
            }
        }

        private static void GetBackgroundTypesPrefix()
        {
            if (CustomBackgroundTypes.Count > 0 && BackgroundTypesAltered < CustomBackgroundTypes.Count)
            {
                PatchUtils.PatchDictionary(CraftData.backgroundTypes, CustomBackgroundTypes);
                BackgroundTypesAltered = CustomBackgroundTypes.Count;
            }
        }

        private static void GetBuildablePrefix()
        {
            if (CustomBuildables.Count > 0 && BuildablesAltered < CustomBuildables.Count)
            {
                PatchUtils.PatchList(CraftData.buildables, CustomBuildables);
                BuildablesAltered = CustomBuildables.Count;
            }
        }

        private static void AddCustomDataToOriginalDictionaries()
        {
            short added = 0;
            short replaced = 0;
            foreach (TechType techType in CustomTechData.Keys)
            {
                ITechData smlTechData = CustomTechData[techType];
                bool techExists = CraftData.techData.TryGetValue(techType, out CraftData.TechData techData);
                bool sameData = true;

                if (techExists)
                {
                    sameData = smlTechData.craftAmount == techData.craftAmount &&
                        smlTechData.ingredientCount == techData.ingredientCount &&
                        smlTechData.linkedItemCount == techData.linkedItemCount;
                    
                    if(sameData)
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
                    Console.WriteLine($"techtype: {techType.ToString()}, techExists: {techExists}, sameData: {sameData}");
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
                        TechDataAltered++;
                    }
                    else
                    {
                        added++;
                        TechDataAltered++;
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
