namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;
    using UnityEngine;

    /// <summary>
    /// A handler for stuff related to the PDALog class.
    /// </summary>
    public class PDALogHandler : IPDALogHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IPDALogHandler Main { get; } = new PDALogHandler();
        
        private PDALogHandler()
        {
            // Hides constructor
        }

        /// <summary>
        /// Adds a custom log entry.
        /// </summary>
        /// <param name="key">The key to refer to this entry.</param>
        /// <param name="languageKey">The subtitles language key for this entry.</param>
        /// <param name="icon">The icon that will be used in the Log tab for this entry. if <c>null</c> It will use the default log entry icon.</param>
        /// <param name="sound">The sound that will be played once this entry is triggered or played in the Log tab.<br/>
        /// If <c>null</c> the Play button in the Log tab will disappear and a sound wont play when this entry is triggered.</param>
        void IPDALogHandler.AddCustomEntry(string key, string languageKey, Sprite icon, FMODAsset sound)
        {
            PDALog.EntryData entry = new()
            {
                key = languageKey,
                icon = icon,
                sound = sound
            };
            PDALogPatcher.CustomEntryData[key] = entry;
        }
        
        /// <summary>
        /// Adds a custom log entry.
        /// </summary>
        /// <param name="key">The key to refer to this entry.</param>
        /// <param name="languageKey">The subtitles language key for this entry.</param>
        /// <param name="icon">The icon that will be used in the Log tab for this entry. if <c>null</c> It will use the default log entry icon.</param>
        /// <param name="sound">The sound that will be played once this entry is triggered or played in the Log tab.<br/>
        /// If <c>null</c> the Play button in the Log tab will disappear and a sound wont play when this entry is triggered.</param>
        public static void AddCustomEntry(string key, string languageKey, Sprite icon = null, FMODAsset sound = null)
        {
            Main.AddCustomEntry(key, languageKey, icon, sound);
        }
    }
}