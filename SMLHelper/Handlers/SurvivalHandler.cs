#if SUBNAUTICA
namespace SMLHelper.V2.Handlers
{
    using System;
    using System.Collections.Generic;
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
            if (SurvivalPatcher.SurvivalDictionaryOnUse.TryGetValue(techType, out List<Action> action))
            {
                action.Add(() => { Player.main.GetComponent<OxygenManager>().AddOxygen(oxygenGiven); }); // add an action to the list
                return;
            }

            // if we reach to this point then the techtype doesn't exist in the dictionary so we add it
            SurvivalPatcher.SurvivalDictionaryOnUse[techType] = new List<Action>()
            {
                () =>
                {
                    Player.main.GetComponent<OxygenManager>().AddOxygen(oxygenGiven);
                }
            };
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
            if (SurvivalPatcher.SurvivalDictionaryOnEat.TryGetValue(techType, out List<Action> action))
            {
                action.Add(() => { Player.main.GetComponent<OxygenManager>().AddOxygen(oxygenGiven); }); // add an action to the list
                return;
            }

            // if we reach to this point then the techtype doesn't exist in the dictionary so we add it
            SurvivalPatcher.SurvivalDictionaryOnEat[techType] = new List<Action>()
            {
                () =>
                {
                    Player.main.GetComponent<OxygenManager>().AddOxygen(oxygenGiven);
                }
            };
        }
        /// <summary>
        /// <para>makes the item Heal the player on use.</para>
        /// </summary>
        /// <param name="techType">the TechType that you want it to heal back</param>
        /// <param name="healthBack">amount to heal the player</param>
        void ISurvivalHandler.GiveHealthOnUse(TechType techType, float healthBack)
        {
            if (SurvivalPatcher.SurvivalDictionaryOnUse.TryGetValue(techType, out List<Action> action))
            {
                action.Add(() => { Player.main.GetComponent<LiveMixin>().AddHealth(healthBack); }); // add an action to the list
                return;
            }

            // if we reach to this point then the techtype doesn't exist in the dictionary so we add it
            SurvivalPatcher.SurvivalDictionaryOnUse[techType] = new List<Action>()
            {
                () =>
                {
                    Player.main.GetComponent<LiveMixin>().AddHealth(healthBack);
                }
            };
            SurvivalPatcher.InventoryUseables.Add(techType);
        }
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
        void ISurvivalHandler.GiveHealthOnEat(TechType techType, float healthBack)
        {
            if (SurvivalPatcher.SurvivalDictionaryOnEat.TryGetValue(techType, out List<Action> action))
            {
                action.Add(() => { Player.main.GetComponent<LiveMixin>().AddHealth(healthBack); }); // add an action to the list
                return;
            }

            // if we reach to this point then the techtype doesn't exist in the dictionary so we add it
            SurvivalPatcher.SurvivalDictionaryOnEat[techType] = new List<Action>()
            {
                () =>
                {
                    Player.main.GetComponent<LiveMixin>().AddHealth(healthBack);
                }
            };
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
        /// Component attached to the item in order to work.
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
        /// Component attached to the item in order to work.
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