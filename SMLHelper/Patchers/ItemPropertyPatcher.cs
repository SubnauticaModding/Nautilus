namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;

    internal static class ItemPropertyPatcher
    {
        internal static readonly IDictionary<TechType, object[]> TechTypeProperties = new SelfCheckingDictionary<TechType, object[]>("TechTypeProperties");
        internal static readonly IDictionary<InventoryItem, object[]> InventoryItemProperties = new SelfCheckingDictionary<InventoryItem, object[]>("InventoryItemProperties");
    }
}
