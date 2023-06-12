using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nautilus.Examples.Upgrades;
public class SeaMothDepthUpgrade
{
    public SeaMothDepthUpgrade()
    {
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
            .WithStepsToFabricatorTab("SeamothUpgrades");
        prefab.SetEquipment(EquipmentType.SeamothModule)
            .WithQuickSlotType(QuickSlotType.Passive)
            .SetUpgradeModule()
                .WithDepthUpgrade(ref Initializer.Configs.MaxDepth, true)
                .WithOnModuleAdded((Vehicle vehicleInstance, int slotId) => {
                    Subtitles.Add($"New seamoth depth: {Initializer.Configs.MaxDepth} meters.\nAdded in slot #{slotId + 1}.");
                })
                .WithOnModuleRemoved((Vehicle vehicleInstance, int slotId) =>
                {
                    Subtitles.Add($"Seamoth depth module removed from slot #{slotId + 1}!");
                });
        prefab.Register();
    }
}
