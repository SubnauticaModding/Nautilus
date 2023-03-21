using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SMLHelper.Assets;
using SMLHelper.Patchers;
using SMLHelper.Utility;

// ReSharper disable once CheckNamespace
namespace SMLHelper.Handlers;

public static partial class EnumExtensions
{
    /// <summary>
    /// Adds a display name, tooltip to this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="displayName">The display name of this Tech Type, can be anything. If null or empty, this will use the language line "{enumName}" instead.</param>
    /// <param name="tooltip">The tooltip displayed when hovered in the PDA, can be anything. If null or empty, this will use the language line "Tooltip_{enumName}" instead.</param>
    /// <param name="language">The language for this entry. Defaults to English.</param>
    /// <param name="unlockAtStart">Whether this instance should be unlocked on game start or not.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechType> WithPdaInfo(this EnumBuilder<TechType> builder, string displayName, string tooltip, string language = "English", bool unlockAtStart = true)
    {
        TechType techType = builder;
        var name = techType.ToString();
        
        
        if (!string.IsNullOrEmpty(displayName))
        {
            LanguageHandler.SetLanguageLine(name, displayName, language);
        }
        else if (string.IsNullOrEmpty(Language.main.Get(name)))
        {
            InternalLogger.Warn($"Display name was not specified and no existing language line has been found for TechType '{name}'.");
        }

        if (!string.IsNullOrEmpty(tooltip))
        {
            LanguageHandler.SetLanguageLine("Tooltip_" + name, tooltip, language);
        }
        else if (string.IsNullOrEmpty(Language.main.Get("Tooltip_" + name)))
        {
            InternalLogger.Warn($"Tooltip was not specified and no existing language line has been found for TechType '{name}'.");
        }
        
        TooltipFactory.techTypeTooltipStrings.valueToString[techType] = "Tooltip_" + name;
        
        if (unlockAtStart)
            KnownTechPatcher.UnlockedAtStart.Add(techType);

        return builder;
    }

#if SUBNAUTICA
    /// <summary>
    /// Adds an icon for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="sprite">The icon to add for this instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechType> WithIcon(this EnumBuilder<TechType> builder, Atlas.Sprite sprite)
    {
        TechType tt = builder;

        if(sprite != null)
            ModSprite.Add(SpriteManager.Group.None, tt.ToString(), sprite);

        return builder;
    }
#endif

    /// <summary>
    /// Adds an icon for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="sprite">The icon to add for this instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechType> WithIcon(this EnumBuilder<TechType> builder, UnityEngine.Sprite sprite)
    {
        TechType tt = builder;

        if(sprite != null)
            ModSprite.Add(SpriteManager.Group.None, tt.ToString(), sprite);

        return builder;
    }

    /// <summary>
    /// Sets the size in inventory for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="size">The 2x2 vector size</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechType> WithSizeInInventory(this EnumBuilder<TechType> builder, Vector2int size)
    {
        CraftDataHandler.SetItemSize(builder, size);
        return builder;
    }
}