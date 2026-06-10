using System;
using JetBrains.Annotations;

namespace Nautilus.Utility.AttributeRegistration;

/// <summary>
/// Attributes a method for use with the <see cref="RegisterAttributeService"/>.
/// </summary>
/// <param name="id"><b><i>Unique</i></b> ID for this registration load event. Failure to have unique IDs will result in errors.</param>
/// <param name="loadAfterIDs">List of IDs that should be loaded before this one. Can be from a different registry execute call.</param>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public sealed class RegisterAttribute(string id, params string[] loadAfterIDs) : Attribute
{
    /// <summary>
    /// <b><i>Unique</i></b> ID for this registration load event. Failure to have unique IDs will result in errors.
    /// </summary>
    public string RegistryID { get; } = id;
    
    /// <summary>
    /// List of IDs that should be loaded before this one. Can be from a different registry execute call.
    /// </summary>
    public string[] LoadAfterIDs { get; } = loadAfterIDs;
}