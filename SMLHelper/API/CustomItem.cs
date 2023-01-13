namespace SMLHelper.API;

using Assets;
internal class CustomItem : CbCore
{
    public CustomItem(CbItem packItem, ItemTypes itemType) : base(packItem)
    {
        PackItem = packItem;
        ItemType = itemType;
    }

    public ItemTypes ItemType { get; }

    public CbItem PackItem { get; }

    protected override TechType PrefabType => ItemType == ItemTypes.Battery ? UsingIonCellSkins ? TechType.PrecursorIonBattery : TechType.Battery : UsingIonCellSkins ? TechType.PrecursorIonPowerCell : TechType.PowerCell;

    protected override EquipmentType ChargerType => ItemType == ItemTypes.Battery ? EquipmentType.BatteryCharger : EquipmentType.PowerCellCharger;

    protected override string[] StepsToFabricatorTab => ItemType == ItemTypes.Battery ? CbDatabase.BatteryCraftPath : CbDatabase.PowCellCraftPath;

    protected override void AddToList()
    {
        if (ItemType == ItemTypes.Battery)
            CbDatabase.BatteryItems.Add(this);
        else
            CbDatabase.PowerCellItems.Add(this);

        CbDatabase.TrackItems.Add(TechType);
    }
}