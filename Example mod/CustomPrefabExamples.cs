using BepInEx;
using SMLHelper.Assets;
using SMLHelper.Assets.Gadgets;
using SMLHelper.Assets.PrefabTemplates;
using UnityEngine;
using BiomeData = LootDistributionData.BiomeData;

namespace SMLHelper.Examples;

[BepInPlugin("com.snmodding.smlhelper.customprefab", "SMLHelper Custom Prefab Example Mod", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.smlhelper")]
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
    }
}