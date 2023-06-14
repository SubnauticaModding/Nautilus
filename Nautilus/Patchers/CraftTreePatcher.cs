using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Crafting;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class CraftTreePatcher
{
    #region Internal Fields

    internal static Dictionary<CraftTree.Type, ModCraftTreeRoot> CustomTrees = new();
    internal static List<Node> NodesToRemove = new();
    internal static List<CraftingNode> CraftingNodes = new();
    internal static List<TabNode> TabNodes = new();

    #endregion

    #region Patches

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(CraftTreePatcher));

        InternalLogger.Log($"CraftTreePatcher is done.", LogLevel.Debug);
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
        foreach (TabNode tab in customTabs)
        {
            // Wrong crafter, skip.
            if (tab.Scheme != scheme)
            {
                continue;
            }

            TreeNode currentNode = default;
            currentNode = nodes;

            // Patch into game's CraftTree.
            for (int i = 0; i < tab.Path.Length; i++)
            {
                string currentPath = tab.Path[i];
                InternalLogger.Log("Tab Current Path: " + currentPath + " Tab: " + tab.Name + " Crafter: " + tab.Scheme.ToString(), LogLevel.Debug);

                TreeNode node = currentNode[currentPath];

                // Reached the end of the line.
                if (node != null)
                {
                    currentNode = node;
                }
                else
                {
                    break;
                }
            }

            if(currentNode.nodes.Any(node=> node is CraftNode craftNode && craftNode.action == TreeAction.Craft))
            {
                InternalLogger.Error($"Cannot add tab: {tab.Name} as it is being added to a parent node that contains crafting nodes.");
                continue;
            }

            // Add the new tab node.
            CraftNode newNode = new(tab.Name, TreeAction.Expand, TechType.None);
            currentNode.AddNode(new TreeNode[]
            {
                newNode
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

            // Have to do this to make sure C# shuts up.
            TreeNode node = default;
            node = nodes;

            // Loop through the path provided by the node.
            // Get the node for the last path.
            for (int i = 0; i < customNode.Path.Length; i++)
            {
                string currentPath = customNode.Path[i];
                TreeNode currentNode = node[currentPath];

                if (currentNode != null)
                {
                    node = currentNode;
                }
                else
                {
                    break;
                }
            }

            if(node.nodes.Any(x => x is CraftNode craftNode && craftNode.action == TreeAction.Expand))
            {
                InternalLogger.Error($"Cannot Crafting node: {customNode.TechType.AsString()} as it is being added to {node.id} that contains Tab nodes.");
                continue;
            }

            // Add the node.
            node.AddNode(new TreeNode[]
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
            TreeNode currentNode = default;
            currentNode = nodes;

            // Travel the path down the tree.
            string currentPath = null;
            for (int step = 0; step < nodeToRemove.Path.Length; step++)
            {
                currentPath = nodeToRemove.Path[step];
                if (step > nodeToRemove.Path.Length)
                {
                    break;
                }

                currentNode = currentNode[currentPath];
            }

            // Safty checks.
            if (currentNode != null && currentNode.id == currentPath)
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

    #endregion
}