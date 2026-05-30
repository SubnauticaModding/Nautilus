using Nautilus.Assets;
using Nautilus.Utility.AttributeRegistration.Injectors;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration;

public static class RegisterAttributeServiceExtensions
{
    extension(RegisterAttributeService service)
    {
        public void AddNautilusDefaultInjector(AssetBundle[] assetBundles)
        {
            service.AddPrefabInfoInjector();
            service.AddTechTypeInjector();
            service.AddAssetBundleAssetInjector(assetBundles);
        }
        
        public void AddAssetBundleAssetInjector(AssetBundle[] assetBundles) =>
            service.AddTypedDependencyInjector<AssetLoadAttribute>(new AssetBundleAssetInjector(assetBundles));
        
        public void AddPrefabInfoInjector() => 
            service.AddTypedDependencyInjector<PrefabInfo>(new PrefabInfoInjector());

        public void AddTechTypeInjector() =>
            service.AddTypedDependencyInjector<TechType>(new TechTypeInjector());
    }
}