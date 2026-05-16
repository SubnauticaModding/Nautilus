using System;
using System.Reflection;
using Nautilus.Assets;

namespace Nautilus.Utility.AttributeRegistration.Injectors;


/// <summary>
/// Represents an injector that checks for <see cref="PrefabInfo"/> as a parameter type
/// </summary>
public sealed class PrefabInfoInjector : IDependencyArgumentInjector
{
    /// <summary>
    /// Checks whether the argument is of type <see cref="PrefabInfo"/>. If so, the method returns true and the result is set to
    /// a new <see cref="PrefabInfo"/> with a <see cref="TechType"/> already defined using the <see cref="RegisterEventAttribute.registryID">
    /// registryID</see> defined in the <see cref="RegisterEventAttribute"/> of this method.
    /// </summary>
    /// <param name="attribute">Attribute attached to the method with.</param>
    /// <param name="argument">Parameter argument to check and inject for.</param>
    /// <param name="result">The result of the injection (can be safely cast to a <see cref="PrefabInfo"/>). Is null when this method returns false.</param>
    /// <returns>True if there is a valid injection for the current argument, otherwise false.</returns>
    public bool TryInjectToArgument(RegisterEventAttribute attribute, ParameterInfo argument, out object result)
    {
        if (argument.ParameterType == typeof(PrefabInfo))
        {
            result = PrefabInfo.WithTechType(attribute.registryID);
            return true;
        }
        result = null;
        return false;
    }
    
    /// <returns>Returns the result of typeof(<see cref="PrefabInfo"/>)</returns>
    public Type InjectorTargetType => typeof(PrefabInfo);
}
