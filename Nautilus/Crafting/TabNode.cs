using Nautilus.Assets;
using Nautilus.Patchers;

namespace Nautilus.Crafting;

#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#else
using Sprite = UnityEngine.Sprite;
#endif

internal class TabNode : Node
{
    internal Sprite Sprite { get; set; }
    internal string DisplayName { get; set; }
    internal string Name { get; set; }

    internal TabNode(string[] path, CraftTree.Type scheme, Sprite sprite, string modName, string name, string displayName, string language = "English") : base(path, scheme)
    {
        Sprite = sprite;
        DisplayName = displayName;
        Name = name;

        ModSprite.Add(new ModSprite(SpriteManager.Group.Category, $"{Scheme.ToString()}_{Name}", Sprite));
        LanguagePatcher.AddCustomLanguageLine($"{Scheme.ToString()}Menu_{Name}", DisplayName, language);
    }

}
