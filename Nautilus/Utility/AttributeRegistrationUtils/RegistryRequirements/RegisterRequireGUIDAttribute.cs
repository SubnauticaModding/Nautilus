using System;
using BepInEx;

namespace Nautilus.Utility.AttributeRegistrationUtils.RegistryRequirements;

/// <summary>
/// When a method is attached with a [<see cref="RegisterEventAttribute"/>] alongside this attribute,
/// Nautilus will only execute the registry if every mod GUID is present. This is similar to a soft <see cref="BepInDependency"/> but allows
/// the creation of registries that optionally load when all other mod GUIDs are present.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RegisterRequireGuidAttribute(params string[] requiredModGUIDs) : Attribute, IRegistryRequirement
{
    /// <summary>
    /// List of BepInEx mod GUIDs to require to be loaded
    /// </summary>
    private readonly string[] _requiredModGUIDs = requiredModGUIDs;

    /// <summary>
    /// Checks whether all defined GUIDs are loaded to allows the attached register to execute.
    /// </summary>
    /// <returns>True if all defined GUIDs are loaded within the BepInEx chain-loader, otherwise false</returns>
    public bool RequirementsMet()
    {
        foreach (string modGuid in _requiredModGUIDs)
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(modGuid, out var pluginInfo)
             || pluginInfo.Instance == null)
            {
                return false;
            }
        }
        return true;
    }
}