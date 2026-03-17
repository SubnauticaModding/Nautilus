using System.Reflection;
using Nautilus.Assets;
using Nautilus.Utility.AttributeRegistrationUtils.Injectors;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistrationUtils;


/// <summary>
/// Utilities for searching for and calling methods attributed with <see cref="RegisterEventAttribute"/>
/// </summary>
public static class RegistryEventUtils
{
    /// <summary>
    /// Utility method to search an <see cref="Assembly"/> and execute every method attached with an <see cref="RegisterEventAttribute"/>.
    /// Method parameters are automatically processed depending on the defined <see cref="IDependencyArgumentInjector">IDependencyArgumentInjectors</see>.
    /// By default, there are 3 injectors added already:
    /// <list type="bullet">
    ///   <item><see cref="PrefabInfoInjector"/> asigns any <see cref="PrefabInfo"/> argument with an instance containing a TechType defined by the <see cref="RegisterEventAttribute.registryID">registryID</see> </item>
    ///   <item><see cref="TechTypeInjector"/> asigns any <see cref="TechType"/> argument with a valid enum value depending on the argument name</item>
    ///   <item><see cref="AssetBundleAssetInjector"/> asigns any argument with a <see cref="AssetLoadAttribute">[AssetBundleLoad]</see> attribute with a prefab from the <see cref="assetBundle"/>.
    ///     The argument name passed as a request to load a prefab from the bundle. It is not ideal load <see cref="GameObject">GameObjects</see> for setting <see cref="CustomPrefab"/>> here;
    ///     use the various async methods over Nautilus loading it synchronously for you</item>
    /// </list>
    /// If your looking to define your own injectors, please use: <see cref="ExecuteAssemblyAttributeRegistriesCustomInjectors"/>
    /// </summary>
    /// <param name="assemblyToSearch">Assembly to search through. For <i>most</i> mods, the result of <see cref="Assembly.GetExecutingAssembly"/>> should be passed as the argument.
    ///  Every Type within the Assembly will be checked for methods containing the Attribute <see cref="RegisterEventAttribute"/>.</param>
    /// <param name="namespaceFilter">Namespace to search and execute. If null/blank the entire Assembly will be searched.
    ///  Should be formatted like <i>"Nautilus.SomeSubNamespace.SubSubNamespace"</i> but with your mod's namespace</param>
    /// <param name="assetBundle">When using the <see cref="AssetBundleAssetInjector"/>>, the injector will pull from this asset bundle.
    ///  This can safely be left null if you don't load assets through argument parameters</param>
    public static void ExecuteAssemblyAttributeRegistries(Assembly assemblyToSearch, string namespaceFilter = null, AssetBundle assetBundle = null)
    {
        RegisterEventAttributeLoader.ExecuteAssemblyAttributeRegistries(assemblyToSearch, namespaceFilter, assetBundle);
    }
    
    /// <summary>
    /// Utility method to search an <see cref="Assembly"/> and execute every method attached with an <see cref="RegisterEventAttribute"/>.
    /// Method parameters are automatically processed depending on the defined <see cref="IDependencyArgumentInjector">IDependencyArgumentInjectors</see>.
    /// By default, there will be no injectors if you supply none, so you must create the injectors and pass them in.
    /// Nautilus' injectors can be instantiated, or you can create your own that implement <see cref="IDependencyArgumentInjector"/>
    /// </summary>
    /// <param name="assemblyToSearch">Assembly to search through. For <i>most</i> mods, the result of <see cref="Assembly.GetExecutingAssembly"/>> should be passed as the argument.
    ///  Every Type within the Assembly will be checked for methods containing the Attribute <see cref="RegisterEventAttribute"/>.</param>
    /// <param name="injectors">List of injectors to use during parsing. Feel free to implement <see cref="IDependencyArgumentInjector"/> in your own class to extend
    /// the functionality of this registration system.</param>
    /// <param name="namespaceFilter">Namespace to search and execute. If null/blank the entire Assembly will be searched.
    ///  Should be formatted like <i>"Nautilus.SomeSubNamespace.SubSubNamespace"</i> but with your mod's namespace</param>
    public static void ExecuteAssemblyAttributeRegistriesCustomInjectors(Assembly assemblyToSearch, IDependencyArgumentInjector[] injectors, string namespaceFilter = null)
    {
        RegisterEventAttributeLoader.ExecuteAssemblyAttributeRegistriesCustom(assemblyToSearch, injectors, namespaceFilter);
    }
}