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
        /// <param name="mode"></param>
        /// <returns>The <see cref="Sound"/> instance</returns>
        public static Sound CreateSound(string path, MODE mode = MODE.DEFAULT)
        {
#if SUBNAUTICA
            RuntimeManager.CoreSystem.createSound(path, mode, out Sound sound);
#elif BELOWZERO
            RuntimeManager.LowlevelSystem.createSound(path, mode, out Sound sound);
#endif
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
#if SUBNAUTICA
            RuntimeManager.CoreSystem.getMasterChannelGroup(out ChannelGroup channels);
            RuntimeManager.CoreSystem.playSound(sound, channels, false, out Channel channel);
#elif BELOWZERO
            RuntimeManager.LowlevelSystem.getMasterChannelGroup(out ChannelGroup channels);
            RuntimeManager.LowlevelSystem.playSound(sound, channels, false, out Channel channel);
#endif
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
            float volumeLevel;
            
            switch (volumeControl)
            {
                case SoundChannel.Master:
                    volumeLevel = SoundSystem.masterVolume;
                    break;
                case SoundChannel.Music:
                    volumeLevel = SoundSystem.musicVolume;
                    break;
                case SoundChannel.Voice:
                    volumeLevel = SoundSystem.voiceVolume;
                    break;
                case SoundChannel.Ambient:
                    volumeLevel = SoundSystem.ambientVolume;
                    break;
                default:
                    volumeLevel = 1f;
                    break;
            }

#if SUBNAUTICA
            RuntimeManager.CoreSystem.getMasterChannelGroup(out ChannelGroup channels);
            var newChannels = channels;
            newChannels.setVolume(volumeLevel);
            RuntimeManager.CoreSystem.playSound(sound, newChannels, false, out Channel channel);
#elif BELOWZERO
            RuntimeManager.LowlevelSystem.getMasterChannelGroup(out ChannelGroup channels);
            var newChannels = channels;
            newChannels.setVolume(volumeLevel);
            RuntimeManager.LowlevelSystem.playSound(sound, newChannels, false, out Channel channel);
#endif
            return channel;
        }
    }
}
