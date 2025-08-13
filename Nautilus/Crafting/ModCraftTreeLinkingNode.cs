using System.Collections.Generic;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nautilus.Crafting;

/// <summary>
/// Abstract class that provides methods for adding new nodes into the crafting tree.
/// </summary>
/// <seealso cref="ModCraftTreeNode" />
public abstract class ModCraftTreeLinkingNode : ModCraftTreeNode
{
    /// <summary>
    /// The child nodes linked bellow this node.
    /// </summary>
    public readonly List<ModCraftTreeNode> ChildNodes = new();

    internal ModCraftTreeLinkingNode(string name, TreeAction action, TechType techType)
        : base(name, action, techType)
    {
    }
    /// <summary>
    /// Creates a new tab node for the crafting tree and links it to the calling node.
    /// </summary>
    /// <param name="nameID">The name/ID of this node.</param>
    /// <param name="displayText">The hover text to display in-game. If null or empty, this will use the language line "{CraftTreeName}_{<paramref name="nameID"/>}" instead.</param>
    /// <param name="sprite">The custom sprite to display on this tab node.</param>
    /// <param name="language">The language for the display name. Defaults to English.</param>
    /// <returns>A new tab node linked to the root node and ready to use.</returns>
    public ModCraftTreeTab AddTabNode(string nameID, string displayText, Sprite sprite, string language = "English")
    {
        ModCraftTreeTab tabNode = new(nameID, displayText, language, sprite);
        tabNode.LinkToParent(this);

        ChildNodes.Add(tabNode);

        return tabNode;
    }

    /// <summary>
    /// Creates a new tab node for the crafting tree and links it to the calling node.
    /// </summary>
    /// <remarks>
    /// Please note that this method will NOT set the language lines for the node and you must do it yourself.
    /// </remarks>
    /// <param name="nameID">The name/ID of this node.</param>
    /// <returns>A new tab node linked to the root node and ready to use.</returns>
    public ModCraftTreeTab AddTabNode(string nameID)
    {
        ModCraftTreeTab tabNode = new(nameID);
        tabNode.LinkToParent(this);

        ChildNodes.Add(tabNode);

        return tabNode;
    }

    /// <summary>
    /// Gets the tab from the calling node.
    /// </summary>
    /// <param name="nameID">The name id of the tab to get.</param>
    /// <returns></returns>
    public ModCraftTreeTab GetTabNode(string nameID)
    {
        foreach (var node in ChildNodes)
        {
            if (node == null)
            {
                continue;
            }

            if (node.Name == nameID && node.Action == TreeAction.Expand && node is ModCraftTreeTab tab)
            {
                return tab;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the crafting node from the calling node.
    /// </summary>
    /// <param name="techType">The TechType whose node to get.</param>
    /// <returns></returns>
    public ModCraftTreeCraft GetCraftingNode(TechType techType)
    {
        foreach (var node in ChildNodes)
        {
            if (node == null)
            {
                continue;
            }

            if (node.TechType == techType && node.Action == TreeAction.Craft && node is ModCraftTreeCraft craftNode)
            {
                return craftNode;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the node associated with the ID specified. Used if you don't know whether node is a tab or a craft node.
    /// </summary>
    /// <param name="nameID"></param>
    /// <returns></returns>
    public ModCraftTreeNode GetNode(string nameID)
    {
        foreach (var node in ChildNodes)
        {
            if (node == null)
            {
                continue;
            }

            if (node.Name == nameID)
            {
                return node;
            }
        }

        return null;
    }

    /// <summary>
    /// Creates a new crafting node for the crafting tree and links it to the calling node.
    /// </summary>
    /// <param name="techType">The TechType to be crafted.</param>
    public void AddCraftingNode(TechType techType)
    {
        Assert.AreNotEqual(TechType.None, techType, "Attempt to add TechType.None as a crafting node.");

        ModCraftTreeCraft craftNode = new(techType);
        craftNode.LinkToParent(this);

        ChildNodes.Add(craftNode);
    }

    /// <summary>
    /// Creates a collection of new crafting nodes for the crafting tree and links it to the calling node.
    /// </summary>
    /// <param name="techTypes">The TechTypes to be crafted.</param>
    public void AddCraftingNode(params TechType[] techTypes)
    {
        AddCraftingNode((IEnumerable<TechType>)techTypes);
    }

    /// <summary>
    /// Creates a collection of new crafting nodes for the crafting tree and links it to the calling node.
    /// </summary>
    /// <param name="techTypes">The TechTypes to be crafted.</param>
    public void AddCraftingNode(IEnumerable<TechType> techTypes)
    {
        foreach (TechType tType in techTypes)
        {
            AddCraftingNode(tType);
        }
    }

    /// <summary>
    /// <para>Creates a new crafting node for a modded item and links it to the calling node.</para>
    /// <para>If the modded item isn't present for the player, this call is safely ignored.</para>
    /// </summary>
    /// <param name="moddedTechTypeName">The internal name of the custom TechType to be crafted.</param>
    /// <remarks>
    /// If the player doesn't have the mod for this TechType installed, then nothing will happen.
    /// </remarks>
    public void AddModdedCraftingNode(string moddedTechTypeName)
    {
        var techTypeFound = EnumHandler.TryGetValue(moddedTechTypeName, out TechType techType);
        Assert.IsTrue(techTypeFound, $"Could not find {moddedTechTypeName} when trying to AddModdedCraftingNode!");
        
        AddCraftingNode(techType);
    }
}