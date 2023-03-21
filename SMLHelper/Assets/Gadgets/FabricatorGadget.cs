using System;
using System.Collections.Generic;
using SMLHelper.Crafting;
using SMLHelper.Handlers;
using SMLHelper.Utility;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using Sprite = UnityEngine.Sprite;
#endif

namespace SMLHelper.Assets.Gadgets;

/// <summary>
/// Represents a craft tree/fabricator gadget.
/// </summary>
public class FabricatorGadget : Gadget
{
    private const string RootNode = "root";
    private readonly Dictionary<string, ModCraftTreeLinkingNode> _craftTreeLinkingNodes = new();
    private readonly List<Action> _orderedCraftTreeActions = new();

    /// <summary>
    /// The ID value for your custom craft tree.
    /// </summary>
    public CraftTree.Type CraftTreeType { get; private set; }

    /// <summary>
    ///  The root node of the crafting tree.
    /// </summary>
    public ModCraftTreeRoot Root { get; private set; }

    /// <summary>
    /// Constructs a fabricator gadget.
    /// </summary>
    /// <param name="prefab"><inheritdoc cref="Gadget(ICustomPrefab)"/></param>
    public FabricatorGadget(ICustomPrefab prefab) : base(prefab)
    {
        CraftTreeType = EnumHandler.AddEntry<CraftTree.Type>(prefab.Info.ClassID).CreateCraftTreeRoot(out var root);
        Root = root;
        _craftTreeLinkingNodes.Add(RootNode, root);
    }

    /// <summary>
    /// Adds a new tab node to the custom crafting tree of this fabricator.
    /// </summary>
    /// <param name="tabId">The internal ID for the tab node.</param>
    /// <param name="displayText">The in-game text shown for the tab node. If null or empty, this will use the language line "{CraftTreeTypeName}_{<paramref name="tabId"/>}" instead.</param>
    /// <param name="tabIcon">The sprite used for the tab node.</param>
    /// <param name="language">The language for the display name. Defaults to English.</param>
    /// <param name="parentTabId">Optional. The parent tab of this tab.
    /// When this value is null, the tab will be added to the root of the craft tree.</param>
    public FabricatorGadget AddTabNode(string tabId, string displayText, Sprite tabIcon, string language = "English", string parentTabId = null)
    {
        _orderedCraftTreeActions.Add(() =>
        {
            var parentNode = _craftTreeLinkingNodes[parentTabId ?? RootNode];
            var tab = parentNode.AddTabNode(tabId, displayText, tabIcon, language);
            _craftTreeLinkingNodes[tabId] = tab;
        });

        return this;
    }

    /// <summary>
    /// Adds a new crafting node to the custom crafting tree of this fabricator.
    /// </summary>
    /// <param name="techType">The item to craft.</param>
    /// <param name="parentTabId">Optional. The parent tab of this craft node.<para/>
    /// When this value is null, the craft node will be added to the root of the craft tree.</param>
    public FabricatorGadget AddCraftNode(TechType techType, string parentTabId = null)
    {
        InternalLogger.Debug($"'{techType.AsString()}' will be added to the custom craft tree '{prefab.Info.ClassID}'");
        _orderedCraftTreeActions.Add(() =>
        {
            ModCraftTreeLinkingNode parentTab = _craftTreeLinkingNodes[parentTabId ?? RootNode];
            parentTab.AddCraftingNode(techType);
        });

        return this;
    }
    
    /// <summary>
    /// Safely attempts to add a new crafting node to the custom crafting tree of this fabricator.<para/>
    /// If the modded TechType is not found, the craft node will not be added.
    /// </summary>
    /// <param name="moddedTechType">The modded item to craft.</param>
    /// <param name="parentTabId">Optional. The parent tab of this craft node.<para/>
    /// When this value is null, the craft node will be added to the root of the craft tree.</param>
    public FabricatorGadget AddCraftNode(string moddedTechType, string parentTabId = null)
    {
        InternalLogger.Debug($"'{moddedTechType}' will be added to the custom craft tree '{prefab.Info.ClassID}'");
        _orderedCraftTreeActions.Add(() =>
        {
            if (EnumHandler.TryGetValue(moddedTechType, out TechType techType))
            {
                ModCraftTreeLinkingNode parentTab = _craftTreeLinkingNodes[parentTabId ?? RootNode];
                parentTab.AddCraftingNode(techType);
            }
            else
            {
                InternalLogger.Info($"Did not find a TechType value for '{moddedTechType}' to add to the custom craft tree '{prefab.Info.ClassID}'");
            }
        });

        return this;
    }

    /// <inheritdoc/>
    protected internal override void Build()
    {
        foreach (var action in _orderedCraftTreeActions)
        {
            action.Invoke();
        }
    }
}