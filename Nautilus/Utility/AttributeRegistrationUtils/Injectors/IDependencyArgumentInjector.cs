using System;
using System.Reflection;

namespace Nautilus.Utility.AttributeRegistrationUtils.Injectors;


/// <summary>
/// Represents an injector that can inject a dependent parameter into a method for execution.
/// </summary>
public interface IDependencyArgumentInjector
{
    /// <summary>
    /// Check whether the argument is valid for this injector. If so, the result of the conversion is outed and the method returns true.
    /// Otherwise, if this injector cannot inject for this argument, this method returns false.
    /// </summary>
    /// <param name="attribute">Attribute attached to the method with </param>
    /// <param name="argument">Parameter argument to check and inject for</param>
    /// <param name="result">the result to inject based on the argument. Is null when this method is false</param>
    /// <returns>True if there is a valid injection for the current argument, otherwise false.</returns>
    public bool TryInjectToArgument(RegisterEventAttribute attribute, ParameterInfo argument, out object result);

    /// <summary>
    /// The <see cref="Type"/> the injector targets. The Type can either represent the argument type or the type behind an <see cref="Attribute"/>.
    /// If the injector does not search for a type or attribute attached to the argument, this should return null;
    /// </summary>
    /// <returns>The <see cref="Type"/> the injector targets</returns>
    public Type InjectorTargetType();
}
