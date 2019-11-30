namespace SMLHelper.V2.Utility
{
    using System;
    using SMLHelper.V2.Patchers;

    /// <summary>
    /// A small collection of save data related utilities.
    /// </summary>
    public static class SaveUtils
    {
        /// <summary>
        /// Returns the path to the current save slot's directory.
        /// </summary>
        public static string GetCurrentSaveDataDir()
        {
            return SaveLoadManager.temporarySavePath;
        }

        /// <summary>
        /// Registers a simple <see cref="Action"/> method to invoke whenever the player saves the game via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke.</param>
        public static void RegisterOnSaveEvent(Action onSaveAction)
        {
            IngameMenuPatcher.OnSaveEvents += onSaveAction;
        }

        /// <summary>
        /// Removes a method previously added through <see cref="RegisterOnSaveEvent(Action)"/> so it is no longer invoked when saving the game.<para/>
        /// If you plan on using this, do not register an anonymous method.
        /// </summary>
        /// <param name="onSaveAction">The method invoked.</param>
        public static void UnregisterOnSaveEvent(Action onSaveAction)
        {
            IngameMenuPatcher.OnSaveEvents -= onSaveAction;
        }
    }
}
