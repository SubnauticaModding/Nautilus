using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Nautilus.Handlers;

public static partial class EnumExtensions
{
    internal static readonly Dictionary<CraftData.BackgroundType, Sprite> BackgroundSprites = new();
    internal static readonly HashSet<Sprite> Splice9GridBackgroundSprites = new();

    /// <summary>
    /// Adds a sprite for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance</param>
    /// <param name="backgroundSprite">The sprite to add for this instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>This overload always registers background icons with a circular background.</remarks>
    public static EnumBuilder<CraftData.BackgroundType> WithBackground(this EnumBuilder<CraftData.BackgroundType> builder, Sprite backgroundSprite)
    {
        BackgroundSprites[builder] = backgroundSprite;
        Splice9GridBackgroundSprites.Add(backgroundSprite);
        return builder;
    }
    
    /// <summary>
    /// Adds a sprite for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance</param>
    /// <param name="backgroundSprite">The sprite to add for this instance.</param>
    /// <param name="useCircularIcon">If true, the 'splice 9' setting will be applied, allowing tall rectangular
    /// sprites (usually 2x23) to become circular.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<CraftData.BackgroundType> WithBackground(this EnumBuilder<CraftData.BackgroundType> builder, Sprite backgroundSprite, bool useCircularIcon)
    {
        BackgroundSprites[builder] = backgroundSprite;
        if (useCircularIcon)
        {
            Splice9GridBackgroundSprites.Add(backgroundSprite);   
        }
        return builder;
    }
}