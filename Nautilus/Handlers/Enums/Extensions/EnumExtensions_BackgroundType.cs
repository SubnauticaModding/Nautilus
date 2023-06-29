using System.Collections.Generic;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#elif BELOWZERO
using Sprite = UnityEngine.Sprite;
#endif

// ReSharper disable once CheckNamespace
namespace Nautilus.Handlers;

public static partial class EnumExtensions
{
    internal static readonly Dictionary<CraftData.BackgroundType, Sprite> BackgroundSprites = new();

    /// <summary>
    /// Adds a sprite for this instance.
    /// </summary>
    /// <param name="builder">The current custom enum object instance</param>
    /// <param name="backgroundSprite">The sprite to add for this instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<CraftData.BackgroundType> WithBackground(this EnumBuilder<CraftData.BackgroundType> builder, Sprite backgroundSprite)
    {
        BackgroundSprites[builder] = backgroundSprite;
        return builder;
    }
}