namespace SMLHelper.Patchers;

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SMLHelper.Utility;
using UnityEngine;
using static Charger;
    
internal static class ChargerPatcher
{
    internal static void Patch(Harmony harmony)
    {
        InternalLogger.Debug($"{nameof(ChargerPatcher)} Applying Harmony Patches");

        MethodInfo chargerPatcherOnEquipMethod = AccessTools.Method(typeof(Charger), nameof(Charger.OnEquip));
        MethodInfo chargerPatcherOnEquipPostfixMethod = AccessTools.Method(typeof(ChargerPatcher), nameof(OnEquipPostfix));

        var harmonyOnEquipPostfix = new HarmonyMethod(chargerPatcherOnEquipPostfixMethod);
        harmony.Patch(chargerPatcherOnEquipMethod, postfix: harmonyOnEquipPostfix); // Patches the ChargerPatcher OnEquipPostfix method.
    }

    private static void OnEquipPostfix(Charger __instance, string slot, InventoryItem item, Dictionary<string, SlotDefinition> ___slots)
    {
        if (___slots.TryGetValue(slot, out SlotDefinition slotDefinition))
        {
            GameObject battery = slotDefinition.battery;
            Pickupable pickupable = item.item;
            if (battery != null && pickupable != null)
            {
                GameObject model;
                switch (__instance)
                {
                    case BatteryCharger _:
                        model = pickupable.gameObject.transform.Find("model/battery_01")?.gameObject ?? pickupable.gameObject.transform.Find("model/battery_ion")?.gameObject;
                        if (model != null && model.TryGetComponent(out Renderer renderer) && battery.TryGetComponent(out Renderer renderer1))
                            renderer1.material.CopyPropertiesFromMaterial(renderer.material);
                        break;
                    case PowerCellCharger _:
                        model = pickupable.gameObject.FindChild("engine_power_cell_01") ?? pickupable.gameObject.FindChild("engine_power_cell_ion");

                        bool modelmesh = model.TryGetComponent(out MeshFilter modelMeshFilter);
                        bool chargermesh = battery.TryGetComponent(out MeshFilter chargerMeshFilter);
                        bool modelRenderer = model.TryGetComponent(out renderer);
                        bool chargerRenderer = battery.TryGetComponent(out renderer1);

                        if (chargermesh && modelmesh && chargerRenderer && modelRenderer)
                        {
                            chargerMeshFilter.mesh = modelMeshFilter.mesh;
                            renderer1.material.CopyPropertiesFromMaterial(renderer.material);
                        }
                        break;
                }
            }
        }
    }
}