#if SUBNAUTICA
namespace SMLHelper.V2.Patchers
{
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
        internal static List<TechType> CustomBuildables = new List<TechType>();

        internal static void AddToCustomTechData(TechType techType, ITechData techData)
        {
            CustomTechData.Add(techType, techData);
        }

        private static void PatchForSubnautica()
        {
            // Direct access to private fields made possible by https://github.com/CabbageCrow/AssemblyPublicizer/
            // See README.md for details.
            PatchUtils.PatchDictionary(CraftData.harvestOutputList, CustomHarvestOutputList);
            PatchUtils.PatchDictionary(CraftData.harvestTypeList, CustomHarvestTypeList);
            PatchUtils.PatchDictionary(CraftData.harvestFinalCutBonusList, CustomFinalCutBonusList);
            PatchUtils.PatchDictionary(CraftData.itemSizes, CustomItemSizes);
            PatchUtils.PatchDictionary(CraftData.equipmentTypes, CustomEquipmentTypes);
            PatchUtils.PatchDictionary(CraftData.slotTypes, CustomSlotTypes);
            PatchUtils.PatchDictionary(CraftData.craftingTimes, CustomCraftingTimes);
            PatchUtils.PatchDictionary(CraftData.cookedCreatureList, CustomCookedCreatureList);
            PatchUtils.PatchDictionary(CraftData.backgroundTypes, CustomBackgroundTypes);
            PatchUtils.PatchList(CraftData.buildables, CustomBuildables);

            AddCustomTechDataToOriginalDictionary();
        }

        private static void AddCustomTechDataToOriginalDictionary()
        {
            short added = 0;
            short replaced = 0;
            foreach (TechType techType in CustomTechData.Keys)
            {
                ITechData smlTechData = CustomTechData[techType];

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

                bool techDataExists = CraftData.techData.ContainsKey(techType);

                if (techDataExists)
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

            if (added > 0)
                Logger.Log($"Added {added} new entries to the CraftData.techData dictionary.", LogLevel.Info);

            if (replaced > 0)
                Logger.Log($"Replaced {replaced} existing entries to the CraftData.techData dictionary.", LogLevel.Info);
        }
    }
}
#endif
