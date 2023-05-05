using FMOD;
using FMOD.Studio;
using FMODUnity;
using Nautilus.FMod.Interfaces;
using Nautilus.Patchers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for adding and overriding Sounds. Also see the <see cref="AudioUtils"/> class.
/// </summary>
public static class CustomSoundHandler
{
    /// <summary>
    /// Register a Custom sound by file path. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
    /// </summary>
    /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
    /// <param name="filePath">The file path on disk of the sound file to load.</param>
    /// <param name="busPath">The bus path to play the sound on.</param>
    /// <returns>the <see cref="Sound"/> loaded</returns>
    public static Sound RegisterCustomSound(string id, string filePath, string busPath)
    {
        Bus bus = RuntimeManager.GetBus(busPath);
        return RegisterCustomSound(id, filePath, bus);
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
    /// <param name="busPath">The bus path to play the sound on.</param>
    /// <returns>the <see cref="Sound"/> loaded</returns>
    public static Sound RegisterCustomSound(string id, AudioClip audio, string busPath)
    {
        Bus bus = RuntimeManager.GetBus(busPath);
        return RegisterCustomSound(id, audio, bus);
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
        Sound sound = AudioUtils.CreateSound(audio);
        CustomSoundPatcher.CustomSounds[id] = sound;
        CustomSoundPatcher.CustomSoundBuses[id] = bus;
        return sound;
    }

    /// <summary>
    /// Registers a Custom sound by an <see cref="IFModSound"/> instance. IFModSound instances have custom logic for playing sounds.
    /// </summary>
    /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
    /// <param name="fModSound">The sound IFModSound object to register.</param>
    public static void RegisterCustomSound(string id, IFModSound fModSound)
    {
        CustomSoundPatcher.CustomFModSounds[id] = fModSound;
    }

    /// <summary>
    /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
    /// </summary>
    /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
    /// <param name="sound">The pre loaded sound</param>
    /// <param name="busPath">The bus path to play the sound on.</param>
    public static void RegisterCustomSound(string id, Sound sound, string busPath)
    {
        Bus bus = RuntimeManager.GetBus(busPath);
        RegisterCustomSound(id, sound, bus);
    }

    /// <summary>
    /// Register a Custom sound that has been loaded using AudioUtils. Some vanilla game sounds can be overridden by matching the id to the <see cref="FMODAsset.path"/>.
    /// </summary>
    /// <param name="id">The Id of your custom sound which is used when checking which sounds to play.</param>
    /// <param name="sound">The pre loaded sound</param>
    /// <param name="bus">The bus to play the sound on.</param>
    public static void RegisterCustomSound(string id, Sound sound, Bus bus)
    {
        CustomSoundPatcher.CustomSounds[id] = sound;
        CustomSoundPatcher.CustomSoundBuses[id] = bus;
    }

    /// <summary>
    /// Try to find and play a custom <see cref="Sound"/> that has been registered.
    /// </summary>
    /// <param name="id">The Id of the custom sound</param>
    /// <param name="channel">the <see cref="Channel"/>the sound is playing on.</param>
    public static bool TryPlayCustomSound(string id, out Channel channel)
    {
        channel = default;
        if(!CustomSoundPatcher.CustomSounds.TryGetValue(id, out Sound sound))
        {
            InternalLogger.Warn($"Unable to find registered sound for id:{id}");
            return false;
        }

        if (!CustomSoundPatcher.CustomSoundBuses.TryGetValue(id, out Bus bus))
        {
            InternalLogger.Warn($"Unable to find registerd bus for id:{id}");
            return false;
        }

        bus.getChannelGroup(out ChannelGroup channelGroup);
        channelGroup.getPaused(out bool paused);
        return RuntimeManager.CoreSystem.playSound(sound, channelGroup, paused, out channel) == RESULT.OK;
    }
 
    /// <summary>
    /// Try to get a registered custom <see cref="Sound"/>.
    /// </summary>
    /// <param name="id">The Id of the custom sound</param>
    /// <param name="sound">Outputs the <see cref="Sound"/> if found and null if not found.</param>
    /// <returns>true or false depending on if the id was found</returns>
    public static bool TryGetCustomSound(string id, out Sound sound)
    {
        return CustomSoundPatcher.CustomSounds.TryGetValue(id, out sound);
    }

    /// <summary>
    /// Try to get a playing custom sound channel for an emitter
    /// </summary>
    /// <param name="id">The emitter's ID, can be retrieved by calling <c>object.GetInstanceID()</c>.</param>
    /// <param name="channel">Outputs the <see cref="Channel"/>.</param>
    /// <returns>True if found, otherwise false.</returns>
    public static bool TryGetCustomSoundChannel(int id, out Channel channel)
    {
        return CustomSoundPatcher.EmitterPlayedChannels.TryGetValue(id, out channel);
    }
}