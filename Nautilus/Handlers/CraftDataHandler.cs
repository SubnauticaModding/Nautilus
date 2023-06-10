﻿using Nautilus.Patchers;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for adding and editing crafted items.
/// </summary>
public static partial class CraftDataHandler
{

    /// <summary>
    /// Allows you to add items to the game's internal grouping system.
    /// Required if you want to make buildable items show up in the Habitat Builder or show in the Blueprints Tab of the PDA.
    /// </summary>
    /// <param name="group">The TechGroup you want to add your TechType to.</param>
    /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
    /// <param name="techType">The TechType you want to add.</param>
    public static void AddToGroup(TechGroup group, TechCategory category, TechType techType)
    {
        CraftDataPatcher.AddToGroup(group, category, techType, true, TechType.None);
    }

    /// <summary>
    /// Allows you to add items to the game's internal grouping system.
    /// Required if you want to make buildable items show up in the Habitat Builder or show in the Blueprints Tab of the PDA.
    /// </summary>
    /// <param name="group">The TechGroup you want to add your TechType to.</param>
    /// <param name="category">The TechCategory (in the TechGroup) you want to add your TechType to.</param>
    /// <param name="techType">The TechType you want to add.</param>
    /// <param name="after">Whether to add after or insert before the target, for sorting purposes.</param>
    /// <param name="target">It will be added/inserted next to this item or at the end/beginning if not found.</param>
    public static void AddToGroup(TechGroup group, TechCategory category, TechType techType, bool after = true, TechType target = TechType.None)
    {
        CraftDataPatcher.AddToGroup(group, category, techType, after, target);
    }

    /// <summary>
    /// Allows you to remove an existing TechType from the game's internal group system.
    /// </summary>
    /// <param name="group">The TechGroup in which the TechType is located.</param>
    /// <param name="category">The TechCategory in which the TechType is located.</param>
    /// <param name="techType">The TechType which you want to remove.</param>
    public static void RemoveFromGroup(TechGroup group, TechCategory category, TechType techType)
    {
        CraftDataPatcher.RemoveFromGroup(group, category, techType);
    }
}