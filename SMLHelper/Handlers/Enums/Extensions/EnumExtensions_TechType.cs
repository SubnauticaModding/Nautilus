using System.Collections.Generic;
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
    /// <param name="ownerAssembly">The owner of this TechType instance. Will be shown under the tooltip.</param>
    /// <param name="displayName">The display name of this Tech Type. Can be anything.</param>
    /// <param name="tooltip">The tooltip displayed when hovered in the PDA. Can be anything.</param>
    /// <param name="unlockAtStart">Whether this instance should be unlocked on game start or not.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechType> WithPdaInfo(this EnumBuilder<TechType> builder, Assembly ownerAssembly, string displayName, string tooltip, bool unlockAtStart = true)
    {
        TechType techType = builder;
        var name = techType.ToString();
        EnsureAsStringSupport(techType);

        var modName = ownerAssembly.GetName().Name;
        
        if (displayName is not null) 
            LanguagePatcher.AddCustomLanguageLine(modName, name, displayName);

        if (tooltip is not null)
        {
            LanguagePatcher.AddCustomLanguageLine(modName, "Tooltip_" + name, tooltip);
            var valueToString = TooltipFactory.techTypeTooltipStrings.valueToString;
            valueToString[techType] = "Tooltip_" + name;
        }
        
        if (unlockAtStart)
            KnownTechPatcher.UnlockedAtStart.Add(techType);

        return builder;
    }
    
    /// <summary>
    /// Adds a display name, tooltip to this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="displayName">The display name of this Tech Type. Can be anything.</param>
    /// <param name="tooltip">The tooltip displayed when hovered in the PDA. Can be anything.</param>
    /// <param name="unlockAtStart">Whether this instance should be unlocked on game start or not.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<TechType> WithPdaInfo(this EnumBuilder<TechType> builder, string displayName, string tooltip, bool unlockAtStart = true)
    {
        var callingAssembly = ReflectionHelper.CallingAssemblyByStackTrace();
        return WithPdaInfo(builder, callingAssembly, displayName, tooltip, unlockAtStart);
    }

    /// <summary>
    /// Adds an icon for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="sprite">The icon to add for this instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    #if SUBNAUTICA
    public static EnumBuilder<TechType> WithIcon(this EnumBuilder<TechType> builder, Atlas.Sprite sprite) 
    #elif BELOWZERO
    public static EnumBuilder<TechType> WithIcon(this EnumBuilder<TechType> builder, UnityEngine.Sprite sprite)
    #endif
    {
        TechType tt = builder;
        EnsureAsStringSupport(tt);
        
        if (sprite != null)
            ModSprite.Add(SpriteManager.Group.None, tt.ToString(), sprite);

        return builder;
    }

    private static void EnsureAsStringSupport(TechType techType)
    {
        var name = techType.ToString();
        var intKey = ((int)techType).ToString();
        
        TechTypeExtensions.stringsNormal[techType] = name;
        TechTypeExtensions.stringsLowercase[techType] = name.ToLowerInvariant();
        TechTypeExtensions.techTypesNormal[name] = techType;
        TechTypeExtensions.techTypesIgnoreCase[name] = techType;
        TechTypeExtensions.techTypeKeys[techType] = intKey;
        TechTypeExtensions.keyTechTypes[intKey] = techType;
    }
}