﻿namespace SMLHelper.V2.Interfaces
{
    public interface IItemPropertyHandler
    {
        void SetProperty(TechType techType, object property);

        void RemoveProperty(TechType techType, object property);

        bool HasProperty(TechType techType, object property);

        bool TryGetProperties(TechType techType, out object[] properties);

        void SetProperty(InventoryItem item, object property);

        bool HasProperty(InventoryItem item, object property);

        void RemoveProperty(InventoryItem item, object property);

        bool TryGetProperties(InventoryItem item, out object[] properties);
    }
}
