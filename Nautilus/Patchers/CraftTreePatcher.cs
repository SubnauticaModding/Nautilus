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
    internal static List<Node> NodesToRemove = new();
    internal static List<CraftingNode> CraftingNodes = new();
    internal static List<TabNode> TabNodes = new();
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.GetTree))]
    private static bool GetTreePreFix(CraftTree.Type treeType, ref CraftTree __result)
    {
        if (CustomTrees.ContainsKey(treeType))
        {
            __result = CustomTrees[treeType].CustomCraftingTree;
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.Initialize))]
    private static void InitializePostFix()
    {
        if (CraftTree.initialized && !ModCraftTreeNode.Initialized)
        {
            foreach (CraftTree.Type cTreeKey in CustomTrees.Keys)
            {
                CraftTree customTree = CustomTrees[cTreeKey].CustomCraftingTree;

                MethodInfo addToCraftableTech = AccessTools.Method(typeof(CraftTree), nameof(CraftTree.AddToCraftableTech));

                addToCraftableTech.Invoke(null, new[] { customTree });
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.FabricatorScheme))]
    private static void FabricatorSchemePostfix(ref CraftNode __result)
    {
        PatchCraftTree(ref __result, CraftTree.Type.Fabricator);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.ConstructorScheme))]
    private static void ConstructorSchemePostfix(ref CraftNode __result)
    {
        PatchCraftTree(ref __result, CraftTree.Type.Constructor);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.WorkbenchScheme))]
    private static void WorkbenchSchemePostfix(ref CraftNode __result)
    {
        PatchCraftTree(ref __result, CraftTree.Type.Workbench);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.SeamothUpgradesScheme))]
    private static void SeamothUpgradesSchemePostfix(ref CraftNode __result)
    {
        PatchCraftTree(ref __result, CraftTree.Type.SeamothUpgrades);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.MapRoomSheme))]
    private static void MapRoomSchemePostfix(ref CraftNode __result)
    {
        PatchCraftTree(ref __result, CraftTree.Type.MapRoom);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.CyclopsFabricatorScheme))]
    private static void CyclopsFabricatorSchemePostfix(ref CraftNode __result)
    {
        PatchCraftTree(ref __result, CraftTree.Type.CyclopsFabricator);
    }

#if BELOWZERO
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CraftTree), nameof(CraftTree.SeaTruckFabricatorScheme))]
        private static void SeaTruckFabricatorSchemePostfix(ref CraftNode __result)
        {
            PatchCraftTree(ref __result, CraftTree.Type.SeaTruckFabricator);
        }
#endif

    #endregion

    #region Handling Nodes

    private static void PatchCraftTree(ref CraftNode __result, CraftTree.Type type)
    {
        var removals = NodesToRemove.Where(x => x.Scheme == type).ToList();
        RemoveNodes(ref __result, ref removals, type);

        var tabNodes = TabNodes.Where(x => x.Scheme == type).ToList();
        AddCustomTabs(ref __result, tabNodes, type);
        var craftingNodes = CraftingNodes.Where(x => x.Scheme == type).ToList();
        PatchNodes(ref __result, craftingNodes, type);

        // Remove any nodes added by mods that were marked for removal by other mods.
        RemoveNodes(ref __result, ref removals, type);
    }

    private static void AddCustomTabs(ref CraftNode nodes, List<TabNode> customTabs, CraftTree.Type scheme)
    {
        foreach (TabNode customNode in customTabs)
        {
            // Wrong crafter, skip.
            if (customNode.Scheme != scheme)
            {
                continue;
            }

            var currentNode = TraverseTree(nodes, customNode.Path);

            if (currentNode.nodes.Any(node => node is CraftNode craftNode && craftNode.action == TreeAction.Craft))
            {
                InternalLogger.Error($"Cannot add tab: {customNode.Name} as it is being added to a parent node that contains crafting nodes. {string.Join(", ", currentNode.nodes.Where(node => node is CraftNode craftNode && craftNode.action == TreeAction.Craft).Select(x => x.id))} ");
                continue;
            }

            // Add the new tab node.
            currentNode.AddNode(new TreeNode[]
            {
                new CraftNode(customNode.Name, TreeAction.Expand, TechType.None)
            });
        }
    }

    private static void PatchNodes(ref CraftNode nodes, List<CraftingNode> customNodes, CraftTree.Type scheme)
    {
        foreach (CraftingNode customNode in customNodes)
        {
            // Wrong crafter, just skip the node.
            if (customNode.Scheme != scheme)
            {
                continue;
            }

            var currentNode = TraverseTree(nodes, customNode.Path);

            if (currentNode.nodes.Any(x => x is CraftNode craftNode && craftNode.action == TreeAction.Expand))
            {
                InternalLogger.Warn($"Cannot add Crafting node: {customNode.TechType.AsString()} as it is being added to {currentNode.id} that contains Tab nodes. {string.Join(", ", currentNode.nodes.Where(node => node is CraftNode craftNode && craftNode.action == TreeAction.Expand).Select(x => x.id))}");
                InternalLogger.Warn($"Adding to Fallback {FallbackTabNode} node in tree root.");

                currentNode = TraverseTree(nodes, new[] { FallbackTabNode + scheme });
                if (currentNode.isRoot)
                    continue;
            }

            // Add the node.
            currentNode.AddNode(new TreeNode[]
            {
                new CraftNode(customNode.TechType.AsString(false), TreeAction.Craft, customNode.TechType)
            });
        }
    }

    private static void RemoveNodes(ref CraftNode nodes, ref List<Node> nodesToRemove, CraftTree.Type scheme)
    {
        var safelist = new List<Node>(nodesToRemove).OrderByDescending(x => x.Path.Length);
        // This method can be used to both remove single child nodes, thus removing one recipe from the tree.
        // Or it can remove entire tabs at once, removing the tab and all the recipes it contained in one go.

        foreach (Node nodeToRemove in safelist)
        {
            // Not for this fabricator. Skip.
            if (nodeToRemove.Scheme != scheme)
            {
                continue;
            }

            if (nodeToRemove.Path == null || nodeToRemove.Path.Length == 0)
            {
                InternalLogger.Warn($"An empty path in {nameof(RemoveNodes)} for '{scheme}' was skipped");
                continue;
            }

            // Get the names of each node in the path to traverse tree until we reach the node we want.
            var currentNode = TraverseTree(nodes, nodeToRemove.Path);

            // Safty checks.
            if (currentNode != null && currentNode.id == nodeToRemove.Path.Last())
            {
                if (currentNode.parent == null)
                {
                    InternalLogger.Warn($"Skipped removing craft tree node in {nameof(RemoveNodes)} for '{scheme}' at '{string.Join("/", nodeToRemove.Path)}'. Could not identify the parent node.");
                }
                else
                {
                    currentNode.Clear(); // Remove all child nodes (if any)
                    currentNode.parent.RemoveNode(currentNode); // Remove the node from its parent
                    nodesToRemove.Remove(nodeToRemove); // Remove the node from the list of nodes to remove
                }
            }
        }
    }

    private static TreeNode TraverseTree(TreeNode nodes, string[] path)
    {
        TreeNode currentNode = nodes;

        // Loop through the path provided by the node.
        // Get the node for the last path.
        for (int i = 0; i < path.Length; i++)
        {
            var currentPath = path[i];
            InternalLogger.Debug($"Traversing path: {currentPath}");
            var lastnode = currentNode;
            var node2 = currentNode[currentPath];

            if (node2 != null)
            {
                currentNode = node2;
            }
            else
            {
                InternalLogger.Warn($"Could not find node at path: {currentPath} in tree {lastnode.id}");
                // log what nodes are available
                InternalLogger.Warn($"Available nodes: {string.Join(", ", lastnode.nodes.Select(x => x.id))}");
                break;
            }
        }

        return currentNode;
    }

    #endregion
}