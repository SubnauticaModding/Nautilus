using FMOD;
using SMLHelper.V2.Utility;

namespace SMLHelper.V2.Interfaces
{
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
        /// <returns>Returns the <see cref="Sound"/> loaded</returns>
        Sound RegisterCustomSound(string id, string filePath, SoundChannel soundChannel = SoundChannel.Master);

        /// <summary>
        /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
        /// </summary>
        /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
        /// <param name="sound">The pre loaded sound</param>
        /// <param name="soundChannel">The sound channel to get the volume to play the sound at. <see cref="SoundChannel"/></param>
        void RegisterCustomSound(string id, Sound sound, SoundChannel soundChannel = SoundChannel.Master);

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
        /// <returns>Returns true or false depending on if the id was found</returns>
        bool TryGetCustomSound(string id, out Sound sound);
    }
}