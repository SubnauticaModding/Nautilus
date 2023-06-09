using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Utility;
using Steamworks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaMoth), nameof(SeaMoth.OnUpgradeModuleChange))]
    private static void DelegateModuleChangeCallback(SeaMoth __instance, int slotID, TechType techType, bool added)
    {
        if (!SeamothUpgradeModules.TryGetValue(techType, out ICustomPrefab prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.delegateOnRemoved != null & !added)
            moduleGadget.delegateOnRemoved.Invoke(__instance, slotID);

        if (moduleGadget.delegateOnAdded != null & added)
            moduleGadget.delegateOnAdded.Invoke(__instance, slotID);
    }

    private static void HullModuleCallback(SeaMoth __instance, TechType techType, Dictionary<TechType, float> crushDepthDictionaryReference)
    {
        if (!SeamothUpgradeModules.TryGetValue(techType, out ICustomPrefab prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.CrushDepth == -1f)
            return;

        crushDepthDictionaryReference.Add(prefab.Info.TechType, moduleGadget.CrushDepth);
    }

    [HarmonyPrefix]
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaMoth), nameof(SeaMoth.OnUpgradeModuleChange))]
    private static void OnUpgradeModuleHull(SeaMoth __instance, int slotID, TechType techType, bool added)
    {
        Dictionary<TechType, float> CrushDepthUpgrades = new()
        {
            {
                TechType.SeamothReinforcementModule,
                800f
            },
            {
                TechType.VehicleHullModule1,
                100f
            },
            {
                TechType.VehicleHullModule2,
                300f
            },
            {
                TechType.VehicleHullModule3,
                700f
            }
        };
        HullModuleCallback(__instance, techType, CrushDepthUpgrades);

        float num = 0f;
        for (int i = 0; i < __instance.slotIDs.Length; i++)
        {
            string slot = __instance.slotIDs[i];
            TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);
            if (CrushDepthUpgrades.ContainsKey(techTypeInSlot))
            {
                float num2 = CrushDepthUpgrades[techTypeInSlot];
                if (num2 > num)
                {
                    num = num2;
                }
            }
        }
        __instance.crushDamage.SetExtraCrushDepth(num);
    }
#elif BELOWZERO
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleChange))]
    private static void DelegateModuleChangeCallback(SeaTruckUpgrades __instance, int slotID, TechType techType, bool added)
    {
        if (!SeatruckUpgradeModules.TryGetValue(techType, out var prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.seatruckOnRemoved != null && !added)
            moduleGadget.seatruckOnRemoved.Invoke(__instance, __instance.motor, slotID);

        if (moduleGadget.seatruckOnAdded != null && added)
            moduleGadget.seatruckOnAdded.Invoke(__instance, __instance.motor, slotID);
    }



    private static void DelegateUseCallback(SeaTruckUpgrades __instance, int slotID, TechType techType)
    {
        if (!SeatruckUpgradeModules.TryGetValue(techType, out var prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        float quickSlotCharge = __instance.quickSlotCharge[slotID];
        float chargeScalar = ((IQuickSlots) __instance).GetSlotCharge(slotID);
        if (moduleGadget.seatruckOnUsed != null)
            moduleGadget.seatruckOnUsed.Invoke(__instance, __instance.motor, slotID, quickSlotCharge, chargeScalar);
        __instance.quickSlotCooldown[slotID] = (float) moduleGadget.Cooldown;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleUse))]
    private static IEnumerable<CodeInstruction> DelegateModuleUseCallback(IEnumerable<CodeInstruction> instructions)
    {


        var returnIndex = 0;
        foreach(CodeInstruction instruction in instructions)
        {
            if(instruction.opcode == OpCodes.Ret)
            {
                returnIndex += 1;
                if(returnIndex == 2)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VehicleUpgradesPatcher), nameof(VehicleUpgradesPatcher.DelegateUseCallback)));
                }
            }
            yield return instruction;

        }
    }

    [HarmonyPrefix]
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleChange))]
    private static void OnUpgradeModuleDelegate(SeaTruckUpgrades __instance, int slotID, TechType techType, bool added)
    {
        if (!SeaTruckUpgrades.crushDepths.ContainsKey(techType))
            return;

        var previousCrushDepth = 0f;
        for (var i = 0; i < SeaTruckUpgrades.slotIDs.Length; i++)
        {
            var slot = SeaTruckUpgrades.slotIDs[i];
            TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);
            if (SeaTruckUpgrades.crushDepths.TryGetValue(techTypeInSlot, out var crushDepth) && crushDepth > previousCrushDepth)
            {
                previousCrushDepth = crushDepth;
            }
        }
        __instance.crushDamage.SetExtraCrushDepth(previousCrushDepth);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hoverbike), nameof(Hoverbike.OnUpgradeModuleChange))]
    private static void OnUpgradeModuleDelegate(Hoverbike __instance, int slotID, TechType techType, bool added)
    {
        if (!SnowbikeUpgradeModules.TryGetValue(techType, out var prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.hoverbikeOnRemoved != null && !added)
            moduleGadget.hoverbikeOnRemoved.Invoke(__instance, slotID);

        if (moduleGadget.hoverbikeOnAdded != null && added)
            moduleGadget.hoverbikeOnAdded.Invoke(__instance, slotID);
    }
#endif
}