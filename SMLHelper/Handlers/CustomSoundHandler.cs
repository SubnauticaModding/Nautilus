using System;
using FMOD;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Patchers;
using SMLHelper.V2.Utility;

namespace SMLHelper.V2.Handlers
{
    /// <summary>
    /// A handler class for adding and overriding Sounds.
    /// </summary>
    public class CustomSoundHandler: ICustomSoundHandler
    {

        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static ICustomSoundHandler Main { get; } = new CustomSoundHandler();

        private CustomSoundHandler()
        {
            // Hides constructor
        }
        
        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. defaults to <see cref="SoundChannel.Master"/></param>
        /// <returns>Returns the <see cref="Sound"/> loaded</returns>

        public Sound RegisterCustomSound(string id, string filePath, SoundChannel soundChannel = SoundChannel.Master)
        {
                Sound sound = AudioUtils.CreateSound(filePath);
                CustomSoundPatcher.CustomSounds[id] = sound;
                CustomSoundPatcher.CustomSoundChannels[id] = soundChannel;
                return sound;
        }

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. <see cref="SoundChannel"/></param>

        public void RegisterCustomSound(string id, Sound sound, SoundChannel soundChannel = SoundChannel.Master)
        {
            CustomSoundPatcher.CustomSounds[id] = sound;
            CustomSoundPatcher.CustomSoundChannels[id] = soundChannel;
        }

        /// <summary>
        /// Try to find and play a custom <see cref="Sound"/> that has been registered.
        /// </summary>
        /// <param name="id">The Id of the custom sound</param>
        public void TryPlayCustomSound(string id)
        {
            if(!CustomSoundPatcher.CustomSounds.TryGetValue(id, out Sound sound)) return;
            if (!CustomSoundPatcher.CustomSoundChannels.TryGetValue(id, out var soundChannel))
                soundChannel = SoundChannel.Master;
            AudioUtils.PlaySound(sound, soundChannel);
        }
 
        /// <summary>
        /// Try to get a registered custom <see cref="Sound"/>.
        /// </summary>
        /// <param name="id">The Id of the custom sound</param>
        /// <param name="sound">Outputs the <see cref="Sound"/> if found and null if not found.</param>
        /// <returns>Returns true or false depending on if the id was found</returns>
        public bool TryGetCustomSound(string id, out Sound sound)
        {
            return CustomSoundPatcher.CustomSounds.TryGetValue(id, out sound);
        }
    }
}