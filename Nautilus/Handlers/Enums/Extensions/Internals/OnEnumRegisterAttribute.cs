using System;

namespace Nautilus.Handlers.Internals;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class OnEnumRegisterAttribute<TEnum> : Attribute  where TEnum : Enum
{
    
}