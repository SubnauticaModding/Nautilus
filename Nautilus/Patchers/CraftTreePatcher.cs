namespace Nautilus.Patchers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;

internal class CraftTreePatcher
{
    #region Internal Fields

    internal static Dictionary<CraftTree.Type, ModCraftTreeRoot> CustomTrees = new();
    internal static Dictionary<CraftTree.Type, List<Node>> NodesToRemove = new();
    internal static Dictionary<CraftTree.Type, List<CraftingNode>> CraftingNodes = new();
    internal static Dictionary<CraftTree.Type, List<TabNode>> TabNodes = new();
    internal static Dictionary<CraftTree.Type, CraftTree> CachedTrees = new();
    private const string FallbackTabNode = "Modded";
    private const string VanillaRoot = "Vanilla";

    #endregion

    #region Patches

    internal static void Patch(Harmony harmony)
    {
        CreateFallbackNodes();
        harmony.PatchAll(typeof(CraftTreePatcher));
        InternalLogger.Log($"CraftTreePatcher is done.", LogLevel.Debug);
    }

    private static void CreateFallbackNodes()
    {
        // Workbench
        CreateVanillaTabNode(CraftTree.Type.Workbench, "Modification Station", TechType.Workbench, CraftTree.WorkbenchScheme().root);
        CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, FallbackTabNode + CraftTree.Type.Workbench, "Mod Items", SpriteManager.Get(TechType.Workbench));

        // Fabricator
        CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, FallbackTabNode + CraftTree.Type.Fabricator, "Mod Items", SpriteManager.Get(TechType.Fabricator));

        // Constructor
        CraftTreeHandler.AddTabNode(CraftTree.Type.Constructor, FallbackTabNode + CraftTree.Type.Constructor, "Mod Items", SpriteManager.Get(TechType.Constructor));

        // Seamoth Upgrades
        CraftTreeHandler.AddTabNode(CraftTree.Type.SeamothUpgrades, FallbackTabNode + CraftTree.Type.SeamothUpgrades, "Mod Items", SpriteManager.Get(TechType.BaseUpgradeConsole));

        // Map Room
        CreateVanillaTabNode(CraftTree.Type.MapRoom, "Scanner Upgrades", TechType.BaseMapRoom, CraftTree.MapRoomSheme().root);
        CraftTreeHandler.AddTabNode(CraftTree.Type.MapRoom, FallbackTabNode + CraftTree.Type.MapRoom, "Mod Items", SpriteManager.Get(TechType.BaseMapRoom));
#if SUBNAUTICA
        // Cyclops Fabricator
        CreateVanillaTabNode(CraftTree.Type.CyclopsFabricator, "Cyclops Fabricator", TechType.Cyclops, CraftTree.CyclopsFabricatorScheme().root);
        CraftTreeHandler.AddTabNode(CraftTree.Type.CyclopsFabricator, FallbackTabNode + CraftTree.Type.CyclopsFabricator, "Mod Items", SpriteManager.Get(TechType.Cyclops));
#elif BELOWZERO
        // SeaTruck Fabricator
        CraftTreeHandler.AddTabNode(CraftTree.Type.SeaTruckFabricator, FallbackTabNode+CraftTree.Type.SeaTruckFabricator, "Mod Items", SpriteManager.Get(TechType.SeaTruckFabricator));
#endif
    }

    private static void CreateVanillaTabNode(CraftTree.Type treeType, string DisplayName, TechType spriteTechType, TreeNode root)
    {
        var removedNodes = new List<CraftNode>();
        foreach (var node in root.nodes)
        {
            if (node is not CraftNode craftNode || craftNode.action == TreeAction.Expand)
                continue;

            CraftTreeHandler.RemoveNode(treeType, new[] { node.id });
            removedNodes.Add(craftNode);
        }

        if (removedNodes.Count == 0)
            return;

        var vanillaTab = VanillaRoot + treeType;
        CraftTreeHandler.AddTabNode(treeType, vanillaTab, DisplayName, SpriteManager.Get(spriteTechType));
        foreach (var node in removedNodes)
        {
            InternalLogger.Debug($"Moved {node.techType0} from {treeType} root into new {vanillaTab} tab.");
            CraftTreeHandler.AddCraftingNode(treeType, node.techType0, new[] { vanillaTab });
        }
        InternalLogger.Info($"Reorganized {removedNodes.Count} {treeType} nodes into new {vanillaTab} tab.");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.GetTree))]
    private static void GetTreePreFix(CraftTree.Type treeType, ref CraftTree __result)
    {
        if (CachedTrees.TryGetValue(treeType, out var cachedTree))
        {
            __result = cachedTree;
            return;
        }

        __result ??= !CustomTrees.TryGetValue(treeType, out var customRoot) ? __result : customRoot.CustomCraftingTree;

        if (__result == null)
        {
            // The game actually has a few CraftTree.Type that are not used...
            // None, Unused1, Unused2, etc...
            // but we still want to log just in case.
            InternalLogger.Debug($"Unable to find the CraftTree for type {treeType}.");
            return;
        }

        PatchCraftTree(ref __result, treeType);
        CraftTree.AddToCraftableTech(__result);
        CachedTrees.Add(treeType, __result);
        return;
    }

    #endregion

    #region Handling Nodes

    private static void PatchCraftTree(ref CraftTree __result, CraftTree.Type type)
    {
        List<Node> removals = NodesToRemove.TryGetValue(type, out removals)? removals: new List<Node>();
        RemoveNodes(ref __result, ref removals);

        AddCustomTabs(ref __result, type);
        PatchNodes(ref __result, type);

        // Remove any nodes added by mods that were marked for removal by other mods.
        RemoveNodes(ref __result, ref removals);
    }

    private static void AddCustomTabs(ref CraftTree tree, CraftTree.Type type)
    {
        List<TabNode> customTabs = TabNodes.TryGetValue(type, out customTabs) ? customTabs : new List<TabNode>();
        foreach (TabNode customNode in customTabs)
        {
            if(!TraverseTree(tree.nodes, customNode.Path, out var currentNode))
            {
                InternalLogger.Error($"Cannot add tab: {customNode.Name} to {customNode.Scheme} at {string.Join("/", customNode.Path)} as the parent node could not be found.");
                continue;
            }

            if (currentNode.nodes.Any(node => node is CraftNode craftNode && craftNode.action == TreeAction.Craft))
            {
                InternalLogger.Error($"Cannot add tab: {customNode.Name} to {customNode.Scheme} at {string.Join("/", customNode.Path)} as it is being added to a parent node that contains crafting nodes. {string.Join(", ", currentNode.nodes.Where(node => node is CraftNode craftNode && craftNode.action == TreeAction.Craft).Select(x => x.id))} ");
                continue;
            }

            // Add the new tab node.
            currentNode.AddNode(new TreeNode[]
            {
                new CraftNode(customNode.Name, TreeAction.Expand, TechType.None)
            });
            InternalLogger.Debug($"Added tab: {customNode.Name} to {customNode.Scheme} at {string.Join("/", customNode.Path)}");
        }
    }

    private static void PatchNodes(ref CraftTree tree, CraftTree.Type type)
    {
        List<CraftingNode> customNodes = CraftingNodes.TryGetValue(type, out customNodes) ? customNodes : new List<CraftingNode>();
        foreach (var customNode in customNodes)
        {
            if (!TraverseTree(tree.nodes, customNode.Path, out var currentNode))
            {
                InternalLogger.Error($"Cannot add Crafting node: {customNode.TechType.AsString()} to {customNode.Scheme} at {string.Join("/", customNode.Path)} as the parent node could not be found.");
                continue;
            }

            if (currentNode.nodes.Any(x => x is CraftNode craftNode && craftNode.action == TreeAction.Expand))
            {
                InternalLogger.Warn($"Cannot add Crafting node: {customNode.TechType.AsString()} as it is being added to {currentNode.id} that contains Tab nodes. {string.Join(", ", currentNode.nodes.Where(node => node is CraftNode craftNode && craftNode.action == TreeAction.Expand).Select(x => x.id))}");
                InternalLogger.Warn($"Adding to Fallback {FallbackTabNode} node in tree root.");

                if (!TraverseTree(tree.nodes, new[] { FallbackTabNode + customNode.Scheme }, out currentNode))
                {
                    InternalLogger.Error($"Cannot add Crafting node: {customNode.TechType.AsString()} to {customNode.Scheme} at {string.Join("/", customNode.Path)} as the fallback node could not be found.");
                    continue;
                }
            }

            // Add the node.
            currentNode.AddNode(new TreeNode[]
            {
                new CraftNode(customNode.TechType.AsString(false), TreeAction.Craft, customNode.TechType)
            });
            InternalLogger.Debug($"Added Crafting node: {customNode.TechType.AsString()} to {customNode.Scheme} at {string.Join("/", customNode.Path)}");
        }
    }

    /// <summary>
    /// This method can be used to both remove single child nodes, thus removing one recipe from the tree.
    /// Or it can remove entire tabs at once, removing the tab and all the recipes it contained in one go.
    /// </summary>
    private static void RemoveNodes(ref CraftTree tree, ref List<Node> nodesToRemove)
    {
        var safelist = new List<Node>(nodesToRemove).OrderByDescending(x => x.Path.Length); // Sort by path length so we remove the deepest nodes first.

        foreach (Node nodeToRemove in safelist)
        {
            if (nodeToRemove.Path == null || nodeToRemove.Path.Length == 0)
            {
                tree.nodes.root.Clear(); // Remove all child nodes (if any)
                nodesToRemove.Remove(nodeToRemove); // Remove the node from the list of nodes to remove
                InternalLogger.Debug($"Removed all nodes from {nodeToRemove.Scheme} tree.");
                continue;
            }

            // Get the names of each node in the path to traverse tree until we reach the node we want.
            if (!TraverseTree(tree.nodes, nodeToRemove.Path, out var currentNode))
            {
                InternalLogger.Warn($"Skipped removing craft tree node in {nameof(RemoveNodes)} for '{nodeToRemove.Scheme}' at '{string.Join("/", nodeToRemove.Path)}'. Could not find the node.");
                continue;
            }

            if (currentNode.parent == null) // should never happen but just in case...
            {
                InternalLogger.Warn($"Skipped removing craft tree node in {nameof(RemoveNodes)} for '{nodeToRemove.Scheme}' at '{string.Join("/", nodeToRemove.Path)}'. Could not identify the parent node.");
                continue;
            }

            currentNode.Clear(); // Remove all child nodes (if any)
            currentNode.parent.RemoveNode(currentNode); // Remove the node from its parent
            nodesToRemove.Remove(nodeToRemove); // Remove the node from the list of nodes to remove
            InternalLogger.Debug($"Removed node from {nodeToRemove.Scheme} tree at {string.Join("/", nodeToRemove.Path)}.");
        }
    }

    private static bool TraverseTree(TreeNode nodes, string[] path, out TreeNode currentNode)
    {
        currentNode = nodes;

        // Loop through the path provided by the node.
        // Get the node for the last path.
        for (int i = 0; i < path.Length; i++)
        {
            var currentPath = path[i];
            var lastnode = currentNode;
            var node2 = currentNode[currentPath];

            if (node2 != null)
            {
                currentNode = node2;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}