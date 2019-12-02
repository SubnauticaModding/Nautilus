#if SUBNAUTICA
#pragma warning disable IDE1006 // Naming Styles - Ignored for backwards compatibility
namespace SMLHelper.V2.Crafting
{
    using System.Collections.Generic;

    /// <summary>
    /// A class that fully describes a recipe for a <see cref="TechType"/> identified item.
    /// </summary>
    /// <seealso cref="ITechData" />
    public class TechData : BlueprintDataBase<Ingredient>, ITechData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TechData"/> class a custom recipe.
        /// </summary>
        public TechData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechData"/> class for a custom recipe with a list of ingridients.
        /// </summary>
        /// <param name="ingredients">The ingredients.</param>
        public TechData(List<Ingredient> ingredients)
            : base(ingredients)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechData"/> class for a custom recipe with a collection of ingridients.
        /// </summary>
        /// <param name="ingredients">The ingredients.</param>
        public TechData(params Ingredient[] ingredients)
            : base(ingredients)
        {            
        }

        /// <summary>
        /// Gets the ingredient at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The <see cref="IIngredient"/> at the requested the index if the index is value; Otherwise returns null.</returns>
        public IIngredient GetIngredient(int index)
        {
            return GetIngredientAtIndex(index);
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
#endif