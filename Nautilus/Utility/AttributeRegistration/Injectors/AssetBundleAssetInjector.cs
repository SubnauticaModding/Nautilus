using System;
using System.Reflection;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration.Injectors;


/// <summary>
/// Represents an injector that checks for an <see cref="AssetLoadAttribute"/> on a method parameter
/// </summary>
/// <param name="bundle"><see cref="AssetBundle"/> to load from</param>
public sealed class AssetBundleAssetInjector(AssetBundle bundle) : IDependencyArgumentInjector
{
    /// <summary>
    /// Checks whether the argument has a <see cref="AssetLoadAttribute"/> on the method parameter. If so, an asset is loaded from the bundle based on 2 criteria.
    /// If the <see cref="AssetLoadAttribute"/> defines an assetToLoad within the attribute definition, the bundle will attempt to load from the bundle with that first.
    /// If there is no assetToLoad defined, the parameter name is used to load the asset from the bundle.
    /// </summary>
    /// <param name="attribute">Attribute attached to the method with.</param>
    /// <param name="arg">Parameter argument to check and inject for.</param>
    /// <param name="value">The result of the asset loaded from the bundle. Is null when this method returns false.</param>
    /// <returns>True if there is a valid injection for the current argument, otherwise false.</returns>
    /// <exception cref="Exception">If no asset bundle was specified but an asset is attempted to load</exception>
    public bool TryInjectToArgument(RegisterEventAttribute attribute, ParameterInfo arg, out object value)
    {
        AssetLoadAttribute assetAttribute = arg.GetCustomAttribute<AssetLoadAttribute>(true);
        if(assetAttribute == null)
        {
            value = null;
            return false;
        }
        
        if(!bundle) throw new Exception($"Asked to load asset {arg.Name} without providing a bundle for {attribute.registryID} registry");

        //First we check if the attribute has an asset specified
        if (assetAttribute.assetNameToLoad != null)
        {
            value = bundle.LoadAsset(assetAttribute.assetNameToLoad, arg.ParameterType);
            return true;
        }
        
        //fallback to name of argument
        value = bundle.LoadAsset(arg.Name, arg.ParameterType);
        return true;
    }
    
    /// <returns>Returns the result of typeof(<see cref="AssetLoadAttribute"/>)</returns>
    public Type injectorTargetType => typeof(AssetLoadAttribute);
}