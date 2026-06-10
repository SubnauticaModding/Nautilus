using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration.Injectors;


/// <summary>
/// A resolver for use within the <see cref="RegisterAttributeServiceExtensions.AddAssetBundleAssetInjector">AssetBundleInjector</see>.
/// If an implementation instance is added to the <see cref="RegisterAttributeService"/> as a <see cref="RegisterAttributeService.AddSingleton">Singleton</see>,
/// it will be used to resolve the AssetBundle for the requested asset.
/// </summary>
public interface IRegistryAssetBundleResolver
{
    /// <summary>
    /// Resolves an AssetBundle for a given injection. The bundle should contain the requested asset.
    /// </summary>
    /// <param name="context">Injection context for a given <see cref="AssetLoadAttribute">[AssetLoad]</see> parameter</param>
    /// <returns>A loaded asset bundle to load the requested asset from.</returns>
    AssetBundle GetAssetBundle(InjectionContext context);
}