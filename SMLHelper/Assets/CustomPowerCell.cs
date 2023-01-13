namespace SMLHelper.Assets;

using System.Collections.Generic;
using Crafting;
using API;
#if SUBNAUTICA
using RecipeData = Crafting.TechData;
#endif

internal class CustomPowerCell : CbCore
{
    private readonly CustomBattery baseBattery;

    public CustomPowerCell(string classId, bool ionCellSkins, CustomBattery customBattery) : base(classId, ionCellSkins)
    {
        baseBattery = customBattery;
    }

    protected override TechType PrefabType => UsingIonCellSkins ? TechType.PrecursorIonPowerCell : TechType.PowerCell;
    protected override EquipmentType ChargerType => EquipmentType.PowerCellCharger;
    protected override string[] StepsToFabricatorTab => CbDatabase.PowCellCraftPath;

    public override RecipeData GetBlueprintRecipe()
    {
        var partsList = new List<Ingredient>()
        {
            new Ingredient(baseBattery.TechType, 2),
            new Ingredient(TechType.Silicone, 1),
        };

        CreateIngredients(Parts, partsList);

        var batteryBlueprint = new RecipeData
        {
            craftAmount = 1,
            Ingredients = partsList
        };

        return batteryBlueprint;
    }

    protected override void AddToList()
    {
        CbDatabase.PowerCellItems.Add(this);
        CbDatabase.TrackItems.Add(TechType);
    }
}