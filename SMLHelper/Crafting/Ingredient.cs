#if SUBNAUTICA
#pragma warning disable IDE1006 // Naming Styles - Ignored for backwards compatibility
namespace SMLHelper.V2.Crafting
{
    /// <summary>
    /// A class for representing a required ingredient in a recipe.<para/>
    /// This class exits to replicate the original one in the game assembly that is both private and sealed.
    /// </summary>
    /// <seealso cref="IIngredient" />
    /// <seealso cref="TechData"/>
    public class Ingredient : IIngredient
    {
        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        /// <value>
        /// The item ID.
        /// </value>
        public TechType techType { get; set; }

        /// <summary>
        /// Gets or sets the number of this item required for the recipe.
        /// </summary>
        /// <value>
        /// The amount of this item needed for the recipe.
        /// </value>
        public int amount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ingredient"/> class.
        /// </summary>
        /// <param name="techType">The item ID.</param>
        /// <param name="amount">The number of instances of this item required for the recipe.</param>
        public Ingredient(TechType techType, int amount)
        {
            this.techType = techType;
            this.amount = amount;
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
#endif