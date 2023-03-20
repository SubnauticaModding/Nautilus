using System.Diagnostics.CodeAnalysis;
using SMLHelper.Assets.Gadgets;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
using SMLHelper.Utility;

namespace SMLHelper.Assets.Gadgets;

/// <summary>
/// Represents a crafting gadget
/// </summary>
public class CraftingGadget : Gadget
{
    /// <summary>
    /// The crafting recipe to add.
    /// </summary>
    public required RecipeData RecipeData { get; set; }
    
    /// <summary>
    /// Craft Tree this node will appear in.
    /// </summary>
    public CraftTree.Type FabricatorType { get; set; }
    
    /// <summary>
    /// The steps to get to a tab you want this node to appear in.<br/>
    /// If null or empty, it will instead appear at the craft tree's root.
    /// </summary>
    public string[] StepsToFabricatorTab { get; set; }
    
    /// <summary>
    /// The amount of seconds it takes to craft this item.
    /// Values equal to or less than zero will be ignored.
    /// </summary>
    public float CraftingTime { get; set; }
    
    /// <summary>
    /// Constructs a crafting gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    /// <param name="techData">The crafting recipe to add.</param>
    [SetsRequiredMembers]
    public CraftingGadget(ICustomPrefab prefab, RecipeData recipeData) : base(prefab)
    {
        RecipeData = recipeData;
    }

    /// <summary>
    /// Adds this prefab to a CraftTree type.
    /// </summary>
    /// <param name="fabricatorType">The craft tree type to add this crafting node to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public CraftingGadget WithFabricatorType(CraftTree.Type fabricatorType)
    {
        this.FabricatorType = fabricatorType;
        return this;
    }

    /// <summary>
    /// Adds this node to a specific tab you want it to appear in.
    /// </summary>
    /// <param name="stepsToFabricator">The steps required to get to the tab in question.<br/>
    /// If null, it will appear at the craft tree's root.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public CraftingGadget WithStepsToFabricatorTab(params string[] stepsToFabricator)
    {
        StepsToFabricatorTab = stepsToFabricator;
        return this;
    }

    /// <summary>
    /// The amount of seconds it takes to craft this item.
    /// Values equal to or less than zero will be ignored.
    /// </summary>
    /// <param name="craftingTime">Amount of seconds</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public CraftingGadget WithCraftingTime(float craftingTime)
    {
        CraftingTime = craftingTime;
        return this;
    }

    /// <inheritdoc/>
    protected internal override void Build()
    {
        if (prefab.Info.TechType is TechType.None)
        {
            InternalLogger.Error($"Prefab '{prefab.Info}' does not contain a TechType. Skipping {nameof(CraftingGadget)} build.");
            return;
        }
        
        CraftDataHandler.SetRecipeData(prefab.Info.TechType, RecipeData);
        
        if (FabricatorType == CraftTree.Type.None)
        {
            InternalLogger.Log($"Prefab '{prefab.Info.ClassID}' was not automatically registered into a crafting tree.");
        }
        else
        {
            if (StepsToFabricatorTab == null || StepsToFabricatorTab.Length == 0)
            {
                CraftTreeHandler.AddCraftingNode(FabricatorType, prefab.Info.TechType);
            }
            else
            {
                CraftTreeHandler.AddCraftingNode(FabricatorType, prefab.Info.TechType, StepsToFabricatorTab);
            }
        }

        if (CraftingTime >= 0f)
        {
            CraftDataHandler.SetCraftingTime(prefab.Info.TechType, CraftingTime);
        }
    }
}