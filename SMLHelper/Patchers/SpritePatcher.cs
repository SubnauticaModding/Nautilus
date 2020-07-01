namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using Assets;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using Sprite = UnityEngine.Sprite;
#endif

    internal class SpritePatcher
    {
        internal static void Patch()
        {
            foreach (SpriteManager.Group moddedGroup in ModSprite.ModSprites.Keys)
            {
                Dictionary<string, Sprite> spriteAtlas = GetSpriteGroup(moddedGroup);
                if (spriteAtlas == null)
                    continue;

                foreach (var sprite in ModSprite.ModSprites[moddedGroup])
                {
                    spriteAtlas.Add(sprite.Key, sprite.Value);
                }
            }

            Logger.Debug("SpritePatcher is done.");
        }

        private static Dictionary<string, Sprite> GetSpriteGroup(SpriteManager.Group groupKey)
        {
            if (!SpriteManager.mapping.TryGetValue(groupKey, out string atlasName))
            {
                Logger.Error($"SpritePatcher was unable to find a sprite mapping for {nameof(SpriteManager.Group)}.{groupKey}");
                return null;
            }

#if SUBNAUTICA
            var atlas = Atlas.GetAtlas(atlasName);
            if (atlas != null)
                return atlas._nameToSprite;

#elif BELOWZERO
            if (SpriteManager.atlases.TryGetValue(atlasName, out var spriteGroup))
                    return spriteGroup;
#endif

            Logger.Error($"SpritePatcher was unable to find a sprite atlas for {nameof(SpriteManager.Group)}.{groupKey}");
            return null;
        }
    }
}
