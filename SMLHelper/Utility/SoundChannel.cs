namespace SMLHelper.Utility
{
    using System;

    /// <summary>
    /// The a list the different volume controls in the game
    /// </summary>
    [Obsolete("Deprecated. Use AudioUtils.BusPaths instead.")]
    public enum SoundChannel
    {
        /// <summary>Master volume control</summary>
        Master,
        /// <summary>Music volume control</summary>
        Music,
        /// <summary>Voice volume control</summary>
        Voice,
        /// <summary>Ambient volume control</summary>
        Ambient
    }
}
