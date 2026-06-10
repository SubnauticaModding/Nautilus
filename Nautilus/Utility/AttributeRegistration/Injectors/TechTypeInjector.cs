using System;
using System.Linq;
using Nautilus.Handlers;

namespace Nautilus.Utility.AttributeRegistration.Injectors;

internal sealed class TechTypeInjector : IDependencyArgumentInjector
{
    public bool TryInjectToArgument(InjectionContext context, out object result)
    {
        if (context.ParameterInfo.ParameterType == typeof(TechType))
        {
            if (EnumHandler.TryGetValue(context.ParameterInfo.Name, out TechType techType))
            {
                result = techType;
                return true;
            }

            bool techTypeIDPresentInDependencies = context.Attribute.LoadAfterIDs.Any(loadAfterID =>
                string.Equals(loadAfterID, context.ParameterInfo.Name, StringComparison.OrdinalIgnoreCase));
            
            if (techTypeIDPresentInDependencies)
            {
                throw new InjectorException(context, $"Could not parse parameter into TechType. " +
                                                     $"Ensure the other RegistryId registers a TechType when requesting a parameter of type TechType.");
            }

            throw new InjectorException(context, $"Could not parse parameter into TechType. " +
                                                 $"Ensure you are waiting for the other modded RegistryID to be loaded within the {nameof(RegisterAttribute)} params.");
        }

        result = null;
        return false;
    }
}