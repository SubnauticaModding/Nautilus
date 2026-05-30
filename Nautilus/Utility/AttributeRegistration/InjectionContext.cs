using System.Reflection;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Represents immutable information about the requested injection.
/// </summary>
public sealed class InjectionContext
{
    internal InjectionContext(RegisterAttribute attribute, ParameterInfo parameterInfo, MethodInfo methodInfo, RegisterAttributeService service)
    {
        this.attribute = attribute;
        this.parameterInfo = parameterInfo;
        this.methodInfo = methodInfo;
        this.service = service;
    }
    
    /// <summary>
    /// The attribute associated with this register method.
    /// </summary>
    public readonly RegisterAttribute attribute;
    
    /// <summary>
    /// Information about the parameter to inject to.
    /// </summary>
    public readonly ParameterInfo parameterInfo;
    
    /// <summary>
    /// Information about the method for attached <see cref="RegisterAttribute"/>.
    /// </summary>
    public readonly MethodInfo methodInfo;
    
    /// <summary>
    /// The Service used to execute this method.
    /// </summary>
    public readonly RegisterAttributeService service;
}