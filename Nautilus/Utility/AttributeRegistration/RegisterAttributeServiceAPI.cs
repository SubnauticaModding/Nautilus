using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Utility.AttributeRegistration.Injectors;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Utility for searching for and calling methods attributed with <see cref="RegisterAttribute"/>.
/// </summary>
public sealed partial class RegisterAttributeService
{
    /// <summary>
    /// Utility for searching for and calling methods attributed with <see cref="RegisterAttribute"/>.
    /// </summary>
    /// <param name="baseUnityPlugin">Supply your BepInEx plugin instance. Your GUID from there will be used for cross mod support.</param>
    public RegisterAttributeService(BaseUnityPlugin baseUnityPlugin) 
    {
        if (baseUnityPlugin == null) throw new ArgumentNullException(nameof(baseUnityPlugin));
        _modGuid = baseUnityPlugin.Info.Metadata.GUID;
    }
    
    /// <summary>
    /// Adds a dependency injector for a given type. Types can be associated in two different ways: the argument's type and the argument's attributes.
    /// First argument attributes are checked, an example of this is the <see cref="AssetLoadAttribute"/> which takes precedence regardless of the argument type.
    /// Next argument types are checked, examples include <see cref="PrefabInfo"/> or <see cref="TechType"/> for their respective injectors.
    /// </summary>
    /// <param name="injector">The injector to check when determining if it can inject for the parameter type/parameter attribute.</param>
    /// <typeparam name="T">The type this injector checks. Can either be of an Attribute for the parameter or the parameter's argument type.</typeparam>
    public void AddTypedDependencyInjector<T>(IDependencyArgumentInjector injector)
    {
        if(_typedDependencyArgumentInjectors.ContainsKey(typeof(T))) throw new ArgumentException($"Type '{typeof(T)}' already registered! There cannot be duplicates.");
        _typedDependencyArgumentInjectors.Add(typeof(T), injector);
    }

    /// <summary>
    /// Adds a dependency injector without a Type. Typeless injectors are only checked when no Typed injector could be determined.
    /// </summary>
    /// <param name="injector">an injector instance</param>
    /// <remarks>Since this injector is not based on a type, it is useful for looking at the parameter name (or some other surrounding context) instead.</remarks>
    public void AddDependencyInjector(IDependencyArgumentInjector injector) => _dependencyArgumentInjectors.Add(injector);

    /// <summary>
    /// Adds a singleton within the service. If a valid injector cannot be determined for an argument but a singleton was added for the type, the last singleton added for the type will be injected.
    /// Singletons can be used within injectors. Multiple singletons can be registered for the same type.
    /// </summary>
    /// <param name="inst">Instance of the singleton</param>
    /// <typeparam name="T">Singleton Type</typeparam>
    public void AddSingleton<T>(T inst)
    {
        if (_singletons.TryGetValue(typeof(T), out List<object> singletons))
        {
            singletons.Add(inst);
            return;
        }
        _singletons.Add(typeof(T), new List<object> { inst });
    }
    
    /// <summary>
    /// Adds a singleton to the service with an associated key for retrieval using <see cref="GetKeyedSingleton"/>.
    /// </summary>
    /// <param name="inst">Instance of the singleton</param>
    /// <param name="key">Key for the singleton</param>
    /// <typeparam name="T">Singleton Type</typeparam>
    /// <remarks>Despite the singleton having an associated key, it is still treated as the latest for the type as if <see cref="AddSingleton"/> was called.</remarks>
    public void AddKeyedSingleton<T>(T inst, string key)
    {
        (Type, string) typeKey = (typeof(T), key);
        if(_keyedSingletons.ContainsKey(typeKey)) throw new ArgumentException($"Type '{typeof(T)}' already registered for the given key! There cannot be duplicates.");
        
        AddSingleton(inst);
        _keyedSingletons.Add(typeKey, inst);
    }

    /// <summary>
    /// Retrieves the last added singleton instance for a given type within the service. EX: if multiple instances were added to the service for the same type, the last one will be returned here.
    /// </summary>
    /// <typeparam name="T">Singleton Type to retrieve</typeparam>
    /// <returns>The last added singleton for a given type. If no singleton was added for the type, null (or default for valuetypes) is returned.</returns>
    public T GetLatestSingleton<T>()
    {
        if (_singletons.TryGetValue(typeof(T), out List<object> singletons))
        {
            return (T) singletons[^1];// last element in the list is the last added
        }
        return default;
    }

    /// <summary>
    /// Gets all singleton instances registered for a given type within the service. The order of retrieval is based on the order the instances were added.
    /// </summary>
    /// <typeparam name="T">Singleton Type to retrieve</typeparam>
    /// <returns>A collection containing every singleton in order of registration for the desired type.</returns>
    public IEnumerable<T> GetAllSingletons<T>()
    {
        if (_singletons.TryGetValue(typeof(T), out List<object> singletons))
        {
            foreach (T item in singletons) yield return item;
        }
    }

    /// <summary>
    /// Retrieves the singleton instance for an associated key within the service.
    /// </summary>
    /// <param name="key">Key to check for</param>
    /// <typeparam name="T">Keyed Singleton Type to retrieve</typeparam>
    /// <returns>The instance for the given type and key. If no keyed instance was added for the type, null (or default for valuetypes) is returned.</returns>
    public T GetKeyedSingleton<T>(string key)
    {
        if (_keyedSingletons.TryGetValue((typeof(T), key), out object singleton))
        {
            return (T) singleton;
        }
        return default;
    }
    
    /// <summary>
    /// Search an <see cref="Assembly"/> and execute every public/nonpublic static method attached with a <see cref="RegisterAttribute"/>.
    /// Method parameters are automatically processed depending on the defined <see cref="IDependencyArgumentInjector">IDependencyArgumentInjectors</see>.
    /// </summary>
    /// <param name="assemblyToSearch">The <see cref="Assembly"/> for which all types will be checked for static methods marked with the <see cref="RegisterAttribute"/>.</param>
    public void ExecuteAssemblyRegisterAttributes(Assembly assemblyToSearch)
    {
        ProcessAttributeRegistriesForAssembly(assemblyToSearch);
    }
}