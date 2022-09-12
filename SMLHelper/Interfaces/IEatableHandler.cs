namespace SMLHelper.V2.Interfaces
{
    /// <summary>
    /// A handler with common methods for changing edible items values.
    /// </summary>
    public interface IEatableHandler
    {
#if SUBNAUTICA
        /// <summary>
        /// Allows you to change the water/food values of a specific item.
        /// </summary>
        /// <param name="item">The TechType of the item you wish to edit.</param>
        /// <param name="food">The food value you want the item to have.</param>
        /// <param name="water">The water value you want the item to have.</param>
        /// <param name="decomposes">Whether or not the item decomposes over time, losing food and water values in the process.</param>
        void ModifyEatable(TechType item, float food, float water, bool decomposes = true);
        /// <summary>
        /// Allows you to make an item that isn't normally edible edible, with specific values
        /// </summary>
        /// <param name="item">The TechType of the item you wish to make edible.</param>
        /// <param name="food">The food value you want the item to have.</param>
        /// <param name="water">The water value you want the item to have.</param>
        /// <param name="decomposes">Whether or not the item decomposes over time, losing food and water values in the process.</param>
        void MakeEatable(TechType item, float food, float water, bool decomposes = true);
#elif BELOWZERO
        /// <summary>
        /// Use this to change the values of a specific TechType.
        /// </summary>
        /// <param name="item">The TechType of the item you want to change.</param>
        /// <param name="food">The food value you want to change it to.</param>
        /// <param name="water">The water value you want to change it to.</param>
        /// <param name="decomposes">Whether or not the item decomposes over time.</param>
        /// <param name="health">How much you want to set the health gained from eating this item to.</param>
        /// <param name="coldValue">How much eating this item changes the current cold meter value.<br/>
        /// Negative values heats up the player while positive values makes the player colder.</param>
        /// <param name="maxCharges">How many times the item can be used before being consumed.</param>
        void ModifyEatable(TechType item, float food, float water, bool decomposes = true, float health = 0f, float coldValue = 0f, int maxCharges = 0);
        /// <summary>
        /// Allows you to make an item that isn't normally edible edible, with specific values
        /// </summary>
        /// <param name="item">The TechType of the item you wish to make edible.</param>
        /// <param name="food">The food value you want to change it to.</param>
        /// <param name="water">The water value you want to change it to.</param>
        /// <param name="decomposes">Whether or not the item decomposes over time.</param>
        /// <param name="health">How much you want to set the health gained from eating this item to.</param>
        /// <param name="coldValue">How much eating this item changes the current cold meter value.<br/>
        /// Negative values heats up the player while positive values makes the player colder.</param>
        /// <param name="maxCharges">How many times the item can be used before being consumed.</param>
        void MakeEatable(TechType item, float food, float water, bool decomposes = true, float health = 0f, float coldValue = 0f, int maxCharges = 0);
#endif
    }
}
