using System.Reflection;

namespace Nautilus.Utility.AttributeRegistration;

public sealed class InjectionContext
{
    internal InjectionContext(RegisterAttribute attribute, ParameterInfo parameterInfo, MethodInfo methodInfo, RegisterAttributeService service)
    {
        this.attribute = attribute;
        this.parameterInfo = parameterInfo;
        this.methodInfo = methodInfo;
        this.service = service;
    }
    
    public readonly RegisterAttribute attribute;
    public readonly ParameterInfo parameterInfo;
    public readonly MethodInfo methodInfo;
    public readonly RegisterAttributeService service;
}