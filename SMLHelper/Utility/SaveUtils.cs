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
        /// Registers a parameterless method to invoke whenever the game is saved via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke.</param>
        public static void RegisterOnSaveEvent(Action onSaveAction)
        {
            IngameMenuPatcher.OnSaveEvents += onSaveAction;
        }

        /// <summary>
        /// Removes a method that would be invoked whenever the game is saved via the in-game menu.
        /// </summary>
        /// <param name="onSaveAction">The method invoked.</param>
        public static void DeregisterOnSaveEvent(Action onSaveAction)
        {
            IngameMenuPatcher.OnSaveEvents -= onSaveAction;
        }
    }
}
