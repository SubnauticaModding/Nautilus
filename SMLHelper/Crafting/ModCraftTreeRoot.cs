namespace SMLHelper.Crafting
{
    using System;
    using System.Collections.Generic;
    using SMLHelper.Assets;
    using SMLHelper.Utility;
    using System.Linq;
    using SMLHelper.Handlers;
    
    /// <summary>
    /// The root node of a CraftTree. The whole tree starts here.<para/>
    /// Build up your custom crafting tree from this root node using the AddCraftingNode and AddTabNode methods.<br/>
    /// This tree will be automatically patched into the game.<para/>
    /// For more advanced usage, you can replace the default value of <see cref="CraftTreeCreation"/> with your own custom function.        
    /// </summary>    
    /// <seealso cref="ModCraftTreeLinkingNode" />
    public class ModCraftTreeRoot: ModCraftTreeLinkingNode
    {
        private readonly string _schemeAsString;
        private readonly CraftTree.Type _scheme;

        private const string RootNode = "root";
        internal Dictionary<string, ModCraftTreeLinkingNode> CraftTreeLinkingNodes { get; } = new();

        internal override string SchemeAsString => _schemeAsString;
        internal override CraftTree.Type Scheme => _scheme;

        internal ModCraftTreeRoot(CraftTree.Type scheme, string schemeAsString)
            : base("Root", TreeAction.None, TechType.None)
        {
            _schemeAsString = schemeAsString;
            _scheme = scheme;
            HasCustomTrees = true;
            CraftTreeLinkingNodes.Add(RootNode, this);
            CraftTreeCreation = () => new CraftTree(_schemeAsString, CraftNode);
        }

        /// <summary>
        /// Dynamically creates the CraftTree object for this crafting tree.
        /// The CraftNode objects were created and linked as the classes of the ModCraftTreeFamily were created and linked.
        /// </summary>
        internal CraftTree CustomCraftingTree => CraftTreeCreation.Invoke();

        /// <summary>
        /// Populates a new ModCraftTreeRoot from a CraftNode tree.
        /// </summary>
        /// <param name="tree">The tree to create the ModCraftTreeRoot from.</param>
        /// <param name="root"></param>
        internal static void CreateFromExistingTree(CraftNode tree, ref ModCraftTreeLinkingNode root)
        {
            foreach(CraftNode node in tree)
            {
                if(node.action == TreeAction.Expand)
                {
                    ModCraftTreeTab tab = root.AddTabNode(node.id);
                    ModCraftTreeLinkingNode thing = (ModCraftTreeLinkingNode)tab;
                    CreateFromExistingTree(node, ref thing);
                }

                if(node.action == TreeAction.Craft)
                {
                    TechTypeExtensions.FromString(node.id, out TechType techType, false);

                    root.AddCraftingNode(techType);
                }
            }
        }

        /// <summary>
        /// The craft tree creation function.<br/>
        /// Default implementaion returns a new <see cref="CraftTree"/> instantiated with <see cref="SchemeAsString"/> and the root <see cref="CraftNode"/>.<para/>
        /// You can replace this function with your own to have more control of the crafting tree when it is being created.
        /// </summary>
        public Func<CraftTree> CraftTreeCreation;

#if SUBNAUTICA
        /// <summary>
        /// Adds a new tab node to the custom crafting tree of this fabricator.
        /// </summary>
        /// <param name="tabId">The internal ID for the tab node.</param>
        /// <param name="displayText">The in-game text shown for the tab node. If null or empty, this will use the language line "{CraftTreeName}_{<paramref name="tabId"/>}" instead.</param>
        /// <param name="tabSprite">The sprite used for the tab node.</param>
        /// <param name="language">The language for the display name. Defaults to English.</param>
        /// <param name="parentTabId">Optional. The parent tab of this tab.<para/>
        /// When this value is null, the tab will be added to the root of the craft tree.</param>
        public ModCraftTreeRoot AddTabNode(string tabId, string displayText, Atlas.Sprite tabSprite, string language = "English", string parentTabId = null)
        {
            if(string.IsNullOrWhiteSpace(tabId))
                throw new ArgumentNullException($"{this.SchemeAsString} tried to add a tab without an id! tabid cannot be null or empty spaces!");

            ModCraftTreeLinkingNode parentTab;
            if(!CraftTreeLinkingNodes.TryGetValue(parentTabId ?? RootNode, out parentTab))
                parentTab = CraftTreeLinkingNodes[RootNode];

            if(!parentTab.ChildNodes.Any(node => node.Action == TreeAction.Craft))
            {
                ModCraftTreeTab tab = parentTab.AddTabNode(tabId, displayText, tabSprite, language);
                CraftTreeLinkingNodes[tabId] = tab;
            }
            else
            {
                InternalLogger.Error($"Cannot add tab: {tabId} as it is being added to a parent node that contains crafting nodes.");
            }
            return this;
        }
#endif

        /// <summary>
        /// Adds a new tab node to the custom crafting tree of this fabricator.
        /// </summary>
        /// <param name="tabId">The internal ID for the tab node.</param>
        /// <param name="displayText">The in-game text shown for the tab node. If null or empty, this will use the language line "{CraftTreeName}_{<paramref name="tabId"/>}" instead.</param>
        /// <param name="tabSprite">The sprite used for the tab node.</param>
        /// <param name="language">The language for the display name. Defaults to English.</param>
        /// <param name="parentTabId">Optional. The parent tab of this tab.<para/>
        /// When this value is null, the tab will be added to the root of the craft tree.</param>
        public ModCraftTreeRoot AddTabNode(string tabId, string displayText, UnityEngine.Sprite tabSprite, string language = "English", string parentTabId = null)
        {
            if(string.IsNullOrWhiteSpace(tabId))
                throw new ArgumentNullException($"{this.SchemeAsString} tried to add a tab without an id! tabid cannot be null or empty spaces!");

            ModCraftTreeLinkingNode parentTab;
            if(!CraftTreeLinkingNodes.TryGetValue(parentTabId ?? RootNode, out parentTab))
                parentTab = CraftTreeLinkingNodes[RootNode];

            if(!parentTab.ChildNodes.Any(node => node.Action == TreeAction.Craft))
            {
                ModCraftTreeTab tab = parentTab.AddTabNode(tabId, displayText, tabSprite, language);
                CraftTreeLinkingNodes[tabId] = tab;
            }
            else
            {
                InternalLogger.Error($"Cannot add tab: {tabId} as it is being added to a parent node that contains crafting nodes.");
            }
            return this;
        }

        /// <summary>
        /// Adds a new crafting node to the custom crafting tree of this fabricator.
        /// </summary>
        /// <param name="techType">The item to craft.</param>
        /// <param name="parentTabId">Optional. The parent tab of this craft node.<para/>
        /// When this value is null, the craft node will be added to the root of the craft tree.</param>
        public ModCraftTreeRoot AddCraftNode(TechType techType, string parentTabId = null)
        {
            ModCraftTreeLinkingNode parentTab;
            if(!CraftTreeLinkingNodes.TryGetValue(parentTabId ?? RootNode, out parentTab))
                parentTab = CraftTreeLinkingNodes[RootNode];

            if(!parentTab.ChildNodes.Any(node => node.Action == TreeAction.Expand))
            {
                InternalLogger.Debug($"'{techType.AsString()}' will be added to the custom craft tree '{this.SchemeAsString}'");
                parentTab.AddCraftingNode(techType);
            }
            else
            {
                InternalLogger.Error($"Cannot add crafting node: {techType} as it is being added to {parentTab.Name} that contains tab nodes. {string.Join(", ",parentTab.ChildNodes.Where(node=> node.Action == TreeAction.Expand).Select(x=>x.Name))} ");
            }
            return this;
        }

        /// <summary>
        /// Safely attempts to add a new crafting node to the custom crafting tree of this fabricator.<para/>
        /// If the modded TechType is not found, the craft node will not be added.
        /// </summary>
        /// <param name="moddedTechType">The modded item to craft.</param>
        /// <param name="parentTabId">Optional. The parent tab of this craft node.<para/>
        /// When this value is null, the craft node will be added to the root of the craft tree.</param>
        public ModCraftTreeRoot AddCraftNode(string moddedTechType, string parentTabId = null)
        {
            if(EnumHandler.TryGetValue(moddedTechType, out TechType techType))
            {
                ModCraftTreeLinkingNode parentTab;
                if(!CraftTreeLinkingNodes.TryGetValue(parentTabId ?? RootNode, out parentTab))
                    parentTab = CraftTreeLinkingNodes[RootNode];

                if(!parentTab.ChildNodes.Any(node => node.Action == TreeAction.Expand))
                {
                    InternalLogger.Debug($"'{techType.AsString()}' will be added to the custom craft tree '{this.SchemeAsString}'");
                    parentTab.AddCraftingNode(techType);
                }
                else
                {
                    InternalLogger.Error($"Cannot add crafting node: {techType} as it is being added to a parent node that contains tab nodes.");
                }
            }
            else
            {
                InternalLogger.Info($"Did not find a TechType value for '{moddedTechType}' to add to the custom craft tree '{this.SchemeAsString}'");
            }
            return this;
        }

        /// <summary>
        /// Gets the tab node at the specified path from the root.
        /// </summary>
        /// <param name="stepsToTab">
        /// <para>The steps to the target tab.</para>
        /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
        /// <para>Do not include "root" in this path.</para>
        /// </param>
        /// <returns>If the specified tab node is found, returns that <see cref="ModCraftTreeTab"/>; Otherwise, returns null.</returns>
        public ModCraftTreeTab GetTabNode(params string[] stepsToTab)
        {
            ModCraftTreeTab tab = base.GetTabNode(stepsToTab[0]);

            for(int i = 1; i < stepsToTab.Length && tab != null; i++)
            {
                tab = tab.GetTabNode(stepsToTab[i]);
            }

            return tab;
        }

        /// <summary>
        /// Gets the node at the specified path from the root.
        /// </summary>
        /// <param name="stepsToNode">
        /// <para>The steps to the target tab.</para>
        /// <para>These must match the id value of the CraftNode in the crafting tree you're targeting.</para>
        /// <para>Do not include "root" in this path.</para>
        /// </param>
        /// <returns>If the specified tab node is found, returns that <see cref="ModCraftTreeNode"/>; Otherwise, returns null.</returns>
        public ModCraftTreeNode GetNode(params string[] stepsToNode)
        {
            if(stepsToNode.Length == 1)
            {
                return base.GetNode(stepsToNode[0]);
            }

            int stepCountToTab = stepsToNode.Length - 1;

            string nodeID = stepsToNode[stepCountToTab];
            string[] stepsToTab = new string[stepCountToTab];
            Array.Copy(stepsToNode, stepsToTab, stepCountToTab);

            ModCraftTreeTab tab = GetTabNode(stepsToTab);

            return tab?.GetNode(nodeID);
        }
    }
}
