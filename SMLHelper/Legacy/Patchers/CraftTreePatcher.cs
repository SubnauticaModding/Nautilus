namespace SMLHelper.Patchers
{
    using System;
    using System.Collections.Generic;
    using CraftTreePatcher2 = SMLHelper.V2.Patchers.CraftTreePatcher;
    using V2.Handlers;
    using V2.Crafting;

    [Obsolete("Use SMLHelper.V2 instead.")]
    public class CraftTreePatcher
    {
        [Obsolete("Use SMLHelper.V2 instead.")]
        public static List<CustomCraftTab> customTabs = new List<CustomCraftTab>();

        [Obsolete("Use SMLHelper.V2 instead.")]
        public static List<CustomCraftNode> customNodes = new List<CustomCraftNode>();

        [Obsolete("Use SMLHelper.V2 instead.")]
        public static List<CraftNodeToScrub> nodesToRemove = new List<CraftNodeToScrub>();

        internal static Dictionary<CraftTree.Type, SMLHelper.CustomCraftTreeRoot> CustomTrees = new Dictionary<CraftTree.Type, SMLHelper.CustomCraftTreeRoot>();

        [Obsolete("Use SMLHelper.V2 instead.")]
        public static Dictionary<string, TechType> customCraftNodes = new Dictionary<string, TechType>();

        internal static void Patch()
        {
            // Old custom tabs added through new CustomCraftTreeNode classes
            foreach (var tab in customTabs)
            {
                if (tab == null) continue;

                var root = CraftTreeHandler.GetExistingTree(tab.Scheme);

                if (root == null) continue;

                if (!tab.Path.Contains("/")) // Added to the root
                {
                    root.AddTabNode(tab.Path, tab.Name, tab.Sprite.Sprite);
                }
                else // Added under an existing tab
                {
                    var path = tab.Path.SplitByChar('/');
                    var tabName = path[path.Length - 1]; // Last
                    var relativeRoot = path[path.Length - 2]; // Last - 1

                    root.GetTabNode(relativeRoot).AddTabNode(tabName, tab.Name, tab.Sprite.Sprite);
                }
            }

            // Old custom craft nodes added through new CustomCraftTreeNode classes

            var craftNodes = new List<CustomCraftNode>();

            foreach (var customNode in customNodes)
                craftNodes.Add(new CustomCraftNode(customNode.TechType, customNode.Scheme, customNode.Path));

            foreach (var customNode in customCraftNodes)
                craftNodes.Add(new CustomCraftNode(customNode.Value, CraftTree.Type.Fabricator, customNode.Key));

            foreach (var node in craftNodes)
            {
                if (node == null) continue;

                var root = CraftTreeHandler.GetExistingTree(node.Scheme);

                if (root == null) continue;

                if (!node.Path.Contains("/")) // Added to the root
                {
                    root.AddCraftingNode(node.TechType);
                }
                else // Added under an existing tab
                {
                    var path = node.Path.SplitByChar('/');
                    var relativeRoot = path[path.Length - 2]; // Last - 1

                    root.GetTabNode(relativeRoot).AddCraftingNode(node.TechType);
                }
            }

            // Old node scrubbing handled through new CustomCraftTreeNode classes
            foreach (var node in nodesToRemove)
            {
                if (node == null) continue;

                var root = CraftTreeHandler.GetExistingTree(node.Scheme);

                if (root == null) continue;

                if (!node.Path.Contains("/")) // Removed from the root
                {
                    root.GetNode(node.Path).RemoveNode();
                }
                else // Removed from an existing tab
                {
                    var path = node.Path.SplitByChar('/');
                    var nodeName = path[path.Length - 1]; // Last

                    root.GetTabNode(nodeName).RemoveNode();
                }
            }

            CustomTrees.ForEach(x => CraftTreePatcher2.CustomTrees.Add(x.Key, x.Value.GetV2RootNode()));

            V2.Logger.Log("Old CraftTreePatcher is done.");
        }
    }
}
