using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Nautilus.Utility.AttributeRegistrationUtils;


/// <summary>
/// Attributes a method for use with the <see cref="RegisterEventAttributeLoader"/>.
/// </summary>
/// <param name="id"><b><i>Unique</i></b> ID for this registration load event. Failure to have a unique ID (including between other mods) will result in errors</param>
/// <param name="loadAfterIDs">List of IDs that should be loaded before this one. Can be from a different registry execute call or mod.</param>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public sealed class RegisterEventAttribute(string id, params string[] loadAfterIDs) : Attribute
{
    /// <summary>
    /// <b><i>Unique</i></b> ID for this registration load event. Failure to have a unique ID will result in errors
    /// </summary>
    public readonly string registryID = id;
    
    /// <summary>
    /// List of IDs that should be loaded before this one. Can be from a different registry execute call or mod.
    /// </summary>
    public readonly string[] loadAfterIDs = loadAfterIDs;

    /// <summary>
    /// The associated method with this attribute.
    /// </summary>
    /// <remarks>NOTE: the methodInfo is null until a <see cref="RegisterEventAttributeLoader"/> parses this attribute.
    /// This should not be used to gain information about the method in an injector (prefer <see cref="ParameterInfo.Member"/>)</remarks>
    internal MethodInfo methodInfo;

    //used to associate this attribute with a specific loader as they can have differing injectors. Only set during parsing
    internal RegisterEventAttributeLoader loader;
}