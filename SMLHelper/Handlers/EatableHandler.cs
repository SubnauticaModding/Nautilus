namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;

    /// <summary>
    /// A handler for editing values for eatable classes
    /// </summary>
    public class EatableHandler : IEatableHandler
    {
        private static bool pickupablePatched = false;
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IEatableHandler Main = new EatableHandler();

        private EatableHandler()
        {
            // hides constructor.
        }

#if SUBNAUTICA
        void IEatableHandler.ModifyEatable(TechType item, float food, float water, bool decomposes)
        {
            EatablePatcher.EditedEatables.Add(item, new EditedEatableValues()
            {
                food = food,
                water = water,
                decomposes = decomposes,
            });
        }
        void IEatableHandler.MakeEatable(TechType item, float food, float water, bool decomposes)
        {
            if(!pickupablePatched)
                PickupablePatcher.Patch(Initializer.harmony);

            PickupablePatcher.AddedEatables.Add(item, new EditedEatableValues()
            {
                food = food,
                water = water,
                decomposes = decomposes,
            });
        }
        /// <summary>
        /// Use this to change the values of a specific TechType.
        /// </summary>
        /// <param name="item">The TechType of the item you want to change.</param>
        /// <param name="food">The food value you want to change it to.</param>
        /// <param name="water">The water value you want to change it to.</param>
        /// <param name="decomposes">Whether or not the item decomposes over time</param>
        public static void ModifyEatable(TechType item, float food, float water, bool decomposes = true)
        {
            Main.ModifyEatable(item, food, water, decomposes);
        }
        /// <summary>
        /// Allows you to make an item that isn't normally edible edible, with specific values
        /// </summary>
        /// <param name="item">The TechType of the item you wish to make edible.</param>
        /// <param name="food">The food value you want to change it to.</param>
        /// <param name="water">The water value you want to change it to.</param>
        /// <param name="decomposes">Whether or not the item decomposes over time</param>
        public static void MakeEatable(TechType item, float food, float water, bool decomposes = true)
        {
            Main.MakeEatable(item, food, water, decomposes);
        }
#elif BELOWZERO 
        void IEatableHandler.ModifyEatable(TechType item, float food, float water, bool decomposes, float health, float coldValue, int maxCharges)
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
        void IEatableHandler.MakeEatable(TechType item, float food, float water, bool decomposes, float health, float coldValue, int maxCharges)
        {
            if(!pickupablePatched)
                PickupablePatcher.Patch(Initializer.harmony);

            PickupablePatcher.AddedEatables.Add(item, new EditedEatableValues()
            {
                food = food,
                water = water,
                decomposes = decomposes,
                health = health,
                maxCharges = maxCharges,
                coldValue = coldValue
            });
        }

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
        public static void ModifyEatable(TechType item, float food, float water, bool decomposes = true, float health = 0f, float coldValue = 0f, int maxCharges = 0)
        {
            Main.ModifyEatable(item, food, water, decomposes, health, coldValue, maxCharges);
        }
        /// <summary>
        /// Allows you to make an item that isn't normally edible edible, with specific values
        /// </summary>
        /// <param name="item">The TechType of the item you wish to make edible.</param>
        /// <param name="food">the food value you want to change it to</param>
        /// <param name="water">the water value you want to change it to</param>
        /// <param name="decomposes">whether or not the item decomposes over time</param>
        /// <param name="health">how much you want to set the health gained from eating this item to</param>
        /// <param name="coldValue">How much eating this item changes the current cold meter value.<br/>
        /// Negative values heats up the player while positive values makes the player colder.</param>
        /// <param name="maxCharges">how many times the item can be used before being consumed</param>
        public static void MakeEatable(TechType item, float food, float water, bool decomposes = true, float health = 0f, float coldValue = 0f, int maxCharges = 0)
        {
            Main.MakeEatable(item, food, water, decomposes, health, coldValue, maxCharges);
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
}
