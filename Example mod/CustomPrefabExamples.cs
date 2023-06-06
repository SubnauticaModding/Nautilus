using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using System.Collections.Generic;
using UnityEngine;
using BiomeData = LootDistributionData.BiomeData;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.customprefab", "Nautilus Custom Prefab Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class CustomPrefabExamples : BaseUnityPlugin
{
    private void Awake()
    {
        /*
         * Here we're setting up an instance of PrefabInfo.
         * The parameters we are assigning respectively are the following:
         * - Class ID: The class identifier used internally in the game, especially for the PrefabIdentifier component
         * - Display Name: The name of this item when hovered on in the world, or in inventory
         * - Description: The tooltip of this item
         *
         * Every custom prefab will require an instance of PrefabInfo, which ultimately has all the info required to
         * make this item work in the game.
         */
        PrefabInfo copperCloneInfo = PrefabInfo.WithTechType("CopperClone", "Copper Ore Clone", "Copper Ore clone that makes me go yes.");
        
        /*
         * Here we're assigning the Copper icon for our item.
         * You may also use a custom image as your icon by calling the ImageUtils.LoadSpriteFromFile method.
         */
        copperCloneInfo.WithIcon(SpriteManager.Get(TechType.Copper));
        
        /*
         * Here we are setting up an instance of CustomPrefab.
         * CustomPrefab is where you actually add logic and birth to your item.
         * Here you will be able to add a game object for your item, set spawns, recipe, etc.. for your item.
         */
        CustomPrefab copperClone = new CustomPrefab(copperCloneInfo);

        /*
         * Here we are creating a clone of the Copper game object.
         * Additionally, we are also changing the color of the clone to red.
         */
        PrefabTemplate cloneTemplate = new CloneTemplate(copperCloneInfo, TechType.Copper)
        {
            // Callback to change all material colors of this clone to red.
            ModifyPrefab = prefab => prefab.GetComponentsInChildren<Renderer>().ForEach(r => r.materials.ForEach(m => m.color = Color.red))
        };
        
        /*
         * Here we are setting the Copper clone we created earlier as our item's prefab.
         */
        copperClone.SetGameObject(cloneTemplate);

        /*
         * Then we added biome spawns for our item.
         */
        copperClone.SetSpawns(new BiomeData { biome = BiomeType.SafeShallows_Grass, count = 4, probability = 0.1f },
            new BiomeData { biome = BiomeType.SafeShallows_CaveFloor, count = 1, probability = 0.4f });
        
        /*
         * And finally, we register it to the game.
         * Now we can spawn our item to the world manually by using the command 'spawn copperclone', or
         * simply looking around in the Safe shallows.
         * Refrain from modifying the item further more or adding more gadgets after Register is called as they will not be called.
         */
        copperClone.Register();



        // Level 2



        /*
         * Now, we'll be doing a custom vehicle upgrade mdoule.
         * It will incrase the Seamoth crush depth to 3 kilometers (3000 meters)!
         */
        PrefabInfo depthUpgradeInfo = PrefabInfo.WithTechType("SeamothDepthUpgrade", "Custom Depth Upgrade", "A custom depth upgrade module for Seamoth that allows you to dive to 3000 meters!");

        /*
         * Here, we're  assigning the hull icon for our item.
         * You may also use a custom image icon by calling the ImageUtils.LoadSPriteFromFile method or with AssetBundles, like mentioned higher.
         */
        depthUpgradeInfo.WithIcon(SpriteManager.Get(TechType.HullReinforcementModule3));

        /*
         * Here we are setting up an instance of CustomPrefab, as we've already done higher.
         */
        CustomPrefab depthUpgrade = new CustomPrefab(depthUpgradeInfo);

        /*
         * Like before, we're creating a clone of an existing techtype so we can have basic components such as a model, a rigidbody, etc...
         */
        PrefabTemplate hullModuleCloneTemplate = new CloneTemplate(depthUpgradeInfo, TechType.HullReinforcementModule3);

        /*
         * Now we're setting our depth upgrade module's gameobject with the hull reinforcement one.
         * Theoretically it can be whatever tech type you want, but we'll take this one.
         */
        depthUpgrade.SetGameObject(hullModuleCloneTemplate);

        /*
         * We will not add any modifier to the item this time.
         * Instead, we're directly gonna make a recipe, and set its other metadata.
         */
        depthUpgrade.SetRecipe(new Crafting.RecipeData()
            {
                /*
                 * Here, wer are saying the amount of the item we want to be crafted.
                 */
                craftAmount = 1,

                /*
                 * And here, we're making a list of arguments.
                 */
                Ingredients = new List<CraftData.Ingredient>()
                {
                    new CraftData.Ingredient(TechType.PlasteelIngot, 3),

                    /*
                     * As you can see, we can refer to a custom item by accessing its TechType, like this.
                     */
                    new CraftData.Ingredient(copperClone.Info.TechType, 1),
                    new CraftData.Ingredient(TechType.Aerogel, 1),
                }
            })
            /*
             * There, we're saying the fabricator type we want the item to be in.
             * In our case, let's say SeamothUpgrades, it's the fabricator in the Moonpool Upgrade Console.
             */
            .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
            /*
             * Here, we are saying that the crafting duration of the item will be 2.5 seconds.
             */
            .WithCraftingTime(2.5f);

        /*
         * Now, we're defining our item as an Equipment. Equipment can be a module, an O2 Tank, a chip, etc...
         * In our case, it will be a SeamothModule.
         */
        depthUpgrade.SetEquipment(EquipmentType.SeamothModule)

            /*
             * This method defines if the upgrade has an action or not.
             * In our case, it does not.
             */
            .WithQuickSlotType(QuickSlotType.None)

            /*
             * Now, we're saying that this Equipment is an Upgrade Module.
             * N.B. Cyclops modules are using an other Gadget.
             */
            .SetUpgradeModule()

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
                Subtitles.Add("Warning! The max depth is now 3000 meters. Say hello to Ghost Leviathans from me!");
            })
            .WithOnModuleRemoved((Vehicle vehicleReference, int quickSlotId) =>
            {
                Subtitles.Add("Warning! The depth upgrade has been removed!");
            });

        /*
         * Finally, we register it.
         */
        depthUpgrade.Register();
    }
}