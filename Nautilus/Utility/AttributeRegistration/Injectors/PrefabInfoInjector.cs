using Nautilus.Assets;

namespace Nautilus.Utility.AttributeRegistration.Injectors;

internal sealed class PrefabInfoInjector : IDependencyArgumentInjector
{
    public bool TryInjectToArgument(InjectionContext context, out object result)
    {
        if (context.parameterInfo.ParameterType == typeof(PrefabInfo))
        {
            result = PrefabInfo.WithTechType(context.attribute.registryID);
            return true;
        }
        result = null;
        return false;
    }
}
