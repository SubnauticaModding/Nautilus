using HarmonyLib;
using Nautilus.Handlers;
using UnityEngine;

namespace Nautilus.Patchers;

internal static class GravSpherePatcher
{
    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(GravSpherePatcher));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Gravsphere), nameof(Gravsphere.IsValidTarget))]
    private static void PatchGravTrapTargeting(GameObject obj, ref bool __result)
    {
        // This patch keeps the original functionality (as to not exclude base-game ones or override other mods' patches)
        if (__result == true) return;
        
        var techType = CraftData.GetTechType(obj);
        bool isCustomInclusion = GravSphereHandler.IsValidCustomGravTrapTarget(techType);

        if (!isCustomInclusion) return;
        
        var pickupable = obj.GetComponent<Pickupable>();
        if (pickupable != null && pickupable.attached)
        {
            return;
        }

        __result = true;
    }
}