using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Nautilus.Utility.AttributeRegistrationUtils.Injectors;
using Nautilus.Utility.AttributeRegistrationUtils.RegistryRequirements;
using UnityEngine;
using Object = System.Object;

namespace Nautilus.Utility.AttributeRegistrationUtils;


/// <summary>
/// Utilities for searching for and calling methods attributed with <see cref="RegisterEventAttribute"/>
/// </summary>
internal class RegisterEventAttributeLoader
{
    // Types like TechType or PrefabInfo (but not limited to) always check here first
    private readonly Dictionary<Type, IDependencyArgumentInjector> _typedDependencyArgumentInjectors = new();
    // Name based Injectors that look at the argument name inject here
    private readonly List<IDependencyArgumentInjector> _dependencyArgumentInjectors = new();

    private readonly Assembly _assemblyToSearch;

    private static readonly HashSet<string> _idsRegistered = new();
    
    // the key represents which id the registry is next waiting for.
    // the list of registries is the events waiting for key to be loaded,
    // these will attempt to execute when the key is but may return under a different key if there are still dependencies to check.
    private static readonly Dictionary<string, List<RegisterEventAttribute>> _deferredRegistrations = new();
    
    private RegisterEventAttributeLoader(Assembly assemblyToSearch)
    {
        _assemblyToSearch = assemblyToSearch;
    }
    
    private void RegisterInjectors(IDependencyArgumentInjector[] injectors)
    {
        foreach (IDependencyArgumentInjector injector in injectors)
        {
            if (injector.injectorTargetType != null)
                _typedDependencyArgumentInjectors.Add(injector.injectorTargetType, injector);
            _dependencyArgumentInjectors.Add(injector);
        }
    }
    
    internal static void ExecuteAssemblyAttributeRegistries(Assembly assemblyToSearch, IDependencyArgumentInjector[] injectors)
    {
        RegisterEventAttributeLoader loader = new(assemblyToSearch);
        loader.RegisterInjectors(injectors);
        loader.ProcessAttributeRegistriesForAssembly();
    }

    private void ProcessAttributeRegistriesForAssembly()
    {
        foreach (Type type in _assemblyToSearch.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                RegisterEventAttribute[] attributes = method.GetCustomAttributes<RegisterEventAttribute>(inherit: false).ToArray();
                if (!attributes.Any())
                {
                    continue;//no attribute present
                }
                
                /*There can only be 1 RegisterEventAttribute (compiler enforced) per method
                  so we can safely assume there is only 1*/
                RegisterEventAttribute attribute = attributes[0];
                attribute.methodInfo = method;
                attribute.loader = this;
                
                if (!AttributeShouldRegister(attribute)) continue;
                
                HandleAttribute(attribute);
            }
        }
    }

    private static bool AttributeShouldRegister(RegisterEventAttribute attribute)
    {
        /*Since IRegistryRequirement is an interface and the typed GetCustomAttributes<> requires the type to extend Attribute,
          we must filter objects to get the instances*/
        IEnumerable<IRegistryRequirement> registryRequirements = attribute
            .methodInfo
            .GetCustomAttributes(true)
            .OfType<IRegistryRequirement>();
        
        foreach (IRegistryRequirement registryRequirement in registryRequirements)
        {
            if (!registryRequirement.RequirementsMet()) return false;
        }
        return true;
    }

    private static void HandleAttribute(RegisterEventAttribute attribute)
    {
        if (_idsRegistered.Contains(attribute.registryID))
        {
            throw new Exception($"Duplicate registry ID found: {attribute.registryID}!");
        }
        
        if (DependenciesFulfilled(attribute, out string firstFailedDependency))
        {
            attribute.loader.ExecuteMethodUnsafe(attribute);
            return;
        }

        if (DependencyCyclic(attribute.registryID, firstFailedDependency, out string chain))
        {
            throw new Exception($"Cyclic dependency registrations found! Dependency chain: {chain}");
        }

        if(_deferredRegistrations.TryGetValue(firstFailedDependency, out var list)){
            list.Add(attribute);
        }
        else
        {
            List<RegisterEventAttribute> newDependentList = new();
            newDependentList.Add(attribute);
            _deferredRegistrations.Add(firstFailedDependency, newDependentList);
        }
    }
    
    private static bool DependencyCyclic(string registryID, string dependencyID, out string chain)
    {
        if (registryID == dependencyID)
        {
            chain = $"{registryID},{dependencyID}";
            return true;
        }
        
        //fancy c# shit put 2 types :0
        Stack<(string registry, string pathTraversed)> toVisit = new();
        HashSet<string> visited = new();

        toVisit.Push((registryID, registryID));

        while (toVisit.Count > 0)
        {
            var (current, path) = toVisit.Pop();

            if (!visited.Add(current))
                continue;

            if (!_deferredRegistrations.TryGetValue(current, out var waiting))
                continue;

            foreach (var attr in waiting)
            {
                if (attr.registryID == dependencyID)
                {
                    path += $",{dependencyID}";
                    path += $",{registryID}";//Add the start to the end to clearly show the path loops
                    
                    //The _deferredRegistrations stores dependencies in reverse (ID, attributes waiting on ID) to how the Attributes are set up.
                    //This has the benefit of only checking deferred registration for cyclic issues, but we have to reverse at the very end to make it logical to what they typed.
                    chain = string.Join(" -> ", path.Split(',').Reverse());
                    return true;
                }
                toVisit.Push((attr.registryID, path + $",{attr.registryID}"));
            }
        }

        chain = null;
        return false;
    }

    private static bool DependenciesFulfilled(RegisterEventAttribute registryEventAttribute, out string firstUnloadedDependency)
    {
        if (registryEventAttribute.loadAfterIDs == null)
        {
            firstUnloadedDependency = null;
            return true;
        }
        
        foreach (string dependentID in registryEventAttribute.loadAfterIDs)
        {
            if (_idsRegistered.Contains(dependentID)) continue;
            
            firstUnloadedDependency = dependentID;
            return false;
        }
        firstUnloadedDependency = null;
        return true;
    }
    
    private void ExecuteMethodUnsafe(RegisterEventAttribute attribute)
    {
        MethodInfo method = attribute.methodInfo;
        
        if(Initializer.ConfigFile.enableDebugLogs)
            InternalLogger.Info($"Calling {method.Name} from {method.DeclaringType}");
        
        ParameterInfo[] parameters = method.GetParameters();
        Object[] parameterValues = new Object[parameters.Length];
        
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            if (TryDependencyInject(attribute, parameter, out Object value))
            {
                parameterValues[i] = value;
            }
            else
            {
                throw new Exception($"No injector found for {parameter.Name} when registering {attribute.registryID}");
            }
        }
        
        method.Invoke(null, parameterValues);
        _idsRegistered.Add(attribute.registryID);
                
        //check and load for any that depended on this ID
        if (!_deferredRegistrations.TryGetValue(attribute.registryID, out var list)) return;
        //remove the dependency from the deferred list, saves a bit of memory but not technically required.
        _deferredRegistrations.Remove(attribute.registryID);
        list.ForEach(HandleAttribute);
    }

    //Priority: Parameter/Argument attribute injector -> Type of argument injector -> Argument name injector.
    private bool TryDependencyInject(RegisterEventAttribute attr, ParameterInfo arg, out Object valueToInject)
    {
        foreach (Attribute parameterAttribute in arg.GetCustomAttributes())
        {
            if (_typedDependencyArgumentInjectors.TryGetValue(parameterAttribute.GetType(), out IDependencyArgumentInjector argumentAttributeInjector))
            {
                argumentAttributeInjector.TryInjectToArgument(attr, arg, out object result);
                valueToInject = result;
                return true;
            }
        }

        if (_typedDependencyArgumentInjectors.TryGetValue(arg.ParameterType, out IDependencyArgumentInjector typedInjector))
        {
            typedInjector.TryInjectToArgument(attr, arg, out Object value);
            valueToInject = value;
            return true;
        }
        
        foreach (IDependencyArgumentInjector injector in _dependencyArgumentInjectors)
        {
            if (!injector.TryInjectToArgument(attr, arg, out Object value)) continue;
            
            valueToInject = value;
            return true;
        }
        
        valueToInject = null;
        return false;
    }

    internal static void LogWarningsForUnloadedDependencies()
    {
        foreach (KeyValuePair<String, List<RegisterEventAttribute>> unloadedRegistryID in _deferredRegistrations)
        {
            string registriesWaitingForDependency = string.Join(", ", unloadedRegistryID.Value.Select(attribute => attribute.registryID));
            InternalLogger.Log($"Registry(ies) {registriesWaitingForDependency} could not be loaded due to missing dependency: {unloadedRegistryID.Key}", LogLevel.Warning);
        }
    }
}