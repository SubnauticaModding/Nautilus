using HarmonyLib;
using HarmonyLib.Tools;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.MonoBehaviours;
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
    // Hoverbikes upgrades are in the HoverbikeModulesSupport MonoBehaviour.
#elif SUBNAUTICA
    internal static IDictionary<TechType, ICustomPrefab> SeamothUpgradeModules = new SelfCheckingDictionary<TechType, ICustomPrefab>("SeamothUpgradeModules");
#endif

    internal static void Patch(Harmony harmony)
    {
        InternalLogger.Debug("VehicleUpgradePatcher: attempting patch...");
        try
        {
            harmony.PatchAll(typeof(VehicleUpgradesPatcher));
        }
        catch (Exception e)
        {
            InternalLogger.Error($"An error occured while running VehicleUpgradesPatcher.\n{e}");
        }
        InternalLogger.Debug("VehicleUpgradesPatcher is done.");
    }

    //
    // VEHICLE
    // ON CHANGE
    //
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnUpgradeModuleChange))]
    private static void OnModuleChangeDelegate(Vehicle __instance, int slotID, TechType techType, bool added)
    {
        ICustomPrefab prefab;
        if (__instance is Exosuit)
        {
            if (!ExosuitUpgradeModules.TryGetValue(techType, out prefab))
                return;
        }
        else
        {
            if (!VehicleUpgradeModules.TryGetValue(techType, out prefab))
                return;
        }

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        InternalLogger.Debug("Will execute OnAdded/Removed.");

        if (moduleGadget.delegateOnRemoved != null && !added)
            moduleGadget.delegateOnRemoved.Invoke(__instance, slotID);

        if (moduleGadget.delegateOnAdded != null && added)
            moduleGadget.delegateOnAdded.Invoke(__instance, slotID);

        InternalLogger.Debug("Executed OnAdded/Removed.");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnUpgradeModuleChange))]
    private static void OnModuleChangeCrushDepth(Vehicle __instance, int slotID, TechType techType, bool added)
    {
        Dictionary<TechType, float> CrushDepthUpgrades = new();
        if (__instance is Exosuit)
            ExosuitUpgradeModules.DoIf(
                (KeyValuePair<TechType, ICustomPrefab> mapElem) => mapElem.Value.TryGetGadget(out UpgradeModuleGadget moduleGadget) && moduleGadget.CrushDepth > 0f,
                (KeyValuePair<TechType, ICustomPrefab> mapElem) => CrushDepthUpgrades.Add(mapElem.Key, mapElem.Value.GetGadget<UpgradeModuleGadget>().CrushDepth)
            );
        else
            VehicleUpgradeModules.DoIf(
                (KeyValuePair<TechType, ICustomPrefab> mapElem) => mapElem.Value.TryGetGadget(out UpgradeModuleGadget moduleGadget) && moduleGadget.CrushDepth > 0f,
                (KeyValuePair<TechType, ICustomPrefab> mapElem) => CrushDepthUpgrades.Add(mapElem.Key, mapElem.Value.GetGadget<UpgradeModuleGadget>().CrushDepth)
            );

        var newCrushDepth = 0f;
        var absolute = false;
        for (var i = 0; i < __instance.slotIDs.Length; i++)
        {
            string slot = __instance.slotIDs[i];
            TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);
            if (CrushDepthUpgrades.ContainsKey(techTypeInSlot))
            {
                float crushDepthToCheck = CrushDepthUpgrades[techTypeInSlot];
                if (crushDepthToCheck > newCrushDepth)
                {
                    newCrushDepth = crushDepthToCheck;
                    if (__instance is Exosuit)
                    {
                        if (ExosuitUpgradeModules[techTypeInSlot].TryGetGadget(out UpgradeModuleGadget moduleGadget))
                            absolute = moduleGadget.AbsoluteDepth;
                    }
                    else
                    {
                        if (VehicleUpgradeModules[techTypeInSlot].TryGetGadget(out UpgradeModuleGadget moduleGadget))
                            absolute = moduleGadget.AbsoluteDepth;
                    }
                }
            }
        }
        if (absolute)
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth - __instance.crushDamage.kBaseCrushDepth);
        else
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth);
    }

    //
    // VEHICLE
    // LAZY INITIALIZE (OnToggle)
    //
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.LazyInitialize))]
    private static void LazyInitialize(Vehicle __instance)
    {
        __instance.onToggle += (int slotID, bool state) =>
        {
            ICustomPrefab prefab;
            var techType = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[slotID]);
            UpgradeModuleGadget moduleGadget;
            double energyCost;
            switch (__instance)
            {
                case Exosuit:
                    break;
#if SUBNAUTICA
                case SeaMoth:
                    if (techType == TechType.None)
                        break;

                    if (!SeamothUpgradeModules.TryGetValue(techType, out prefab))
                        break;

                    if (!prefab.TryGetGadget(out moduleGadget))
                        break;

                    energyCost = moduleGadget.EnergyCost;
                    moduleGadget.delegateOnToggled?.Invoke(__instance, slotID, (float) energyCost, state);
                    break;
#endif
                default:
                    if (techType == TechType.None)
                        break;

                    if (!VehicleUpgradeModules.TryGetValue(techType, out prefab))
                        break;

                    if (!prefab.TryGetGadget(out moduleGadget))
                        break;

                    energyCost = moduleGadget.EnergyCost;
                    moduleGadget.delegateOnToggled?.Invoke(__instance, slotID, (float) energyCost, state);
                    break;
            }
        };
    }

    //
    // VEHICLE
    // ON USE + SLOT KEY DOWN
    //
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnUpgradeModuleUse))]
    private static void UpgradeModuleUseDelegate(Vehicle __instance, TechType techType, int slotID)
    {
        ICustomPrefab prefab;
        UpgradeModuleGadget moduleGadget;
        float quickSlotCharge;
        float chargeScalar;
        float cooldown;
        if (__instance is Exosuit)
        {
            if (!ExosuitUpgradeModules.TryGetValue(techType, out prefab))
                return;
        }
        else
        {
            if (!VehicleUpgradeModules.TryGetValue(techType, out prefab))
                return;
        }

        if (!prefab.TryGetGadget(out moduleGadget))
            return;

        quickSlotCharge = __instance.quickSlotCharge[slotID];
        chargeScalar = __instance.GetSlotCharge(slotID);

        moduleGadget.delegateOnUsed?.Invoke(__instance, slotID, quickSlotCharge, chargeScalar);
        InternalLogger.Debug("Executed...");

        cooldown = 0f;
        if (moduleGadget.Cooldown > 0f)
            cooldown = (float) moduleGadget.Cooldown;

        InternalLogger.Debug("Cooldown set.");

        __instance.quickSlotTimeUsed[slotID] = Time.time;
        __instance.quickSlotCooldown[slotID] = cooldown;

        return;
    }

    //
    // PRAWN
    // ON CHANGE + HULL UPGRADE FIX
    //
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Exosuit), nameof(Exosuit.OnUpgradeModuleChange))]
    private static void RefreshExosuitCrushDepthModules(Exosuit __instance, int slotID, TechType techType, bool added)
    {
        ExosuitUpgradeModules.DoIf(
            (KeyValuePair<TechType, ICustomPrefab> mapElem) =>
                mapElem.Value.TryGetGadget(out UpgradeModuleGadget moduleGadget)
                && moduleGadget.AbsoluteDepth == true   // Check if the provided module wants to set AbsoluteDepth or not. If not, break, the module is already added at registering of the prefab.
                && moduleGadget.CrushDepth > 0f         // Check if the provided module wants to change depth or not. -1f is the default value if the crush depth is not meant to be changed.
                && !Exosuit.crushDepths.ContainsKey(mapElem.Key),   // Abort if the module is already existing in crushDepths.
                                                                    // For example if the AbsoluteDepth bool is changed on runtime, it won't do anything because the module already exists in crushDepths Dictionary.
            (KeyValuePair<TechType, ICustomPrefab> mapElem) =>
                Exosuit.crushDepths.Add(
                    mapElem.Key,
                    (mapElem.Value.GetGadget<UpgradeModuleGadget>().CrushDepth - __instance.crushDamage.crushDepth)
                )
        );
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Exosuit), nameof(Exosuit.OnUpgradeModuleChange))]
    private static void OnUpgradeModuleExoHull(Exosuit __instance, int slotID, TechType techType, bool added)
    {
        float newCrushDepth = 0f;
        bool absolute = false;
        for (int i = 0; i < __instance.slotIDs.Length; i++)
        {
            string slot = __instance.slotIDs[i];
            TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);
            float depthToCheck;
            if (Exosuit.crushDepths.TryGetValue(techTypeInSlot, out depthToCheck) && depthToCheck > newCrushDepth)
            {
                newCrushDepth = depthToCheck;
                if (ExosuitUpgradeModules.TryGetValue(techTypeInSlot, out var gadget) && gadget.TryGetGadget(out UpgradeModuleGadget mdlGadget))
                    absolute = mdlGadget.AbsoluteDepth;
            }
        }
        if (absolute)
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth - __instance.crushDamage.kBaseCrushDepth);
        else
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth);


        if (!ExosuitUpgradeModules.TryGetValue(techType, out var prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.delegateOnRemoved != null && !added)
            moduleGadget.delegateOnRemoved.Invoke(__instance, slotID);

        if (moduleGadget.delegateOnAdded != null && added)
            moduleGadget.delegateOnAdded.Invoke(__instance, slotID);
    }

#if SUBNAUTICA
    //
    // SEAMOTH
    // ON CHANGE + HULL UPGRADE FIX
    //
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaMoth), nameof(SeaMoth.OnUpgradeModuleChange))]
    private static void SeamothDelegateModuleChangeCallback(SeaMoth __instance, int slotID, TechType techType, bool added)
    {
        if (!SeamothUpgradeModules.TryGetValue(techType, out ICustomPrefab prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.delegateOnRemoved != null && !added)
            moduleGadget.delegateOnRemoved.Invoke(__instance, slotID);

        if (moduleGadget.delegateOnAdded != null && added)
            moduleGadget.delegateOnAdded.Invoke(__instance, slotID);
    }

    //[HarmonyPrefix]
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaMoth), nameof(SeaMoth.OnUpgradeModuleChange))]
    private static void SeamothOnUpgradeModuleHull(SeaMoth __instance, int slotID, TechType techType, bool added)
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
        SeamothUpgradeModules.Do((KeyValuePair<TechType, ICustomPrefab> mapElem) =>
        {
            var flag = mapElem.Value.TryGetGadget(out UpgradeModuleGadget moduleGadget) && moduleGadget.CrushDepth > 0f;
            InternalLogger.Debug($"SeamothUpgradesModule will do ? {flag}");
        });

        SeamothUpgradeModules.DoIf(
            (KeyValuePair<TechType, ICustomPrefab> mapElem) => mapElem.Value.TryGetGadget(out UpgradeModuleGadget moduleGadget) && moduleGadget.CrushDepth > 0f,
            (KeyValuePair<TechType, ICustomPrefab> mapElem) => CrushDepthUpgrades.Add(mapElem.Key, mapElem.Value.GetGadget<UpgradeModuleGadget>().CrushDepth));


        var newCrushDepth = 0f;
        var absolute = false;
        for (var i = 0; i < __instance.slotIDs.Length; i++)
        {
            string slot = __instance.slotIDs[i];
            TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);
            if (CrushDepthUpgrades.ContainsKey(techTypeInSlot))
            {
                float crushDepthToCheck = CrushDepthUpgrades[techTypeInSlot];
                if (crushDepthToCheck > newCrushDepth)
                {
                    newCrushDepth = crushDepthToCheck;
                    if (SeamothUpgradeModules.TryGetValue(techTypeInSlot, out var gadget) && gadget.TryGetGadget(out UpgradeModuleGadget moduleGadget))
                        absolute = moduleGadget.AbsoluteDepth;
                }
            }
        }
        InternalLogger.Debug("NewCrushDepth: {0}", newCrushDepth);
        if (absolute)
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth - __instance.crushDamage.kBaseCrushDepth);
        else
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth);
        InternalLogger.Debug("CrushDepth: {0} ({1})", __instance.crushDamage.crushDepth, __instance.crushDamage.extraCrushDepth);
    }


    //
    // SEAMOTH
    // ON USE
    // 
    public static void SeamothDelegateUse(SeaMoth __instance, ref float cooldown, int slotID, TechType techType)
    {
        if (!SeamothUpgradeModules.TryGetValue(techType, out ICustomPrefab prefab) && !VehicleUpgradeModules.TryGetValue(techType, out prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        float quickSlotCharge = __instance.quickSlotCharge[slotID];
        float chargeScalar = __instance.GetSlotCharge(slotID);

        moduleGadget.delegateOnUsed?.Invoke(__instance, slotID, quickSlotCharge, chargeScalar);

        if (moduleGadget.Cooldown > 0f)
            cooldown = (float) moduleGadget.Cooldown;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SeaMoth), nameof(SeaMoth.OnUpgradeModuleUse))]
    private static IEnumerable<CodeInstruction> OnUpgradeModuleUse(IEnumerable<CodeInstruction> instructions)
    {
        InternalLogger.Log("Transpiling SeaMoth OnUpgradeModuleUse!", BepInEx.Logging.LogLevel.Debug);
        var matcher = new CodeMatcher(instructions);

        matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldstr, "VehicleTorpedoNoAmmo"),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Call))
            .Advance(3)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_1),
                Transpilers.EmitDelegate(SeamothDelegateUse)
            );

        return matcher.InstructionEnumeration();
    }


#elif BELOWZERO
    //
    // SEATRUCK
    // ON USE
    //
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleUse))]
    private static void DelegateModuleUseCallback(SeaTruckUpgrades __instance, TechType techType, int slotID)
    {
        if (!SeatruckUpgradeModules.TryGetValue(techType, out var prefab))
            return;
        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        float quickSlotCharge = __instance.quickSlotCharge[slotID];
        float chargeScalar = ((IQuickSlots) __instance).GetSlotCharge(slotID);
        moduleGadget.seatruckOnUsed?.Invoke(__instance, __instance.motor, slotID, quickSlotCharge, chargeScalar);
        __instance.quickSlotTimeUsed[slotID] = Time.time;
        __instance.quickSlotCooldown[slotID] = (float) moduleGadget.Cooldown;
    }

    //
    // SEATRUCK
    // ON CHANGE + HULL UPGRADE FIX
    //
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleChange))]
    private static void RefreshSeaTruckCrushDepthModules(SeaTruckUpgrades __instance, int slotID, TechType techType, bool added)
    {
        SeatruckUpgradeModules.DoIf(
            (KeyValuePair<TechType, ICustomPrefab> mapElem) =>
                mapElem.Value.TryGetGadget(out UpgradeModuleGadget moduleGadget)
                && moduleGadget.AbsoluteDepth == true  // Check if the provided module wants to set AbsoluteDepth or not. If not, break, the module is already added at registering of the prefab.
                && moduleGadget.CrushDepth > 0f  // Check if the provided module wants to change depth or not. -1f is the default value if the crush depth is not meant to be changed.
                && !SeaTruckUpgrades.crushDepths.ContainsKey(mapElem.Key),  // Abort if the module is already existing in crushDepths.
                                                                            // For example if the AbsoluteDepth bool is changed on runtime, it won't do anything because the module already exists in crushDepths Dictionary.
            (KeyValuePair<TechType, ICustomPrefab> mapElem) =>
                SeaTruckUpgrades.crushDepths.Add(
                    mapElem.Key,
                    (mapElem.Value.GetGadget<UpgradeModuleGadget>().CrushDepth - __instance.crushDamage.crushDepth)
                )
        );
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleChange))]
    private static void OnUpgradeChangeDelegate(SeaTruckUpgrades __instance, int slotID, TechType techType, bool added)
    {

        float newCrushDepth = 0f;
        bool absolute = false;
        for (int i = 0; i < SeaTruckUpgrades.slotIDs.Length; i++)
        {
            string slot = SeaTruckUpgrades.slotIDs[i];
            TechType techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);
            float depthToCheck;
            if (SeaTruckUpgrades.crushDepths.TryGetValue(techTypeInSlot, out depthToCheck) && depthToCheck > newCrushDepth)
            {
                newCrushDepth = depthToCheck;
                if (SeatruckUpgradeModules.TryGetValue(techTypeInSlot, out var gadget) && gadget.TryGetGadget(out UpgradeModuleGadget mdlGadget))
                    absolute = mdlGadget.AbsoluteDepth;
            }
        }
        if (absolute)
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth - __instance.crushDamage.kBaseCrushDepth);
        else
            __instance.crushDamage.SetExtraCrushDepth(newCrushDepth);

        if (!SeatruckUpgradeModules.TryGetValue(techType, out var prefab))
            return;

        if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
            return;

        if (moduleGadget.seatruckOnRemoved != null && !added)
            moduleGadget.seatruckOnRemoved.Invoke(__instance, __instance.motor, slotID);

        if (moduleGadget.seatruckOnAdded != null && added)
            moduleGadget.seatruckOnAdded.Invoke(__instance, __instance.motor, slotID);
    }

    //
    // SEATRUCK
    // LAZY INITIALIZE (Adding our patch for Toggle)
    //
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.LazyInitialize))]
    private static void LazyInitialize(SeaTruckUpgrades __instance)
    {
        __instance.onToggle += (int slotID, bool state) =>
        {
            var techType = __instance.modules.GetTechTypeInSlot(SeaTruckUpgrades.slotIDs[slotID]);
            if (techType == TechType.None)
                return;

            if (!SeatruckUpgradeModules.TryGetValue(techType, out var prefab))
                return;

            if (!prefab.TryGetGadget(out UpgradeModuleGadget moduleGadget))
                return;

            double energyCost = moduleGadget.EnergyCost;
            moduleGadget.seatruckOnToggled?.Invoke(__instance, __instance.motor, slotID, (float) energyCost, state);
        };
    }

    //
    // HOVERBIKE
    // AWAKE / ENTER / EXIT
    //
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Hoverbike), nameof(Hoverbike.Awake))]
    private static void HoverbikeAwake(Hoverbike __instance)
    {
        InternalLogger.Info("Adding hoverbike modules support component to Hoverbike.");
        __instance.gameObject.EnsureComponent<HoverbikeModulesSupport>();
        InternalLogger.Info("Added hoverbike modules support component to Hoverbike.");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hoverbike), nameof(Hoverbike.EnterVehicle))]
    private static void HoverbikeEnterVehicle(Hoverbike __instance)
    {
        if (__instance.gameObject.TryGetComponent<HoverbikeModulesSupport>(out var hoverbikeModules))
        {
            InternalLogger.Debug("Running EnterVehicle of the hoverbike modules support mono.");
            hoverbikeModules.EnterVehicle();
        }
        else InternalLogger.Error($"Missing {nameof(HoverbikeModulesSupport)} component to hoverbike.");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hoverbike), nameof(Hoverbike.ExitVehicle))]
    private static void HoverbikeExitVehicle(Hoverbike __instance)
    {
        if (__instance.gameObject.TryGetComponent<HoverbikeModulesSupport>(out var hoverbikeModules))
        {
            InternalLogger.Debug("Running ExitVehicle of the hoverbike modules support mono.");
            hoverbikeModules.ExitVehicle();
        }
        else InternalLogger.Error($"Missing {nameof(HoverbikeModulesSupport)} component to hoverbike.");
    }
#endif
}