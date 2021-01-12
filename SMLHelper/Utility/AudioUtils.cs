namespace SMLHelper.V2.Utility
{
    using FMOD;
    using FMODUnity;
    using NAudio.Wave;
    using System;

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
                SoundChannel.Master => SoundSystem.masterVolume,
                SoundChannel.Music => SoundSystem.musicVolume,
                SoundChannel.Voice => SoundSystem.voiceVolume,
                SoundChannel.Ambient => SoundSystem.ambientVolume,
                _ => 1f,
            };

            FMOD_System.getMasterChannelGroup(out ChannelGroup channels);
            var newChannels = channels;
            newChannels.setVolume(volumeLevel);
            FMOD_System.playSound(sound, newChannels, false, out Channel channel);

            return channel;
        }

        /// <summary>
        /// Convert an audio file from mp3 to wav format.
        /// </summary>
        /// <param name="inputFile">The filename of origin.</param>
        /// <param name="outputFile">The destination filename.</param>
        /// <param name="prefix">The audio file prefix.</param>
        /// <param name="compressionType">How much compression you want to applay.</param>
        /// <param name="convertType">The conversion type from => to</param>
        /// <returns>The output filename produced.</returns>
        public static string ConvertAudio(string inputFile, string outputFile = "", string prefix = "", CompressionType compressionType = CompressionType.None, ConvertType convertType = ConvertType.Mp3ToWav)
        {
            var result = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(outputFile))
                    outputFile = inputFile.Replace(".mp3", ".wav");

                if (!string.IsNullOrEmpty(prefix))
                    outputFile = outputFile.Replace(".wav", $"{prefix}.wav");

                using (var mp3Reader = new Mp3FileReader(inputFile))
                {
                    var wavFormat = new WaveFormat(mp3Reader.WaveFormat.SampleRate, mp3Reader.WaveFormat.BitsPerSample, mp3Reader.WaveFormat.Channels);
                    switch (compressionType)
                    {
                        case CompressionType.None:
                            break;
                        case CompressionType.Medium:
                            wavFormat = new WaveFormat(mp3Reader.WaveFormat.SampleRate / 2, mp3Reader.WaveFormat.BitsPerSample, mp3Reader.WaveFormat.Channels);
                            break;
                        case CompressionType.High:
                            wavFormat = new WaveFormat(mp3Reader.WaveFormat.SampleRate / 4, mp3Reader.WaveFormat.BitsPerSample, mp3Reader.WaveFormat.Channels / 2);
                            break;
                    }
                    using (var wavStream = new WaveFormatConversionStream(wavFormat, mp3Reader))
                    {
                        WaveFileWriter.CreateWaveFile(outputFile, wavStream);
                        result = outputFile;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Conversion Error : {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// The compression level to optimiz the file size.
        /// </summary>
        public enum CompressionType
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            None,
            Medium,
            High
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// The file type conversion.
        /// </summary>
        public enum ConvertType
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            Mp3ToWav,
            WavToMp3
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}
