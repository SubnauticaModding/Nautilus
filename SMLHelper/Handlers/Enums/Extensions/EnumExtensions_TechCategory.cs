using System.Collections.Generic;
using BepInEx.Logging;
using SMLHelper.Utility;

// ReSharper disable once CheckNamespace
namespace SMLHelper.Handlers;

public static partial class EnumExtensions
{
    /// <summary>
    /// Adds a display name to this instance.
    /// </summary>
    /// <param name="builder">The current enum object instance.</param>
    /// <param name="displayName">The display name of the Tech Category, can be anything. If null or empty, this will use the language line "TechCategory{enumName}" instead.</param>
    /// <param name="language">The language for the display name. Defaults to English.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechCategory> WithPdaInfo(this EnumBuilder<TechCategory> builder, string displayName, string language = "English")
    {
        var category = (TechCategory)builder;
        var name = category.ToString();
        
        if (!string.IsNullOrEmpty(displayName))
        {
            LanguageHandler.SetLanguageLine("TechCategory" + name, displayName, language);
            uGUI_BlueprintsTab.techCategoryStrings.valueToString[category] = "TechCategory" + displayName;
            return builder;
        }

        var friendlyName = Language.main.Get("TechCategory" + name);
        if (string.IsNullOrEmpty(friendlyName))
        {
            InternalLogger.Warn($"Display name for TechCategory '{name}' is not specified and no language key has been found. Setting display name to 'TechCategory{name}'.");
            uGUI_BlueprintsTab.techCategoryStrings.valueToString[category] = "TechCategory" + name;
            return builder;
        }
        
        uGUI_BlueprintsTab.techCategoryStrings.valueToString[category] = "TechCategory" + friendlyName;
        return builder;
    }

    /// <summary>
    /// Registers this TechCategory instance to a TechGroup.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="techGroup">The Tech Group to add this TechCategory to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechCategory> RegisterToTechGroup(this EnumBuilder<TechCategory> builder,
        TechGroup techGroup)
    {
        TechCategory category = builder.Value;
        
        if (!CraftData.groups.TryGetValue(techGroup, out var techCategories))
        {
            InternalLogger.Error($"Cannot Register to {techGroup} as it does not have PDAInfo set. Use EnumBuilder<TechGroup>.WithPdaInfo(\"Description\") to setup the Modded TechGroup before trying to register to it.");
            return builder;
        }

        if (techCategories.ContainsKey(category))
            return builder;

        techCategories[category] = new List<TechType>();
        return builder;
    }
}