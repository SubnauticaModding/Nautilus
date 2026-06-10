using System.Reflection;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Represents immutable information about the requested injection.
/// </summary>
public sealed class InjectionContext
{
    internal InjectionContext(RegisterAttribute attribute, ParameterInfo parameterInfo, MethodInfo methodInfo, RegisterAttributeService service)
    {
        Attribute = attribute;
        ParameterInfo = parameterInfo;
        MethodInfo = methodInfo;
        Service = service;
    }
    
    /// <summary>
    /// The attribute associated with this register method.
    /// </summary>
    public RegisterAttribute Attribute { get; }
    
    /// <summary>
    /// Information about the parameter to inject to.
    /// </summary>
    public ParameterInfo ParameterInfo { get; }
    
    /// <summary>
    /// Information about the method for attached <see cref="RegisterAttribute"/>.
    /// </summary>
    public MethodInfo MethodInfo { get; }
    
    /// <summary>
    /// The Service used to execute this method.
    /// </summary>
    public RegisterAttributeService Service { get; }
}