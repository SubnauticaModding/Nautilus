namespace SMLHelper.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SMLHelper.Assets;
using SMLHelper.Handlers;

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
        }
    }

    private void RegisterPrefab(ModPrefabRoot modPrefabRoot)
    {
        switch(modPrefabRoot)
        {
            case Spawnable spawnable:
                spawnable.Patch();
                break;
            case CustomPrefab customPrefab:
                PrefabHandler.RegisterPrefab(customPrefab);
                foreach(var (position, angles) in customPrefab.CoordinatedSpawns ?? Enumerable.Empty<Spawnable.SpawnLocation>())
                {
                    CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(customPrefab.PrefabInfo.ClassID, position, angles));
                }

                if(customPrefab.BiomesToSpawnIn != null)
                    LootDistributionHandler.AddLootDistributionData(customPrefab.PrefabInfo.ClassID, customPrefab.PrefabInfo.PrefabPath, customPrefab.BiomesToSpawnIn);

                if(customPrefab.Recipe != null)
                    CraftDataHandler.SetTechData(customPrefab.PrefabInfo.TechType, customPrefab.Recipe);
                break;
            case ModPrefab modPrefab:
                PrefabHandler.RegisterPrefab(modPrefab);
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