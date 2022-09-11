using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Patchers;

namespace SMLHelper.V2.Handlers
{
    /// <summary>
    /// a handler for editing values for eatable classes
    /// </summary>
    public class EatableHandler : IEatableHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IEatableHandler Main = new EatableHandler();

        private EatableHandler()
        {
            //hides constructor. no idea if necessary, just saw it in bio reactor handler and threw it in because I thought it was good idea
        }
#if SUBNAUTICA
        void IEatableHandler.ModifyEatable(TechType item, float food, float water, bool decomposes, bool overfill)
        {
            ModifyEatable(item, food, water, decomposes, overfill);
        }
        /// <summary>
        /// use this to change the values of a specific techtype
        /// </summary>
        /// <param name="item">the techtype of the item you want to change</param>
        /// <param name="food">the food value you want to change it to</param>
        /// <param name="water">the water value you want to change it to</param>
        /// <param name="decomposes">whether or not the item decomposes over time</param>
        /// <param name="overfill">whether or not this item allows the player's food to go above 100</param>
        public static void ModifyEatable(TechType item, float food, float water, bool decomposes = true, bool overfill = true)
        {
            EatablePatcher.EditedEatables.Add(item, new EditedEatableValues()
            {
                food = food,
                water = water,
                decomposes = decomposes,
                allowOverfill = overfill
            });
        }
#elif BELOWZERO 
        void IEatableHandler.ModifyEatable(TechType item, float food, float water, float health, float coldValue, bool decomposes, int maxCharges = 0)
        {
            ModifyEatable(item, food, water, health, coldValue, decomposes, maxCharges);
        }
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
        public static void ModifyEatable(TechType item, float food, float water, float health, float coldValue, bool decomposes = true, int maxCharges = 0)
        {
            EatablePatcher.EditedEatables.Add(item, new EditedEatableValues()
            {
                food = food,
                water = water,
                decomposes = decomposes
            });
        }
#endif
        internal class EditedEatableValues
        {
            public bool decomposes;
            public float food;
            public float water;
#if SUBNAUTICA
            public bool allowOverfill;
#elif BELOWZERO
            public float health;
            public int maxcharges;
            public float coldValue;
#endif
        }
    }
}
