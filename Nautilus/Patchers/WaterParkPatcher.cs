using System.Collections.Generic;
using HarmonyLib;
using Nautilus.Assets.Gadgets;
using Nautilus.Extensions;

namespace Nautilus.Patchers;

[HarmonyPatch(typeof(WaterPark))]
internal static class WaterParkPatcher
{
    private const string NormalAcuOnlyKey = "Nautilus_WaterParkNormalAcuOnly";
    private const string LargeAcuOnlyKey = "Nautilus_WaterParkLargeAcuOnly";
    private const string LargeAcuTooSmallKey = "Nautilus_WaterParkLargeAcuTooSmall";
    private const string NormalAcuTooSmallKey = "Nautilus_WaterParkNormalAcuTooSmall";

    internal static Dictionary<TechType, EggGadget> requiredAcuSize = new();

    internal static void Patch(Harmony harmony)
    {
        LanguagePatcher.AddDefaultLanguageLine(NormalAcuOnlyKey, "Cannot drop {0} here. Drop in the normal ACU instead.");
        LanguagePatcher.AddDefaultLanguageLine(LargeAcuOnlyKey, "Cannot drop {0} here. Drop in the large ACU instead.");
        LanguagePatcher.AddDefaultLanguageLine(LargeAcuTooSmallKey, "Cannot drop {0} here, the large ACU must be at least {1} floors tall.");
        LanguagePatcher.AddDefaultLanguageLine(NormalAcuTooSmallKey, "Cannot drop {0} here, the ACU must be at least {1} floors tall.");
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
                    ErrorMessage.main.AddHint(Language.main.GetFormat(NormalAcuOnlyKey, Language.main.Get(tt)));
                    __result = false;
                }
                else if (largeRoomWaterPark.size < eggGadget.RequiredLargeAcuSize)
                {
                    ErrorMessage.main.AddHint(Language.main.GetFormat(LargeAcuTooSmallKey, Language.main.Get(tt), eggGadget.RequiredLargeAcuSize));
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
                    ErrorMessage.main.AddHint(Language.main.GetFormat(LargeAcuOnlyKey, Language.main.Get(tt)));
                    __result = false;
                }
                else if (waterPark.height < eggGadget.RequiredAcuSize)
                {
                    ErrorMessage.main.AddHint(Language.main.GetFormat(NormalAcuTooSmallKey, Language.main.Get(tt), eggGadget.RequiredAcuSize));
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