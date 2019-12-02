namespace SMLHelper.V2.Crafting
{
    using System.Collections.Generic;

#pragma warning disable IDE1006 // Naming Styles - Ignored for backwards compatibility

    /// <summary>
    /// Base class of common behavior between RecipeData (BelowZero) and TechData (Subnautica).
    /// </summary>
    /// <typeparam name="IngredientType">The type of the ngredient type.</typeparam>
    public abstract class BlueprintBase<IngredientType>
        where IngredientType : class
    {
        /// <summary>
        /// Gets or sets the how many copies of the item are created when crafting this recipe.
        /// </summary>
        /// <value>
        /// The quantity of the item this recipe yields.
        /// </value>
        public int craftAmount { get; set; }

        /// <summary>
        /// Gets the number of different ingredients for this recipe.
        /// </summary>
        /// <value>
        /// The number of ingredients for this recipe.
        /// </value>
        public int ingredientCount => Ingredients.Count;

        /// <summary>
        /// Gets the number of items linked to this recipe.
        /// </summary>
        /// <value>
        /// The number of linked items.
        /// </value>
        public int linkedItemCount => LinkedItems.Count;

        /// <summary>
        /// The list of ingredients required for this recipe.
        /// </summary>
        public List<IngredientType> Ingredients = new List<IngredientType>();

        /// <summary>
        /// The items that will also be created when this recipe is crafted.
        /// </summary>
        public List<TechType> LinkedItems = new List<TechType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TechData"/> class a custom recipe.
        /// </summary>
        protected BlueprintBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechData"/> class for a custom recipe with a list of ingridients.
        /// </summary>
        /// <param name="ingredients">The ingredients.</param>
        protected BlueprintBase(List<IngredientType> ingredients)
        {
            Ingredients = ingredients;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechData"/> class for a custom recipe with a collection of ingridients.
        /// </summary>
        /// <param name="ingredients">The ingredients.</param>
        protected BlueprintBase(params IngredientType[] ingredients)
        {
            Ingredients.AddRange(ingredients);
        }

        /// <summary>
        /// Gets the linked item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="TechType"/> at the requested the index if the index is value; Otherwise returns null.</returns>
        public TechType GetLinkedItem(int index)
        {
            if (LinkedItems != null && LinkedItems.Count > index)
            {
                return LinkedItems[index];
            }

            return TechType.None;
        }

        /// <summary>
        /// Gets the ingredient at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The ingredient at the requested the index if the index is value; Otherwise returns null.</returns>
        protected IngredientType GetIngredientAtIndex(int index)
        {
            if (Ingredients != null && Ingredients.Count > index)
            {
                return Ingredients[index];
            }

            return null;
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
