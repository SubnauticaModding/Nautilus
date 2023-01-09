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
    /// <param name="displayName">The display name of the Tech Category. Can be anything.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechCategory> WithPdaInfo(this EnumBuilder<TechCategory> builder, string displayName)
    {
        var category = (TechCategory)builder;
        var name = category.ToString();
        
        uGUI_BlueprintsTab.techCategoryStrings.valueToString[category] = "TechCategory" + displayName;
        LanguageHandler.SetLanguageLine("TechCategory" + name, displayName);

        return builder;
    }

    /// <summary>
    /// Registers this TechCategory instance to a TechGroup.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="techGroup">The Tech Group to add this TechCategory to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static bool TryRegisterToTechGroup(this EnumBuilder<TechCategory> builder,
        TechGroup techGroup)
    {
        var category = (TechCategory)builder;
        
        if (!CraftData.groups.TryGetValue(techGroup, out var techCategories))
        {
            // Should not even really be possible but just in-case.
            InternalLogger.Log($"TechGroup: {category.ToString()} is not registered.", LogLevel.Error);
            return false;
        }

        if (techCategories.ContainsKey(category))
            return true;

        techCategories[category] = new List<TechType>();
        return true;
    }
}