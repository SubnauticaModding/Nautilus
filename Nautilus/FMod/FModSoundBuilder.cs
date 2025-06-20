using System;
using FMOD;
using Nautilus.Extensions;
using Nautilus.FMod.Interfaces;
using Nautilus.Handlers;
using Nautilus.Utility;

namespace Nautilus.FMod;

/// <summary>
/// Allows for easy creation and registration of sounds from an Asset Bundle.
/// </summary>
public class FModSoundBuilder : IFModSoundBuilder
{
    // - Persistent data -

    private readonly CustomSoundSourceBase _loader;

    // - Builder class data -

    private string _id;
    private string _bus;
    private MODE? _mode;
    private (float, float)? _minAndMaxDistances; // Item1: min, Item2: max
    private float? _fadeDuration;
    private string _clipName;
    private string[] _clipNamesForMultipleSounds;
    private Func<string, bool> _predicateForMultipleSounds;
    private bool _randomizeSoundOrder;

    /// <summary>
    /// Creates a new sound builder that loads sounds from a given source.
    /// </summary>
    /// <param name="loader">Determines how the sound builder locates and loads sounds.</param>
    /// <seealso cref="AssetBundleSoundSource"/>
    /// <seealso cref="ModFolderSoundSource"/>
    public FModSoundBuilder(CustomSoundSourceBase loader)
    {
        _loader = loader;
    }

    // Essential methods

    /// <summary>
    /// Begins constructing a new FMOD sound event with the given parameters.
    /// </summary>
    /// <param name="id">The unique ID or event path of the sound.</param>
    /// <param name="bus">The bus that the sound is played on.
    /// See <see cref="Nautilus.Utility.AudioUtils.BusPaths"/>.</param>
    /// <returns>An instance of the builder for further setup.</returns>
    public IFModSoundBuilder CreateNewEvent(string id, string bus)
    {
        Reset();
        _id = id;
        _bus = bus;
        return this;
    }

    IFModSoundBuilder IFModSoundBuilder.SetMode(MODE mode)
    {
        if (_mode.HasValue)
        {
            InternalLogger.Warn($"Trying to override mode on sound builder '{this}' when it has already been set!");
        }

        _mode = mode;
        return this;
    }

    IFModSoundBuilder IFModSoundBuilder.SetSound(string clipName)
    {
        _clipNamesForMultipleSounds = null;
        _predicateForMultipleSounds = null;

        _clipName = clipName;

        return this;
    }

    IFModSoundBuilder IFModSoundBuilder.SetSounds(bool randomizeOrder, params string[] clipNames)
    {
        _clipName = null;
        _predicateForMultipleSounds = null;

        _clipNamesForMultipleSounds = clipNames;
        _randomizeSoundOrder = randomizeOrder;

        return this;
    }

    IFModSoundBuilder IFModSoundBuilder.SetSounds(bool randomizeOrder, Func<string, bool> predicate)
    {
        _clipName = null;
        _clipNamesForMultipleSounds = null;

        _predicateForMultipleSounds = predicate;
        _randomizeSoundOrder = randomizeOrder;

        return this;
    }

    // Optional methods

    IFModSoundBuilder IFModSoundBuilder.SetMode3D(float minDistance, float maxDistance, bool looping)
    {
        _minAndMaxDistances = (minDistance, maxDistance);
        var mode = AudioUtils.StandardSoundModes_3D;
        if (looping) mode |= MODE.LOOP_NORMAL;
        return ((IFModSoundBuilder) this).SetMode(mode);
    }

    IFModSoundBuilder IFModSoundBuilder.SetMode2D(bool looping)
    {
        var mode = AudioUtils.StandardSoundModes_2D;
        if (looping) mode |= MODE.LOOP_NORMAL;
        return ((IFModSoundBuilder) this).SetMode(mode);
    }

    IFModSoundBuilder IFModSoundBuilder.SetModeMusic(bool looping)
    {
        var mode = AudioUtils.StandardSoundModes_Stream;
        if (looping) mode |= MODE.LOOP_NORMAL;
        return ((IFModSoundBuilder) this).SetMode(mode);
    }

    IFModSoundBuilder IFModSoundBuilder.SetFadeDuration(float fadeDuration)
    {
        _fadeDuration = fadeDuration;
        return this;
    }

    // Registration

    void IFModSoundBuilder.Register()
    {
        // Throw exceptions and print warnings for common mistakes
        if (string.IsNullOrEmpty(_id))
            throw new SoundBuilderException($"{this}: Cannot register a sound with no ID!");

        if (string.IsNullOrEmpty(_bus))
            throw new SoundBuilderException($"{this}: Cannot register a sound with no bus!");

        if (_mode is null or MODE.DEFAULT)
            throw new SoundBuilderException($"{this}: Cannot register a sound with no mode assigned!");

        if (_mode.Value.HasFlag(MODE._2D) && _minAndMaxDistances.HasValue)
            InternalLogger.Warn(
                $"{this}: Sound '{_id}' is 2D but has a max distance assigned! This may have unexpected results.");

        // Registration
        // For single clips
        if (_clipName != null)
        {
            Sound sound;
            try
            {
                sound = _loader.LoadSound(_clipName, _mode.Value);
            }
            catch (Exception e)
            {
                throw new SoundBuilderException($"{this}: Exception thrown while loading sound '{_clipName}': {e}");
            }

            AssignSoundData(sound);

            CustomSoundHandler.RegisterCustomSound(_id, sound, _bus);
        }
        // For multiple clips
        else if (_clipNamesForMultipleSounds != null || _predicateForMultipleSounds != null)
        {
            Sound[] sounds;

            try
            {
                if (_predicateForMultipleSounds != null)
                {
                    sounds = _loader.LoadSoundsWithPredicate(_predicateForMultipleSounds, _mode.Value);
                }
                else
                {
                    sounds = _loader.LoadSounds(_clipNamesForMultipleSounds, _mode.Value);
                }
            }
            catch (Exception e)
            {
                throw new SoundBuilderException($"{this}: Exception thrown while loading sounds: {e}");
            }

            foreach (var sound in sounds)
            {
                AssignSoundData(sound);
            }

            var multiSoundsEvent = new FModMultiSounds(sounds, _bus, _randomizeSoundOrder);
            CustomSoundHandler.RegisterCustomSound(_id, multiSoundsEvent);
        }
        else
        {
            throw new SoundBuilderException($"{this}: Cannot register a sound with no sound clip names assigned!");
        }
    }

    private void AssignSoundData(Sound sound)
    {
        if (_minAndMaxDistances.HasValue)
        {
            sound.set3DMinMaxDistance(_minAndMaxDistances.Value.Item1, _minAndMaxDistances.Value.Item2);
        }

        if (_fadeDuration is > 0)
        {
            sound.AddFadeOut(_fadeDuration.Value);
        }
    }

    private void Reset()
    {
        _id = null;
        _bus = null;
        _mode = null;
        _minAndMaxDistances = null;
        _fadeDuration = null;
        _randomizeSoundOrder = false;
        _predicateForMultipleSounds = null;
    }

    /// <summary>
    /// Returns a string representing this object for debugging purposes.
    /// </summary>
    /// <returns>A string with some information to help identify the context of the object.</returns>
    public override string ToString()
    {
        if (_loader != null)
            return _loader + "SoundBuilder";
        return base.ToString();
    }

    private class SoundBuilderException : Exception
    {
        public SoundBuilderException(string message) : base(message)
        {
        }
    }
}