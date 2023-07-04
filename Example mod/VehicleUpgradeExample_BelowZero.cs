#if BELOWZERO
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using UnityEngine;
using UWE;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.vehicleupgrades", "Nautilus Vehicle Upgrades Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
public class VehicleUpgradeExample : BaseUnityPlugin
{
    GameObject electricalDefensePrefab;
    private void Awake()
    {

        var DefenseRecipe = new Crafting.RecipeData()
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>()
            {
                new Ingredient(TechType.Polyaniline, 1),
                new Ingredient(TechType.Quartz, 2),
                new Ingredient(TechType.Battery, 2),
                new Ingredient(TechType.AluminumOxide, 1)
            }
        };
        Action<Hoverbike, int> onAdded = (Hoverbike instance, int slotID) =>
        {
            if (electricalDefensePrefab == null)
                CoroutineHost.StartCoroutine(InitElectricalPrefab());
            Subtitles.Add("Hoverbike perimeter defense upgrade module engaged.");
        };

        Action<Hoverbike, int> onRemoved = (Hoverbike instance, int slotID) =>
        {
            Subtitles.Add("Hoverbike perimeter defense upgrade module disengaged. Caution advised.");
        };

        Action<Hoverbike, int, float, float> onUsed = (Hoverbike instance, int slotID, float charge, float chargeScalar) =>
        {
            if (electricalDefensePrefab != null)
            {
                var electricalDefense = Utils.SpawnZeroedAt(electricalDefensePrefab, instance.transform, false).GetComponent<ElectricalDefense>();
                electricalDefense.charge = charge;
                electricalDefense.chargeScalar = chargeScalar;
            }
            else
            {
                CoroutineHost.StartCoroutine(InitElectricalPrefab());
                Subtitles.Add("Hoverbike perimeter defense upgrade was not initialized. Initializing.");
            }
        };

        /*
         * Here we will make an Hoverbike module.
         * This system is very easy to use but was complex enough to implement.
         */
        var hoverbikePeriDefInfo = PrefabInfo.WithTechType(
                "HoverbikePerimeterDefense",
                "Hoverbike Perimeter Defense Module",
                "A perimeter defense upgrade for the Hoverbike... No one knows if it will be useful.",
                techTypeOwner: Assembly.GetExecutingAssembly())
            .WithIcon(SpriteManager.Get(TechType.SeaTruckUpgradePerimeterDefense));

        CustomPrefab hoverbikePeriDefPrefab = new CustomPrefab(hoverbikePeriDefInfo);
        CloneTemplate seatruckPeriDefClone = new CloneTemplate(hoverbikePeriDefInfo, TechType.SeaTruckUpgradePerimeterDefense);

        hoverbikePeriDefPrefab.SetGameObject(seatruckPeriDefClone);

        hoverbikePeriDefPrefab.SetRecipe(DefenseRecipe)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Machines")
            .WithCraftingTime(2.5f);

        hoverbikePeriDefPrefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
        hoverbikePeriDefPrefab.SetUnlock(TechType.Polyaniline);
        hoverbikePeriDefPrefab.SetVehicleUpgradeModule(EquipmentType.HoverbikeModule, QuickSlotType.Chargeable)
            .WithCooldown(5f)
            .WithEnergyCost(5f)
            .WithMaxCharge(10f)
            .WithOnModuleAdded(onAdded)
            .WithOnModuleRemoved(onRemoved)
            .WithOnModuleUsed(onUsed);

        hoverbikePeriDefPrefab.Register();


        /*
         * Now let's make one that can be selected and charged.
         */
        var hoverbikeSelectDef = PrefabInfo.WithTechType("HoverbikeSelectDefense",
                "Hoverbike Selectable Perimeter Defense Module",
                "The same than the other module, but selectable.")
            .WithIcon(SpriteManager.Get(TechType.SeaTruckUpgradePerimeterDefense));

        var hoverbikeSelDefPrefab = new CustomPrefab(hoverbikeSelectDef);

        hoverbikeSelDefPrefab.SetGameObject(seatruckPeriDefClone);
        hoverbikeSelDefPrefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades)
            .WithCompoundTechsForUnlock(new()
            {
                TechType.Polyaniline
            });
        hoverbikeSelDefPrefab.SetRecipe(DefenseRecipe)
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Machines")
            .WithCraftingTime(2.5f);
        hoverbikeSelDefPrefab.SetVehicleUpgradeModule(EquipmentType.HoverbikeModule, QuickSlotType.SelectableChargeable)
            .WithCooldown(5f)
            .WithEnergyCost(5f)
            .WithMaxCharge(10f)
            .WithOnModuleAdded(onAdded)
            .WithOnModuleRemoved(onRemoved)
            .WithOnModuleUsed(onUsed);

        hoverbikeSelDefPrefab.Register();
    }

    internal IEnumerator InitElectricalPrefab()
    {
        /*
         * This function is getting the ElectricalDefense prefab from the SeaTruckUpgrades.
         * You can see that this monobehaviour is not stored in the upgrade prefab.
         */
        var task = CraftData.GetPrefabForTechTypeAsync(TechType.SeaTruck);
        yield return task;
        var result = task.GetResult();
        var seatruckUpgrades = result.GetComponent<SeaTruckUpgrades>();
        electricalDefensePrefab = seatruckUpgrades.electricalDefensePrefab;
        Subtitles.Add("Hoverbike perimeter defense upgrade is initialized.");
    }
}
#endif
