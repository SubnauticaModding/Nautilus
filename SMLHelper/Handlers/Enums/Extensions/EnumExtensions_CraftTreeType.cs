using SMLHelper.Crafting;
using SMLHelper.Patchers;

// ReSharper disable once CheckNamespace
namespace SMLHelper.Handlers;

public static partial class EnumExtensions
{
    /// <summary>
    /// Creates a custom crafting tree.<br/>
    /// Creating a new CraftTree only makes sense if you're going to use it in a new type of <see cref="GhostCrafter"/>.
    /// </summary>
    /// <param name="builder">The custom enum object to make a crafting tree for.</param>
    /// <param name="craftTreeRoot">
    /// The root node for your custom craft tree, as a new <see cref="ModCraftTreeRoot"/> instance.<br/>
    /// Build up your custom crafting tree from this root node.<br/>
    /// This tree will be automatically patched into the game.<para/>
    /// For more advanced usage, you can replace the default value of <see cref="ModCraftTreeRoot.CraftTreeCreation"/> with your own custom function.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<CraftTree.Type> CreateCraftTreeRoot(this EnumBuilder<CraftTree.Type> builder,
        out ModCraftTreeRoot craftTreeRoot)
    {
        var craftTreeType = (CraftTree.Type)builder;
        var name = craftTreeType.ToString();
        
        craftTreeRoot = new ModCraftTreeRoot(craftTreeType, name);
        CraftTreePatcher.CustomTrees[craftTreeType] = craftTreeRoot;

        return builder;
    }
}