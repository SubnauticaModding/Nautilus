using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Crafting;

/// <summary>
/// A tab node of a CraftTree. Tab nodes help organize crafting nodes by grouping them into categories.
/// </summary>
/// <seealso cref="ModCraftTreeLinkingNode" />
public class ModCraftTreeTab : ModCraftTreeLinkingNode
{
    private readonly string _displayText;
    private readonly string _language;
    private readonly Sprite _uSprite;
    private readonly bool _isExistingTab;

    internal ModCraftTreeTab(string nameID, string displayText, string language, Sprite sprite)
        : base(nameID, TreeAction.Expand, TechType.None)
    {
        _displayText = displayText;
        _language = language;
        _uSprite = sprite;
    }

    internal ModCraftTreeTab(string nameID)
        : base(nameID, TreeAction.Expand, TechType.None)
    {
        _isExistingTab = true;
    }

    internal override void LinkToParent(ModCraftTreeLinkingNode parent)
    {
        base.LinkToParent(parent);

        if (_isExistingTab)
        {
            return;
        }

        var langKey = $"{base.SchemeAsString}Menu_{Name}";
        if (!string.IsNullOrEmpty(_displayText))
        {
            LanguageHandler.SetLanguageLine(langKey, _displayText, _language);
        }
        else if (string.IsNullOrEmpty(Language.main.Get(langKey)))
        {
            InternalLogger.Warn($"Display name was not specified and no existing language line has been found for CraftTree tab '{Name}'.");
        }

        string spriteID = $"{SchemeAsString}_{Name}";
        ModSprite modSprite;
        modSprite = new ModSprite(SpriteManager.Group.Category, spriteID, _uSprite);
        ModSprite.Add(modSprite);
    }
}