namespace SMLHelper.V2.Interfaces
{
    public interface IItemPropertyHandler
    {
        /// <summary>
        /// Adds a property to a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
        void AddProperty(TechType techType, object property);

        /// <summary>
        /// Determines whether a <see cref="TechType"/> value has a property
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="TechType"/> has the property; otherwise, <c>false</c>.
        /// </returns>
        bool HasProperty(TechType techType, object property);

        /// <summary>
        /// Removes a property from a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> value</param>
        /// <param name="property">The property</param>
        void RemoveProperty(TechType techType, object property);

        /// <summary>
        /// Gets all of the properties of a <see cref="TechType"/> value
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/></param>
        /// <param name="properties">The properties of the <see cref="TechType"/></param>
        /// <returns>
        ///   Whether or not the object had any properties
        /// </returns>
        bool TryGetProperties(TechType techType, out object[] properties);

        /// <summary>
        /// Adds a property to an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/> object</param>
        /// <param name="property">The property</param>
        void AddProperty(InventoryItem item, object property);

        /// <summary>
        /// Determines whether an <see cref="InventoryItem"/> object has a property
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/> object</param>
        /// <param name="property">The property</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="InventoryItem"/> has the property; otherwise, <c>false</c>.
        /// </returns>
        bool HasProperty(InventoryItem item, object property);

        /// <summary>
        /// Removes a property from an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/></param>
        /// <param name="property">The property</param>
        void RemoveProperty(InventoryItem item, object property);

        /// <summary>
        /// Gets all of the properties of an <see cref="InventoryItem"/> object
        /// </summary>
        /// <param name="item">The <see cref="InventoryItem"/></param>
        /// <param name="properties">The properties of the <see cref="InventoryItem"/></param>
        /// <returns>
        ///   Whether or not the object had any properties
        /// </returns>
        bool TryGetProperties(InventoryItem item, out object[] properties);
    }
}
