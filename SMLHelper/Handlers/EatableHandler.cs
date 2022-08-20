using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Patchers;

namespace SMLHelper.V2.Handlers
{
    /// <summary>
    /// a handler for editing values for eatable classes
    /// </summary>
    public class EatableHandler
    {
        /// <summary>
        /// use this to change the values of a specific techtype
        /// </summary>
        /// <param name="item">the techtype of the item you want to change</param>
        /// <param name="food">the food value you want to change it to</param>
        /// <param name="water">the water value you want to change it to</param>
        /// <param name="decomposes">whether or not the item decomposes over time</param>
        /// <param name="overfill">whether or not this item allows the player's food to go above 100</param>
        public static void ModifyEatable(TechType item, int food, int water, bool decomposes = true, bool overfill = false)
        {
            EatablePatcher.EditedEatables.Add(item, new EditedEatableValues()
            {
                food = food,
                water = water,
                allowOverfill = overfill,
                decomposes = decomposes
            });
        }

        internal class EditedEatableValues
        {
            public bool decomposes;
            public bool allowOverfill;
            public int food;
            public int water;
        }
    }
}
