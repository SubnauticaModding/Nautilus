namespace SMLHelper.V2.Crafting
{
    using Assets;
    using Patchers;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using Sprite = UnityEngine.Sprite;
#endif

    internal class TabNode : Node
    {
        internal Sprite Sprite { get; set; }
        internal string DisplayName { get; set; }
        internal string Name { get; set; }

        internal TabNode(string[] path, CraftTree.Type scheme, UnityEngine.Sprite sprite, string modName, string name, string displayName) 
            : this(path, scheme, modName, name, displayName)
        {
#if SUBNAUTICA
            this.Sprite = new Atlas.Sprite(sprite);
#elif BELOWZERO
            this.Sprite = sprite;
#endif
        }

#if SUBNAUTICA
        internal TabNode(string[] path, CraftTree.Type scheme, Atlas.Sprite sprite, string modName, string name, string displayName)
            : this(path, scheme, modName, name, displayName)
        {
            this.Sprite = sprite;
        }
#endif
        private TabNode(string[] path, CraftTree.Type scheme, string modName, string name, string displayName) 
            : base(path, scheme)
        {
            
            this.DisplayName = displayName;
            this.Name = name;

            ModSprite.Add(new ModSprite(SpriteManager.Group.Category, $"{this.Scheme.ToString()}_{this.Name}", this.Sprite));
            LanguagePatcher.AddCustomLanguageLine(modName, $"{this.Scheme.ToString()}Menu_{this.Name}", this.DisplayName);
        }

    }
}
