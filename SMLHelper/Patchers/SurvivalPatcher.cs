#if SUBNAUTICA
namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using HarmonyLib;
    using UnityEngine;
    using Logger = Logger;

    internal class SurvivalPatcher
    {
        internal static IDictionary<TechType, float> CustomOxygenOutputsOnUse = new SelfCheckingDictionary<TechType, float>("CustomOxygenOutputsOnUse", TechTypeExtensions.sTechTypeComparer);
        internal static IDictionary<TechType, float> CustomHealersOnUse = new SelfCheckingDictionary<TechType, float>("CustomHealersOnUse", TechTypeExtensions.sTechTypeComparer);
        internal static IDictionary<TechType, float> CustomOxygenOutputsOnEat = new SelfCheckingDictionary<TechType, float>("CustomOxygenOutputsOnEat", TechTypeExtensions.sTechTypeComparer);
        internal static IDictionary<TechType, float> CustomHealersOnEat = new SelfCheckingDictionary<TechType, float>("CustomHealersOnEat", TechTypeExtensions.sTechTypeComparer);
        internal static List<TechType> InventoryUseables = new List<TechType>();

        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Survival), nameof(Survival.Use)),
                postfix: new HarmonyMethod(typeof(SurvivalPatcher), nameof(UsePostfix)));

            harmony.Patch(AccessTools.Method(typeof(Survival), nameof(Survival.Eat)),
                postfix: new HarmonyMethod(typeof(SurvivalPatcher), nameof(EatPostfix)));

            Logger.Log($"SurvivalPatcher is done.", LogLevel.Debug);
        }
        private static void UsePostfix(GameObject useObj, ref bool __result)
        {
            SurvivalPatchings(CustomHealersOnUse, CustomOxygenOutputsOnUse, useObj, ref __result);
        }
        private static void EatPostfix(GameObject useObj, ref bool __result)
        {
            SurvivalPatchings(CustomHealersOnEat, CustomOxygenOutputsOnEat, useObj, ref __result);
        }
        private static void SurvivalPatchings(IDictionary<TechType, float> healers, IDictionary<TechType, float> oxygeners, GameObject obj, ref bool result)
        {
            TechType tt = CraftData.GetTechType(obj);
            foreach (KeyValuePair<TechType, float> kvp in oxygeners)
            {
                if (tt == kvp.Key)
                {
                    Player.main.GetComponent<OxygenManager>().AddOxygen(kvp.Value);
                    result = true;
                }
            }
            foreach (KeyValuePair<TechType, float> kvp in healers)
            {
                if (tt == kvp.Key && Player.main.GetComponent<LiveMixin>().AddHealth(kvp.Value) > 0.1f)
                {
                    result = true;
                }
            }
            if (result)
                FMODUWE.PlayOneShot(CraftData.GetUseEatSound(tt), Player.main.transform.position); // only play the sound if its useable
        }
    }
}
#endif
