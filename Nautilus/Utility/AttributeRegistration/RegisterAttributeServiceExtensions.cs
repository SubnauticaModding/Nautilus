using Nautilus.Assets;
using Nautilus.Utility.AttributeRegistration.Injectors;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Extensions for easy access to injectors that Nautilus provides within the <see cref="RegisterAttributeService"/> system.
/// </summary>
public static class RegisterAttributeServiceExtensions
{
    extension(RegisterAttributeService service)
    {
        /// <summary>
        /// Adds the following injectors to the Service:
        /// <list type="bullet">
        ///   <item><see cref="RegisterAttributeServiceExtensions.AddAssetBundleAssetInjector">AssetBundle Asset Injector</see></item>
        ///   <item><see cref="RegisterAttributeServiceExtensions.AddPrefabInfoInjector">Prefab Info Injector</see></item>
        ///   <item><see cref="RegisterAttributeServiceExtensions.AddTechTypeInjector">Tech Type Injector</see></item>
        /// </list>
        /// Some injectors require additional configuration within the service to use properly. This only needs to be done if the above injector is used.
        /// </summary>
        /// <remarks>Unless you know what you are doing, it is recommended to use the default injectors for most mods. Ensure you read the docs for each injector as some require additonal configuration</remarks>
        public void AddBasicInjectors()
        {
            service.AddAssetBundleAssetInjector();
            service.AddPrefabInfoInjector();
            service.AddTechTypeInjector();
        }
        
        /// <summary>
        /// Assigns any argument with a <see cref="AssetLoadAttribute">[AssetLoad]</see> attribute with a prefab from the given Asset Bundles. The type to load is determined from the argument's type.
        /// There are 2 ways to add asset bundles into the system:
        /// <list type="bullet">
        ///   <item>For mods that use a singular asset bundle, add it as a <see cref="RegisterAttributeService.AddSingleton">Singleton</see> for the injector to pull from.</item>
        ///   <item>For mods with multiple asset bundles, add them as <see cref="RegisterAttributeService.AddKeyedSingleton">KeyedSingletons</see>.</item>
        /// </list>
        /// If multiple asset bundles are used, the register MUST determine which bundle to load from. There are 2 ways to accomplish this,
        /// both using the key from the <see cref="RegisterAttributeService.AddKeyedSingleton">AddKeyedSingleton</see>:
        /// <list type="bullet">
        ///   <item>The <see cref="AssetLoadAttribute">[AssetLoad("AssetName", "BundleKey")]</see> attribute has an optional argument for the bundle key.</item>
        ///   <item>An <see cref="IAssetBundleKeyResolver"/> can be added as a <see cref="RegisterAttributeService.AddSingleton">Singleton</see> to the service.
        ///   Ensure the type <see cref="IAssetBundleKeyResolver"/> is used for the singleton and NOT the implementing class. If a resolver is added to the service,
        ///   it will take precedence over anything defined in a <see cref="AssetLoadAttribute">[AssetLoad]</see> attribute</item>
        /// </list>
        /// </summary>
        public void AddAssetBundleAssetInjector() =>
            service.AddTypedDependencyInjector<AssetLoadAttribute>(new AssetBundleAssetInjector());
        
        /// <summary>
        /// Assigns any <see cref="PrefabInfo"/> argument with an instance containing a TechType defined by the <see cref="RegisterAttribute.RegistryID">RegistryID</see>.
        /// </summary>
        public void AddPrefabInfoInjector() => 
            service.AddTypedDependencyInjector<PrefabInfo>(new PrefabInfoInjector());

        /// <summary>
        /// Assigns any <see cref="TechType"/> argument with a valid enum value depending on the argument name.
        /// </summary>
        public void AddTechTypeInjector() =>
            service.AddTypedDependencyInjector<TechType>(new TechTypeInjector());
    }
}