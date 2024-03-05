namespace Nautilus.Crafting;

using Assets;
using Handlers;
using Utility;

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
    internal string Id { get; }

    internal TabNode(string[] path, CraftTree.Type scheme, Sprite sprite, string name, string displayName) : base(path, scheme)
    {
        Sprite = sprite;
        DisplayName = displayName;
        Name = name;
        Id = $"{Scheme.ToString()}_{Name}";

        ModSprite.Add(new ModSprite(SpriteManager.Group.Category, Id, Sprite));

        if (!string.IsNullOrEmpty(displayName))
        {
            LanguageHandler.SetLanguageLine(Id, displayName);
        }
        else if (string.IsNullOrEmpty(Language.main.Get(name)))
        {
            InternalLogger.Warn($"Display name was not specified and no existing language line has been found for Tab node '{name}'.");
        }
    }

}
