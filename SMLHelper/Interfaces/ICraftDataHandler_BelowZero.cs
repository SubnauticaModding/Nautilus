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
        /// <para>Allows you to edit JsonValues Directly for TechTypes.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to edit.</param>
        /// <param name="jsonValue">The JsonValue for that TechType.</param>
        /// <seealso cref="TechData.defaults"/>
        void SetTechData(TechType techType, JsonValue jsonValue);

        /// <summary>
        /// Safely accesses the crafting data from a modded item.<para/>
        /// WARNING: This method is highly dependent on mod load order. 
        /// Make sure your mod is loading after the mod whose TechData you are trying to access.
        /// </summary>
        /// <param name="techType">The TechType whose TechData you want to access.</param>
        /// <returns>The JsonValue from the modded item if it exists; Otherwise, returns <c>null</c>.</returns>
        JsonValue GetModdedTechData(TechType techType);

        /// <summary>
        /// <para>Allows you to add ingredients for a TechType crafting recipe.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose ingredient list you want to edit.</param>
        /// <param name="ingredients">The collection of Ingredients for that TechType.</param>
        /// <seealso cref="Ingredient"/>
        void AddIngredients(TechType techType, ICollection<Ingredient> ingredients);

        /// <summary>
        /// <para>Allows you to add linked items for a TechType crafting recipe.</para>
        /// <para>Can be used for existing TechTypes too.</para>
        /// </summary>
        /// <param name="techType">The TechType whose ingredient list you want to edit.</param>
        /// <param name="linkedItems">The collection of linked items for that TechType</param>
        void AddLinkedItems(TechType techType, ICollection<TechType> linkedItems);
    }
}
#endif
