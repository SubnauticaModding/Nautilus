using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Nautilus.Handlers;

namespace Nautilus.Utility.AttributeRegistration.Injectors;


/// <summary>
/// Represents an injector that checks for <see cref="TechType"/> as a parameter type
/// </summary>
public sealed class TechTypeInjector : IDependencyArgumentInjector
{
    /// <summary>
    /// Checks whether the argument is of type <see cref="TechType"/>. 
    /// </summary>
    /// <param name="attribute">Attribute attached to the method with.</param>
    /// <param name="argument">Parameter argument to check and inject for.</param>
    /// <param name="result">The result of the injection (can be safely cast to a <see cref="TechType"/>). Is null when this method returns false.</param>
    /// <returns>True if there is a valid injection for the current argument, otherwise false.</returns>
    /// <exception cref="Exception"> is thrown when there is no TechType for the associated argument</exception>
    public bool TryInjectToArgument(RegisterEventAttribute attribute, ParameterInfo argument, out object result)
    {
        if(argument.ParameterType == typeof(TechType))
        {
            if(EnumHandler.TryGetValue(argument.Name, out TechType techType))
            {
                result = techType;
                return true;
            }
            
            bool techTypeIDPresentInDependencies = attribute.loadAfterIDs.Any(loadAfterID => loadAfterID.ToLower() == argument.Name.ToLower());
            if(techTypeIDPresentInDependencies)
            {
                throw new InvalidEnumArgumentException($"Failed to parse parameter TechType while registering {attribute.registryID} within {attribute.methodInfo.MemberType}!" +
                                                       $"Ensure the other registryId registers a TechType when requesting a parameter of type TechType.");
            }
            throw new InvalidEnumArgumentException($"Failed to parse parameter TechType while registering {attribute.registryID} within {attribute.methodInfo.MemberType}!" +
                                                   $"Ensure you are waiting for the other modded registryID to be loaded within the {nameof(RegisterEventAttribute)} params.");
        }
        result = null;
        return false;
    }
    
    /// <returns>Returns the result of typeof(<see cref="TechType"/>)</returns>
    public Type InjectorTargetType => typeof(TechType);
}
