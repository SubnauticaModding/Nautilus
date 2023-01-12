namespace SMLHelper.Assets
{
    using System;
    using Handlers;
    using SMLHelper.Utility;

    /// <summary>
    /// Extensions for the ModPrefab class to set things up without having to use Inheritance based prefabs.
    /// </summary>
    public static partial class BuilderExtensions
    {
        /// <summary>
        /// Registers a custom left click action for a <see cref="TechType"/>
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="callback">The method which will be called when a matching <see cref="InventoryItem"/> with the specified <see cref="TechType"/> was left-clicked</param>
        /// <param name="tooltip">The secondary tooltip which will appear in the description of the item</param>
        /// <param name="condition">The condition which must return <see langword="true"/> for the action to be called when the item is clicked<para/>If ommited, the action will always be called</param>

        public static ModPrefabBuilder SetLeftClickInventoryAction(this ModPrefabBuilder modPrefabBuilder, Action<InventoryItem> callback, string tooltip, Predicate<InventoryItem> condition = null)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set inventory action for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            ItemActionHandler.RegisterLeftClickAction(modPrefab.TechType, callback, tooltip, condition);
            return modPrefabBuilder;
        }


        /// <summary>
        /// Registers a custom middle click action for a <see cref="TechType"/>
        /// </summary>
        /// <param name="modPrefabBuilder">The prefab to handle</param>
        /// <param name="callback">The method which will be called when a matching <see cref="InventoryItem"/> with the specified <see cref="TechType"/> was middle-clicked</param>
        /// <param name="tooltip">The secondary tooltip which will appear in the description of the item</param>
        /// <param name="condition">The condition which must return <see langword="true"/> for the action to be called when the item is clicked<para/>If ommited, the action will always be called</param>
        public static ModPrefabBuilder SetMiddleClickInventoryAction(this ModPrefabBuilder modPrefabBuilder, Action<InventoryItem> callback, string tooltip, Predicate<InventoryItem> condition = null)
        {
            ModPrefab modPrefab = modPrefabBuilder.ModPrefab;
            if(modPrefab.TechType == TechType.None)
            {
                InternalLogger.Error($"Cannot set inventory action for {modPrefab.ClassID} as it does not have a TechType.");
                return modPrefabBuilder;
            }

            ItemActionHandler.RegisterMiddleClickAction(modPrefab.TechType, callback, tooltip, condition);
            return modPrefabBuilder;
        }

    }
}
