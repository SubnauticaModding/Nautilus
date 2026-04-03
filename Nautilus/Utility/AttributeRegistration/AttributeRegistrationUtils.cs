using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Utility.AttributeRegistration.Injectors;
using UnityEngine;

namespace Nautilus.Utility.AttributeRegistration;


/// <summary>
/// Utilities for searching for and calling methods attributed with <see cref="RegisterEventAttribute"/>
/// </summary>
public static class AttributeRegistrationUtils
{
    /// <summary>
    /// Utility method to search an <see cref="Assembly"/> and execute every method attached with an <see cref="RegisterEventAttribute"/>.
    /// Method parameters are automatically processed depending on the defined <see cref="IDependencyArgumentInjector">IDependencyArgumentInjectors</see>.
    /// By default, there are 3 injectors added already:
    /// <list type="bullet">
    ///   <item><see cref="PrefabInfoInjector"/> asigns any <see cref="PrefabInfo"/> argument with an instance containing a TechType defined by the <see cref="RegisterEventAttribute.registryID">registryID</see> </item>
    ///   <item><see cref="TechTypeInjector"/> asigns any <see cref="TechType"/> argument with a valid enum value depending on the argument name</item>
    ///   <item><see cref="AssetBundleAssetInjector"/> asigns any argument with a <see cref="AssetLoadAttribute">[AssetBundleLoad]</see> attribute with a prefab from the <see cref="assetBundle"/>.
    ///     The argument name passed as a request to load a prefab from the bundle. It is not ideal load <see cref="GameObject">GameObjects</see> for setting <see cref="CustomPrefab"/> here.
    ///     Use the various async methods over Nautilus loading it synchronously for you when it comes to large assets.</item>
    /// </list>
    /// If your looking to define your own injectors, please use: <see cref="ExecuteAssemblyAttributeRegistriesCustomInjectors"/>
    /// </summary>
    /// <param name="assemblyToSearch">Assembly to search through. For <i>most</i> mods, the result of <see cref="Assembly.GetExecutingAssembly"/>> should be passed as the argument.
    ///  Every Type within the Assembly will be checked for methods containing the Attribute <see cref="RegisterEventAttribute"/>.</param>
    /// <param name="assetBundle">When using the <see cref="AssetBundleAssetInjector"/>>, the injector will pull from this asset bundle.
    ///  This can safely be left null if you don't load assets through argument parameters</param>
    /// <param name="overrideInjectors">Optional injectors to override the defaults and or add new injectors to the default list. If you desire more control,
    ///  look into <see cref="ExecuteAssemblyAttributeRegistriesCustomInjectors"/></param>
    public static void ExecuteAssemblyAttributeRegistries(Assembly assemblyToSearch, AssetBundle assetBundle = null, IDependencyArgumentInjector[] overrideInjectors = null)
    {
        IDependencyArgumentInjector[] selectedInjectors = [new AssetBundleAssetInjector(assetBundle), new PrefabInfoInjector(), new TechTypeInjector()];
        
        if (overrideInjectors != null)
        {
            Dictionary<Type, IDependencyArgumentInjector> injectorDict = new ();
            selectedInjectors.ForEach(injector => injectorDict.Add(injector.InjectorTargetType, injector));

            overrideInjectors.ForEach(overrideInjector => injectorDict[overrideInjector.InjectorTargetType] = overrideInjector);

            selectedInjectors = injectorDict.Values.ToArray();
        }
        
        RegisterEventAttributeLoader.ExecuteAssemblyAttributeRegistries(assemblyToSearch, selectedInjectors);
    }
    
    /// <summary>
    /// Utility method to search an <see cref="Assembly"/> and execute every method attached with an <see cref="RegisterEventAttribute"/>.
    /// Method parameters are automatically processed depending on the defined <see cref="IDependencyArgumentInjector">IDependencyArgumentInjectors</see>.
    /// By default, there will be no injectors, so you must create the injectors for Nautilus to use.
    /// Either Nautilus' injectors can be instantiated, or you can create your own that implement <see cref="IDependencyArgumentInjector"/>
    /// </summary>
    /// <param name="assemblyToSearch">Assembly to search through. For <i>most</i> mods, the result of <see cref="Assembly.GetExecutingAssembly"/>> should be passed as the argument.
    ///  Every Type within the Assembly will be checked for methods containing the Attribute <see cref="RegisterEventAttribute"/>.</param>
    /// <param name="injectors">List of injectors to use during parsing. Feel free to implement <see cref="IDependencyArgumentInjector"/> in your own class to extend
    /// the functionality of this registration system.</param>
    public static void ExecuteAssemblyAttributeRegistriesCustomInjectors(Assembly assemblyToSearch, IDependencyArgumentInjector[] injectors)
    {
        RegisterEventAttributeLoader.ExecuteAssemblyAttributeRegistries(assemblyToSearch, injectors);
    }
}