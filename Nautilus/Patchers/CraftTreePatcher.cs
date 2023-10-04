namespace Nautilus.Patchers;

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
        ReorganizeWorkbench();

        harmony.PatchAll(typeof(CraftTreePatcher));
        InternalLogger.Log($"CraftTreePatcher is done.", LogLevel.Debug);
    }

    private static void ReorganizeWorkbench()
    {
        CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, VanillaRoot, "Modification Station", SpriteManager.Get(TechType.Workbench));
        var workbench = CraftTree.WorkbenchScheme().root;

        var removedNodes = new List<CraftNode>();

        foreach (var node in workbench.nodes.Cast<CraftNode>())
        {
            CraftTreeHandler.RemoveNode(CraftTree.Type.Workbench, new[] { node.id });
            removedNodes.Add(node);
        }

        foreach (var node in removedNodes)
        {
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Workbench, node.techType0, new[] { VanillaRoot });
        }
        InternalLogger.Debug($"Reorganized Workbench nodes into new {VanillaRoot} tab.");
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
        RemoveNodes(ref __result, NodesToRemove, type);
        AddCustomTabs(ref __result, TabNodes, type);
        PatchNodes(ref __result, CraftingNodes, type);
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

            if (currentNode.nodes.Any(node=> node is CraftNode craftNode && craftNode.action == TreeAction.Craft))
            {
                InternalLogger.Error($"Cannot add tab: {customNode.Name} as it is being added to a parent node that contains crafting nodes. {string.Join(", ", currentNode.nodes.Where(node=> node is CraftNode craftNode && craftNode.action == TreeAction.Craft).Select(x => x.id))} ");
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

                currentNode = TraverseTree(nodes, new[] { FallbackTabNode });
                if (currentNode.isRoot)
                {
                    currentNode.AddNode(new TreeNode[]
                    {
                        new CraftNode(FallbackTabNode, TreeAction.Expand, TechType.None)
                    });
                    currentNode = TraverseTree(nodes, new[] { FallbackTabNode });
                }
                continue;
            }

            // Add the node.
            currentNode.AddNode(new TreeNode[]
            {
                new CraftNode(customNode.TechType.AsString(false), TreeAction.Craft, customNode.TechType)
            });
        }
    }

    private static void RemoveNodes(ref CraftNode nodes, List<Node> nodesToRemove, CraftTree.Type scheme)
    {
        // This method can be used to both remove single child nodes, thus removing one recipe from the tree.
        // Or it can remove entire tabs at once, removing the tab and all the recipes it contained in one go.

        foreach (Node nodeToRemove in nodesToRemove)
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
                    InternalLogger.Warn($"Skipped removing craft tree node in {nameof(RemoveNodes)} for '{scheme}'. Could not identify the parent node.");
                }
                else
                {
                    currentNode.Clear(); // Remove all child nodes (if any)
                    currentNode.parent.RemoveNode(currentNode); // Remove the node from its parent
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
            string currentPath = path[i];
            TreeNode node2 = currentNode[currentPath];

            if (node2 != null)
            {
                currentNode = node2;
            }
            else
            {
                break;
            }
        }

        return currentNode;
    }

    #endregion
}