using System;

namespace SMLHelper.DependencyInjection;

[AttributeUsage(AttributeTargets.Method)]
public sealed class InjectionSetupAttribute : Attribute
{}
