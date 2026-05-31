using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration.Injectors;

internal sealed class AssetBundleAssetInjector : IDependencyArgumentInjector
{
    private readonly Dictionary<string, AssetBundle> _bundles = new();
    
    internal AssetBundleAssetInjector(AssetBundle[] bundles)
    {
        bundles?.ForEach(bundle => _bundles.Add(bundle.name, bundle));
    }

    public bool TryInjectToArgument(InjectionContext context, out object value)
    {
        AssetLoadAttribute assetAttribute = context.parameterInfo.GetCustomAttribute<AssetLoadAttribute>(true);
        if(assetAttribute == null)
        {
            value = null;
            return false;
        }
        
        if(_bundles.Count == 0) throw new InjectorException(context, $"Asked to load asset {context.parameterInfo.Name} without providing a bundle for {context.attribute.registryID} registry");
        
        string assetName = context.parameterInfo.Name;
        if (assetAttribute.AssetToLoad != null) // Attribute asset name gets priority over the argument name (if defined)
        {
            assetName = assetAttribute.AssetToLoad;
        }

        if (_bundles.Count == 1 || (_bundles.Count > 1 && assetAttribute.BundleName == null))
        {
            value = LoadBruteForce(assetName, context.parameterInfo.ParameterType);
        }
        else
        {
            value = LoadFromBundle(assetAttribute.BundleName, assetName, context.parameterInfo.ParameterType, context);
        }
        
        if (value == null)
        {
            throw new InjectorException(context, $"Asset {assetAttribute.AssetToLoad} could not be loaded! Returned null for the given bundle(s).");
        }
        
        return true;
    }

    private object LoadFromBundle(string bundleName, string assetName, Type assetType, InjectionContext context)
    {
        if (_bundles.TryGetValue(bundleName, out AssetBundle bundle))
        {
            return bundle.LoadAsset(assetName, assetType);
        }
        throw new InjectorException(context, $"Unknown AssetBundle: {bundleName}! The bundle name should match the internal name set within UnityEditor.");
    }

    private object LoadBruteForce(string assetName, Type assetType)
    {
        object asset = null;
        foreach (AssetBundle bundle in _bundles.Values)
        {
            asset = bundle.LoadAsset(assetName, assetType);
            if (asset != null) break;
        }
        return asset;
    }
}