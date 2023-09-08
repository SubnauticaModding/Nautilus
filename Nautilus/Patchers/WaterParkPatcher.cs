using System.Collections.Generic;
using HarmonyLib;
using Nautilus.Assets.Gadgets;
using Nautilus.Extensions;

namespace Nautilus.Patchers;

[HarmonyPatch(typeof(WaterPark))]
internal static class WaterParkPatcher
{
    internal static Dictionary<TechType, EggGadget> requiredAcuSize = new();

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(WaterParkPatcher));
    }
    
    [HarmonyPatch(nameof(WaterPark.CanDropItemInside))]
    [HarmonyPrefix]
    private static bool CanDropItemInsidePrefix(Pickupable item, ref bool __result)
    {
        var tt = CraftData.GetTechType(item.gameObject);
        if (requiredAcuSize.TryGetValue(tt, out var eggGadget))
        {
            var waterPark = Player.main.currentWaterPark;
            // large ACU
            if (waterPark is LargeRoomWaterPark largeRoomWaterPark)
            {
                if (eggGadget.RequiredLargeAcuSize == 0)
                {
                    ErrorMessage.main.AddHint($"Cannot drop {Language.main.Get(tt)} here. Drop in the normal ACU instead.");
                    __result = false;
                }
                else if (largeRoomWaterPark.size < eggGadget.RequiredLargeAcuSize)
                {
                    ErrorMessage.main.AddHint($"Cannot drop {Language.main.Get(tt)} here, the large ACU must be at least {eggGadget.RequiredAcuSize} floors tall.");
                    __result = false;
                }
                else
                {
                    __result = true;
                }
                
            }
            // normal ACU
            else if (waterPark is not null)
            {
                if (eggGadget.RequiredAcuSize == 0)
                {
                    ErrorMessage.main.AddHint($"Cannot drop {Language.main.Get(tt)} here. Drop in the large ACU instead.");
                    __result = false;
                }
                else if (waterPark.height < eggGadget.RequiredAcuSize)
                {
                    ErrorMessage.main.AddHint($"Cannot drop {Language.main.Get(tt)} here, the large ACU must be at least {eggGadget.RequiredAcuSize} floors tall.");
                    __result = false;
                }
                else
                {
                    __result = true;
                }
            }

            return false;
        }

        return true;
    }
}