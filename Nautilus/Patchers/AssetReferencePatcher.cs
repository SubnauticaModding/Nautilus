using System.Collections.Generic;
using HarmonyLib;
using UnityEngine.AddressableAssets;

namespace Nautilus.Patchers;

[HarmonyPatch(typeof(AssetReference))]
internal static class AssetReferencePatcher
{
    private static HashSet<string> _validKeys = new();
    
    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(AssetReferencePatcher));
    }

    internal static void AddValidKey(string key)
    {
        _validKeys.Add(key);
    }

    [HarmonyPatch(nameof(AssetReference.RuntimeKeyIsValid))]
    [HarmonyPrefix]
    private static bool RuntimeKeyIsValidPrefix(AssetReference __instance, ref bool __result)
    {
        if (_validKeys.Contains(__instance.AssetGUID))
        {
            __result = true;
            return false;
        }

        return true;
    }
}