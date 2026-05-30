using Nautilus.Assets;
using Nautilus.Utility.AttributeRegistration.Injectors;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Extensions for easy access to injectors Nautilus provides within the <see cref="RegisterAttributeService"/> system
/// </summary>
public static class RegisterAttributeServiceExtensions
{
    extension(RegisterAttributeService service)
    {
        /// <summary>
        /// Adds the following injectors to the Service:
        /// <list type="bullet">
        ///   <item><see cref="RegisterAttributeServiceExtensions.AddAssetBundleAssetInjector">AssetBundle Asset Injector</see></item>
        ///   <item><see cref="RegisterAttributeServiceExtensions.AddAssetBundleAssetInjector">Prefab Info Injector</see></item>
        ///   <item><see cref="RegisterAttributeServiceExtensions.AddAssetBundleAssetInjector">Tech Type Injector</see></item>
        /// </list>
        /// </summary>
        /// <param name="assetBundles">Asset bundle(s) to load from. Can be safely left null if <see cref="AssetLoadAttribute">[AssetLoad]</see> is never used</param>
        /// <remarks>Unless you know what you are doing, it is recommended to use the default injectors for most mods.</remarks>
        public void AddDefaultInjectors(AssetBundle[] assetBundles)
        {
            service.AddAssetBundleAssetInjector(assetBundles);
            service.AddPrefabInfoInjector();
            service.AddTechTypeInjector();
        }
        
        /// <summary>
        /// Assigns any argument with a <see cref="AssetLoadAttribute">[AssetLoad]</see> attribute with a prefab from the given asset bundles.
        /// The type to load is determined from the argument's type.
        /// </summary>
        /// <param name="assetBundles">Asset bundle(s) to load from. Can be safely left null if <see cref="AssetLoadAttribute">[AssetLoad]</see> is never used</param>
        /// <remarks>If multiple asset bundles are defined and your <see cref="AssetLoadAttribute">[AssetLoad]</see> attributes do not define an asset bundle name,
        /// a brute force search will be used on every bundle to determine a prefab. This may work for most use cases, but could also bring in unexpected behavior if you
        /// have multiple assets under the same name across bundles.</remarks>
        public void AddAssetBundleAssetInjector(AssetBundle[] assetBundles) =>
            service.AddTypedDependencyInjector<AssetLoadAttribute>(new AssetBundleAssetInjector(assetBundles));
        
        /// <summary>
        /// Assigns any <see cref="PrefabInfo"/> argument with an instance containing a TechType defined by the <see cref="RegisterAttribute.registryID">registryID</see>
        /// </summary>
        public void AddPrefabInfoInjector() => 
            service.AddTypedDependencyInjector<PrefabInfo>(new PrefabInfoInjector());

        /// <summary>
        /// Assigns any <see cref="TechType"/> argument with a valid enum value depending on the argument name
        /// </summary>
        public void AddTechTypeInjector() =>
            service.AddTypedDependencyInjector<TechType>(new TechTypeInjector());
    }
}