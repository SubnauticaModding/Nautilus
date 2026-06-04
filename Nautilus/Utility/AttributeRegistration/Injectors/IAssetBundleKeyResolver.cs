namespace Nautilus.Utility.AttributeRegistration.Injectors;


/// <summary>
/// A resolver for use within the <see cref="RegisterAttributeServiceExtensions.AddAssetBundleAssetInjector">AssetBundleInjector</see>.
/// If an implementation instance is added to the <see cref="RegisterAttributeService"/> as a <see cref="RegisterAttributeService.AddSingleton">Singleton</see>,
/// it will be used to resolve the asset bundle name.
/// </summary>
public interface IAssetBundleKeyResolver
{
    /// <summary>
    /// Resolves the asset bundle key for a given injection.
    /// </summary>
    /// <param name="context">Injection context for a given <see cref="AssetLoadAttribute">[AssetLoad]</see> paramter</param>
    /// <returns>A valid asset bundle key, must be registered already as a <see cref="RegisterAttributeService.AddKeyedSingleton">KeyedSingleton</see></returns>
    string GetAssetBundleKey(InjectionContext context);
}