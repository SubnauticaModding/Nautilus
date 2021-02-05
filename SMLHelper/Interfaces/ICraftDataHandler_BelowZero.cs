#if BELOWZERO
namespace SMLHelper.V2.Interfaces
{
    using System.Collections.Generic;
    using Crafting;

    public partial interface ICraftDataHandler
    {
        /// <summary>
        /// <para>Allows you to edit RecipeData for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="techData">The TechData for that TechType.</param>
        /// <seealso cref="RecipeData"/>
        void SetTechData(TechType techType, RecipeData techData);

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The RecipeData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        RecipeData GetRecipeData(TechType techType);

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The RecipeData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        RecipeData GetTechData(TechType techType);

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The RecipeData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        RecipeData GetModdedRecipeData(TechType techType);

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The RecipeData from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        RecipeData GetModdedTechData(TechType techType);

        /// <summary>
        /// <para>Allows you to set ingredients for a TechType crafting recipe.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose ingredient list you want to edit.</param>
        /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        void SetIngredients(TechType techType, ICollection<Ingredient> ingredients);

        /// <summary>
        /// <para>Allows you to set linked items for a TechType crafting recipe.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose ingredient list you want to edit.</param>
        /// <param name="linkedItems">The collection of linked items for that TechType</param>
        void SetLinkedItems(TechType techType, ICollection<TechType> linkedItems);


        /// <summary>
        /// <para>Allows you to edit the Cold Resistance for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose Cold Resistance you want to edit.</param>
        /// <param name="resistance">The int value for the Cold Resistance.</param>
        void SetColdResistance(TechType techType, int resistance);
    }
}
#endif
