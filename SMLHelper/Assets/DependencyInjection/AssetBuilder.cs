namespace SMLHelper.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using SMLHelper.API;
using SMLHelper.Assets;
using SMLHelper.Assets.Interfaces;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
using SMLHelper.Utility;

public class AssetBuilder
{
    public AssetCollection Assets { get; } = new();

    private readonly BaseUnityPlugin _plugin;

    public AssetBuilder(BaseUnityPlugin plugin)
    {
        _plugin = plugin;
    }

    public object GetService(Type assetType)
    {
        var descriptor = Assets.GetAssetDescriptors().LastOrDefault(a => a.AssetType == assetType);

        if (descriptor is null)
        {
            throw new Exception($"Couldn't find service: {assetType.FullName}");
        }
        
        if (descriptor.ImplementationInstance is not null)
            return descriptor.ImplementationInstance;

        if (descriptor.ImplementationType is null)
        {
            throw new Exception($"No implementation found for service: {assetType.FullName}");
        }

        var setupMethod = AccessTools.FirstMethod(descriptor.ImplementationType,
            method => method.GetCustomAttribute<InjectionSetupAttribute>() is not null);

        if (setupMethod is null)
        {
            return descriptor.ImplementationInstance = Activator.CreateInstance(descriptor.ImplementationType);
        }

        var parameters = setupMethod.GetParameters();

        var args = parameters.Select(param => GetService(param.ParameterType)).ToArray(); 
        
        var obj = Activator.CreateInstance(descriptor.ImplementationType); 
        setupMethod.Invoke(obj, args);
        descriptor.ImplementationInstance = obj;

        return obj;
    }

    public TAsset GetService<TAsset>()
    {
        var assetType = typeof(TAsset);
        return (TAsset)GetService(assetType);
    }

    private void ResolveSetup(object obj)
    {
        // Get the first method with InjectionSetup attribute
        var setupMethod = AccessTools.FirstMethod(obj.GetType(),
            method => method.GetCustomAttribute<InjectionSetupAttribute>() is not null);
        if (setupMethod is not null)
        {
            // Inject dependencies from the parameters if any
            var args = setupMethod.GetParameters().Select(param => ResolveParam(param)).ToArray();
            // invoke it
            setupMethod.Invoke(obj, args);
        }
    }

    private static readonly MethodInfo _logger = AccessTools.PropertyGetter(typeof(BaseUnityPlugin), "Logger");
    
    /*
     * Execution order:
     * 1. ResolveSetup()
     * 2. RegisterPrefab()
     */
    public void Build()
    {
        // add logger as dependency service
        var logger = _logger.Invoke(_plugin, new object[] { }) as ManualLogSource;
        Assets.AddService(logger);

        foreach (var customPrefab in Assets.GetCustomPrefabs())
        {
            ResolveSetup(customPrefab);
            RegisterPrefab(customPrefab);
            HandleInterfaces(customPrefab);
        }
    }

    private void HandleInterfaces(IModPrefab customPrefab)
    {
        var techType = customPrefab.PrefabInfo.TechType;

        if(customPrefab is ICraftable craftable && techType != TechType.None && craftable.FabricatorType != CraftTree.Type.None)
        {
            InternalLogger.Debug($"{customPrefab.PrefabInfo.ClassID} is ICraftable, Registering Craft Node, Recipe and Craft Speed.");

            if(craftable.StepsToFabricatorTab == null || craftable.StepsToFabricatorTab.Length == 0)
                CraftTreeHandler.AddCraftingNode(craftable.FabricatorType, techType);
            else
                CraftTreeHandler.AddCraftingNode(craftable.FabricatorType, techType, craftable.StepsToFabricatorTab);
            if(craftable.CraftingTime >= 0f)
                CraftDataHandler.SetCraftingTime(techType, craftable.CraftingTime);

            if(craftable.RecipeData != null)
                CraftDataHandler.SetTechData(techType, craftable.RecipeData);
            else
                CraftDataHandler.SetTechData(techType, new RecipeData() { craftAmount = 1, Ingredients = new() { new(TechType.Titanium) } });

        }

        if(customPrefab is ICustomBattery customBattery)
        {
            InternalLogger.Debug($"{customPrefab.PrefabInfo.ClassID} is ICustomBattery, Adding to the Battery Registry.");
            var batteryType = customBattery.BatteryType;
            if(batteryType == API.BatteryType.Battery || batteryType == API.BatteryType.Both)
                CustomBatteryHandler.RegisterCustomBattery(techType);

            if(batteryType == API.BatteryType.PowerCell || batteryType == API.BatteryType.Both)
                CustomBatteryHandler.RegisterCustomPowerCell(techType);
        }
    }

    private void RegisterPrefab(IModPrefab prefab)
    {
        switch(prefab.PrefabInfo.ModPrefab)
        {
            case Spawnable spawnable:
                spawnable.Patch();
                break;
            default:
                PrefabHandler.RegisterPrefab(prefab.PrefabInfo);
                break;
        }
    }
    
    private object ResolveParam(ParameterInfo param)
    {
        if (GetService(param.ParameterType) is {} service)
            return service;

        throw new Exception($"Service of type: {param.ParameterType} not found.");
    }

    /*private void ResolveItemFactory(ItemFactory factory)
    {
        var args = factory.Configure.Method.GetParameters().Select(param => ResolveParam(param)).ToArray();
        factory.Configure.DynamicInvoke(args);
    }*/
}