#if SUBNAUTICA
namespace SMLHelper.V2.Interfaces
{
    /// <summary>
    /// a handler for common uses to the Survival component
    /// </summary>
    public interface ISurvivalHandler
    {
        /// <summary>
        /// <para>makes the item gives oxygen on use.</para>
        /// </summary>
        /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
        /// <param name="oxygenGiven">the oxygen amount the item gives</param>
        void GiveOxygenOnUse(TechType techType, float oxygenGiven);
        /// <summary>
        /// <para>makes the item Heal the player on use.</para>
        /// </summary>
        /// <param name="techType">the TechType that you want it to heal back</param>
        /// <param name="healthBack">amount to heal the player</param>
        void GiveHealthOnUse(TechType techType, float healthBack);
        /// <summary>
        /// <para>makes the item gives oxygen on use.</para>
        /// <para>
        /// must have the
        /// <seealso cref="Eatable"/>
        /// Component attached to the item in order to work.
        /// </para>
        /// </summary>
        /// <param name="techType">the TechType that you want to make it give oxygen on use</param>
        /// <param name="oxygenGiven">the oxygen amount the item gives</param>
        void GiveOxygenOnEat(TechType techType, float oxygenGiven);
        /// <summary>
        /// makes the item Heal the player on eat.
        /// <para>
        /// must have the
        /// <seealso cref="Eatable"/>
        /// Component attached to the item in order to work.
        /// </para>
        /// </summary>
        /// <param name="techType">the TechType that you want it to heal back</param>
        /// <param name="healthBack">amount to heal the player</param>
        void GiveHealthOnEat(TechType techType, float healthBack);
    }
}
#endif
