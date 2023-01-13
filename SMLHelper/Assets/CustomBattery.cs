namespace SMLHelper.Assets;

using System.Collections.Generic;
using Crafting;
using API;
#if SUBNAUTICA
using RecipeData = Crafting.TechData;
#endif

internal class CustomBattery : CbCore
{
    public CustomBattery(string classId, bool ionCellSkins) : base(classId, ionCellSkins)
    {
    }

    protected override TechType PrefabType => UsingIonCellSkins ? TechType.PrecursorIonBattery : TechType.Battery;
    protected override EquipmentType ChargerType => EquipmentType.BatteryCharger;
    protected override string[] StepsToFabricatorTab => CbDatabase.BatteryCraftPath;

    public override RecipeData GetBlueprintRecipe()
    {
        var partsList = new List<Ingredient>();

        CreateIngredients(Parts, partsList);

        if (partsList.Count == 0)
            partsList.Add(new Ingredient(TechType.Titanium, 1));

        var batteryBlueprint = new RecipeData
        {
            craftAmount = 1,
            Ingredients = partsList
        };

        return batteryBlueprint;
    }

    protected override void AddToList()
    {
        CbDatabase.BatteryItems.Add(this);
        CbDatabase.TrackItems.Add(TechType);
    }
}