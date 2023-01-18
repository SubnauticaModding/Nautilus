namespace SMLHelper.Patchers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using SMLHelper.Assets;
using SMLHelper.Assets.Interfaces;
using SMLHelper.Handlers;
using SMLHelper.Utility;
using UnityEngine;
using UWE;
using static Charger;
using static EnergyMixin;

internal static class CustomBatteriesPatcher
{
    public const string BatteryCraftTab = "BatteryTab";
    public const string PowCellCraftTab = "PowCellTab";
    public const string ElecCraftTab = "Electronics";
    public const string ResCraftTab = "Resources";

    public static readonly string[] BatteryCraftPath = new[] { ResCraftTab, BatteryCraftTab };
    public static readonly string[] PowCellCraftPath = new[] { ResCraftTab, PowCellCraftTab };

    public static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static List<TechType> BatteryItems { get; } = new List<TechType>();
    public static List<TechType> PowerCellItems { get; } = new List<TechType>();

    public static Dictionary<TechType, ICustomBattery> BatteryModels { get; } = new Dictionary<TechType, ICustomBattery>();
    public static Dictionary<TechType, ICustomBattery> PowerCellModels { get; } = new Dictionary<TechType, ICustomBattery>();

    public static HashSet<TechType> TrackItems { get; } = new HashSet<TechType>();

    private static bool _decoModDetectionRun = false;
    private static FieldInfo enablePlaceBatteriesField { get; set; }

    public static bool PlaceBatteriesFeatureEnabled
    {
        get
        {
            if(!_decoModDetectionRun)
            {
                DecorationsModCheck();
            }

            return enablePlaceBatteriesField != null ? (bool)enablePlaceBatteriesField.GetValue(null) : false;
        }
    }

    private static void DecorationsModCheck()
    {
        PluginInfo puginInfo = Chainloader.PluginInfos.Values.Where((x) => x.Metadata.Name == "DecorationsMod" && x.Instance.enabled).FirstOrFallback(null);
        Assembly decorationsModAssembly = null;
        if(puginInfo == null)
        {
            _decoModDetectionRun = true;
            InternalLogger.Debug($"DecorationsMod not detected.");
            return;
        }

        decorationsModAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.Location == puginInfo.Location).FirstOrFallback(decorationsModAssembly);
        if(decorationsModAssembly == null)
        {
            InternalLogger.Debug($"DecorationsMod detected but unable to find assembly.");
            _decoModDetectionRun = true;
            return;
        }

        Type decorationsModConfig = decorationsModAssembly.GetType("DecorationsMod.ConfigSwitcher", false);
        if(decorationsModConfig == null)
        {
            InternalLogger.Debug($"DecorationsMod assembly found but unable to find DecorationsMod.ConfigSwitcher Type.");
            _decoModDetectionRun = true;
            return;
        }

        enablePlaceBatteriesField = decorationsModConfig.GetField("EnablePlaceBatteries", BindingFlags.Public | BindingFlags.Static);
        if(enablePlaceBatteriesField == null || !enablePlaceBatteriesField.IsStatic)
        {
            InternalLogger.Debug($"DecorationsMod.ConfigSwitcher Type found but unable to find Static EnablePlaceBatteries Field.");
            enablePlaceBatteriesField = null;
            _decoModDetectionRun = true;
            return;
        }
        _decoModDetectionRun = true;
    }

    internal static void Patch(Harmony harmony)
    {
        InternalLogger.Debug($"{nameof(CustomBatteriesPatcher)} Applying Harmony Patches");

        MethodInfo energyMixinNotifyHasBattery = AccessTools.Method(typeof(EnergyMixin), nameof(EnergyMixin.NotifyHasBattery));
        MethodInfo notifyHasBatteryPostfixMethod = AccessTools.Method(typeof(CustomBatteriesPatcher), nameof(NotifyHasBatteryPostfix));

        var harmonyNotifyPostfix = new HarmonyMethod(notifyHasBatteryPostfixMethod);
        harmony.Patch(energyMixinNotifyHasBattery, postfix: harmonyNotifyPostfix); // Patches the EnergyMixin NotifyHasBattery method

        MethodInfo energyMixStartMethod = AccessTools.Method(typeof(EnergyMixin), nameof(EnergyMixin.Awake));
        MethodInfo startPrefixMethod = AccessTools.Method(typeof(CustomBatteriesPatcher), nameof(AwakePrefix));

        var harmonyStartPrefix = new HarmonyMethod(startPrefixMethod);
        harmony.Patch(energyMixStartMethod, prefix: harmonyStartPrefix); // Patches the EnergyMixin Awake method

        MethodInfo chargerPatcherOnEquipMethod = AccessTools.Method(typeof(Charger), nameof(Charger.OnEquip));
        MethodInfo chargerPatcherOnEquipPostfixMethod = AccessTools.Method(typeof(CustomBatteriesPatcher), nameof(OnEquipPostfix));

        var harmonyOnEquipPostfix = new HarmonyMethod(chargerPatcherOnEquipPostfixMethod);
        harmony.Patch(chargerPatcherOnEquipMethod, postfix: harmonyOnEquipPostfix); // Patches the ChargerPatcher OnEquipPostfix method.

        PatchCraftingTabs();

        InternalLogger.Debug($"{nameof(CustomBatteriesPatcher)} Patched.");
    }


    internal static void PatchCraftingTabs()
    {
        InternalLogger.Info("Separating batteries and power cells into their own fabricator crafting tabs");

        // Remove original crafting nodes
        CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.Battery.ToString());
        CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PrecursorIonBattery.ToString());
        CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PowerCell.ToString());
        CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PrecursorIonPowerCell.ToString());

        // Add a new set of tab nodes for batteries and power cells
        CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, BatteryCraftTab, "Batteries", SpriteManager.Get(TechType.Battery), ResCraftTab);
        CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, PowCellCraftTab, "Power Cells", SpriteManager.Get(TechType.PowerCell), ResCraftTab);

        // Move the original batteries and power cells into these new tabs
        CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.Battery, BatteryCraftPath);
        CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonBattery, BatteryCraftPath);
        CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PowerCell, PowCellCraftPath);
        CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonPowerCell, PowCellCraftPath);
    }

    private static void OnEquipPostfix(Charger __instance, string slot, InventoryItem item, Dictionary<string, SlotDefinition> ___slots)
    {
        if(___slots.TryGetValue(slot, out SlotDefinition slotDefinition))
        {
            GameObject battery = slotDefinition.battery;
            Pickupable pickupable = item.item;
            if(battery != null && pickupable != null)
            {
                GameObject model;
                switch(__instance)
                {
                    case BatteryCharger _:
                        model = pickupable.gameObject.transform.Find("model/battery_01")?.gameObject ?? pickupable.gameObject.transform.Find("model/battery_ion")?.gameObject;
                        if(model != null && model.TryGetComponent(out Renderer renderer) && battery.TryGetComponent(out Renderer renderer1))
                            renderer1.material.CopyPropertiesFromMaterial(renderer.material);
                        break;
                    case PowerCellCharger _:
                        model = pickupable.gameObject.FindChild("engine_power_cell_01") ?? pickupable.gameObject.FindChild("engine_power_cell_ion");

                        bool modelmesh = model.TryGetComponent(out MeshFilter modelMeshFilter);
                        bool chargermesh = battery.TryGetComponent(out MeshFilter chargerMeshFilter);
                        bool modelRenderer = model.TryGetComponent(out renderer);
                        bool chargerRenderer = battery.TryGetComponent(out renderer1);

                        if(chargermesh && modelmesh && chargerRenderer && modelRenderer)
                        {
                            chargerMeshFilter.mesh = modelMeshFilter.mesh;
                            renderer1.material.CopyPropertiesFromMaterial(renderer.material);
                        }
                        break;
                }
            }
        }
    }

    public static void NotifyHasBatteryPostfix(ref EnergyMixin __instance, InventoryItem item)
    {
        if (PowerCellItems.Count == 0)
            return;

        // For vehicles that show a battery model when one is equipped,
        // this will replicate the model for the normal Power Cell so it doesn't look empty

        // Null checks added on every step of the way
        TechType? itemInSlot = item?.item?.GetTechType();

        if (!itemInSlot.HasValue || itemInSlot.Value == TechType.None)
            return; // Nothing here

        TechType powerCellTechType = itemInSlot.Value;
        bool isKnownModdedPowerCell = PowerCellItems.Find(techType => techType == powerCellTechType) != TechType.None;

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

    private static Material IonBatteryMaterial;
    private static Material IonPowerCellMaterial;

    public static void AwakePrefix(ref EnergyMixin __instance)
    {
        // This is necessary to allow the new batteries to be compatible with tools and vehicles

        if(!__instance.allowBatteryReplacement)
            return; // Battery replacement not allowed - No need to make changes

        if(BatteryItems.Count == 0 && PowerCellItems.Count == 0)
            return;

        CoroutineHost.StartCoroutine(PatchAllowedBatteriesAsync(__instance));
    }

    private static IEnumerator PatchAllowedBatteriesAsync(EnergyMixin __instance)
    {
        List<TechType> compatibleBatteries = __instance.compatibleBatteries;
        List<BatteryModels> Models = new List<BatteryModels>(__instance.batteryModels ?? new BatteryModels[0]);

        GameObject batteryModel = null;
        GameObject powerCellModel = null;
        GameObject ionBatteryModel = null;
        GameObject ionPowerCellModel = null;

        List<TechType> existingTechtypes = new List<TechType>();
        List<GameObject> existingModels = new List<GameObject>();


        //First check for models already setup
        for(int b = 0; b < Models.Count; b++)
        {
            BatteryModels model = Models[b];
            switch(model.techType)
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
        foreach(Renderer renderer in __instance.gameObject.GetComponentsInChildren<Renderer>(true))
        {
            if(renderer.gameObject.GetComponentInParent<Battery>(true) != null)
                continue;

            switch(renderer?.material?.mainTexture?.name)
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
        if(batteryModel != null && !existingTechtypes.Contains(TechType.Battery))
        {
            Models.Add(new BatteryModels() { model = batteryModel, techType = TechType.Battery });
            existingTechtypes.Add(TechType.Battery);
            existingModels.Add(batteryModel);
        }

        //Add missing models that were found or create new ones if possible.
        if(!existingTechtypes.Contains(TechType.PrecursorIonBattery))
        {
            if(ionBatteryModel != null)
            {
                Models.Add(new BatteryModels() { model = ionBatteryModel, techType = TechType.PrecursorIonBattery });
                existingTechtypes.Add(TechType.PrecursorIonBattery);
                existingModels.Add(ionBatteryModel);
            }
            else if(batteryModel != null)
            {
                if(IonBatteryMaterial == null)
                {
                    var task = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorIonBattery);
                    yield return task;
                    IonBatteryMaterial = task.GetResult()?.GetComponentInChildren<Renderer>()?.material;               
                }

                if(IonBatteryMaterial != null)
                {
                    ionBatteryModel = GameObject.Instantiate(batteryModel, batteryModel.transform.parent);
                    ionBatteryModel.name = "precursorIonBatteryModel";
                    ionBatteryModel.GetComponentInChildren<Renderer>().material = new Material(IonBatteryMaterial);
                    Models.Add(new BatteryModels() { model = ionBatteryModel, techType = TechType.PrecursorIonBattery });
                    existingTechtypes.Add(TechType.PrecursorIonBattery);
                    existingModels.Add(ionBatteryModel);
                }
            }
        }

        //Add missing models that were found or create new ones if possible.
        if(powerCellModel != null && !existingTechtypes.Contains(TechType.PowerCell))
        {
            Models.Add(new BatteryModels() { model = powerCellModel, techType = TechType.PowerCell });
            existingTechtypes.Add(TechType.PowerCell);
            existingModels.Add(powerCellModel);
        }

        //Add missing models that were found or create new ones if possible.
        if(!existingTechtypes.Contains(TechType.PrecursorIonPowerCell))
        {
            if(ionPowerCellModel != null)
            {
                Models.Add(new BatteryModels() { model = ionPowerCellModel, techType = TechType.PrecursorIonPowerCell });
                existingTechtypes.Add(TechType.PrecursorIonPowerCell);
                existingModels.Add(ionPowerCellModel);
            }
            else if(powerCellModel != null)
            {
                if(IonPowerCellMaterial == null)
                {
                    var task = CraftData.GetPrefabForTechTypeAsync(TechType.PrecursorIonPowerCell);
                    yield return task;
                    IonPowerCellMaterial = task.GetResult()?.GetComponentInChildren<Renderer>()?.material;
                }

                if(IonPowerCellMaterial != null)
                {
                    ionPowerCellModel = GameObject.Instantiate(powerCellModel, powerCellModel.transform.parent);
                    ionPowerCellModel.name = "PrecursorIonPowerCellModel";
                    ionPowerCellModel.GetComponentInChildren<Renderer>().material = new Material(IonPowerCellMaterial);
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
            if(!existingModels.Contains(gameObject))
                controlledObjects.Add(gameObject);
        }
        __instance.controlledObjects = controlledObjects.ToArray();

        if(compatibleBatteries.Contains(TechType.Battery) || compatibleBatteries.Contains(TechType.PrecursorIonBattery))
        {
            // If the regular Battery or Ion Battery is compatible with this item, then modded batteries should also be compatible
            AddMissingTechTypesToList(compatibleBatteries, BatteryItems);

            if(batteryModel != null && ionBatteryModel != null)
            {
                //If we have enough information to make custom models for this tool or vehicle then create them.
                AddCustomModels(batteryModel, ionBatteryModel, ref Models, BatteryModels, existingTechtypes);
            }
        }

        if(compatibleBatteries.Contains(TechType.PowerCell) || compatibleBatteries.Contains(TechType.PrecursorIonPowerCell))
        {
            // If the regular Power Cell or Ion Power Cell is compatible with this item, then modded power cells should also be compatible
            AddMissingTechTypesToList(compatibleBatteries, PowerCellItems);

            if(powerCellModel != null && ionPowerCellModel != null)
            {
                //If we have enough information to make custom models for this tool or vehicle then create them.
                AddCustomModels(powerCellModel, ionPowerCellModel, ref Models, PowerCellModels, existingTechtypes);
            }
        }

        __instance.batteryModels = Models.ToArray();
        yield break;
    }

    private static void AddCustomModels(GameObject originalModel, GameObject ionModel, ref List<BatteryModels> Models, Dictionary<TechType, ICustomBattery> customModels, List<TechType> existingTechtypes)
    {
        var originalModelState = originalModel.activeSelf;
        originalModel.SetActive(false);

        var ionModelState = ionModel.activeSelf;
        ionModel.SetActive(false);        
        
        Renderer originalRenderer = originalModel.GetComponentInChildren<Renderer>(true);

        SkyApplier skyApplier = null;
        List<Renderer> renderers = null;
        foreach (SkyApplier sa in originalModel.GetComponentsInParent<SkyApplier>(true))
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

        foreach (KeyValuePair<TechType, ICustomBattery> pair in customModels)
        {
            //dont add models that already exist.
            if (existingTechtypes.Contains(pair.Key))
                continue;

            CustomModelData modelData = null;
            bool UseIonModelsAsBase = false;

            var modPrefab = pair.Value;
            //check which model to base the new model from
            if(modPrefab is ICustomBattery customBattery)
                UseIonModelsAsBase = customBattery.BatteryModel == BatteryModel.IonBattery || customBattery.BatteryModel == BatteryModel.IonPowerCell || customBattery.BatteryModel == BatteryModel.IonCustom;
            
            if(modPrefab is ICustomModelData customModelData)
                modelData = customModelData.ModelDatas?.FirstOrFallback(null);

            GameObject modelBase = UseIonModelsAsBase ? ionModel : originalModel;

            //create the new model and set it to have the same parent as the original
            GameObject obj = GameObject.Instantiate(modelBase, modelBase.transform.parent);
            obj.name = pair.Key.AsString() + "_model";
            obj.SetActive(false);

            Renderer renderer = obj.GetComponentInChildren<Renderer>(true);

            if (renderer != null)
            {
                if (modelData != null)
                {
                    //Set the customized textures for the newly created model to the textures given by the modder.

                    if (modelData.CustomTexture != null)
                        renderer.material.SetTexture(ShaderPropertyID._MainTex, modelData.CustomTexture);

                    if (modelData.CustomNormalMap != null)
                        renderer.material.SetTexture(ShaderPropertyID._BumpMap, modelData.CustomNormalMap);

                    if (modelData.CustomSpecMap != null)
                        renderer.material.SetTexture(ShaderPropertyID._SpecTex, modelData.CustomSpecMap);

                    if (modelData.CustomIllumMap != null)
                    {
                        renderer.material.SetTexture(ShaderPropertyID._Illum, modelData.CustomIllumMap);
                        renderer.material.SetFloat(ShaderPropertyID._GlowStrength, modelData.CustomIllumStrength);
                        renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, modelData.CustomIllumStrength);
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

        originalModel.SetActive(originalModelState);
        ionModel.SetActive(ionModelState);
    }

    private static void AddMissingTechTypesToList(List<TechType> compatibleTechTypes, List<TechType> toBeAdded)
    {
        for (int i = toBeAdded.Count - 1; i >= 0; i--)
        {
            TechType entry = toBeAdded[i];
            if (compatibleTechTypes.Contains(entry))
                return;

            compatibleTechTypes.Add(entry);
        }
    }
}