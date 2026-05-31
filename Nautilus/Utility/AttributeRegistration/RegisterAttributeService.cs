using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Nautilus.Assets;
using Nautilus.Utility.AttributeRegistration.Injectors;
using Nautilus.Utility.AttributeRegistration.RegistryRequirements;
using Object = System.Object;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Utility for searching for and calling methods attributed with <see cref="RegisterAttribute"/>.
/// </summary>
public sealed class RegisterAttributeService
{
    private readonly string modGuid;
    private readonly Dictionary<Type, IDependencyArgumentInjector> _typedDependencyArgumentInjectors = new();
    private readonly List<IDependencyArgumentInjector> _dependencyArgumentInjectors = new();
    
    private static readonly HashSet<string> _idsRegistered = new();
    // the key represents which id the registry is next waiting for.
    // the list of registries is the events waiting for key to be loaded,
    // these will attempt to execute when the key is but may return under a different key if there are still dependencies to check.
    private static readonly Dictionary<string, List<RegisterMethod>> _deferredRegistrations = new();
    
    private record RegisterMethod(RegisterAttribute Attribute, MethodInfo MethodInfo, RegisterAttributeService Service);

    /// <summary>
    /// Utility for searching for and calling methods attributed with <see cref="RegisterAttribute"/>.
    /// </summary>
    /// <param name="modGuid">Used as a prefix for cross mod dependencies. Use the same guid from your BepInEx plugin</param>
    public RegisterAttributeService(string modGuid)
    {
        this.modGuid = modGuid;
    }
    
    /// <summary>
    /// Adds a dependency injector for a given type. Types can be associated in two different ways: the argument's type and the argument's attributes.
    /// First argument attributes are checked, an example of this is the <see cref="AssetLoadAttribute"/> which takes precedence regardless of the argument type.
    /// Next argument types are checked, examples include <see cref="PrefabInfo"/> or <see cref="TechType"/> for their respective injectors.
    /// </summary>
    /// <param name="injector">The injector to check when determining if it can inject for the parameter type/parameter attribute</param>
    /// <typeparam name="T">The type this injector checks. Can either be of an Attribute for the paramter or the paramter's argument type.</typeparam>
    public void AddTypedDependencyInjector<T>(IDependencyArgumentInjector injector) => _typedDependencyArgumentInjectors.Add(typeof(T), injector);
    
    /// <summary>
    /// Adds a dependency injector without a Type. Typeless injectors are only checked when no Typed injector could be determined.
    /// </summary>
    /// <param name="injector"></param>
    /// <remarks>Since this injector is not based on a type, you may want to look at the parameter name instead</remarks>
    public void AddDependencyInjector(IDependencyArgumentInjector injector) => _dependencyArgumentInjectors.Add(injector);
    
    /// <summary>
    /// Search an <see cref="Assembly"/> and execute every public/nonpublic static method attached with a <see cref="RegisterAttribute"/>.
    /// Method parameters are automatically processed depending on the defined <see cref="IDependencyArgumentInjector">IDependencyArgumentInjectors</see>.
    /// </summary>
    /// <param name="assemblyToSearch">The assembly to search through</param>
    public void ExecuteAssemblyRegisterAttributes(Assembly assemblyToSearch)
    {
        ProcessAttributeRegistriesForAssembly(assemblyToSearch);
    }

    private void ProcessAttributeRegistriesForAssembly(Assembly assemblyToSearch)
    {
        foreach (Type type in assemblyToSearch.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                RegisterAttribute[] attributes = method.GetCustomAttributes<RegisterAttribute>(inherit: false).ToArray();
                if (!attributes.Any())
                {
                    continue;// no attribute present
                }
                
                // There can only be 1 RegisterEventAttribute (compiler enforced)
                RegisterAttribute attribute = attributes[0];

                RegisterMethod registerMethod = new RegisterMethod(attribute, method, this);
                
                if (!AttributeShouldRegister(registerMethod)) continue;
                
                HandleRegisterMethod(registerMethod);
            }
        }
    }
    
    private void HandleRegisterMethod(RegisterMethod registerMethod)
    {
        string globalRegistryID = GetGlobalRegistryID(registerMethod.Attribute.registryID);
        
        if (_idsRegistered.Contains(globalRegistryID))
        {
            throw new Exception($"Duplicate registry ID found: {registerMethod.Attribute.registryID}!");
        }
        
        if (DependenciesFulfilled(registerMethod.Attribute, out string firstFailedGlobalDependency))
        {
            registerMethod.Service.ExecuteMethodUnsafe(registerMethod);
            return;
        }

        if (DependencyCyclic(globalRegistryID, firstFailedGlobalDependency, out string chain))
        {
            throw new Exception($"Cyclic dependency registrations found! Dependency chain: {chain}");
        }

        if(_deferredRegistrations.TryGetValue(firstFailedGlobalDependency, out var list)){
            list.Add(registerMethod);
        }
        else
        {
            List<RegisterMethod> newDependentList = new();
            newDependentList.Add(registerMethod);
            _deferredRegistrations.Add(firstFailedGlobalDependency, newDependentList);
        }
    }
    
    private bool DependencyCyclic(string registryID, string dependencyID, out string chain)
    {
        if (registryID == dependencyID)
        {
            chain = $"{registryID},{dependencyID}";
            return true;
        }
        
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

            foreach (var registerMethod in waiting)
            {
                if (registerMethod.Attribute.registryID == dependencyID)
                {
                    path += $",{dependencyID}";
                    path += $",{registryID}";// Add the start to the end to clearly show the path loops
                    
                    // The _deferredRegistrations stores dependencies in reverse (ID, attributes waiting on ID) to how the Attributes are set up.
                    // This has the benefit of only checking deferred registration for cyclic issues, but we have to reverse at the very end to make it logical to what they typed.
                    chain = string.Join(" -> ", path.Split(',').Reverse());
                    return true;
                }
                toVisit.Push((registerMethod.Attribute.registryID, path + $",{registerMethod.Attribute.registryID}"));
            }
        }

        chain = null;
        return false;
    }

    private bool DependenciesFulfilled(RegisterAttribute registryAttribute, out string firstUnloadedGlobalDependency)
    {
        if (registryAttribute.loadAfterIDs == null)
        {
            firstUnloadedGlobalDependency = null;
            return true;
        }
        
        foreach (string dependentID in registryAttribute.loadAfterIDs)
        {
            string globalDependencyId = GetGlobalRegistryID(dependentID);
            
            if (_idsRegistered.Contains(globalDependencyId)) continue;
            
            firstUnloadedGlobalDependency = globalDependencyId;
            return false;
        }
        firstUnloadedGlobalDependency = null;
        return true;
    }
    
    private void ExecuteMethodUnsafe(RegisterMethod registerMethod)
    {
        if(Initializer.ConfigFile.enableDebugLogs)
            InternalLogger.Info($"Executing registry: {registerMethod.Attribute.registryID} from {registerMethod.MethodInfo.DeclaringType}");
        
        ParameterInfo[] parameters = registerMethod.MethodInfo.GetParameters();
        Object[] parameterValues = new Object[parameters.Length];
        
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameterInfo = parameters[i];
            InjectionContext context = new InjectionContext(registerMethod.Attribute, parameterInfo, registerMethod.MethodInfo,this);
            
            if (TryDependencyInject(context, out Object value))
            {
                parameterValues[i] = value;
            }
            else
            {
                throw new Exception($"No injector found for {parameterInfo.Name} when registering {context.attribute.registryID}");
            }
        }
        
        registerMethod.MethodInfo.Invoke(null, parameterValues);
        string globalRegistryId = GetGlobalRegistryID(registerMethod.Attribute.registryID);
        _idsRegistered.Add(globalRegistryId);
                
        // check and load for any that depended on this ID
        if (!_deferredRegistrations.TryGetValue(globalRegistryId, out var list)) return;
        // remove the dependency from the deferred list, saves a bit of memory but not technically required.
        _deferredRegistrations.Remove(globalRegistryId);
        list.ForEach(HandleRegisterMethod);
    }

    // Priority: Attribute injector -> Type of argument injector -> Argument name injector.
    private bool TryDependencyInject(InjectionContext context, out Object valueToInject)
    {
        foreach (Attribute parameterAttribute in context.parameterInfo.GetCustomAttributes())
        {
            if (_typedDependencyArgumentInjectors.TryGetValue(parameterAttribute.GetType(), out IDependencyArgumentInjector argumentAttributeInjector))
            {
                argumentAttributeInjector.TryInjectToArgument(context, out object result);
                valueToInject = result;
                return true;
            }
        }

        if (_typedDependencyArgumentInjectors.TryGetValue(context.parameterInfo.ParameterType, out IDependencyArgumentInjector typedInjector))
        {
            typedInjector.TryInjectToArgument(context, out Object value);
            valueToInject = value;
            return true;
        }
        
        foreach (IDependencyArgumentInjector injector in _dependencyArgumentInjectors)
        {
            if (!injector.TryInjectToArgument(context, out Object value)) continue;
            
            valueToInject = value;
            return true;
        }
        
        valueToInject = null;
        return false;
    }

    private static bool AttributeShouldRegister(RegisterMethod registerMethod)
    {
        /* Since IRegistryRequirement is an interface and the typed GetCustomAttributes<> requires the type to extend Attribute,
          we must filter objects to get the instances */
        IEnumerable<IRegistryRequirement> registryRequirements = registerMethod.MethodInfo
            .GetCustomAttributes(true)
            .OfType<IRegistryRequirement>();
        
        foreach (IRegistryRequirement registryRequirement in registryRequirements)
        {
            if (!registryRequirement.RequirementsMet()) return false;
        }
        return true;
    }

    private string GetGlobalRegistryID(string registryID)
    {
        var parts = registryID.Split(["::"], StringSplitOptions.None);
        if (parts.Length > 1)// registryId is already globalized
        {
            return registryID;
        }
        return modGuid + "::" + registryID;
    }
    
    internal static void LogErrorsForUnloadedDependencies()
    {
        foreach (KeyValuePair<string, List<RegisterMethod>> unloadedRegistryID in _deferredRegistrations)
        {
            string registriesWaitingForDependency = string.Join(", ", unloadedRegistryID.Value.Select(registerMethod => registerMethod.Attribute.registryID));
            InternalLogger.Log($"Registry(ies) {registriesWaitingForDependency} could not be loaded due to missing dependency: {unloadedRegistryID.Key}! Use conditional registration to avoid this, registrations should only be made when they are expected to load.", LogLevel.Error);
        }
    }
}