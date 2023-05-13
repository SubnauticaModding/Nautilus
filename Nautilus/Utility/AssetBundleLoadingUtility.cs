using System.IO;
using System.Reflection;
using UnityEngine;

namespace Nautilus.Utility;

/// <summary>
/// Utilities related to loading Asset Bundles.
/// </summary>
public static class AssetBundleLoadingUtils
{
    /// <summary>
    /// Loads an <see cref="AssetBundle"/> from the the mod's Assets folder. Expects a folder named Assets to exist in the same folder as the Assembly,
    /// and expects this folder to contain an asset bundle with the same name as defined in the <paramref name="assetBundleFileName"/> parameter.
    /// </summary>
    /// <param name="modAssembly">The mod assembly, in the same folder that contains the Assets folder. See <see cref="Assembly.GetExecutingAssembly"/>.</param>
    /// <param name="assetBundleFileName">The name of the Asset Bundle file in your Assets folder, i.e. "deextinctionassets" or "gargantuanassets".
    /// These typically do not have a file extension.</param>
    public static AssetBundle LoadFromAssetsFolder(Assembly modAssembly, string assetBundleFileName)
    {
        return AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(modAssembly.Location), "Assets", assetBundleFileName));
    }

    /// <summary>
    /// Loads an <see cref="AssetBundle"/> from a given path. Expects an asset bundle to exist at <paramref name="pathToBundle"/> (relative to the containing folder of the DLL).
    /// </summary>
    /// <param name="modAssembly">The mod assembly used to find the mod's folder. See <see cref="Assembly.GetExecutingAssembly"/>.</param>
    /// <param name="pathToBundle">The relative path to the Asset Bundle file from your plugin folder, i.e. "Assets/deextinctionassets" or "Assets/AssetBundles/gargantuanassets". See <see cref="Path.Combine(string, string)"/> for creating the path.
    /// These typically do not have a file extension.</param>
    /// <seealso cref="AssetBundle.LoadFromFile(string)"/>
    public static AssetBundle LoadFromModFolder(Assembly modAssembly, string pathToBundle)
    {
        return AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(modAssembly.Location), pathToBundle));
    }
}