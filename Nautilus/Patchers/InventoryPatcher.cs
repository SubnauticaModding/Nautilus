using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Nautilus.Patchers;

/* This class fixes a bug related to picked up items not having "verbose" analysis tech
 * In other words... there are no notifications for unlocked tech when you pick up creatures, resources, etc...
 * We're technically fixing a vanilla bug, which does go beyond the normal scope of Nautilus. However, this bug does affect Nautilus!
 */
internal static class InventoryPatcher
{
    private const string CHANGESET_WHERE_BUG_EXISTS = "71288";

    internal static void Patch(Harmony harmony)
    {
        // The bug does NOT exist in Below Zero!
#if SN_STABLE
        // Only enable the fix for the latest version of Subnautica:
        if (SNUtils.GetPlasticChangeSetOfBuild() != CHANGESET_WHERE_BUG_EXISTS)
            return;

        var transpiler = new HarmonyMethod(AccessTools.Method(typeof(InventoryPatcher), nameof(VerbosePickupFixTranspiler)));

        harmony.Patch(AccessTools.Method(typeof(Inventory), nameof(Inventory.Pickup)),
            transpiler: transpiler);
        harmony.Patch(AccessTools.Method(typeof(Inventory), nameof(Inventory.OnAddItem)),
            transpiler: transpiler);
#endif
    }

    // Modifies both the Inventory.Pickup AND Inventory.OnAddItem method to always analyze picked up technology verbosely
    private static IEnumerable<CodeInstruction> VerbosePickupFixTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(KnownTech), nameof(KnownTech.Analyze))))
            .Advance(-1)
            .Set(OpCodes.Ldc_I4_1, null)
            .Instructions();
    }
}