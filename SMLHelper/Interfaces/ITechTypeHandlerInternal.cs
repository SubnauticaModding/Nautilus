namespace SMLHelper.V2.Interfaces
{
    internal interface ITechTypeHandlerInternal : ITechTypeHandler
    {
        TechType AddTechType(string modName, string internalName, string displayName, string tooltip);
        TechType AddTechType(string modName, string internalName, string displayName, string tooltip, bool unlockAtStart);
    }
}
