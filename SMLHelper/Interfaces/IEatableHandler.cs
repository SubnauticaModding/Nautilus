using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMLHelper.V2.Interfaces
{
    /// <summary>
    /// A handler with common methods for changing edible items values.
    /// </summary>
    public interface IEatableHandler
    {
#if SUBNAUTICA
        /// <summary>
        /// Allows you to change the water/food values of a specific item
        /// </summary>
        /// <param name="item">The techtype of the item you wish to edit</param>
        /// <param name="food">The food value you want the item to have</param>
        /// <param name="water">The water value you want the item to have</param>
        /// <param name="decomposes">Whether or not the item decomposes over time, losing food and water values in the process</param>
        /// <param name="overfill">Whether or not the item allows the player's food value to go above 100 when consumed</param>
        void ModifyEatable(TechType item, float food, float water, bool decomposes = true, bool overfill = true);
#elif BELOWZERO
        /// <summary>
        /// use this to change the values of a specific techtype
        /// </summary>
        /// <param name="item">the techtype of the item you want to change</param>
        /// <param name="food">the food value you want to change it to</param>
        /// <param name="water">the water value you want to change it to</param>
        /// <param name="health">how much you want to set the health gained from eating this item to</param>
        /// <param name="coldValue">How much eating this item changes the current cold meter value. Cold meter goes from 0 to 100.</param>
        /// <param name="decomposes">whether or not the item decomposes over time</param>
        /// <param name="maxCharges">how many times the item can be used before being consumed</param>
        void ModifyEatable(TechType item, float food, float water, float health, float coldValue, bool decomposes, int maxCharges = 0);
#endif
    }
}
