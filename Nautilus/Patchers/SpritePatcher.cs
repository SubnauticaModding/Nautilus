using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Handlers;
using UnityEngine;

namespace Nautilus.Patchers;

internal class SpritePatcher
{

    internal static void PatchCheck(SpriteManager.Group group, string name)
    {
        if (string.IsNullOrEmpty(name) || !ModSprite.ModSprites.TryGetValue(group, out Dictionary<string, Sprite> groupDict) || !groupDict.TryGetValue(name, out _))
        {
            return;
        }

        if (!SpriteManager.hasInitialized)
        {
            return;
        }

        Dictionary<string, Sprite> atlas = GetSpriteAtlas(group);
            
        if (atlas != null && !atlas.TryGetValue(name, out _))
        {
            PatchSprites();
        }
    }

    internal static void Patch(Harmony harmony)
    {
        PatchSprites();
        MethodInfo spriteManagerGet = AccessTools.Method(typeof(SpriteManager), nameof(SpriteManager.Get), new Type[] { typeof(SpriteManager.Group), typeof(string), typeof(Sprite) });
        MethodInfo spriteManagerGetBackground = AccessTools.Method(typeof(SpriteManager), nameof(SpriteManager.GetBackground), new Type[] { typeof(CraftData.BackgroundType) });

        HarmonyMethod patchCheck = new(AccessTools.Method(typeof(SpritePatcher), nameof(SpritePatcher.PatchCheck)));
        HarmonyMethod patchBackgrounds = new(AccessTools.Method(typeof(SpritePatcher), nameof(PatchBackgrounds)));
        harmony.Patch(spriteManagerGet, prefix: patchCheck);
        harmony.Patch(spriteManagerGetBackground, prefix: patchBackgrounds);
    }

    private static void PatchSprites()
    {
        foreach (KeyValuePair<SpriteManager.Group, Dictionary<string, Sprite>> moddedSpriteGroup in ModSprite.ModSprites)
        {
            SpriteManager.Group moddedGroup = moddedSpriteGroup.Key;

            Dictionary<string, Sprite> spriteAtlas = GetSpriteAtlas(moddedGroup);
            if (spriteAtlas == null)
            {
                continue;
            }

            Dictionary<string, Sprite> moddedSprites = moddedSpriteGroup.Value;
            foreach (KeyValuePair<string, Sprite> sprite in moddedSprites)
            {
                if (spriteAtlas.ContainsKey(sprite.Key))
                {
                    InternalLogger.Debug($"Overwriting Sprite {sprite.Key} in {nameof(SpriteManager.Group)}.{moddedSpriteGroup.Key}");
                    spriteAtlas[sprite.Key] = sprite.Value;
                }
                else
                {
                    InternalLogger.Debug($"Adding Sprite {sprite.Key} to {nameof(SpriteManager.Group)}.{moddedSpriteGroup.Key}");
                    spriteAtlas.Add(sprite.Key, sprite.Value);
                }
            }
        }
        InternalLogger.Debug("SpritePatcher is done.");
    }

    private static bool PatchBackgrounds(CraftData.BackgroundType backgroundType, ref Sprite __result)
    {
        if (EnumExtensions.BackgroundSprites.TryGetValue(backgroundType, out Sprite value))
        {
            __result = value;
            return false;
        }
        return true;
    }

    private static Dictionary<string, Sprite> GetSpriteAtlas(SpriteManager.Group groupKey)
    {
        if (!SpriteManager.mapping.TryGetValue(groupKey, out string atlasName))
        {
            InternalLogger.Error($"SpritePatcher was unable to find a sprite mapping for {nameof(SpriteManager.Group)}.{groupKey}");
            return null;
        }

        if (SpriteManager.atlases.TryGetValue(atlasName, out Dictionary<string, Sprite> spriteGroup))
        {
            return spriteGroup;
        }

        InternalLogger.Error($"SpritePatcher was unable to find a sprite atlas for {nameof(SpriteManager.Group)}.{groupKey}");
        return null;
    }
}