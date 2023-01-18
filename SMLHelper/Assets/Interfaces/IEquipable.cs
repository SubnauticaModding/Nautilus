namespace SMLHelper.Assets.Interfaces
{
    public interface IEquipable
    {
        /// <summary>
        /// Gets the type of equipment slot this item can fit into.
        /// </summary>
        /// <value>
        /// The type of the equipment slot compatible with this item.
        /// </value>
        public EquipmentType EquipmentType { get; }

        /// <summary>
        /// Gets the type of equipment slot this item can fit into.
        /// </summary>
        /// <value>
        /// The type of the equipment slot compatible with this item.
        /// </value>
        public QuickSlotType QuickSlotType { get; }
    }
}
