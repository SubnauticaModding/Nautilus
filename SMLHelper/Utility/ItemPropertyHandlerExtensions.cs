namespace SMLHelper.V2.Utility
{
    using Handlers;

    public static class ItemPropertyHandlerExtensions
    {
        public static void SetProperty(this TechType techType, object property)
        {
            ItemPropertyHandler.Main.SetProperty(techType, property);
        }

        public static void RemoveProperty(this TechType techType, object property)
        {
            ItemPropertyHandler.Main.RemoveProperty(techType, property);
        }

        public static bool HasProperty(this TechType techType, object property)
        {
            return ItemPropertyHandler.Main.HasProperty(techType, property);
        }

        public static bool TryGetProperties(this TechType techType, out object[] Properties)
        {
            return ItemPropertyHandler.Main.TryGetProperties(techType, out Properties);
        }

        public static void SetProperty(this InventoryItem item, object property)
        {
            ItemPropertyHandler.Main.SetProperty(item, property);
        }

        public static bool HasProperty(this InventoryItem item, object property)
        {
            return ItemPropertyHandler.Main.HasProperty(item, property);
        }

        public static void RemoveProperty(this InventoryItem item, object property)
        {
            ItemPropertyHandler.Main.RemoveProperty(item, property);
        }

        public static bool TryGetProperties(this InventoryItem item, out object[] Properties)
        {
            return ItemPropertyHandler.Main.TryGetProperties(item, out Properties);
        }
    }
}
