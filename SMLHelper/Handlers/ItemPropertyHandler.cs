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

        /// <summary>
        /// Adds a property to a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
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

        /// <summary>
        /// Determines whether a <see cref="TechType"/> value has a property
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="TechType"/> has the property; otherwise, <c>false</c>.
        /// </returns>

        bool IItemPropertyHandler.HasProperty(TechType techType, object property)
        {
            if (!Main.TryGetProperties(techType, out object[] properties)) return false;
            return properties.Contains(property);
        }

        /// <summary>
        /// Removes a property from a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
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

        /// <summary>
        /// Gets all of the properties of a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/></param>
        /// <param name="properties">The properties of the <see cref="TechType"/></param>
        /// <returns>
        ///   Whether or not the object had any properties
        /// </returns>
        bool IItemPropertyHandler.TryGetProperties(TechType techType, out object[] properties)
        {
            if (TechTypeProperties.TryGetValue(techType, out object[] existingProperties))
            {
                if (existingProperties == null || existingProperties.Count() <= 0)
                {
                    properties = null;
                    return false;
                }
                properties = existingProperties;
                return true;
            }
            properties = null;
            return false;
        }

        /// <summary>
        /// Adds a property to an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/> object</param>
        /// <param name="property">The property</param>
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

        /// <summary>
        /// Determines whether an <see cref="InventoryItem"/> object has a property
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/> object</param>
        /// <param name="property">The property</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="InventoryItem"/> has the property; otherwise, <c>false</c>.
        /// </returns>
        bool IItemPropertyHandler.HasProperty(InventoryItem item, object property)
        {
            if (!Main.TryGetProperties(item, out object[] properties)) return false;
            return properties.Contains(property);
        }

        /// <summary>
        /// Removes a property from an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/></param>
        /// <param name="property">The property</param>
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

        /// <summary>
        /// Gets all of the properties of an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/></param>
        /// <param name="properties">The properties of the <see cref="InventoryItem"/></param>
        /// <returns>
        ///   Whether or not the object had any properties
        /// </returns>
        bool IItemPropertyHandler.TryGetProperties(InventoryItem item, out object[] properties)
        {
            if (InventoryItemProperties.TryGetValue(item, out object[] existingProperties))
            {
                if (existingProperties == null || existingProperties.Count() <= 0)
                {
                    properties = null;
                    return false;
                }
                properties = existingProperties;
                return true;
            }
            properties = null;
            return false;
        }

        #region Static Methods

        /// <summary>
        /// Adds a property to a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
        public static void AddProperty(TechType techType, object property)
        {
            Main.AddProperty(techType, property);
        }

        /// <summary>
        /// Determines whether a <see cref="TechType"/> value has a property
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="TechType"/> has the property; otherwise, <c>false</c>.
        /// </returns>

        public static bool HasProperty(TechType techType, object property)
        {
            return Main.HasProperty(techType, property);
        }

        /// <summary>
        /// Removes a property from a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
        public static void RemoveProperty(TechType techType, object property)
        {
            Main.RemoveProperty(techType, property);
        }

        /// <summary>
        /// Gets all of the properties of a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/></param>
        /// <param name="properties">The properties of the <see cref="TechType"/></param>
        /// <returns>
        ///   Whether or not the object had any properties
        /// </returns>
        public static bool TryGetProperties(TechType techType, out object[] properties)
        {
            return Main.TryGetProperties(techType, out properties);
        }

        /// <summary>
        /// Adds a property to an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/> object</param>
        /// <param name="property">The property</param>
        public static void AddProperty(InventoryItem item, object property)
        {
            Main.AddProperty(item, property);
        }

        /// <summary>
        /// Determines whether an <see cref="InventoryItem"/> object has a property
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/> object</param>
        /// <param name="property">The property</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="InventoryItem"/> has the property; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasProperty(InventoryItem item, object property)
        {
            return Main.HasProperty(item, property);
        }

        /// <summary>
        /// Removes a property from an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/></param>
        /// <param name="property">The property</param>
        public static void RemoveProperty(InventoryItem item, object property)
        {
            Main.RemoveProperty(item, property);
        }

        /// <summary>
        /// Gets all of the properties of an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/></param>
        /// <param name="properties">The properties of the <see cref="InventoryItem"/></param>
        /// <returns>
        ///   Whether or not the object had any properties
        /// </returns>
        public static bool TryGetProperties(InventoryItem item, out object[] properties)
        {
            return Main.TryGetProperties(item, out properties);
        }

        #endregion
    }
}
