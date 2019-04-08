namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;
    using System.Collections.Generic;
    using System.Linq;

    public class ItemPropertyHandler : IItemPropertyHandler
    {
        public static IItemPropertyHandler Main { get; } = new ItemPropertyHandler();

        private ItemPropertyHandler()
        {
            // Hide constructor
        }

        internal static readonly IDictionary<TechType, object[]> TechTypeProperties = new SelfCheckingDictionary<TechType, object[]>("TechTypeProperties");

        internal static readonly IDictionary<InventoryItem, object[]> InventoryItemProperties = new SelfCheckingDictionary<InventoryItem, object[]>("InventoryItemProperties");

        void IItemPropertyHandler.AddProperty(TechType techType, object property)
        {
            if (TechTypeProperties.TryGetValue(techType, out object[] existingProperties))
            {
                List<object> propertyList = existingProperties.ToList();
                if (propertyList.Contains(property))
                {
                    propertyList.Add(property);
                    TechTypeProperties.Remove(techType);
                    TechTypeProperties.Add(techType, propertyList.ToArray());
                }
            }
            else
            {
                List<object> propertyList = new List<object>
                {
                    property
                };
                TechTypeProperties.Add(techType, propertyList.ToArray());
            }
        }

        void IItemPropertyHandler.RemoveProperty(TechType techType, object property)
        {
            if (TechTypeProperties.TryGetValue(techType, out object[] existingProperties))
            {
                List<object> propertyList = existingProperties.ToList();
                if (propertyList.Contains(property))
                {
                    propertyList.Remove(property);
                    TechTypeProperties.Remove(techType);
                    TechTypeProperties.Add(techType, propertyList.ToArray());
                }
            }
        }

        bool IItemPropertyHandler.HasProperty(TechType techType, object property)
        {
            if (!Main.TryGetProperties(techType, out object[] properties)) return false;
            return properties.Contains(property);
        }

        bool IItemPropertyHandler.TryGetProperties(TechType techType, out object[] properties)
        {
            if (TechTypeProperties.TryGetValue(techType, out object[] existingProperties))
            {
                properties = existingProperties;
                return true;
            }
            properties = null;
            return false;
        }

        void IItemPropertyHandler.AddProperty(InventoryItem item, object property)
        {
            if (InventoryItemProperties.TryGetValue(item, out object[] existingProperties))
            {
                List<object> propertyList = existingProperties.ToList();
                if (propertyList.Contains(property))
                {
                    propertyList.Add(property);
                    InventoryItemProperties.Remove(item);
                    InventoryItemProperties.Add(item, propertyList.ToArray());
                }
            }
            else
            {
                List<object> propertyList = new List<object>
                {
                    property
                };
                InventoryItemProperties.Add(item, propertyList.ToArray());
            }
        }

        bool IItemPropertyHandler.HasProperty(InventoryItem item, object property)
        {
            if (!Main.TryGetProperties(item, out object[] properties)) return false;
            return properties.Contains(property);
        }

        void IItemPropertyHandler.RemoveProperty(InventoryItem item, object property)
        {
            if (InventoryItemProperties.TryGetValue(item, out object[] existingProperties))
            {
                List<object> propertyList = existingProperties.ToList();
                if (propertyList.Contains(property))
                {
                    propertyList.Remove(property);
                    InventoryItemProperties.Remove(item);
                    InventoryItemProperties.Add(item, propertyList.ToArray());
                }
            }
        }

        bool IItemPropertyHandler.TryGetProperties(InventoryItem item, out object[] properties)
        {
            if (InventoryItemProperties.TryGetValue(item, out object[] existingProperties))
            {
                properties = existingProperties;
                return true;
            }
            properties = null;
            return false;
        }

        #region Static Methods

        public static void AddProperty(TechType techType, object property)
        {
            Main.AddProperty(techType, property);
        }

        public static void RemoveProperty(TechType techType, object property)
        {
            Main.RemoveProperty(techType, property);
        }

        public static bool HasProperty(TechType techType, object property)
        {
            return Main.HasProperty(techType, property);
        }

        public static bool TryGetProperties(TechType techType, out object[] Properties)
        {
            return Main.TryGetProperties(techType, out Properties);
        }

        public static void AddProperty(InventoryItem item, object property)
        {
            Main.AddProperty(item, property);
        }

        public static bool HasProperty(InventoryItem item, object property)
        {
            return Main.HasProperty(item, property);
        }

        public static void RemoveProperty(InventoryItem item, object property)
        {
            Main.RemoveProperty(item, property);
        }

        public static bool TryGetProperties(InventoryItem item, out object[] Properties)
        {
            return Main.TryGetProperties(item, out Properties);
        }

        #endregion
    }
}
