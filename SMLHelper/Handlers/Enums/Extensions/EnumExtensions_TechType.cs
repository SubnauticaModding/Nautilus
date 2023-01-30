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