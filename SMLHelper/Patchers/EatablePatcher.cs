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
            foreach(KeyValuePair<TechType, EditedEatableValues> pair in EditedEatables)
            {
                if(pair.Key == tt)
                {
                    __instance.foodValue = pair.Value.food;
                    __instance.waterValue = pair.Value.water;
                    __instance.decomposes = pair.Value.decomposes;
                    __instance.allowOverfill = pair.Value.allowOverfill;
                }
            }
        }
    }
}
