using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMLHelper.V2.Patchers
{
    using HarmonyLib;
    using System.Collections.Generic;
    using static Handlers.EatableHandler;
    internal class PickupablePatcher
    {
        internal static readonly IDictionary<TechType, EditedEatableValues> AddedEatables = new SelfCheckingDictionary<TechType, EditedEatableValues>("EditedEatableValues", TechTypeExtensions.sTechTypeComparer);

        public static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Pickupable), nameof(Pickupable.Awake)),
                new HarmonyMethod(typeof(PickupablePatcher), nameof(AwakePrefix)));

            Logger.Debug("PickupablePatcher is done.");
        }
        private static void AwakePrefix(Pickupable __instance)
        {
            TechType tt = CraftData.GetTechType(__instance.gameObject);

            if (AddedEatables.TryGetValue(tt, out var value))
                {
                var eatable = __instance.gameObject.EnsureComponent<Eatable>();
                eatable.foodValue = value.food;
                eatable.waterValue = value.water;
                eatable.decomposes = value.decomposes;
#if BELOWZERO
                eatable.healthValue = value.health;
                eatable.maxCharges = value.maxCharges;
                eatable.coldMeterValue = value.coldValue;
#endif
            }
        }
    }
}
