namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using Assets;

    internal class SpritePatcher
    {
        internal static void Patch()
        {
            // Direct access to private fields made possible by https://github.com/CabbageCrow/AssemblyPublicizer/
            // See README.md for details.

            Dictionary<SpriteManager.Group, string> mapping = SpriteManager.mapping;
            Dictionary<string, Dictionary<string, Atlas.Sprite>> atlases = SpriteManager.atlases;

            foreach (SpriteManager.Group moddedGroup in ModSprite.ModSprites.Keys)
            {
                string groupName = mapping[moddedGroup];
                Dictionary<string, Atlas.Sprite> spriteGroup = atlases[groupName];

                foreach (string spriteKey in ModSprite.ModSprites[moddedGroup].Keys)
                {
                    spriteGroup.Add(spriteKey, ModSprite.ModSprites[moddedGroup][spriteKey]);
                }
            }

            Logger.Log("SpritePatcher is done.", LogLevel.Debug);
        }
    }
}
