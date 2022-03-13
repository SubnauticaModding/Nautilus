using System;
using SMLHelper.V2.FMod.Interfaces;

namespace SMLHelper.V2.Handlers
{
    using FMOD;
    using FMOD.Studio;
    using FMODUnity;
    using Interfaces;
    using Patchers;
    using UnityEngine;
    using Utility;
    
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

        #region Interface Methods

        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. defaults to <see cref="SoundChannel.Master"/></param>
        /// <returns>the <see cref="Sound"/> loaded</returns>

        [Obsolete("Deprecated. Use ICustomSoundHandler.RegisterCustomSound(string, string, FMOD.Studio.Bus) instead.")]
        Sound ICustomSoundHandler.RegisterCustomSound(string id, string filePath, SoundChannel soundChannel)
        {
            Sound sound = AudioUtils.CreateSound(filePath);
            CustomSoundPatcher.CustomSounds[id] = sound;
            CustomSoundPatcher.CustomSoundChannels[id] = soundChannel;
            return sound;
        }

        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load.</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound ICustomSoundHandler.RegisterCustomSound(string id, string filePath, string busPath)
        {
            var bus = RuntimeManager.GetBus(busPath);
            return Main.RegisterCustomSound(id, filePath, bus);
        }

        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load.</param>
        /// <param name="bus">The bus to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound ICustomSoundHandler.RegisterCustomSound(string id, string filePath, Bus bus)
        {
            Sound sound = AudioUtils.CreateSound(filePath);
            CustomSoundPatcher.CustomSounds[id] = sound;
            CustomSoundPatcher.CustomSoundBuses[id] = bus;
            return sound;
        }

        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. defaults to <see cref="SoundChannel.Master"/></param>
        /// <returns>the <see cref="Sound"/> registered.</returns>
        
        [Obsolete("Deprecated. Use ICustomSoundHandler.RegisterCustomSound(string, UnityEngine.AudioClip, FMOD.Studio.Bus) instead.")]
        Sound ICustomSoundHandler.RegisterCustomSound(string id, AudioClip audio, SoundChannel soundChannel)
        {
            var sound = AudioUtils.CreateSound(audio);
            CustomSoundPatcher.CustomSounds[id] = sound;
            CustomSoundPatcher.CustomSoundChannels[id] = soundChannel;
            return sound;
        }

        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound ICustomSoundHandler.RegisterCustomSound(string id, AudioClip audio, string busPath)
        {
            var bus = RuntimeManager.GetBus(busPath);
            return Main.RegisterCustomSound(id, audio, bus);
        }

        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="bus">The bus to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound ICustomSoundHandler.RegisterCustomSound(string id, AudioClip audio, Bus bus)
        {
            var sound = AudioUtils.CreateSound(audio);
            CustomSoundPatcher.CustomSounds[id] = sound;
            CustomSoundPatcher.CustomSoundBuses[id] = bus;
            return sound;
        }

        /// <summary>
        /// Registers a Custom sound by an <see cref="IFModSound"/> instance. IFModSound instances have custom logic for playing sounds.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="fModSound">The sound IFModSound object to register.</param>
        void ICustomSoundHandler.RegisterCustomSound(string id, IFModSound fModSound)
        {
            CustomSoundPatcher.CustomFModSounds[id] = fModSound;
        }

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. <see cref="SoundChannel"/></param>

        [Obsolete("Deprecated. Use ICustomSoundHandler.RegisterCustomSound(string, FMOD.Sound, FMOD.Studio.Bus) instead.")]
        void ICustomSoundHandler.RegisterCustomSound(string id, Sound sound, SoundChannel soundChannel)
        {
            CustomSoundPatcher.CustomSounds[id] = sound;
            CustomSoundPatcher.CustomSoundChannels[id] = soundChannel;
        }

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        void ICustomSoundHandler.RegisterCustomSound(string id, Sound sound, string busPath)
        {
            var bus = RuntimeManager.GetBus(busPath);
            Main.RegisterCustomSound(id, sound, bus);
        }

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="bus">The bus to play the sound on.</param>
        void ICustomSoundHandler.RegisterCustomSound(string id, Sound sound, Bus bus)
        {
            CustomSoundPatcher.CustomSounds[id] = sound;
            CustomSoundPatcher.CustomSoundBuses[id] = bus;
        }

        /// <summary>
        /// Try to find and play a custom <see cref="Sound"/> that has been registered.
        /// </summary>
        /// <param name="id">The Id of the custom sound</param>
        void ICustomSoundHandler.TryPlayCustomSound(string id)
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
        /// <returns>true or false depending on if the id was found</returns>
        bool ICustomSoundHandler.TryGetCustomSound(string id, out Sound sound)
        {
            return CustomSoundPatcher.CustomSounds.TryGetValue(id, out sound);
        }

        /// <summary>
        /// Try to get a playing custom sound channel for an emitter
        /// </summary>
        /// <param name="id">The emitter's ID, can be retrieved by calling <c>object.GetInstanceID()</c>.</param>
        /// <param name="channel">Outputs the <see cref="Channel"/>.</param>
        /// <returns>True if found, otherwise false.</returns>
        bool ICustomSoundHandler.TryGetCustomSoundChannel(int id, out Channel channel)
        {
            return CustomSoundPatcher.EmitterPlayedChannels.TryGetValue(id, out channel);
        }

        #endregion
        #region Static Methods

        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. defaults to <see cref="SoundChannel.Master"/></param>
        /// <returns>the <see cref="Sound"/> loaded</returns>

        [Obsolete("Deprecated. Use RegisterCustomSound(string, string, FMOD.Studio.Bus) instead.")]
        public static Sound RegisterCustomSound(string id, string filePath, SoundChannel soundChannel = SoundChannel.Master)
        {
            return Main.RegisterCustomSound(id, filePath, soundChannel);
        }

        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load.</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        public static Sound RegisterCustomSound(string id, string filePath, string busPath)
        {
            return Main.RegisterCustomSound(id, filePath, busPath);
        }
        
        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load.</param>
        /// <param name="bus">The bus to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        public static Sound RegisterCustomSound(string id, string filePath, Bus bus)
        {
            return Main.RegisterCustomSound(id, filePath, bus);
        }

        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. defaults to <see cref="SoundChannel.Master"/></param>
        /// <returns>the <see cref="Sound"/> registered.</returns>
        
        [Obsolete("Deprecated. Use RegisterCustomSound(string, UnityEngine.AudioClip, FMOD.Studio.Bus) instead.")]
        public static Sound RegisterCustomSound(string id, AudioClip audio, SoundChannel soundChannel = SoundChannel.Master)
        {
            return Main.RegisterCustomSound(id, audio, soundChannel);
        }

        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        public static Sound RegisterCustomSound(string id, AudioClip audio, string busPath)
        {
            return Main.RegisterCustomSound(id, audio, busPath);
        }

        /// <summary>
        /// Registers a Custom sound by an <see cref="IFModSound"/> instance. IFModSound instances have custom logic for playing sounds.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="fModSound">The sound IFModSound object to register.</param>
        public static void RegisterCustomSound(string id, IFModSound fModSound)
        {
            Main.RegisterCustomSound(id, fModSound);
        }

        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="bus">The bus to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        public static Sound RegisterCustomSound(string id, AudioClip audio, Bus bus)
        {
            return Main.RegisterCustomSound(id, audio, bus);
        }

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. <see cref="SoundChannel"/></param>

        [Obsolete("Deprecated. Use RegisterCustomSound(string, FMOD.Sound, FMOD.Studio.Bus) instead.")]
        public static void RegisterCustomSound(string id, Sound sound, SoundChannel soundChannel = SoundChannel.Master)
        {
            Main.RegisterCustomSound(id, sound, soundChannel);
        }

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        public static void RegisterCustomSound(string id, Sound sound, string busPath)
        {
            Main.RegisterCustomSound(id, sound, busPath);
        }
        
        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="bus">The bus to play the sound on.</param>
        public static void RegisterCustomSound(string id, Sound sound, Bus bus)
        {
            Main.RegisterCustomSound(id, sound, bus);
        }

        /// <summary>
        /// Try to find and play a custom <see cref="Sound"/> that has been registered.
        /// </summary>
        /// <param name="id">The Id of the custom sound</param>
        public static void TryPlayCustomSound(string id)
        {
            Main.TryPlayCustomSound(id);
        }
 
        /// <summary>
        /// Try to get a registered custom <see cref="Sound"/>.
        /// </summary>
        /// <param name="id">The Id of the custom sound</param>
        /// <param name="sound">Outputs the <see cref="Sound"/> if found and null if not found.</param>
        /// <returns>true or false depending on if the id was found</returns>
        public static bool TryGetCustomSound(string id, out Sound sound)
        {
            return Main.TryGetCustomSound(id, out sound);
        }

        /// <summary>
        /// Try to get a playing custom sound channel for an emitter
        /// </summary>
        /// <param name="id">The emitter's ID, can be retrieved by calling <c>object.GetInstanceID()</c>.</param>
        /// <param name="channel">Outputs the <see cref="Channel"/>.</param>
        /// <returns>True if found, otherwise false.</returns>
        public static bool TryGetCustomSoundChannel(int id, out Channel channel)
        {
            return Main.TryGetCustomSoundChannel(id, out channel);
        }

        #endregion
    }
}