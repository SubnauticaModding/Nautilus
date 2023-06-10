using HarmonyLib;
using Nautilus.Handlers;

namespace Nautilus.Patchers;

internal class PDAEncyclopediaTabPatcher
{
    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(PDAEncyclopediaTabPatcher));
    }
    [HarmonyPatch(typeof(uGUI_EncyclopediaTab))]
    [HarmonyPatch(nameof(uGUI_EncyclopediaTab.Awake))]
    [HarmonyPostfix]
    internal static void EncyTabAwakePostfix(uGUI_EncyclopediaTab __instance)
    {
      ModDatabankHandler.Initialize();
    }
}
