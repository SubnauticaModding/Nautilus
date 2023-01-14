using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SMLHelper.Assets;
using SMLHelper.Handlers;

namespace SMLHelper.DependencyInjection;

public static partial class Extensions
{
    public static IAssetCollection AddService<TService, TImplementation>(this IAssetCollection assets)
        where TService : class 
        where TImplementation : class, TService
    {
        assets.AddDescriptor(AssetDescriptor.ServiceSingleton(typeof(TService), typeof(TImplementation)));
        return assets;
    }
    
    public static IAssetCollection AddService<TService>(this IAssetCollection assets, TService implementation)
        where TService : class
    {
        assets.AddDescriptor(AssetDescriptor.ServiceSingleton(typeof(TService), implementation));
        return assets;
    }


    public static IAssetCollection AddPrefab<TCustomPrefab>(this IAssetCollection assets)
        where TCustomPrefab : ModPrefabRoot, new()
    {
        var prefab = new TCustomPrefab();
        assets.AddCustomPrefab(prefab);
        return assets;
    }
    
    public static IAssetCollection AddPrefab(this IAssetCollection assets, ModPrefabRoot customPrefab)
    {
        assets.AddCustomPrefab(customPrefab);
        return assets;
    }
}