using System.Collections.Generic;
using HarmonyLib;
using Nautilus.Extensions;

namespace Nautilus.Patchers;

[HarmonyPatch(typeof(WaterPark))]
internal static class WaterParkPatcher
{
    internal static Dictionary<TechType, int> requiredAcuSize = new();

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(WaterParkPatcher));
    }
    
    [HarmonyPatch(nameof(WaterPark.CanDropItemInside))]
    [HarmonyPrefix]
    private static bool CanDropItemInsidePrefix(Pickupable item, ref bool __result)
    {
        var tt = CraftData.GetTechType(item.gameObject);
        if (requiredAcuSize.TryGetValue(tt, out var maxHeight))
        {
            var waterPark = Player.main.currentWaterPark;
            if (waterPark is not null && waterPark.height >= maxHeight)
            {
                __result = true;
            }
            else
            {
                ErrorMessage.main.AddHint(string.Format("Cannot drop {0} here, the ACU must be at least {1} units tall.", Language.main.Get(tt), maxHeight));
                __result = false;
            }

            return false;
        }

        return true;
    }
}