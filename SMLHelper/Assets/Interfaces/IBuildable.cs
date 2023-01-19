namespace SMLHelper.Assets.Interfaces
{
    using SMLHelper.Crafting;

    public interface IBuildable: IPDAInfo
    {
        /// <summary>
        /// The Recipe that the game uses for this Prefab.
        /// </summary>
        RecipeData RecipeData { get; }
    }
}
