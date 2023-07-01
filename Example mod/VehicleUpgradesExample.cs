#if SUBNAUTICA
using BepInEx;
using Nautilus.Handlers;
using Nautilus.Options.Attributes;
using Nautilus.Json;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using System.Collections.Generic;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.vehicleupgrades", "Nautilus Vehicle Upgrades Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class VehicleUpgradesExample : BaseUnityPlugin
{
    private void Awake()
    {
        VehicleUpgradesExampleConfig config = OptionsPanelHandler.RegisterModOptions<VehicleUpgradesExampleConfig>();

        /*
         * Now, we'll be doing a custom vehicle upgrade mdoule.
         * It will incrase the Seamoth crush depth to 3 kilometers (3000 meters)!
         */
        PrefabInfo depthUpgradeInfo = PrefabInfo.WithTechType("SeamothDepthUpgrade", "Custom Depth Upgrade", "A custom depth upgrade module for Seamoth that allows you to dive to 3000 meters!");
        Logger.LogDebug("Registered depth upgrade info");

        /*
         * Here, we're  assigning the hull icon for our item.
         * You may also use a custom image icon by calling the ImageUtils.LoadSpriteFromFile method or with AssetBundles, like mentioned higher.
         */
        depthUpgradeInfo.WithIcon(SpriteManager.Get(TechType.HullReinforcementModule2));
        Logger.LogDebug("Registerd depth upgrade icon");

        /*
         * Here we are setting up an instance of CustomPrefab, as we've already done higher.
         */
        CustomPrefab depthUpgrade = new CustomPrefab(depthUpgradeInfo);
        Logger.LogDebug("Registered depth upgrade custom prefab");

        /*
         * Like before, we're creating a clone of an existing techtype so we can have basic components such as a model, a rigidbody, etc...
         */
        PrefabTemplate hullModuleCloneTemplate = new CloneTemplate(depthUpgradeInfo, TechType.HullReinforcementModule);
        Logger.LogDebug("Cloned prefab of HullReinforcementModule3");

        /*
         * Now we're setting our depth upgrade module's gameobject with the hull reinforcement one.
         * Theoretically it can be whatever tech type you want, but we'll take this one.
         */
        depthUpgrade.SetGameObject(hullModuleCloneTemplate);
        Logger.LogDebug("Setted depth upgrade prefab");

        /*
         * We will not add any modifier to the item this time.
         * Instead, we're directly gonna make a recipe, and set its other metadata.
         */
        depthUpgrade.SetRecipe(new Crafting.RecipeData()
        {
            /*
             * Here, we are saying the amount of the item we want to be crafted.
             */
            craftAmount = 1,

            /*
             * And here, we're making a list of ingredients.
             */
            Ingredients = new List<CraftData.Ingredient>()
            {
                new CraftData.Ingredient(TechType.PlasteelIngot, 3),
                new CraftData.Ingredient(TechType.Copper, 1),
                new CraftData.Ingredient(TechType.Aerogel, 1),
            }
        })
            /*
             * There, we're saying the fabricator type we want the item to be in.
             * In our case, let's say SeamothUpgrades, it's the fabricator in the Moonpool Upgrade Console.
             */
            .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
            /*
             * With this function, we set the node to access the module in the crafting bench.
             * In the seamoth upgrades, there's no subnodes, only nodes.
             */
            .WithStepsToFabricatorTab("SeamothModules")
            /*
             * Here, we are saying that the crafting duration of the item will be 2.5 seconds.
             */
            .WithCraftingTime(2.5f);
        Logger.LogDebug("Registered crafting gadget of depth upgrade");

        /*
         * Now, we're saying that this Equipment is an Upgrade Module.
         * N.B. Cyclops modules are using an other Gadget.
         */
        depthUpgrade.SetVehicleUpgradeModule(EquipmentType.SeamothModule, QuickSlotType.Passive)

            /*
             * Here we're defining the main thing that interests us, the max depth!
             * We set it to 3000f, which is the new max depth.
             * They are not stackable.
             */
            .WithDepthUpgrade(3000f)

            /*
             * You can also add an action for when the module is added and removed from the vehicle.
             * We'll add a subtitle for when the module is added and for when the module is removed.
             */
            .WithOnModuleAdded((Vehicle vehicleReference, int quickSlotId) =>
            {
                Subtitles.Add($"Warning! The max depth is now 3000 meters. Say hello to Ghost Leviathans from me!");
            })
            .WithOnModuleRemoved((Vehicle vehicleReference, int quickSlotId) =>
            {
                Subtitles.Add("Warning! The depth upgrade has been removed!");
            });
        Logger.LogDebug("Registered equipment and upgrade module gadgets of depth upgrade");

        /*
         * Finally, we register it.
         */
        depthUpgrade.Register();
        Logger.LogDebug("Registered depth upgrade.");


        /*
         * Now, let's try to do that with an interactable module !
         */
        var SelfDefenseMK2Info = PrefabInfo
            .WithTechType("PerimeterDefenseMK2", "Perimeter Defense MK2 for Seamoth", "A new electrical defense for Seamoth")
            .WithIcon(SpriteManager.Get(TechType.SeamothElectricalDefense));

        var selfDefMK2prefab = new CustomPrefab(SelfDefenseMK2Info);
        var selfDefClone = new CloneTemplate(SelfDefenseMK2Info, TechType.SeamothElectricalDefense);

        selfDefMK2prefab.SetGameObject(selfDefClone);

        selfDefMK2prefab.SetRecipe(new Crafting.RecipeData()
        {
            craftAmount = 1,
            Ingredients = new List<CraftData.Ingredient>()
                {
                    new CraftData.Ingredient(TechType.SeamothElectricalDefense),
                    new CraftData.Ingredient(TechType.Diamond, 2)
                }
        })
            .WithFabricatorType(CraftTree.Type.Workbench);

        selfDefMK2prefab.SetVehicleUpgradeModule(EquipmentType.SeamothModule, QuickSlotType.SelectableChargeable)
            .WithCooldown(2.5f)
            .WithEnergyCost(10f)
            .WithMaxCharge(25f)
            .WithOnModuleUsed((Vehicle vehicleInst, int slotID, float charge, float chargeScalar) =>
            {
                var __instance = vehicleInst as SeaMoth;
                ElectricalDefense defense = Utils.SpawnZeroedAt(__instance.seamothElectricalDefensePrefab, __instance.transform, false).GetComponent<ElectricalDefense>();
                defense.charge = charge;
                defense.chargeScalar = chargeScalar;
            });
        selfDefMK2prefab.Register();

        /*
         * And finally, a CUSTOMIZABLE-after-restart depth module!
         */

        // PLANNED FOR ANOTHER PR.
/*
        PrefabInfo prefabInfo = PrefabInfo.WithTechType("SeamothCustomDepthModule", "Seamoth Variable Depth Upgrade", "Customize the depth of your upgrade from the game settings!")
            .WithIcon(SpriteManager.Get(TechType.SeamothReinforcementModule))
            .WithSizeInInventory(new Vector2int(1, 1));

        CustomPrefab prefab = new(prefabInfo);
        CloneTemplate clone = new(prefabInfo, TechType.SeamothReinforcementModule);

        prefab.SetGameObject(clone);
        prefab.SetPdaGroupCategory(TechGroup.VehicleUpgrades, TechCategory.VehicleUpgrades);
        prefab.SetRecipe(new Crafting.RecipeData()
        {
            craftAmount = 1,
            Ingredients = new List<CraftData.Ingredient>()
            {
                new CraftData.Ingredient(TechType.SeamothReinforcementModule),
                new CraftData.Ingredient(TechType.Diamond, 2)
            }
        })
            .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
            .WithCraftingTime(4f)
            .WithStepsToFabricatorTab("SeamothModules");
        prefab.SetVehicleUpgradeModule(EquipmentType.SeamothModule)
                .WithDepthUpgrade(() => config.MaxDepth, true)
                .WithOnModuleAdded((Vehicle vehicleInstance, int slotId) => {
                    Subtitles.Add($"New seamoth depth: {config.MaxDepth} meters.\nAdded in slot #{slotId + 1}.");
                })
                .WithOnModuleRemoved((Vehicle vehicleInstance, int slotId) =>
                {
                    Subtitles.Add($"Seamoth depth module removed from slot #{slotId + 1}!");
                });
        prefab.Register();
*/
    }
}

// Disable the naming convention violation warning.
#pragma warning disable IDE1006

[Menu("Vehicle Upgrade Example mod")]
public class VehicleUpgradesExampleConfig : ConfigFile
{
    [Slider(Format = "{0:F0}m", Label = "Seamoth Upgrade Max Depth", Min = 100f, Max = 10000f, Step = 10f,
        Tooltip = "This is the max depth of the seamtoh when the depth module is equipped. It is absolute.")]
    public float MaxDepth = 500.0f;
}
#endif