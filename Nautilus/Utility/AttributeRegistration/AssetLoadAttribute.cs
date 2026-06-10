using System;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Marks an argument for a method attributed with a <see cref="RegisterAttribute"/> to load an asset of any given type from an Asset Bundle.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class AssetLoadAttribute(string assetToLoad = null, string bundleKey = null) : Attribute
{
    internal readonly string AssetToLoad = assetToLoad;
    internal readonly string BundleKey = bundleKey;
}