using System.Collections.Generic;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;

namespace Nautilus.Patchers;

using static EatableHandler;

internal class EatablePatcher
{
    internal static readonly IDictionary<TechType, EditedEatableValues> EditedEatables = new SelfCheckingDictionary<TechType, EditedEatableValues>("EditedEatableValues", TechTypeExtensions.sTechTypeComparer);

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(Eatable), nameof(Eatable.Awake)),
            new HarmonyMethod(typeof(EatablePatcher), nameof(AwakePrefix)));

        InternalLogger.Debug("EatablePatcher is done.");
    }
    private static void AwakePrefix(Eatable __instance)
    {
        TechType tt = CraftData.GetTechType(__instance.gameObject);
        if (EditedEatables.TryGetValue(tt, out EditedEatableValues value))
        {
            __instance.foodValue = value.food;
            __instance.waterValue = value.water;
            __instance.decomposes = value.decomposes;
#if BELOWZERO
                __instance.healthValue = value.health;
                __instance.maxCharges = value.maxCharges;
                __instance.coldMeterValue = value.coldValue;
#endif
        }
    }
}