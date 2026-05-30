using System;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Marks an argument for a method attributed with a <see cref="RegisterAttribute"/> to load an asset from an asset bundle.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class AssetLoadAttribute(string assetToLoad = null, string bundleName = null) : Attribute
{
    internal readonly string AssetToLoad = assetToLoad;
    internal readonly string BundleName = bundleName;
}