#if SUBNAUTICA
namespace SMLHelper.V2.Handlers
{
    using Patchers;
    using Interfaces;

    /// <summary>
    /// a common handler for uses specified to the Survival component
    /// </summary>
    public class SurvivalHandler : ISurvivalHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static ISurvivalHandler Main { get; } = new SurvivalHandler();

        private SurvivalHandler()
        {
            // Hides constructor
        }
        /// <summary>
        /// <para>makes the item gives oxygen on use.</para>
        /// </summary>
        /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
        /// <param name="oxygenGiven">the oxygen amount the item gives</param>
        void ISurvivalHandler.GiveOxygenOnUse(TechType techType, float oxygenGiven)
        {
            SurvivalPatcher.CustomOxygenOutputsOnUse.Add(techType, oxygenGiven);
            SurvivalPatcher.InventoryUseables.Add(techType);
        }
        /// <summary>
        /// <para>makes the item gives oxygen on eat.</para>
        /// <para>
        /// must have the
        /// <seealso cref="Eatable"/>
        /// Component attached to it in order to work.
        /// </para>
        /// </summary>
        /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
        /// <param name="oxygenGiven">the oxygen amount the item gives</param>
        void ISurvivalHandler.GiveOxygenOnEat(TechType techType, float oxygenGiven)
        {
            SurvivalPatcher.CustomOxygenOutputsOnEat.Add(techType, oxygenGiven);
        }
        /// <summary>
        /// <para>makes the item Heal the player on use.</para>
        /// </summary>
        /// <param name="techType">the TechType that you want it to heal back</param>
        /// <param name="healthBack">amount to heal the player</param>
        void ISurvivalHandler.GiveHealthOnUse(TechType techType, float healthBack)
        {
            SurvivalPatcher.CustomHealersOnUse.Add(techType, healthBack);
            SurvivalPatcher.InventoryUseables.Add(techType);
        }
        /// <summary>
        /// makes the item Heal the player on eat.
        /// <para>
        /// must have the
        /// <seealso cref="Eatable"/>
        /// Component attached to it in order to work.
        /// </para>
        /// </summary>
        /// <param name="techType">the TechType that you want it to heal back</param>
        /// <param name="healthBack">amount to heal the player</param>
        void ISurvivalHandler.GiveHealthOnEat(TechType techType, float healthBack)
        {
            SurvivalPatcher.CustomHealersOnEat.Add(techType, healthBack);
        }
        /// <summary>
        /// <para>makes the item gives oxygen on use.</para>
        /// </summary>
        /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
        /// <param name="oxygenGiven">the oxygen amount the item gives</param>
        public static void GiveOxygenOnUse(TechType techType, float oxygenGiven)
        {
            Main.GiveOxygenOnUse(techType, oxygenGiven);
        }
        /// <summary>
        /// <para>makes the item gives oxygen on eat.</para>
        /// <para>
        /// must have the
        /// <seealso cref="Eatable"/>
        /// Component attached to it in order to work.
        /// </para>
        /// </summary>
        /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
        /// <param name="oxygenGiven">the oxygen amount the item gives</param>
        public static void GiveOxygenOnEat(TechType techType, float oxygenGiven)
        {
            Main.GiveOxygenOnEat(techType, oxygenGiven);
        }
        /// <summary>
        /// <para>makes the item Heal the player on use.</para>
        /// </summary>
        /// <param name="techType">the TechType that you want it to heal back</param>
        /// <param name="healthBack">amount to heal the player</param>
        public static void GiveHealthOnUse(TechType techType, float healthBack)
        {
            Main.GiveHealthOnUse(techType, healthBack);
        }
        /// <summary>
        /// makes the item Heal the player on eat.
        /// <para>
        /// must have the
        /// <seealso cref="Eatable"/>
        /// Component attached to it in order to work.
        /// </para>
        /// </summary>
        /// <param name="techType">the TechType that you want it to heal back</param>
        /// <param name="healthBack">amount to heal the player</param>
        public static void GiveHealthOnEat(TechType techType, float healthBack)
        {
            Main.GiveHealthOnEat(techType, healthBack);
        }
    }
}
#endif