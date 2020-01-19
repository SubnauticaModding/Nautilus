namespace SMLHelper.V2.Crafting
{
    using Assets;
    using Patchers;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using Sprite = UnityEngine.Sprite;
#endif

    /// <summary>
    /// A tab node of a CraftTree. Tab nodes help organize crafting nodes by grouping them into categories.
    /// </summary>
    /// <seealso cref="ModCraftTreeLinkingNode" />
    public class ModCraftTreeTab : ModCraftTreeLinkingNode
    {
        private readonly string DisplayText;
        private readonly Sprite XSprite;
        private readonly bool IsExistingTab;
        private readonly string ModName;

        internal ModCraftTreeTab(string modName, string nameID, string displayText, Sprite sprite)
            : base(nameID, TreeAction.Expand, TechType.None)
        {
            DisplayText = displayText;
            XSprite = sprite;
            ModName = modName;
        }

        internal ModCraftTreeTab(string modName, string nameID)
            : base(nameID, TreeAction.Expand, TechType.None)
        {
            IsExistingTab = true;
            ModName = modName;
        }

        internal override void LinkToParent(ModCraftTreeLinkingNode parent)
        {
            base.LinkToParent(parent);

            if (IsExistingTab)
                return;

            LanguagePatcher.AddCustomLanguageLine(ModName, $"{base.SchemeAsString}Menu_{Name}", DisplayText);

            string spriteID = $"{this.SchemeAsString}_{Name}";

            ModSprite modSprite;
            modSprite = new ModSprite(SpriteManager.Group.Category, spriteID, XSprite);
            ModSprite.Add(modSprite);
        }
    }
}
