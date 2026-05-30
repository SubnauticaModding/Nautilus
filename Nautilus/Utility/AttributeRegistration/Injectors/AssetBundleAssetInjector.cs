using System;
using System.Reflection;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration.Injectors;

internal sealed class AssetBundleAssetInjector(AssetBundle[] bundles) : IDependencyArgumentInjector
{
    public bool TryInjectToArgument(InjectionContext context, out object value)
    {
        AssetLoadAttribute assetAttribute = context.parameterInfo.GetCustomAttribute<AssetLoadAttribute>(true);
        if(assetAttribute == null)
        {
            value = null;
            return false;
        }
        
        if(bundles == null) throw new Exception($"Asked to load asset {context.parameterInfo.Name} without providing a bundle for {context.attribute.registryID} registry");
        
        string assetName = context.parameterInfo.Name;
        if (assetAttribute.assetNameToLoad != null) // Attribute asset name gets priority over the argument name (if defined)
        {
            assetName = assetAttribute.assetNameToLoad;
        }

        object asset = null;
        foreach (AssetBundle bundle in bundles)
        {
            asset = bundle.LoadAsset(assetName, context.parameterInfo.ParameterType);
            if (asset != null) break;
        }
        value = asset;
        return true;
    }
}