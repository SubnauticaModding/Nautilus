using System.Linq;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Nautilus.FMod.Interfaces;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.FMod;

using Random = UnityEngine.Random;

/// <summary>
/// This class is used to register FMOD events with multiple sounds in one event.
/// </summary>
public class FModMultiSounds : IFModSound
{
    /// <summary>
    /// Makes the sounds play in a randomized order. when <c>false</c>, sounds will play subsequently.
    /// </summary>
    public bool randomizeSounds;
        
    private Sound[] _sounds;

    private Bus _bus;
    
    private int Index
    {
        get
        {
            if (field >= _sounds.Length - 1)
            {
                field = 0;
            }

            return field++;
        }
    }

    /// <summary>
    /// Constructs a new instance of <see cref="FModMultiSounds"/>. Used to register FMOD events with multiple sounds in one event.
    /// </summary>
    /// <param name="sounds">The sounds to register for this object. Please ensure that none of the sounds are on <see cref="MODE.LOOP_NORMAL"/> or <see cref="MODE.LOOP_BIDI"/> modes.</param>
    /// <param name="busPath"><see cref="Bus"/> path to play these sounds under.</param>
    /// <param name="randomizeSounds">Makes the sounds play in a randomized order. when <c>false</c>, sounds will play subsequently.</param>
    public FModMultiSounds(Sound[] sounds, string busPath, bool randomizeSounds = false)
    {
        _sounds = sounds;
        _bus = RuntimeManager.GetBus(busPath);
        this.randomizeSounds = randomizeSounds;
    }
        
    /// <summary>
    /// Constructs a new instance of <see cref="FModMultiSounds"/>. Used to register FMOD events with multiple sounds in one event.
    /// </summary>
    /// <param name="clips">The clips to register for this object.</param>
    /// <param name="mode">The mode to set the clips to. Cannot be <c>MODE.LOOP_NORMAL</c> or <c>MODE.LOOP_BIDI</c>.</param>
    /// <param name="busPath"><see cref="Bus"/> path to play these sounds under.</param>
    /// <param name="randomizeSounds">Makes the sounds play in a randomized order. when <c>false</c>, sounds will play subsequently.</param>
    public FModMultiSounds(AudioClip[] clips, MODE mode, string busPath, bool randomizeSounds = false)
    {
        mode &= ~MODE.LOOP_NORMAL & ~MODE.LOOP_BIDI; // Remove unsupported modes
        _sounds = AudioUtils.CreateSounds(clips, mode).ToArray();
        _bus = RuntimeManager.GetBus(busPath);
        this.randomizeSounds = randomizeSounds;
    }

    /// <summary>
    /// Constructs a new instance of <see cref="FModMultiSounds"/>. Used to register FMOD events with multiple sounds in one event.
    /// </summary>
    /// <param name="soundPaths">The sound paths to register for this object. Paths must be relative to the base game folder.</param>
    /// <param name="mode">The mode to set the clips to. Cannot be <c>MODE.LOOP_NORMAL</c> or <c>MODE.LOOP_BIDI</c>.</param>
    /// <param name="busPath"><see cref="Bus"/> path to play these sounds under.</param>
    /// <param name="randomizeSounds">Makes the sounds play in a randomized order. when <c>false</c>, sounds will play subsequently.</param>
    public FModMultiSounds(string[] soundPaths, MODE mode, string busPath, bool randomizeSounds = false)
    {
        mode &= ~MODE.LOOP_NORMAL & ~MODE.LOOP_BIDI; // Remove unsupported modes
        _sounds = AudioUtils.CreateSounds(soundPaths, mode).ToArray();
        _bus = RuntimeManager.GetBus(busPath);
        this.randomizeSounds = randomizeSounds;
    }

    bool IFModSound.TryPlaySound(out Channel channel)
    {
        channel = default;
        if (_sounds is {Length: > 0})
        {
            if (randomizeSounds)
            {
                int index = Random.Range(0, _sounds.Length);
                return AudioUtils.TryPlaySound(_sounds[index], _bus, out channel);
            }

            return AudioUtils.TryPlaySound(_sounds[Index], _bus, out channel);
        }

        InternalLogger.Error("MultiSounds must have some sounds.");
        return false;
    }
}