using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Nautilus.Patchers;
internal class VehicleUpgradesPatcher
{
    internal static IDictionary<TechType, ICustomPrefab> VehicleUpgradeModules = new SelfCheckingDictionary<TechType, ICustomPrefab>("VehicleUpgradeModules");
    internal static IDictionary<TechType, ICustomPrefab> ExosuitUpgradeModules = new SelfCheckingDictionary<TechType, ICustomPrefab>("ExosuitUpgradeModules");
#if BELOWZERO
    internal static IDictionary<TechType, ICustomPrefab> SeatruckUpgradeModules = new SelfCheckingDictionary<TechType, ICustomPrefab>("SeatruckUpgradeModules");
    internal static IDictionary<TechType, ICustomPrefab> SnowbikeUpgradeModules = new SelfCheckingDictionary<TechType, ICustomPrefab>("SnowbikeUpgradeModules");
#elif SUBNAUTICA
    internal static IDictionary<TechType, ICustomPrefab> SeamothUpgradeModules = new SelfCheckingDictionary<TechType, ICustomPrefab>("SeamothUpgradeModules");
#endif

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(VehicleUpgradesPatcher));
    }

#if SUBNAUTICA
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SeaMoth), nameof(SeaMoth.OnUpgradeModuleChange))]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        static void DelegateModuleChangeCallback(SeaMoth __instance, TechType techType, int slotId, bool added)
        {
            if (!SeamothUpgradeModules.TryGetValue(techType, out ICustomPrefab prefab))
                return;

            if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
                return;

            if (moduleGadget.delegateOnRemoved != null & !added)
                moduleGadget.delegateOnRemoved.Invoke(__instance, slotId);

            if (moduleGadget.delegateOnAdded != null & added)
                moduleGadget.delegateOnAdded.Invoke(__instance, slotId);
        }

        static void HullModuleCallback(SeaMoth __instance, TechType techType, ref Dictionary<TechType, float> crushDepthDictionaryReference)
        {
            if (!SeamothUpgradeModules.TryGetValue(techType, out ICustomPrefab prefab))
                return;

            if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
                return;

            if (moduleGadget.CrushDepth == -1f)
                return;

            crushDepthDictionaryReference.Add(prefab.Info.TechType, moduleGadget.CrushDepth);
        }

        new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Br_S),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Callvirt, "SetActive") )
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_2))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_3))
            .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(nameof(DelegateModuleChangeCallback))))
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4, 2114),
                new CodeMatch(OpCodes.Ldc_R4, 700),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Stloc_1))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_2))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, "dictionary"))
            .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(nameof(HullModuleCallback))));

        return instructions;
    }
#elif BELOWZERO
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleChange))]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        static void DelegateModuleChangeCallback(SeaTruckUpgrades __instance, SeaTruckMotor __vehicle, TechType techType, int slotId, bool added)
        {
            foreach(ICustomPrefab prefab in SeatruckUpgradeModules.Values)
            {
                UpgradeModuleGadget moduleGadget;
                prefab.TryGetGadget<UpgradeModuleGadget>(out moduleGadget);
                if (moduleGadget is null)
                    continue;

                if (prefab.Info.TechType != techType)
                    continue;

                if (moduleGadget.seatruckOnAdded is null & moduleGadget.seatruckOnRemoved is null)
                    continue;

                if (moduleGadget.seatruckOnRemoved != null & !added)
                    moduleGadget.seatruckOnRemoved.Invoke(__instance, __vehicle, slotId);

                if (moduleGadget.seatruckOnAdded != null & added)
                    moduleGadget.seatruckOnAdded.Invoke(__instance, __vehicle, slotId);
            }
        }

        new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, "get_modules"),
                new CodeMatch(OpCodes.Ldarg_2),
                new CodeMatch(OpCodes.Callvirt, "GetCount"),
                new CodeMatch(OpCodes.Pop))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldfld, (SeaTruckUpgrades stu) => stu.motor))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_2))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_3))
            .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(nameof(DelegateModuleChangeCallback))));

        return instructions;
    }
#endif
}