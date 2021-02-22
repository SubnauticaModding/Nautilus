#if SUBNAUTICA
namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using UnityEngine;
    using Logger = Logger;

    internal class SurvivalPatcher
    {
        internal static IDictionary<TechType, List<Action>> CustomSurvivalInventoryAction = new SelfCheckingDictionary<TechType, List<Action>>("CustomSurvivalInventoryAction", TechTypeExtensions.sTechTypeComparer);
        internal static List<TechType> InventoryUseables = new List<TechType>();

        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Survival), nameof(Survival.Use)),
                postfix: new HarmonyMethod(typeof(SurvivalPatcher), nameof(SurvivalPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Survival), nameof(Survival.Eat)),
                postfix: new HarmonyMethod(typeof(SurvivalPatcher), nameof(SurvivalPostfix)));

            Logger.Log($"SurvivalPatcher is done.", LogLevel.Debug);
        }
        private static void SurvivalPostfix(GameObject useObj, ref bool __result)
        {
            SurvivalPatchings(CustomSurvivalInventoryAction, useObj, ref __result);
        }
        private static void SurvivalPatchings(IDictionary<TechType, List<Action>> dictionary, GameObject obj, ref bool result)
        {
            TechType tt = CraftData.GetTechType(obj);
            if (dictionary.TryGetValue(tt, out List<Action> action))
            {
                action.ForEach((x) => x.Invoke());
                result = true;
            }
            if (result)
                FMODUWE.PlayOneShot(CraftData.GetUseEatSound(tt), Player.main.transform.position); // only play the sound if its useable
        }
    }
}
#endif
