namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Assets;
    using Harmony;

    internal class SpritePatcher
    {
        // The groups field is present in Subnautica and BZ Stable.
        private static readonly FieldInfo groupsInfo = AccessTools.Field(typeof(SpriteManager), "groups");

        // In BZ Experimental, it is replaced by the atlases field.
        private static readonly FieldInfo atlasesInfo = AccessTools.Field(typeof(SpriteManager), "atlases");

        // To avoid having to create a third build configuration, this is the one patcher class that will used literal strings insteead of nameof.
        // TODO - Once BZ stable is updated with the changes from Experimental, we can return this back to using fields directly.

        private static Dictionary<SpriteManager.Group, Dictionary<string, Atlas.Sprite>> _groups;
        private static Dictionary<SpriteManager.Group, Dictionary<string, Atlas.Sprite>> Groups => _groups ?? (_groups = (Dictionary<SpriteManager.Group, Dictionary<string, Atlas.Sprite>>)groupsInfo.GetValue(null));

        private static Dictionary<string, Dictionary<string, Atlas.Sprite>> _atlases;
        private static Dictionary<string, Dictionary<string, Atlas.Sprite>> Atlases => _atlases ?? (_atlases = (Dictionary<string, Dictionary<string, Atlas.Sprite>>)atlasesInfo.GetValue(null));

        internal static void Patch()
        {
            foreach (SpriteManager.Group moddedGroup in ModSprite.ModSprites.Keys)
            {
                Dictionary<string, Atlas.Sprite> spriteGroup = GetSpriteGroup(moddedGroup);
                if (spriteGroup == null)
                    return;

                foreach (string spriteKey in ModSprite.ModSprites[moddedGroup].Keys)
                {
                    spriteGroup.Add(spriteKey, ModSprite.ModSprites[moddedGroup][spriteKey]);
                }

            }

            Logger.Log("SpritePatcher is done.", LogLevel.Debug);
        }

        private static Dictionary<string, Atlas.Sprite> GetSpriteGroup(SpriteManager.Group groupKey)
        {
            if (groupsInfo != null)
            {
                return Groups[groupKey];
            }
            else if (atlasesInfo != null)
            {
                string groupName = SpriteManager.mapping[groupKey];
                return Atlases[groupName];
            }

            Logger.Error("SpritePatcher was unable to find a sprite ictionary");
            return null;
        }
    }
}
