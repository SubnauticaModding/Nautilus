namespace SMLHelper.Examples;

using SMLHelper.Assets;
using SMLHelper.Assets.Interfaces;
using SMLHelper.Crafting;
using SMLHelper.Handlers;

#if SUBNAUTICA
using static CraftData;
#endif

public class NuclearBattery: ICraftable, ICustomBattery
{
    public CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;
    public string[] StepsToFabricatorTab => CustomBatteryHandler.BatteryCraftPath;
    public float CraftingTime => 1;
    public BatteryModel BatteryModel => BatteryModel.IonBattery;
    public BatteryType BatteryType => BatteryType.Battery;
    public float PowerCapacity => 69420;
    public RecipeData RecipeData { get; } = new()
    {
        craftAmount = 1,
        Ingredients = new()
        {
            new Ingredient(TechType.ReactorRod, 1),
            new Ingredient(TechType.Lead, 2),
            new Ingredient(TechType.CopperWire, 1)
        },
        LinkedItems = new()
        {
            TechType.DepletedReactorRod
        }
    };
}