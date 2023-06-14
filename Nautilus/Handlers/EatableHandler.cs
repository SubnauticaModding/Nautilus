using Nautilus.Patchers;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for modyfing the data of edible objects (objects with the <see cref="Eatable"/> component).
/// </summary>
public static class EatableHandler 
{
#if SUBNAUTICA
    /// <summary>
    /// Use this to change the values of a specific TechType.
    /// </summary>
    /// <param name="item">The TechType of the item you want to change.</param>
    /// <param name="food">The food value you want to change it to.</param>
    /// <param name="water">The water value you want to change it to.</param>
    /// <param name="decomposes">Whether or not the item decomposes over time</param>
    public static void ModifyEatable(TechType item, float food, float water, bool decomposes)
    {
        EatablePatcher.EditedEatables.Add(item, new EditedEatableValues()
        {
            food = food,
            water = water,
            decomposes = decomposes,
        });
    }

#elif BELOWZERO 
        /// <summary>
        /// Use this to change the values of a specific TechType
        /// </summary>
        /// <param name="item">the techtype of the item you want to change</param>
        /// <param name="food">the food value you want to change it to</param>
        /// <param name="water">the water value you want to change it to</param>
        /// <param name="decomposes">whether or not the item decomposes over time</param>
        /// <param name="health">how much you want to set the health gained from eating this item to</param>
        /// <param name="coldValue">How much eating this item changes the current cold meter value.<br/>
        /// Negative values heats up the player while positive values makes the player colder.</param>
        /// <param name="maxCharges">how many times the item can be used before being consumed</param>
        public static void ModifyEatable(TechType item, float food, float water, bool decomposes, float health, float coldValue, int maxCharges)
        {
            EatablePatcher.EditedEatables.Add(item, new EditedEatableValues()
            {
                food = food,
                water = water,
                decomposes = decomposes,
                health = health,
                maxCharges = maxCharges,
                coldValue = coldValue
            });
        }
#endif
    internal class EditedEatableValues
    {
        public bool decomposes;
        public float food;
        public float water;
#if BELOWZERO
            public float health;
            public int maxCharges;
            public float coldValue;
#endif
    }
}