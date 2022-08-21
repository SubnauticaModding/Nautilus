using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SMLHelper.V2.Handlers.EatableHandler;

namespace SMLHelper.V2.Patchers
{
    internal class EatablePatcher
    {
        internal static readonly IDictionary<TechType, EditedEatableValues> EditedEatables = new SelfCheckingDictionary<TechType, EditedEatableValues>("EditedEatableValues", TechTypeExtensions.sTechTypeComparer);

        public static void Patch(Harmony harmony)
        {
            Type eatableType = typeof(Eatable);
            Type thisType = typeof(EatablePatcher);

            harmony.Patch(AccessTools.Method(typeof(Eatable), nameof(Eatable.Awake)),
                            postfix: new HarmonyMethod(typeof(EatablePatcher), nameof(EatablePatcher.AwakePostfix)));

            Logger.Debug("EatablePatcher is done.");
        }
        public static void AwakePostfix(Eatable __instance)
        {
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            if (EditedEatables.TryGetValue(tt, out EditedEatableValues value)
            {
                    __instance.foodValue = value.food;
                    __instance.waterValue = value.water;
                    __instance.decomposes = value.decomposes;
                    __instance.allowOverfill = value.allowOverfill;
            }
        }
    }
}
