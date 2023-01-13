namespace SMLHelper.Assets;

using SMLHelper.API;

internal class CustomItem : CbCore
{

    public CustomItem(CbItem packItem, ItemTypes itemType) : base(packItem)
    {
        PackItem = packItem;
        ItemType = itemType;
        UnlocksWith = packItem.UnlocksWith;
        PrefabType = ItemType == ItemTypes.Battery ? UsingIonCellSkins ? TechType.PrecursorIonBattery : TechType.Battery : UsingIonCellSkins ? TechType.PrecursorIonPowerCell : TechType.PowerCell;
    }

    /// <summary>
    /// The techtype required to unlock this battery.
    /// </summary>
    public TechType UnlocksWith = TechType.None;

    /// <summary>
    /// Sets the Required unlock to use <see cref="UnlocksWith"/> field that can be set instead of overridden.
    /// </summary>
    public sealed override TechType RequiredForUnlock => UnlocksWith;

    public ItemTypes ItemType { get; }

    public CbItem PackItem { get; }

    public override string[] StepsToFabricatorTab => ItemType == ItemTypes.Battery ? CbDatabase.BatteryCraftPath : CbDatabase.PowCellCraftPath;
}