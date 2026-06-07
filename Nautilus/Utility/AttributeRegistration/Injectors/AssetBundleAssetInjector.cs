using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration.Injectors;

internal sealed class AssetBundleAssetInjector : IDependencyArgumentInjector
{
    public bool TryInjectToArgument(InjectionContext context, out object value)
    {
        AssetLoadAttribute assetAttribute = context.ParameterInfo.GetCustomAttribute<AssetLoadAttribute>(true);
        if(assetAttribute == null)
        {
            value = null;
            return false;
        }

        List<AssetBundle> bundles = context.Service.GetAllSingletons<AssetBundle>().ToList();
        
        if (bundles.Count == 0) throw new InjectorException(context, $"Asked to load asset {context.ParameterInfo.Name} without providing bundles within the service! Add then as a singleton/keyedSingleton");
        
        string assetName = GetAssetNameToLoad(context, assetAttribute);
        
        if (bundles.Count == 1)
        {
            value = bundles[0].LoadAsset(assetName, context.ParameterInfo.ParameterType);
        }
        else
        {
            value = LoadWithMultipleBundles(assetAttribute, assetName, context);
        }
        
        if (value == null)
        {
            throw new InjectorException(context, $"Asset {assetAttribute.AssetToLoad} could not be loaded! Returned null for the given bundle(s).");
        }
           
        return true;
    }

    // Priority: IAssetBundleKeyResolver > AssetToLoad in Attribute > Parameter Name
    private static string GetAssetNameToLoad(InjectionContext context, AssetLoadAttribute assetAttribute)
    {
        if (assetAttribute.AssetToLoad != null)
        {
            return assetAttribute.AssetToLoad;
        }
        return context.ParameterInfo.Name;
    }

    private static object LoadWithMultipleBundles(AssetLoadAttribute assetAttribute, string assetName, InjectionContext context)
    {
        AssetBundle bundle = GetSpecificBundle(assetAttribute, context);
        return bundle.LoadAsset(assetName, context.ParameterInfo.ParameterType);
    }

    private static AssetBundle GetSpecificBundle(AssetLoadAttribute assetAttribute, InjectionContext context)
    {
        IRegistryAssetBundleResolver resolver = context.Service.GetLatestSingleton<IRegistryAssetBundleResolver>();
        if (resolver != null)
        {
            AssetBundle resolvedBundle = resolver.GetAssetBundle(context);
            if (resolvedBundle == null) throw new InjectorException(context, "The AssetBundle given from a IRegistryAssetBundleResolver is null!");
            return resolvedBundle;
        }

        if (string.IsNullOrEmpty(assetAttribute.BundleKey)) throw new InjectorException(context, "Asset bundle name could not be determined with multiple bundles added as singletons!");
        
        AssetBundle keyedBundle = context.Service.GetKeyedSingleton<AssetBundle>(assetAttribute.BundleKey);

        if (keyedBundle == null) throw new InjectorException(context, $"Unknown AssetBundle: {assetAttribute.BundleKey}! The bundle key should be resolved " +
                                                                        $"to the same value used when adding the bundle as a keyedSingleton.");
        
        return keyedBundle;
    }
}