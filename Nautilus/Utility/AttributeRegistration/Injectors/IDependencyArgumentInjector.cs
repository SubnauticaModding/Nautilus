namespace Nautilus.Utility.AttributeRegistration.Injectors;

/// <summary>
/// Represents an injector that can inject a required parameter into a method for execution.
/// </summary>
public interface IDependencyArgumentInjector
{
    /// <summary>
    /// Check whether the argument is valid for this injector. If so, the result of the conversion is outed and the method returns true.
    /// Otherwise, if this injector cannot inject for this argument, this method returns false.
    /// </summary>
    /// <param name="context">The <see cref="InjectionContext"/> for the given argument.</param>
    /// <param name="result">The result to inject based on the argument. Is null when this method is false.</param>
    /// <returns>True if there is a valid injection for the current argument, otherwise false.</returns>
    bool TryInjectToArgument(InjectionContext context, out object result);
}
