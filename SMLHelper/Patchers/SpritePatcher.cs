namespace SMLHelper.V2.Patchers
{
    using System.Collections;
    using System.Collections.Generic;
    using Assets;
    using UnityEngine;
    using UWE;
    using Logger = Logger;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using Sprite = UnityEngine.Sprite;
#endif

    internal class SpritePatcher
    {
        internal static void Patch()
        {
            CoroutineHost.StartCoroutine(PatchSpritesAsync());
        }

        private static IEnumerator PatchSpritesAsync()
        {
            while(SpriteManager.atlases is null)
            {
                yield return new WaitForSecondsRealtime(1);
            }

            foreach (var moddedSpriteGroup in ModSprite.ModSprites)
            {
                SpriteManager.Group moddedGroup = moddedSpriteGroup.Key;

                Dictionary<string, Sprite> spriteAtlas = GetSpriteAtlas(moddedGroup);
                if (spriteAtlas == null)
                    continue;

                Dictionary<string, Sprite> moddedSprites = moddedSpriteGroup.Value;
                foreach (var sprite in moddedSprites)
                {
                    if (spriteAtlas.ContainsKey(sprite.Key))
                    {
                        Logger.Debug($"Overwriting Sprite {sprite.Key} in {nameof(SpriteManager.Group)}.{moddedSpriteGroup.Key}");
                        spriteAtlas[sprite.Key] = sprite.Value;
                    }
                    else
                    {
                        Logger.Debug($"Adding Sprite {sprite.Key} to {nameof(SpriteManager.Group)}.{moddedSpriteGroup.Key}");
                        spriteAtlas.Add(sprite.Key, sprite.Value);
                    }
                }
            }

            Logger.Debug("SpritePatcher is done.");
        }

        private static Dictionary<string, Sprite> GetSpriteAtlas(SpriteManager.Group groupKey)
        {
            if (!SpriteManager.mapping.TryGetValue(groupKey, out string atlasName))
            {
                Logger.Error($"SpritePatcher was unable to find a sprite mapping for {nameof(SpriteManager.Group)}.{groupKey}");
                return null;
            }

#if SUBNAUTICA
            var atlas = Atlas.GetAtlas(atlasName);
            if (atlas?.nameToSprite != null)
                return atlas.nameToSprite;

#elif BELOWZERO
            if (SpriteManager.atlases.TryGetValue(atlasName, out var spriteGroup))
                    return spriteGroup;
#endif

            Logger.Error($"SpritePatcher was unable to find a sprite atlas for {nameof(SpriteManager.Group)}.{groupKey}");
            return null;
        }
    }
}
