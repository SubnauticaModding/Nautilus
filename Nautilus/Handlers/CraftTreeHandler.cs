namespace Nautilus.Handlers;

using System.Collections.Generic;
using Nautilus.Crafting;
using Nautilus.Patchers;

/// <summary>
/// A handler class for creating and modifying crafting trees.
/// </summary>
public static class CraftTreeHandler
{
    /// <summary>
    /// Adds a new crafting node to the root of the specified crafting tree, at the provided tab location.
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="craftingItem">The item to craft.</param>
    /// <param name="stepsToTab">
    /// <para>The steps to the target tab.</para>
    /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
    /// <para>Do not include "root" in this path.</para>
    /// <para>See <see href="https://subnauticamodding.github.io/Nautilus/tutorials/craft-tree-paths.html"/> or use the <see cref="Paths"/> class for examples of valid parameters.</para>
    /// </param>        
    public static void AddCraftingNode(CraftTree.Type craftTree, TechType craftingItem, params string[] stepsToTab)
    {
        if (!CraftTreePatcher.CraftingNodes.TryGetValue(craftTree, out var nodes))
        {
            nodes = new List<CraftingNode>();
        }

        nodes.Add(new CraftingNode(stepsToTab, craftTree, craftingItem));
        CraftTreePatcher.CraftingNodes[craftTree] = nodes;
    }

    /// <summary>
    /// Adds a new crafting node to the root of the specified crafting tree
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="craftingItem">The item to craft.</param>

    public static void AddCraftingNode(CraftTree.Type craftTree, TechType craftingItem)
    {
        if (!CraftTreePatcher.CraftingNodes.TryGetValue(craftTree, out var nodes))
        {
            nodes = new List<CraftingNode>();
        }

        nodes.Add(new CraftingNode(new string[0], craftTree, craftingItem));
        CraftTreePatcher.CraftingNodes[craftTree] = nodes;
    }

#if SUBNAUTICA
    /// <summary>
    /// Adds a new tab node to the root of the specified crafting tree.
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="name">The ID of the tab node. Must be unique!</param>
    /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab. If null or empty, this will use the language line "{craftTreeName}_{tabName}" instead.</param>
    /// <param name="sprite">The sprite of the tab.</param>        
    public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, Atlas.Sprite sprite)
    {
        if (!CraftTreePatcher.TabNodes.TryGetValue(craftTree, out var craftTreeTabNodes))
        {
            craftTreeTabNodes = new List<TabNode>();
        }

        craftTreeTabNodes.Add(new TabNode(new string[0], craftTree, sprite, name, displayName));
        CraftTreePatcher.TabNodes[craftTree] = craftTreeTabNodes;
    }

    /// <summary>
    /// Adds a new tab node to the root of the specified crafting tree.
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="name">The ID of the tab node. Must be unique!</param>
    /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab. If null or empty, this will use the language line "{craftTreeName}_{tabName}" instead.</param>
    /// <param name="sprite">The sprite of the tab.</param>

    public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, UnityEngine.Sprite sprite)
    {
        if (!CraftTreePatcher.TabNodes.TryGetValue(craftTree, out var craftTreeTabNodes))
        {
            craftTreeTabNodes = new List<TabNode>();
        }

        craftTreeTabNodes.Add(new TabNode(new string[0], craftTree, new Atlas.Sprite(sprite), name, displayName));
        CraftTreePatcher.TabNodes[craftTree] = craftTreeTabNodes;
    }

    /// <summary>
    /// Adds a new tab node to the root of the specified crafting tree, at the specified tab location.
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="name">The ID of the tab node. Must be unique!</param>
    /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab. If null or empty, this will use the language line "{craftTreeName}_{tabName}" instead.</param>
    /// <param name="sprite">The sprite of the tab.</param>
    /// <param name="stepsToTab">
    /// <para>The steps to the target tab.</para>
    /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
    /// <para>Do not include "root" in this path.</para>
    /// <para>See <see href="https://subnauticamodding.github.io/Nautilus/tutorials/craft-tree-paths.html"/> or use the <see cref="Paths"/> class for examples of valid parameters.</para>
    /// </param>        
    public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, Atlas.Sprite sprite, params string[] stepsToTab)
    {
        if (!CraftTreePatcher.TabNodes.TryGetValue(craftTree, out var craftTreeTabNodes))
        {
            craftTreeTabNodes = new List<TabNode>();
        }

        craftTreeTabNodes.Add(new TabNode(stepsToTab, craftTree, sprite, name, displayName));
        CraftTreePatcher.TabNodes[craftTree] = craftTreeTabNodes;
    }

    /// <summary>
    /// Adds a new tab node to the root of the specified crafting tree, at the specified tab location.
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="name">The ID of the tab node. Must be unique!</param>
    /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab. If null or empty, this will use the language line "{craftTreeName}_{tabName}" instead.</param>
    /// <param name="sprite">The sprite of the tab.</param>
    /// <param name="stepsToTab">
    /// <para>The steps to the target tab.</para>
    /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
    /// <para>Do not include "root" in this path.</para>
    /// <para>See <see href="https://subnauticamodding.github.io/Nautilus/tutorials/craft-tree-paths.html"/> or use the <see cref="Paths"/> class for examples of valid parameters.</para>
    /// </param>        
    public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, UnityEngine.Sprite sprite, params string[] stepsToTab)
    {
        if (!CraftTreePatcher.TabNodes.TryGetValue(craftTree, out var craftTreeTabNodes))
        {
            craftTreeTabNodes = new List<TabNode>();
        }

        craftTreeTabNodes.Add(new TabNode(stepsToTab, craftTree, new Atlas.Sprite(sprite), name, displayName));
        CraftTreePatcher.TabNodes[craftTree] = craftTreeTabNodes;
    }

#elif BELOWZERO
    /// <summary>
    /// Adds a new tab node to the root of the specified crafting tree.
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="name">The ID of the tab node. Must be unique!</param>
    /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab. If null or empty, this will use the language line "{craftTreeName}_{tabName}" instead.</param>
    /// <param name="sprite">The sprite of the tab.</param>        
    public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, UnityEngine.Sprite sprite)
    {
        if (!CraftTreePatcher.TabNodes.TryGetValue(craftTree, out var craftTreeTabNodes))
        {
            craftTreeTabNodes = new List<TabNode>();
        }

        craftTreeTabNodes.Add(new TabNode(new string[0], craftTree, sprite, name, displayName));
        CraftTreePatcher.TabNodes[craftTree] = craftTreeTabNodes;
    }

    /// <summary>
    /// Adds a new tab node to the root of the specified crafting tree, at the specified tab location.
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="name">The ID of the tab node. Must be unique!</param>
    /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab. If null or empty, this will use the language line "{craftTreeName}_{tabName}" instead.</param>
    /// <param name="sprite">The sprite of the tab.</param>
    /// <param name="stepsToTab">
    /// <para>The steps to the target tab.</para>
    /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
    /// <para>Do not include "root" in this path.</para>
    /// <para>See <see href="https://subnauticamodding.github.io/Nautilus/tutorials/craft-tree-paths.html"/> or use the <see cref="Paths"/> class for examples of valid parameters.</para>
    /// </param>        
    public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, UnityEngine.Sprite sprite, params string[] stepsToTab)
    {
        if (!CraftTreePatcher.TabNodes.TryGetValue(craftTree, out var craftTreeTabNodes))
        {
            craftTreeTabNodes = new List<TabNode>();
        }

        craftTreeTabNodes.Add(new TabNode(stepsToTab, craftTree, sprite, name, displayName));
        CraftTreePatcher.TabNodes[craftTree] = craftTreeTabNodes;
    }

#endif

    /// <summary>
    /// <para>Removes a node at the specified node location. Can be used to remove either tabs or craft nodes.</para>
    /// <para>If a tab node is selected, all child nodes to it will also be removed.</para>
    /// </summary>
    /// <param name="craftTree">The target craft tree to edit.</param>
    /// <param name="stepsToNode">
    /// <para>The steps to the target node.</para>
    /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
    /// <para>This means matching the id of the crafted item or the id of the tab name.</para>
    /// <para>Do not include "root" in this path.</para>
    /// <para>See <see href="https://subnauticamodding.github.io/Nautilus/tutorials/craft-tree-paths.html"/> or use the <see cref="Paths"/> class for examples of valid parameters.</para>
    /// </param>

    public static void RemoveNode(CraftTree.Type craftTree, params string[] stepsToNode)
    {
        if (!CraftTreePatcher.NodesToRemove.TryGetValue(craftTree, out var nodesToRemove))
        {
            nodesToRemove =  new List<Node>();
        }

        nodesToRemove.Add(new Node(stepsToNode, craftTree));
        CraftTreePatcher.NodesToRemove[craftTree] = nodesToRemove;
    }

    /// <summary>
    /// A list of all the built-in craft tree paths (AKA "steps to tab") for use in methods such as <see cref="AddCraftingNode(CraftTree.Type, TechType, string[])"/>.
    /// </summary>
    public static class Paths
    {
#if SUBNAUTICA
        /// <summary> Steps to the Basic Materials tab in the Fabricator (<c>Resources/BasicMaterials</c>). </summary>
        public static string[] FabricatorsBasicMaterials => new string[] { "Resources", "BasicMaterials" };

        /// <summary> Steps to the Advanced Materials tab in the Fabricator (<c>Resources/AdvancedMaterials</c>). </summary>
        public static string[] FabricatorsAdvancedMaterials => new string[] { "Resources", "AdvancedMaterials" };

        /// <summary> Steps to the Electronics tab in the Fabricator (<c>Resources/Electronics</c>). </summary>
        public static string[] FabricatorsElectronics => new string[] { "Resources", "Electronics" };

        /// <summary> Steps to the Water tab in the Fabricator (<c>Survival/Water</c>). </summary>
        public static string[] FabricatorWater => new string[] { "Survival", "Water" };

        /// <summary> Steps to the Cooked Food tab in the Fabricator (<c>Survival/CookedFood</c>). </summary>
        public static string[] FabricatorCookedFood => new string[] { "Survival", "CookedFood" };

        /// <summary> Steps to the Cured Food tab in the Fabricator (<c>Survival/CuredFood</c>). </summary>
        public static string[] FabricatorCuredFood => new string[] { "Survival", "CuredFood" };

        /// <summary> Steps to the Equipment tab in the Fabricator (<c>Personal/Equipment</c>). </summary>
        public static string[] FabricatorEquipment => new string[] { "Personal", "Equipment" };

        /// <summary> Steps to the Tools tab in the Fabricator (<c>Personal/Tools</c>). </summary>
        public static string[] FabricatorTools => new string[] { "Personal", "Tools" };

        /// <summary> Steps to the Deployables tab in the Fabricator (<c>Machines</c>). </summary>
        public static string[] FabricatorMachines => new string[] { "Machines" };

        /// <summary> Steps to the Vehicles tab in the Mobile Vehicle Bay (<c>Vehicles</c>). </summary>
        public static string[] ConstructorVehicles => new string[] { "Vehicles" };

        /// <summary> Steps to the Rocket tab in the Mobile Vehicle Bay (<c>Rocket</c>). </summary>
        public static string[] ConstructorRocket => new string[] { "Rocket" };

        /// <summary> Steps to the Common Modules tab in the Vehicle Upgrade Console (<c>CommonModules</c>). </summary>
        public static string[] VehicleUpgradesCommonModules => new string[] { "CommonModules" };

        /// <summary> Steps to the Seamoth Modules tab in the Vehicle Upgrade Console (<c>SeamothModules</c>). </summary>
        public static string[] VehicleUpgradesSeamothModules => new string[] { "SeamothModules" };

        /// <summary> Steps to the Prawn Suit Modules tab in the Vehicle Upgrade Console (<c>ExosuitModules</c>). </summary>
        public static string[] VehicleUpgradesExosuitModules => new string[] { "ExosuitModules " };

        /// <summary> Steps to the Torpedoes tab in the Vehicle Upgrade Console (<c>Torpedoes</c>). </summary>
        public static string[] VehicleUpgradesTorpedoes => new string[] { "Torpedoes " };
#endif
#if BELOWZERO
        /// <summary> Steps to the Basic Materials tab in the Fabricator (<c>Resources/BasicMaterials</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorsBasicMaterials => new string[] { "Resources", "BasicMaterials" };

        /// <summary> Steps to the Advanced Materials tab in the Fabricator (<c>Resources/AdvancedMaterials</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorsAdvancedMaterials => new string[] { "Resources", "AdvancedMaterials" };

        /// <summary> Steps to the Electronics tab in the Fabricator (<c>Resources/Electronics</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorsElectronics => new string[] { "Resources", "Electronics" };

        /// <summary> Steps to the Water tab in the Fabricator (<c>Survival/Water</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorWater => new string[] { "Survival", "Water" };

        /// <summary> Steps to the Cooked Food tab in the Fabricator (<c>Survival/CookedFood</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorCookedFood => new string[] { "Survival", "CookedFood" };

        /// <summary> Steps to the Cured Food tab in the Fabricator (<c>Survival/CuredFood</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorCuredFood => new string[] { "Survival", "CuredFood" };

        /// <summary> Steps to the Equipment tab in the Fabricator (<c>Personal/Equipment</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorEquipment => new string[] { "Personal", "Equipment" };

        /// <summary> Steps to the Tools tab in the Fabricator (<c>Personal/Tools</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorTools => new string[] { "Personal", "Tools" };

        /// <summary> Steps to the Deployables tab in the Fabricator (<c>Machines</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorMachines => new string[] { "Machines" };

        /// <summary> Steps to the Prawn Suit Upgrades tab in the Fabricator (<c>Upgrades/ExosuitUpgrades</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorExosuitUpgrades => new string[] { "Upgrades", "ExosuitUpgrades" };

        /// <summary> Steps to the Seatruck Upgrades tab in the Fabricator (<c>Upgrades/SeatruckUpgrades</c>). Also applicable for the <see cref="CraftTree.seatruckFabricator"/>. </summary>
        public static string[] FabricatorSeatruckUpgrades => new string[] { "Upgrades", "SeatruckUpgrades" };

        /// <summary> Steps to the Vehicles tab in the Mobile Vehicle Bay (<c>Vehicles</c>). </summary>
        public static string[] ConstructorVehicles => new string[] { "Vehicles" };

        /// <summary> Steps to the (Seatruck) Modules tab in the Mobile Vehicle Bay (<c>Modules</c>). </summary>
        public static string[] ConstructorModules => new string[] { "Modules" };

        /// <summary> Steps to the Prawn Suit Upgrades tab in the Vehicle Upgrade Console (<c>ExosuitModules</c>). </summary>
        public static string[] VehicleUpgradesExosuitModules => new string[] { "ExosuitModules" };

        /// <summary> Steps to the Seatruck Upgrades tab in the Vehicle Upgrade Console (<c>SeaTruckUpgrade</c>). </summary>
        public static string[] VehicleUpgradesSeaTruckUpgrade => new string[] { "SeaTruckUpgrade" };
#endif
    }
}
