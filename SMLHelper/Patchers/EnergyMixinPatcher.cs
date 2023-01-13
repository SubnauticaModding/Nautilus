namespace SMLHelper.Patchers;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using API;
using Assets;
using HarmonyLib;
using SMLHelper.Utility;
using UnityEngine;
using static EnergyMixin;

internal static class EnergyMixinPatcher
{
    internal static void Patch(Harmony harmony)
    {
        InternalLogger.Debug($"{nameof(EnergyMixinPatcher)} Applying Harmony Patches");

        MethodInfo energyMixinNotifyHasBattery = AccessTools.Method(typeof(EnergyMixin), nameof(EnergyMixin.NotifyHasBattery));
        MethodInfo notifyHasBatteryPostfixMethod = AccessTools.Method(typeof(EnergyMixinPatcher), nameof(NotifyHasBatteryPostfix));

        var harmonyNotifyPostfix = new HarmonyMethod(notifyHasBatteryPostfixMethod);
        harmony.Patch(energyMixinNotifyHasBattery, postfix: harmonyNotifyPostfix); // Patches the EnergyMixin NotifyHasBattery method

        MethodInfo energyMixStartMethod = AccessTools.Method(typeof(EnergyMixin), nameof(EnergyMixin.Awake));
        MethodInfo startPrefixMethod = AccessTools.Method(typeof(EnergyMixinPatcher), nameof(AwakePrefix));

        var harmonyStartPrefix = new HarmonyMethod(startPrefixMethod);
        harmony.Patch(energyMixStartMethod, prefix: harmonyStartPrefix); // Patches the EnergyMixin Awake method
    }

    public static void NotifyHasBatteryPostfix(ref EnergyMixin __instance, InventoryItem item)
    {
        if (CbDatabase.PowerCellItems.Count == 0)
            return;

        // For vehicles that show a battery model when one is equipped,
        // this will replicate the model for the normal Power Cell so it doesn't look empty

        // Null checks added on every step of the way
        TechType? itemInSlot = item?.item?.GetTechType();

        if (!itemInSlot.HasValue || itemInSlot.Value == TechType.None)
            return; // Nothing here

        TechType powerCellTechType = itemInSlot.Value;
        bool isKnownModdedPowerCell = CbDatabase.PowerCellItems.Find(pc => pc.TechType == powerCellTechType) != null;

        if (isKnownModdedPowerCell)
        {
            int modelToDisplay = 0; // If a matching model cannot be found, the standard PowerCell model will be used instead.
            for (int b = 0; b < __instance.batteryModels.Length; b++)
            {
                if (__instance.batteryModels[b].techType == powerCellTechType)
                {
                    modelToDisplay = b;
                    break;
                }
            }

            __instance.batteryModels[modelToDisplay].model.SetActive(true);
        }
    }

    public static void AwakePrefix(ref EnergyMixin __instance)
    {
        InternalLogger.Debug($"{__instance.gameObject.name}: {__instance.allowBatteryReplacement} : {CbDatabase.BatteryItems.Count}:{CbDatabase.PowerCellItems.Count}");
        // This is necessary to allow the new batteries to be compatible with tools and vehicles

        if (!__instance.allowBatteryReplacement)
            return; // Battery replacement not allowed - No need to make changes

        if (CbDatabase.BatteryItems.Count == 0 && CbDatabase.PowerCellItems.Count == 0)
            return;

        List<TechType> compatibleBatteries = __instance.compatibleBatteries;
        List<BatteryModels> Models = new List<BatteryModels>(__instance.batteryModels ?? new BatteryModels[0]);

        GameObject batteryModel = null;
        GameObject powerCellModel = null;
        GameObject ionBatteryModel = null;
        GameObject ionPowerCellModel = null;

        List<TechType> existingTechtypes = new List<TechType>();
        List<GameObject> existingModels = new List<GameObject>();


        //First check for models already setup
        for (int b = 0; b < Models.Count; b++)
        {
            BatteryModels model = Models[b]; 
            switch (model.techType)
            {
                case TechType.Battery:
                    batteryModel = model.model;
                    break;
                case TechType.PowerCell:
                    powerCellModel = model.model;
                    break;
                case TechType.PrecursorIonBattery:
                    ionBatteryModel = model.model;
                    break;
                case TechType.PrecursorIonPowerCell:
                    ionPowerCellModel = model.model;
                    break;
            }
            existingTechtypes.Add(Models[b].techType);
            existingModels.Add(Models[b].model);
        }

        //Then check for models not already setup.
        foreach (Renderer renderer in __instance.gameObject.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.gameObject.GetComponentInParent<Battery>(true) != null)
                continue;

            switch (renderer?.material?.mainTexture?.name)
            {
                case "battery_01":
                    batteryModel = batteryModel ??= renderer.gameObject;
                    break;
                case "battery_ion":
                    ionBatteryModel = ionBatteryModel ??= renderer.gameObject;
                    break;
                case "power_cell_01":
                    powerCellModel = powerCellModel ??= renderer.gameObject;
                    break;
                case "engine_power_cell_ion":
                    ionPowerCellModel = ionPowerCellModel ??= renderer.gameObject;
                    break;
            }
        }

        //Add missing models that were found or create new ones if possible.
        if (batteryModel != null && !existingTechtypes.Contains(TechType.Battery))
        {
            Models.Add(new BatteryModels() { model = batteryModel, techType = TechType.Battery });
            existingTechtypes.Add(TechType.Battery);
            existingModels.Add(batteryModel);
        }

        //Add missing models that were found or create new ones if possible.
        if (!existingTechtypes.Contains(TechType.PrecursorIonBattery))
        {
            if (ionBatteryModel != null)
            {
                Models.Add(new BatteryModels() { model = ionBatteryModel, techType = TechType.PrecursorIonBattery });
                existingTechtypes.Add(TechType.PrecursorIonBattery);
                existingModels.Add(ionBatteryModel);
            }
            else if (batteryModel != null)
            {

                GameObject ionBatteryPrefab = Resources.Load<GameObject>("worldentities/tools/precursorionbattery");
                ionBatteryModel = GameObject.Instantiate(batteryModel, batteryModel.transform.parent);
                ionBatteryModel.name = "precursorIonBatteryModel";


                Material ionBatteryMaterial = ionBatteryPrefab?.GetComponentInChildren<Renderer>()?.material;
                if (ionBatteryMaterial != null)
                {
                    ionBatteryModel.GetComponentInChildren<Renderer>().material = new Material(ionBatteryMaterial);
                    Models.Add(new BatteryModels() { model = ionBatteryModel, techType = TechType.PrecursorIonBattery });
                    existingTechtypes.Add(TechType.PrecursorIonBattery);
                    existingModels.Add(ionBatteryModel);
                }
            }
        }

        //Add missing models that were found or create new ones if possible.
        if (powerCellModel != null && !existingTechtypes.Contains(TechType.PowerCell))
        {
            Models.Add(new BatteryModels() { model = powerCellModel, techType = TechType.PowerCell });
            existingTechtypes.Add(TechType.PowerCell);
            existingModels.Add(powerCellModel);
        }

        //Add missing models that were found or create new ones if possible.
        if (!existingTechtypes.Contains(TechType.PrecursorIonPowerCell))
        {
            if (ionPowerCellModel != null)
            {
                Models.Add(new BatteryModels() { model = ionPowerCellModel, techType = TechType.PrecursorIonPowerCell });
                existingTechtypes.Add(TechType.PrecursorIonPowerCell);
                existingModels.Add(ionPowerCellModel);
            }
            else if (powerCellModel != null)
            {

                GameObject ionPowerCellPrefab = Resources.Load<GameObject>("worldentities/tools/precursorionpowercell");
                ionPowerCellModel = GameObject.Instantiate(powerCellModel, powerCellModel.transform.parent);
                ionPowerCellModel.name = "PrecursorIonPowerCellModel";


                Material precursorIonPowerCellMaterial = ionPowerCellPrefab?.GetComponentInChildren<Renderer>()?.material;
                if (precursorIonPowerCellMaterial != null)
                {
                    ionPowerCellModel.GetComponentInChildren<Renderer>().material = new Material(precursorIonPowerCellMaterial);
                    Models.Add(new BatteryModels() { model = ionPowerCellModel, techType = TechType.PrecursorIonPowerCell });
                    existingTechtypes.Add(TechType.PrecursorIonPowerCell);
                    existingModels.Add(ionPowerCellModel);
                }
            }
        }

        //Remove models from the controlled objects list after we have added them as controlled models instead.
        List<GameObject> controlledObjects = new List<GameObject>(__instance.controlledObjects ?? new GameObject[0]);

        foreach(GameObject gameObject in __instance.controlledObjects ?? new GameObject[0])
        {
            if (!existingModels.Contains(gameObject))
                controlledObjects.Add(gameObject);
        }
        __instance.controlledObjects = controlledObjects.ToArray();

        if (compatibleBatteries.Contains(TechType.Battery) || compatibleBatteries.Contains(TechType.PrecursorIonBattery))
        {
            InternalLogger.Debug($"Ensuring {__instance.gameObject.name} can use these batteries: {string.Join(", ", CbDatabase.BatteryItems.Select(x=>x.ClassID))}");
            // If the regular Battery or Ion Battery is compatible with this item, then modded batteries should also be compatible
            AddMissingTechTypesToList(compatibleBatteries, CbDatabase.BatteryItems);

            if (batteryModel != null && ionBatteryModel != null)
            {
                //If we have enough information to make custom models for this tool or vehicle then create them.
                AddCustomModels(batteryModel, ionBatteryModel, ref Models, CbDatabase.BatteryModels, existingTechtypes);
            }
        }

        if (compatibleBatteries.Contains(TechType.PowerCell) || compatibleBatteries.Contains(TechType.PrecursorIonPowerCell))
        {
            InternalLogger.Debug($"Ensuring {__instance.gameObject.name} can use these PowerCells: {string.Join(", ", CbDatabase.PowerCellItems.Select(x => x.ClassID))}");
            // If the regular Power Cell or Ion Power Cell is compatible with this item, then modded power cells should also be compatible
            AddMissingTechTypesToList(compatibleBatteries, CbDatabase.PowerCellItems);

            if (powerCellModel != null && ionPowerCellModel != null)
            {
                //If we have enough information to make custom models for this tool or vehicle then create them.
                AddCustomModels(powerCellModel, ionPowerCellModel, ref Models, CbDatabase.PowerCellModels, existingTechtypes);
            }
        }

        __instance.batteryModels = Models.ToArray();
    }

    private static void AddCustomModels(GameObject originalModel, GameObject ionModel, ref List<BatteryModels> Models, Dictionary<TechType, CBModelData> customModels, List<TechType> existingTechtypes)
    {
        Renderer originalRenderer = originalModel.GetComponentInChildren<Renderer>();

        SkyApplier skyApplier = null;
        List<Renderer> renderers = null;
        foreach (SkyApplier sa in originalModel.GetComponentsInParent<SkyApplier>())
        {
            foreach(Renderer renderer in sa.renderers)
            {
                if (renderer == originalRenderer)
                {
                    skyApplier = sa;
                    renderers = new List<Renderer>(skyApplier.renderers);
                    break;
                }
            }
            if (skyApplier != null)
                break;
        }

        foreach (KeyValuePair<TechType, CBModelData> pair in customModels)
        {
            //dont add models that already exist.
            if (existingTechtypes.Contains(pair.Key))
                continue;

            //check which model to base the new model from
            GameObject modelBase = (pair.Value?.UseIonModelsAsBase ?? false) ? ionModel : originalModel;

            //create the new model and set it to have the same parent as the original
            GameObject obj = GameObject.Instantiate(modelBase, modelBase.transform.parent);
            obj.name = pair.Key.AsString() + "_model";
            obj.SetActive(false);

            Renderer renderer = obj.GetComponentInChildren<Renderer>();

            if (renderer != null)
            {
                if (pair.Value != null)
                {
                    //Set the customized textures for the newly created model to the textures given by the modder.

                    if (pair.Value.CustomTexture != null)
                        renderer.material.SetTexture(ShaderPropertyID._MainTex, pair.Value.CustomTexture);

                    if (pair.Value.CustomNormalMap != null)
                        renderer.material.SetTexture(ShaderPropertyID._BumpMap, pair.Value.CustomNormalMap);

                    if (pair.Value.CustomSpecMap != null)
                        renderer.material.SetTexture(ShaderPropertyID._SpecTex, pair.Value.CustomSpecMap);

                    if (pair.Value.CustomIllumMap != null)
                    {
                        renderer.material.SetTexture(ShaderPropertyID._Illum, pair.Value.CustomIllumMap);
                        renderer.material.SetFloat(ShaderPropertyID._GlowStrength, pair.Value.CustomIllumStrength);
                        renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, pair.Value.CustomIllumStrength);
                    }
                }

                if(skyApplier != null)
                    renderers.Add(renderer);
            }

            Models.Add(new BatteryModels() { model = obj, techType = pair.Key });
            existingTechtypes.Add(pair.Key);
        }

        if(skyApplier != null)
            skyApplier.renderers = renderers.ToArray();

    }

    private static void AddMissingTechTypesToList(List<TechType> compatibleTechTypes, List<CbCore> toBeAdded)
    {
        for (int i = toBeAdded.Count - 1; i >= 0; i--)
        {
            TechType entry = toBeAdded[i].TechType;

            if (compatibleTechTypes.Contains(entry))
                return;

            compatibleTechTypes.Add(entry);
        }
    }
}