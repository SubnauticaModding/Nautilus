using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace SMLHelper.Handlers;

public static partial class EnumExtensions
{
    /// <summary>
    /// Adds a display name to this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="displayName">The display name of the Tech Group. Can be anything.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechGroup> WithPdaInfo(this EnumBuilder<TechGroup> builder, string displayName)
    {
        var techGroup = (TechGroup)builder;
        var name = builder.ToString();
        LanguageHandler.SetLanguageLine("Group" + name, displayName);
        
        if (!uGUI_BlueprintsTab.groups.Contains(techGroup))
            uGUI_BlueprintsTab.groups.Add(techGroup);

        if (!CraftData.groups.ContainsKey(techGroup))
            CraftData.groups[techGroup] = new Dictionary<TechCategory, List<TechType>>();

        return builder;
    }
}