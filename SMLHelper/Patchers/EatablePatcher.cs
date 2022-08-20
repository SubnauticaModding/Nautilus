using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMLHelper.V2.Patchers
{
    internal class EatablePatcher
    {
        private static readonly IDictionary<TechType, EditedEatableValues> EditedEatables = new SelfCheckingDictionary<TechType, EditedEatableValues>("EditedEatableValues", TechTypeExtensions.sTechTypeComparer);

        public class EditedEatableValues
        {
            public EditedEatableValues(int food, int water, TechType typeToEdit)
            {
                EditedEatables.Add(typeToEdit, this);
                this.food = food;
                this.water = water;
            }

            public readonly int food;
            public readonly int water;
        }

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
                }
            }
        }
    }
}
