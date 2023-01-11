namespace SMLHelper.Handlers
{
    using System;
    using Crafting;
    using Patchers;
    using Utility;

    /// <summary>
    /// A handler class for creating and editing of crafting trees.
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
        /// </param>        
        public static void AddCraftingNode(CraftTree.Type craftTree, TechType craftingItem, params string[] stepsToTab)
        {
            CraftTreePatcher.CraftingNodes.Add(new CraftingNode(stepsToTab, craftTree, craftingItem));
        }

        /// <summary>
        /// Adds a new crafting node to the root of the specified crafting tree
        /// </summary>
        /// <param name="craftTree">The target craft tree to edit.</param>
        /// <param name="craftingItem">The item to craft.</param>

        public static void AddCraftingNode(CraftTree.Type craftTree, TechType craftingItem)
        {
            CraftTreePatcher.CraftingNodes.Add(new CraftingNode(new string[0], craftTree, craftingItem));
        }

#if SUBNAUTICA
        /// <summary>
        /// Adds a new tab node to the root of the specified crafting tree.
        /// </summary>
        /// <param name="craftTree">The target craft tree to edit.</param>
        /// <param name="name">The ID of the tab node. Must be unique!</param>
        /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab.</param>
        /// <param name="sprite">The sprite of the tab.</param>        
        public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, Atlas.Sprite sprite)
        {
            string modName = ReflectionHelper.CallingAssemblyNameByStackTrace();
            CraftTreePatcher.TabNodes.Add(new TabNode(new string[0], craftTree, sprite, modName, name, displayName));
        }

        /// <summary>
        /// Adds a new tab node to the root of the specified crafting tree.
        /// </summary>
        /// <param name="craftTree">The target craft tree to edit.</param>
        /// <param name="name">The ID of the tab node. Must be unique!</param>
        /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab.</param>
        /// <param name="sprite">The sprite of the tab.</param>

        public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, UnityEngine.Sprite sprite)
        {
            string modName = ReflectionHelper.CallingAssemblyNameByStackTrace();
            CraftTreePatcher.TabNodes.Add(new TabNode(new string[0], craftTree, new Atlas.Sprite(sprite), modName, name, displayName));
        }

        /// <summary>
        /// Adds a new tab node to the root of the specified crafting tree, at the specified tab location.
        /// </summary>
        /// <param name="craftTree">The target craft tree to edit.</param>
        /// <param name="name">The ID of the tab node. Must be unique!</param>
        /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab.</param>
        /// <param name="sprite">The sprite of the tab.</param>
        /// <param name="stepsToTab">
        /// <para>The steps to the target tab.</para>
        /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
        /// <para>Do not include "root" in this path.</para>
        /// </param>        
        public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, Atlas.Sprite sprite, params string[] stepsToTab)
        {
            string modName = ReflectionHelper.CallingAssemblyNameByStackTrace();
            CraftTreePatcher.TabNodes.Add(new TabNode(stepsToTab, craftTree, sprite, modName, name, displayName));
        }

        /// <summary>
        /// Adds a new tab node to the root of the specified crafting tree, at the specified tab location.
        /// </summary>
        /// <param name="craftTree">The target craft tree to edit.</param>
        /// <param name="name">The ID of the tab node. Must be unique!</param>
        /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab.</param>
        /// <param name="sprite">The sprite of the tab.</param>
        /// <param name="stepsToTab">
        /// <para>The steps to the target tab.</para>
        /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
        /// <para>Do not include "root" in this path.</para>
        /// </param>        
        public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, UnityEngine.Sprite sprite, params string[] stepsToTab)
        {      
            string modName = ReflectionHelper.CallingAssemblyNameByStackTrace();
            CraftTreePatcher.TabNodes.Add(new TabNode(stepsToTab, craftTree, new Atlas.Sprite(sprite), modName, name, displayName));
        }

#elif BELOWZERO
        /// <summary>
        /// Adds a new tab node to the root of the specified crafting tree.
        /// </summary>
        /// <param name="craftTree">The target craft tree to edit.</param>
        /// <param name="name">The ID of the tab node. Must be unique!</param>
        /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab.</param>
        /// <param name="sprite">The sprite of the tab.</param>        
        public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, Sprite sprite)
        {
            string modName = ReflectionHelper.CallingAssemblyNameByStackTrace();
            CraftTreePatcher.TabNodes.Add(new TabNode(new string[0], craftTree, sprite, modName, name, displayName));
        }

        /// <summary>
        /// Adds a new tab node to the root of the specified crafting tree, at the specified tab location.
        /// </summary>
        /// <param name="craftTree">The target craft tree to edit.</param>
        /// <param name="name">The ID of the tab node. Must be unique!</param>
        /// <param name="displayName">The display name of the tab, which will show up when you hover your mouse on the tab.</param>
        /// <param name="sprite">The sprite of the tab.</param>
        /// <param name="stepsToTab">
        /// <para>The steps to the target tab.</para>
        /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
        /// <para>Do not include "root" in this path.</para>
        /// </param>        
        public static void AddTabNode(CraftTree.Type craftTree, string name, string displayName, Sprite sprite, params string[] stepsToTab)
        {
            string modName = ReflectionHelper.CallingAssemblyNameByStackTrace();
            CraftTreePatcher.TabNodes.Add(new TabNode(stepsToTab, craftTree, sprite, modName, name, displayName));
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
        /// </param>

        public static void RemoveNode(CraftTree.Type craftTree, params string[] stepsToNode)
        {
            CraftTreePatcher.NodesToRemove.Add(new Node(stepsToNode, craftTree));
        }
    }
}
