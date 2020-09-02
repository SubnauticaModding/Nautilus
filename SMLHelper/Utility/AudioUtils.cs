namespace SMLHelper.V2.Utility
{
    using FMOD;
    using FMODUnity;

    /// <summary>
    /// Utilities for audio and sound
    /// </summary>
    public static class AudioUtils
    {
#if SUBNAUTICA_STABLE
        private static System FMOD_System => RuntimeManager.LowlevelSystem;
#else
        private static System FMOD_System => RuntimeManager.CoreSystem;
#endif
        /// <summary>
        /// Creates a <see cref="Sound"/> instance from a path. Can be stored and later used with <see cref="PlaySound(Sound)"/>
        /// </summary>
        /// <param name="path">The path of the sound. Relative to the base game folder.</param>
        /// <param name="mode"></param>
        /// <returns>The <see cref="Sound"/> instance</returns>
        public static Sound CreateSound(string path, MODE mode = MODE.DEFAULT)
        {
            FMOD_System.createSound(path, mode, out Sound sound);
            return sound;
        }

        /// <summary>
        /// Plays a <see cref="Sound"/> globally from a path. Must be a .wav file
        /// </summary>
        /// <param name="path">The path of the sound. Relative to the base game folder.</param>
        /// <param name="mode"></param>
        /// <returns>The channel on which the sound was created</returns>
        public static Channel PlaySound(string path, MODE mode = MODE.DEFAULT)
        {
            return PlaySound(CreateSound(path, mode));
        }

        /// <summary>
        /// Plays a <see cref="Sound"/> globally from a path. Must be a .wav file. Has overload for controlling volume
        /// </summary>
        /// <param name="path">The path of the sound. Relative to the base game folder.</param>
        /// <param name="mode"></param>
        /// <param name="volumeControl">Which volume control to adjust sound levels by. How loud sound is.</param>
        /// <returns>The channel on which the sound was created</returns>
        public static Channel PlaySound(string path, SoundChannel volumeControl, MODE mode = MODE.DEFAULT)
        {
            return PlaySound(CreateSound(path, mode), volumeControl);
        }

        /// <summary>
        /// Plays a <see cref="Sound"/> globally
        /// </summary>
        /// <param name="sound">The sound which should be played</param>
        /// <returns>The channel on which the sound was created</returns>
        public static Channel PlaySound(Sound sound)
        {
            FMOD_System.getMasterChannelGroup(out ChannelGroup channels);
            FMOD_System.playSound(sound, channels, false, out Channel channel);

            return channel;
        }

        /// <summary>
        /// Plays a <see cref="Sound"/> globally at specified volume
        /// </summary>
        /// <param name="sound">The sound which should be played</param>
        /// <param name="volumeControl">Which volume control to adjust sound levels by. How loud sound is.</param>
        /// <returns>The channel on which the sound was created</returns>
        public static Channel PlaySound(Sound sound, SoundChannel volumeControl)
        {
            float volumeLevel = volumeControl switch
            {
                SoundChannel.Master  => SoundSystem.masterVolume,
                SoundChannel.Music   => SoundSystem.musicVolume,
                SoundChannel.Voice   => SoundSystem.voiceVolume,
                SoundChannel.Ambient => SoundSystem.ambientVolume,
                _ => 1f,
            };

            FMOD_System.getMasterChannelGroup(out ChannelGroup channels);
            var newChannels = channels;
            newChannels.setVolume(volumeLevel);
            FMOD_System.playSound(sound, newChannels, false, out Channel channel);

            return channel;
        }
    }
}
