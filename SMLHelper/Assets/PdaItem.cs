namespace SMLHelper.V2.Assets
{
    using Crafting;
    using SMLHelper.V2.Interfaces;

    /// <summary>
    /// A <see cref="Spawnable"/> item that appears in the PDA blueprints.
    /// </summary>
    /// <seealso cref="Spawnable" />
    public abstract class PdaItem: Spawnable
    {
        internal IKnownTechHandler KnownTechHandler { get; set; } = Handlers.KnownTechHandler.Main;

        /// <summary>
        /// Override to set the <see cref="TechType"/> that must first be scanned or picked up to unlock the blueprint for this item.
        /// If not overriden, it this item will be unlocked from the start of the game.
        /// </summary>
        public virtual TechType RequiredForUnlock => TechType.None;

        /// <summary>
        /// Override with the main group in the PDA blueprints where this item appears.
        /// </summary>
        public virtual TechGroup GroupForPDA => TechGroup.Uncategorized;

        /// <summary>
        /// Override with the category within the group in the PDA blueprints where this item appears.
        /// </summary>
        public virtual TechCategory CategoryForPDA => TechCategory.Misc;

        /// <summary>
        /// Gets a value indicating whether <see cref="RequiredForUnlock"/> has been set to lock this blueprint behind another <see cref="TechType"/>.
        /// </summary>
        /// <value>
        ///   Returns <c>true</c> if will be unlocked from the start of the game; otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="RequiredForUnlock"/>
        public bool UnlockedAtStart => RequiredForUnlock == TechType.None;

        /// <summary>
        /// Message which should be shown when the item is unlocked. <para/>
        /// If not overridden, the message will default to Subnautica's (language key "<see langword="NotificationBlueprintUnlocked"/>").
        /// </summary>
        public virtual string DiscoverMessage => null;

        internal string DiscoverMessageResolved => DiscoverMessage == null ? "NotificationBlueprintUnlocked" : $"{TechType.AsString()}_DiscoverMessage";

        /// <summary>
        /// Initializes a new <see cref="PdaItem"/>, the basic class for any item that appears among your PDA blueprints.
        /// </summary>
        /// <param name="classId">The main internal identifier for this item. Your item's <see cref="TechType" /> will be created using this name.</param>
        /// <param name="friendlyName">The name displayed in-game for this item whether in the open world or in the inventory.</param>
        /// <param name="description">The description for this item; Typically seen in the PDA, inventory, or crafting screens.</param>
        protected PdaItem(string classId, string friendlyName, string description)
            : base(classId, friendlyName, description)
        {
            CorePatchEvents += PatchTechDataEntry;
        }
#if SUBNAUTICA
        /// <summary>
        /// This provides the <see cref="TechData"/> instance used to designate how this item is crafted or constructed.
        /// </summary>
        protected abstract TechData GetBlueprintRecipe();
#elif BELOWZERO
        /// <summary>
        /// This provides the <see cref="RecipeData"/> instance used to designate how this item is crafted or constructed.
        /// </summary>
        protected abstract RecipeData GetBlueprintRecipe();
#endif
        private void PatchTechDataEntry()
        {
            CraftDataHandler.SetTechData(TechType, GetBlueprintRecipe());

            if(GroupForPDA != TechGroup.Uncategorized)
            {
                CraftDataHandler.AddToGroup(GroupForPDA, CategoryForPDA, TechType);
            }

            if(!UnlockedAtStart)
            {
                KnownTechHandler.SetAnalysisTechEntry(RequiredForUnlock, new TechType[1] { TechType }, DiscoverMessageResolved);
            }
        }

        internal sealed override void PatchTechType()
        {
            TechType = TechTypeHandler.AddTechType(ModName, ClassID, FriendlyName, Description, UnlockedAtStart);
        }
    }
}
