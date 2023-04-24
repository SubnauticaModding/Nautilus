using HarmonyLib;
using Nautilus.Utility;
using TMPro;

namespace Nautilus.Patchers;

internal static class FontReferencesPatcher
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Start)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FontReferencesPatcher), nameof(GetAllerRgFont))));
        harmony.Patch(AccessTools.Method(typeof(uGUI), nameof(uGUI.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FontReferencesPatcher), nameof(GetAllerWBdFont))));
    }

    internal static void GetAllerRgFont(uGUI_MainMenu __instance)
    {
         FontUtils.Aller_Rg = __instance.transform.Find("Panel/MainMenu/GraphicsDeviceName").GetComponent<TextMeshProUGUI>().font;
    }

    internal static void GetAllerWBdFont(uGUI_MainMenu __instance)
    {
        FontUtils.Aller_W_Bd = __instance.transform.Find("ScreenCanvas/HUD/Content/DepthCompass/Compass/NW").GetComponent<TextMeshProUGUI>().font;
    }
}