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
            return SaveLoadManager.GetTemporarySavePath();
        }
    }
}
