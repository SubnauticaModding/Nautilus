namespace SMLHelper.Assets.Interfaces
{
    using SMLHelper.Crafting;

    public interface ICraftable
    {
        /// <summary>
        /// The Recipe that the game uses to craft this Prefab.
        /// </summary>
        RecipeData RecipeData { get; }

        /// <summary>
        /// What fabricator to patch the crafting node into.
        /// </summary>
        CraftTree.Type FabricatorType { get; }

        /// <summary>
        /// Where in the specified fabricator to place the new craft node.
        /// </summary>
        string[] StepsToFabricatorTab { get; }

        /// <summary>
        /// How long it takes the fabricator to craft the recipe.
        /// </summary>
        float CraftingTime { get; }

    }
}
