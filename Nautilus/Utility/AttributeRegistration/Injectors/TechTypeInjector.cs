using System.ComponentModel;
using System.Linq;
using Nautilus.Handlers;

namespace Nautilus.Utility.AttributeRegistration.Injectors;

internal sealed class TechTypeInjector : IDependencyArgumentInjector
{
    public bool TryInjectToArgument(InjectionContext context, out object result)
    {
        if(context.parameterInfo.ParameterType == typeof(TechType))
        {
            if(EnumHandler.TryGetValue(context.parameterInfo.Name, out TechType techType))
            {
                result = techType;
                return true;
            }
            
            bool techTypeIDPresentInDependencies = context.attribute.loadAfterIDs.Any(loadAfterID => loadAfterID.ToLower() == context.parameterInfo.Name.ToLower());
            if(techTypeIDPresentInDependencies)
            {
                throw new InvalidEnumArgumentException($"Failed to parse parameter TechType while registering {context.attribute.registryID} within {context.parameterInfo.Member.MemberType}!" +
                                                       $"Ensure the other registryId registers a TechType when requesting a parameter of type TechType.");
            }
            throw new InvalidEnumArgumentException($"Failed to parse parameter TechType while registering {context.attribute.registryID} within {context.parameterInfo.Member.MemberType}!" +
                                                   $"Ensure you are waiting for the other modded registryID to be loaded within the {nameof(RegisterAttribute)} params.");
        }
        result = null;
        return false;
    }
}
