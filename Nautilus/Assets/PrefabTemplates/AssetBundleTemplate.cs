using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nautilus.Assets.PrefabTemplates;

/// <summary>
/// A PrefabTemplate used for loading objects in from asset bundles
/// </summary>
public class AssetBundleTemplate : PrefabTemplate
{
    private static Dictionary<Assembly, AssetBundle> _loadedBundles = new Dictionary<Assembly, AssetBundle>();

    /// <summary>
    /// Instantiates a new AssetBundleTemplate
    /// </summary>
    /// <param name="bundle">The AssetBundle to load the asset from</param>
    /// <param name="prefabName">The name of the prefab gameobject to load from the bundle</param>
    /// <param name="info">The prefab info to base this template off of.</param>
    public AssetBundleTemplate(AssetBundle bundle, string prefabName, PrefabInfo info) : base(info)
    {
        _prefab = bundle.LoadAsset<GameObject>(prefabName);
    }

    /// <summary>
    /// Instantiates a new AssetBundleTemplate. Automatically loads the bundle by calling <see cref = "Utility.AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly, string)"/>,
    /// which expects the bundle to be in your mod's Assets folder.
    /// <para>Also caches the loaded bundle for future use.</para>
    /// <para>If you are loading and using your bundle on your own, it's highly recommended to use the AssetBundle constructor overload instead.</para>
    /// <para>Bundles are cached per Assembly, and won't work with mods that use multiple seperate bundles.</para>
    /// </summary>
    /// <param name="assetBundleFileName">The file name of the asset bundle. These often do not have file extensions.</param>
    /// <param name="prefabName">The name of the prefab GameObject to load from the bundle.</param>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="modAssembly">The <see cref="Assembly"/> of the mod that contains the given Asset Bundle. If left unassigned, this will be automatically set to
    /// <see cref="Assembly.GetCallingAssembly"/>.</param>
    public AssetBundleTemplate(string assetBundleFileName, string prefabName, PrefabInfo info, Assembly modAssembly = null) : base(info)
    {
        AssetBundle bundle;

        var assembly = modAssembly ?? Assembly.GetCallingAssembly();

        if (!_loadedBundles.TryGetValue(assembly, out bundle))
        {
            bundle = Utility.AssetBundleLoadingUtils.LoadFromAssetsFolder(assembly, assetBundleFileName);
            _loadedBundles.Add(assembly, bundle);
        }

        _prefab = bundle.LoadAsset<GameObject>(prefabName);
    }

    private GameObject _prefab;

    /// <inheritdoc/>
    public override IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject)
    {
        gameObject.Set(_prefab);
        yield return null;
    }
}
