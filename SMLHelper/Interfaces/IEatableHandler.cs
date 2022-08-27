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
        /// <summary>
        /// Allows you to change the water/food values of a specific item
        /// </summary>
        /// <param name="item">The techtype of the item you wish to edit</param>
        /// <param name="food">The food value you want the item to have</param>
        /// <param name="water">The water value you want the item to have</param>
        /// <param name="decomposes">Whether or not the item decomposes over time, losing food and water values in the process</param>
        /// <param name="overfill">Whether or not the item allows the player's food value to go above 100 when consumed</param>
        void ModifyEatable(TechType item, int food, int water, bool decomposes = true, bool overfill = true);
    }
}
