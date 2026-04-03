using System;
using Nautilus.Utility.AttributeRegistrationUtils.Injectors;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistrationUtils;


/// <summary>
/// Marks an argument for a method attributed with a <see cref="RegisterEventAttribute"/> to load an asset from an asset bundle.
/// The name of the argument corresponds an asset from the <see cref="AssetBundle"/> supplied at <see cref="AttributeRegistrationUtils.ExecuteAssemblyAttributeRegistries"/>
/// or during the creation of <see cref="AssetBundleAssetInjector"/>>
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class AssetLoadAttribute(string assetToLoad = null) : Attribute
{
    internal readonly string assetNameToLoad = assetToLoad;
}