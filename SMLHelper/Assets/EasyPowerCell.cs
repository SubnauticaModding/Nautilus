namespace SMLHelper.Assets;

using System.Collections.Generic;
using Crafting;
using API;
#if SUBNAUTICA
using RecipeData = Crafting.TechData;
#endif

/// <summary>
/// A simple to make PowerCell preconfigured to be added to the fabricator and enable use in the Powercell Charger.
/// </summary>
public class EasyPowerCell: CbCore
{
    private readonly EasyBattery baseBattery;

    /// <summary>
    /// A super simple system to add a new PowerCell to the game.
    /// </summary>
    /// <param name="classId">The id that will become the TechType string that people will use to spawn it using the console.</param>
    /// <param name="displayName">The ingame name visable to players.</param>
    /// <param name="description">The description shown on tooltips ingame.</param>
    /// <param name="customBattery">The EasyBattery instance to base the EasyPowerCell off of.</param>
    public EasyPowerCell(string classId, string displayName, string description, EasyBattery customBattery) : base(classId, displayName, description, customBattery.UsingIonCellSkins)
    {
        baseBattery = customBattery;
        PrefabType = customBattery.UsingIonCellSkins ? TechType.PrecursorIonPowerCell : TechType.PowerCell;
    }

    /// <summary>
    /// A super simple system to add a new PowerCell to the game.
    /// </summary>
    /// <param name="classId">The id that will become the TechType string that people will use to spawn it using the console.</param>
    /// <param name="displayName">The ingame name visable to players.</param>
    /// <param name="description">The description shown on tooltips ingame.</param>
    /// <param name="usingIonCellSkins">If this PowerCell should use the Ion Cells model or the Regular model.</param>
    public EasyPowerCell(string classId, string displayName, string description, bool usingIonCellSkins) : base(classId, displayName, description, usingIonCellSkins)
    {
        PowerCellIconFile = $"{ClassID}.png";
        PrefabType = usingIonCellSkins ? TechType.PrecursorIonPowerCell : TechType.PowerCell;
    }

    public string PowerCellIconFile;

    /// <summary>
    /// Override with the file name for this item's icon.
    /// If not overriden, this defaults to "[this item's ClassID].png".
    /// </summary>
    /// <example>"MyClassID.png"</example>
    public override string IconFileName => PowerCellIconFile;


    /// <summary>
    /// Sets the location in the fabricator to place the craft node. Only matters if <see cref="CbCore.AddToFabricator"/> is true.
    /// </summary>
    public override string[] StepsToFabricatorTab => FabricatorType == CraftTree.Type.Fabricator? CbDatabase.PowCellCraftPath: new string [0];

    /// <summary>
    /// The techtype required to unlock this battery.
    /// </summary>
    public TechType UnlocksWith = TechType.None;

    /// <summary>
    /// Sets the Required unlock to use <see cref="UnlocksWith"/> field that can be set instead of overridden.
    /// </summary>
    public sealed override TechType RequiredForUnlock => UnlocksWith;

    /// <summary>
    /// Sets the Recipe to use the <see cref="CbCore.Parts"/> field that can be set instead of overridden.
    /// </summary>
    protected override RecipeData GetBlueprintRecipe()
    {
        List<Ingredient> partsList = new();
        if((Parts == null || Parts.Count == 0) && baseBattery != null)
        {
            partsList.Add(new Ingredient(baseBattery.TechType, 2));
            partsList.Add(new Ingredient(TechType.Silicone, 1));
        }
        else
        {
            CreateIngredients(Parts, partsList);
        }


        var batteryBlueprint = new RecipeData
        {
            craftAmount = 1,
            Ingredients = partsList
        };

        return batteryBlueprint;
    }

}