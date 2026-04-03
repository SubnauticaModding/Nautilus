using System;

namespace Nautilus.Utility.AttributeRegistrationUtils.RegistryRequirements;


/// <summary>
/// When an attribute implement this interface, Nautilus will only execute the attributed registry event when <see cref="RequirementsMet"/> returns true
/// for all <see cref="IRegistryRequirement">IRegistryRequirements</see> attribute implementors attached.
/// </summary>
public interface IRegistryRequirement
{
    /// <summary>
    /// Checks whether this attribute allows the attached register to execute.
    /// </summary>
    /// <returns>True if the requirements to register are met for this attribute, otherwise false</returns>
    public bool RequirementsMet();
}