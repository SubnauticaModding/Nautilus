using System;

namespace Nautilus.Utility.AttributeRegistrationUtils.RegistryRequirements;


/// <summary>
/// Base attribute class used for parsing all required attributes. Can not be used as an attribute itself as it is abstract
/// </summary>
public interface IRegistryRequirement
{
    /// <summary>
    /// Checks whether this attribute allows the attached register to execute.
    /// </summary>
    /// <returns>True if the requirements to register are met for this attribute, otherwise false</returns>
    public bool RequirementsMet();
}