using System.Collections.Generic;
using Nautilus.Utility;

// ReSharper disable once CheckNamespace
namespace Nautilus.Handlers;

public static partial class EnumExtensions
{
    /// <summary>
    /// Adds a display name to this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="displayName">The display name of the Tech Group, can be anything. If null or empty, this will use the language line "Group{enumName}" instead.</param>
    /// <param name="language">The language for the display name. Defaults to English.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechGroup> WithPdaInfo(this EnumBuilder<TechGroup> builder, string displayName, string language = "English")
    {
        var techGroup = (TechGroup)builder;
        var name = builder.ToString();
        var fullName = "Group" + name;
        
        if (!string.IsNullOrEmpty(displayName))
        {
            LanguageHandler.SetLanguageLine(fullName, displayName, language);
        }
        else if (string.IsNullOrEmpty(Language.main.Get(fullName)))
        {
            InternalLogger.Warn($"Display name was not specified and no existing language line has been found for TechGroup '{name}'.");
        }
        
        if (!uGUI_BlueprintsTab.groups.Contains(techGroup))
            uGUI_BlueprintsTab.groups.Add(techGroup);

        if (!CraftData.groups.ContainsKey(techGroup))
            CraftData.groups[techGroup] = new Dictionary<TechCategory, List<TechType>>();

        return builder;
    }
}