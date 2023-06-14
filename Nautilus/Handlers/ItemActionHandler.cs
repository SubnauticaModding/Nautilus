using System;
using Nautilus.Patchers;
using Nautilus.Utility;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for registering custom actions when left clicking or middle clicking on an item.
/// </summary>
public static class ItemActionHandler
{
    /// <summary>
    /// Registers a custom left click action for a <see cref="TechType"/>
    /// </summary>
    /// <param name="targetTechType">The <see cref="TechType"/> to which the left click action will be assigned</param>
    /// <param name="callback">The method which will be called when a matching <see cref="InventoryItem"/> with the specified <see cref="TechType"/> was left-clicked</param>
    /// <param name="tooltip">The secondary tooltip which will appear in the description of the item. If null or empty, this will use the language line "LeftClickAction_{<paramref name="targetTechType"/>}" instead.</param>
    /// <param name="language">The language for the tooltip. Defaults to English.</param>
    /// <param name="condition">The condition which must return <see langword="true"/> for the action to be called when the item is clicked<para/>If omitted, the action will always be called</param>
    public static void RegisterLeftClickAction(TechType targetTechType, Action<InventoryItem> callback, string tooltip, string language = null, Predicate<InventoryItem> condition = null)
    {
        string languageLine = $"LeftClickAction_{targetTechType.AsString()}";
        if (!string.IsNullOrEmpty(tooltip))
        {
            LanguageHandler.SetLanguageLine(languageLine, tooltip, language);
        }
        else if (string.IsNullOrEmpty(Language.main.Get(languageLine)))
        {
            InternalLogger.Warn($"Tooltip was not specified and no existing language line has been found for LeftClickAction '{targetTechType}'.");
        }

        condition = condition ?? ((item) => true);
        ItemActionPatcher.LeftClickActions.Add(targetTechType, new ItemActionPatcher.CustomItemAction(callback, languageLine, condition));
    }

    /// <summary>
    /// Registers a custom middle click action for a <see cref="TechType"/>
    /// </summary>
    /// <param name="targetTechType">The <see cref="TechType"/> which the middle click action will be assigned</param>
    /// <param name="callback">The method which will be called when a matching <see cref="InventoryItem"/> with the specified <see cref="TechType"/> was middle-clicked</param>
    /// <param name="tooltip">The secondary tooltip which will appear in the description of the item</param>
    /// <param name="language">The language for the tooltip. Defaults to English.</param>
    /// <param name="condition">The condition which must return <see langword="true"/> for the action to be called when the item is clicked<para/>If omitted, the action will always be called</param>
    public static void RegisterMiddleClickAction(TechType targetTechType, Action<InventoryItem> callback, string tooltip, string language = null, Predicate<InventoryItem> condition = null)
    {
        string languageLine = $"MiddleClickAction_{targetTechType.AsString()}";
        LanguageHandler.SetLanguageLine(languageLine, tooltip, language);

        condition = condition ?? ((item) => true);
        ItemActionPatcher.MiddleClickActions.Add(targetTechType, new ItemActionPatcher.CustomItemAction(callback, languageLine, condition));
    }
}