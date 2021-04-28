using System.Reflection;

namespace SMLHelper.V2.Interfaces
{
    internal interface ITechTypeHandlerInternal : ITechTypeHandler
    {
        TechType AddTechType(Assembly assembly, string internalName, string displayName, string tooltip);
        TechType AddTechType(Assembly assembly, string internalName, string displayName, string tooltip, bool unlockAtStart);
    }
}
