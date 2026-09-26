using System.Collections.Generic;

namespace Nautilus.Handlers;

/// <summary>
/// Allows registering objects to be affected by the Grav Trap.
/// </summary>
public static class GravSphereHandler
{
    private static readonly HashSet<TechType> _customTechTypes = new();
    
    internal static bool IsValidCustomGravTrapTarget(TechType techType) => _customTechTypes.Contains(techType);

    /// <summary>
    /// Allows objects with the TechType <paramref name="techType"/> to be grabbed by the Grav Trap.
    /// </summary>
    /// <remarks>
    /// In the base game, most small and medium creatures, outcrops, resources, crafting items, and eggs can be grabbed.
    /// </remarks>
    /// <param name="techType">The TechType of the item, creature, egg, etc. to include.</param>
    public static void AddAffectedTechType(TechType techType)
    {
        _customTechTypes.Add(techType);
    }
    
    /// <summary>
    /// Removes the given TechType from the collection of custom inclusions for the Grav Trap. 
    /// </summary>
    /// <param name="techType">The TechType to remove, which must have been previously registered.</param>
    /// <remarks>This method only removes TechTypes that were added through the <see cref="GravSphereHandler"/> class;
    /// it cannot exclude default entries defined as part of the base game.</remarks>
    public static void RemoveAffectedTechType(TechType techType)
    {
        _customTechTypes.Remove(techType);
    }
}