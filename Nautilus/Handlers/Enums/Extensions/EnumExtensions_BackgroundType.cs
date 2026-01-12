using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Nautilus.Handlers;

public static partial class EnumExtensions
{
    internal static readonly Dictionary<CraftData.BackgroundType, Sprite> BackgroundSprites = new();
    internal static readonly HashSet<Sprite> Splice9GridBackgroundSprites = new();

    /// <param name="builder">The current custom enum object instance</param>
    extension(EnumBuilder<CraftData.BackgroundType> builder)
    {
        /// <summary>
        /// Adds a sprite for this instance.
        /// </summary>
        /// <param name="backgroundSprite">The sprite to add for this instance.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <remarks>This overload always registers background icons with a circular background.</remarks>
        public EnumBuilder<CraftData.BackgroundType> WithBackground(Sprite backgroundSprite)
        {
            return WithBackground(builder, backgroundSprite, true);
        }

        /// <summary>
        /// Adds a sprite for this instance.
        /// </summary>
        /// <param name="backgroundSprite">The sprite to add for this instance.</param>
        /// <param name="useCircularIcon">If true, the 'splice 9' setting will be applied, allowing tall rectangular
        /// sprites (usually 2x23) to become circular.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public EnumBuilder<CraftData.BackgroundType> WithBackground(Sprite backgroundSprite, bool useCircularIcon)
        {
            BackgroundSprites[builder] = backgroundSprite;
            if (useCircularIcon)
            {
                Splice9GridBackgroundSprites.Add(backgroundSprite);   
            }
            return builder;
        }
    }
}