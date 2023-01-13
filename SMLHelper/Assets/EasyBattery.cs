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
public class EasyBattery : CbCore
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="classId">The id that will become the TechType string that people will use to spawn it using the console.</param>
    /// <param name="displayName">The ingame name visable to players.</param>
    /// <param name="description">The description shown on tooltips ingame.</param>
    /// <param name="ionCellSkins">whether to base the batteries looks off of the Ion Battery or the regular one.</param>
    public EasyBattery(string classId, string displayName, string description, bool ionCellSkins = false) : base(classId, displayName, description, ionCellSkins)
    {
        BatteryIconFile = $"{ClassID}.png";
        PrefabType = ionCellSkins ? TechType.PrecursorIonBattery : TechType.Battery;
    }

    public string BatteryIconFile;

    /// <summary>
    /// Override with the file name for this item's icon.
    /// If not overriden, this defaults to "[this item's ClassID].png".
    /// </summary>
    /// <example>"MyClassID.png"</example>
    public override string IconFileName => BatteryIconFile;

    /// <summary>
    /// Sets the location in the fabricator to place the craft node. Only matters if <see cref="CbCore.AddToFabricator"/> is true.
    /// </summary>
    public sealed override string[] StepsToFabricatorTab => CbDatabase.BatteryCraftPath;

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
    protected sealed override RecipeData GetBlueprintRecipe()
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

}