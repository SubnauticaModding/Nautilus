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
    /// <summary>
    /// Instantiates a new AssetBundleTemplate
    /// </summary>
    /// <param name="bundle">The AssetBundle to load the asset from</param>
    /// <param name="prefabName">The </param>
    /// <param name="info"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AssetBundleTemplate(AssetBundle bundle, string prefabName, PrefabInfo info) : base(info)
    {
        _prefab = bundle.LoadAsset<GameObject>(prefabName);
    }
    /// <summary>
    /// Instantiates a new AssetBundleTemplate. Automatically loads the bundle by calling <see cref = "Utility.AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly, string)"/>
    /// </summary>
    /// <param name="modAssembly">The Assembly of the mod to load the bundle from</param>
    /// <param name="assetBundleFileName"></param>
    /// <param name="prefabName"></param>
    /// <param name="info"></param>
    public AssetBundleTemplate(Assembly modAssembly, string assetBundleFileName, string prefabName, PrefabInfo info) : base(info)
    {
        var bundle = Utility.AssetBundleLoadingUtils.LoadFromAssetsFolder(modAssembly, assetBundleFileName);
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
