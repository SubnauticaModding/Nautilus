using Nautilus.Assets;

namespace Nautilus.Utility.AttributeRegistration.Injectors;

internal sealed class PrefabInfoInjector : IDependencyArgumentInjector
{
    public bool TryInjectToArgument(InjectionContext context, out object result)
    {
        if (context.ParameterInfo.ParameterType == typeof(PrefabInfo))
        {
            result = PrefabInfo.WithTechType(context.Attribute.RegistryID);
            return true;
        }
        result = null;
        return false;
    }
}
