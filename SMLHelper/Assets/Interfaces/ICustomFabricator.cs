namespace SMLHelper.Assets.Interfaces
{
    using SMLHelper.Handlers;
    using Crafting;

    /// <summary>
    /// Defines a list of available models for your <see cref="ICustomFabricator"/>.
    /// </summary>
    public enum FabricatorModel
    {
        /// <summary>
        /// The regular fabricator like the one in the life pod.
        /// </summary>
        Fabricator,

        /// <summary>
        /// The modification station that upgrades your equipment.
        /// </summary>
        Workbench,
#if SUBNAUTICA
        /// <summary>
        /// The style of fabricator found in the Moon Pool and the Cyclops sub.
        /// </summary>
        MoonPool,
#endif
        /// <summary>
        /// Use this option only if you want to provide your own custom model for your fabricator.<para/>
        /// To use this value, you must override the <see cref="IModPrefab.GetGameObjectAsync"/> method.
        /// </summary>
        Custom
    }

    public interface ICustomFabricator
    {
        /// <summary>
        /// This value determines if the game uses your <see cref="FabricatorModel.Custom"/> model or one of the default ones from the game.
        /// </summary>
        public FabricatorModel FabricatorModel { get; }

        /// <summary>
        /// The <see cref="CraftTree.Type"/> this fabricator will display.
        /// Use <see cref="EnumHandler.AddEntry{TEnum}(string)"/> to create a custom Tree type or use one of the defaults from <see cref="CraftTree.Type"/>
        /// If you make your own you can then use <see cref="EnumExtensions.CreateCraftTreeRoot(EnumBuilder{CraftTree.Type}, out ModCraftTreeRoot)"/> to make your root crafting node.
        /// You can then use the following methods to build up your tree.<br/>
        /// <see cref="ModCraftTreeRoot.AddTabNode(string, string, Atlas.Sprite, string)"/>
        /// <see cref="ModCraftTreeRoot.AddCraftNode(string, string)"/>
        /// <see cref="ModCraftTreeRoot.AddCraftNode(TechType, string)"/>
        /// <see cref="ModCraftTreeRoot.AddCraftNode(PrefabInfo, string)"/>
        /// </summary>
        public CraftTree.Type TreeTypeID { get; }
    }
}
