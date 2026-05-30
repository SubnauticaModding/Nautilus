using System;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Represents an error that is thrown when injecting arguments within <see cref="RegisterAttributeServiceExtensions"/>
/// </summary>
/// <param name="context">the given context of the injection</param>
/// <param name="message">further error specification to tack on</param>
public class InjectorException(InjectionContext context, string message = null) : Exception
{
    /// <inheritdoc />
    public override string Message => $"Failed to execute registry: {context.attribute.registryID}! " + message;
}