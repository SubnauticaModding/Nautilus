namespace SMLHelper.V2.Utility
{
    using FMOD;
    using FMODUnity;

    /// <summary>
    /// Utilities for audio and sound
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Creates a <see cref="Sound"/> instance from a path. Can be stored and later used with <see cref="PlaySound(Sound)"/>
        /// </summary>
        /// <param name="path">The path of the sound. Relative to the base game folder.</param>
        /// <returns>The <see cref="Sound"/> instance</returns>
        public static Sound CreateSound(string path)
        {
            RuntimeManager.LowlevelSystem.createSound(path, MODE.DEFAULT, out Sound sound);
            return sound;
        }

        /// <summary>
        /// Plays a <see cref="Sound"/> globally from a path. Must be a .wav file
        /// </summary>
        /// <param name="path">The path of the sound. Relative to the base game folder.</param>
        /// <returns>The channel on which the sound was created</returns>
        public static Channel PlaySound(string path)
        {
            return PlaySound(CreateSound(path));
        }

        /// <summary>
        /// Plays a <see cref="Sound"/> globally
        /// </summary>
        /// <param name="sound">The sound which should be played</param>
        /// <returns>The channel on which the sound was created</returns>
        public static Channel PlaySound(Sound sound)
        {
            RuntimeManager.LowlevelSystem.getMasterChannelGroup(out ChannelGroup channels);
            RuntimeManager.LowlevelSystem.playSound(sound, channels, false, out Channel newChannel);
            return newChannel;
        }
    }
}
