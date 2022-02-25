using SMLHelper.V2.FMod.Interfaces;

namespace SMLHelper.V2.Interfaces
{
    using FMOD;
    using FMOD.Studio;
    using UnityEngine;
    using Utility;
    
    /// <summary>
    /// A handler class for adding and overriding Sounds.
    /// </summary>
    public interface ICustomSoundHandler
    {
        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. defaults to <see cref="SoundChannel.Master"/></param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound RegisterCustomSound(string id, string filePath, SoundChannel soundChannel = SoundChannel.Master);
        
        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load.</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound RegisterCustomSound(string id, string filePath, string busPath);
        
        /// <summary>
        /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="filePath">The file path on disk of the sound file to load.</param>
        /// <param name="bus">The bus to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound RegisterCustomSound(string id, string filePath, Bus bus);

        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. defaults to <see cref="SoundChannel.Master"/></param>
        /// <returns>the <see cref="Sound"/> registered.</returns>
        Sound RegisterCustomSound(string id, AudioClip audio, SoundChannel soundChannel = SoundChannel.Master);
        
        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound RegisterCustomSound(string id, AudioClip audio, string busPath);
        
        /// <summary>
        /// Register a custom sound by an <see cref="AudioClip"/> instance. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="audio">The AudioClip to register.</param>
        /// <param name="bus">The bus to play the sound on.</param>
        /// <returns>the <see cref="Sound"/> loaded</returns>
        Sound RegisterCustomSound(string id, AudioClip audio, Bus bus);
        
        /// <summary>
        /// Registers a Custom sound by an <see cref="IFModSound"/> instance. IFModSound instances have custom logic for playing sounds.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="fModSound">The sound IFModSound object to register.</param>
        void RegisterCustomSound(string id, IFModSound fModSound);

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. <see cref="SoundChannel"/></param>
        void RegisterCustomSound(string id, Sound sound, SoundChannel soundChannel = SoundChannel.Master);

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="busPath">The bus path to play the sound on.</param>
        void RegisterCustomSound(string id, Sound sound, string busPath);
        
        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="bus">The bus to play the sound on.</param>
        void RegisterCustomSound(string id, Sound sound, Bus bus);

        /// <summary>
        /// Try to find and play a custom <see cref="Sound"/> that has been registered.
        /// </summary>
        /// <param name="id">The Id of the custom sound</param>
        void TryPlayCustomSound(string id);
        
        /// <summary>
        /// Try to get a registered custom <see cref="Sound"/>.
        /// </summary>
        /// <param name="id">The Id of the custom sound</param>
        /// <param name="sound">Outputs the <see cref="Sound"/> if found and null if not found.</param>
        /// <returns>true or false depending on if the id was found</returns>
        bool TryGetCustomSound(string id, out Sound sound);

        /// <summary>
        /// Try to get a playing custom sound channel for an emitter
        /// </summary>
        /// <param name="id">The emitter's ID, can be retrieved by calling <c>object.GetInstanceID()</c>.</param>
        /// <param name="channel">Outputs the <see cref="Channel"/>.</param>
        /// <returns>True if found, otherwise false.</returns>
        bool TryGetCustomSoundChannel(int id, out Channel channel);
    }
}