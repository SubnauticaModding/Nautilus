using System;
using System.Collections.Generic;
using FMOD;

namespace Nautilus.FMod;

/// <summary>
/// The base class used for loading sounds.
/// </summary>
/// <remarks>
/// Used by the <see cref="FModSoundBuilder"/> to determine how it locates and loads its sounds.
/// </remarks>
public abstract class CustomSoundSourceBase
{
    private readonly string _identifier;

    private bool _loadedAllSoundNames;
    private Dictionary<string, CachedSound> _soundNames;
    
    /// <summary>
    /// The base constructor for a custom sound source.
    /// </summary>
    /// <param name="identifier">Used for debugging and identification purposes.</param>
    protected CustomSoundSourceBase(string identifier)
    {
        _identifier = identifier;
    }
    
    internal abstract Sound LoadSound(string soundName, MODE mode);

    internal abstract Sound[] LoadSounds(string[] soundNames, MODE mode);
    
    internal abstract Sound[] LoadSoundsWithPredicate(Func<string, bool> predicate, MODE mode);

    /// <summary>
    /// Fetches the names of all valid sounds in the source. Is expected to only be called once.
    /// </summary>
    /// <returns>Must return a list of all valid sound names.</returns>
    protected abstract Dictionary<string, CachedSound> InitializeCachedSounds();

    /// <summary>
    /// Returns the 'identifier' as determined by the constructor.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return _identifier ?? "UnknownSoundSource";
    }
    
    /// <summary>
    /// Gets or creates a dictionary of all valid sound names.
    /// </summary>
    /// <returns>A dictionary that contains the name of every sound name. The key is the truncated 'name' of the sound,
    /// equivalent to <see cref="CachedSound.Name"/>.</returns>
    protected Dictionary<string, CachedSound> GetAllCachedSounds()
    {
        if (_loadedAllSoundNames)
            return _soundNames;
        _soundNames = InitializeCachedSounds();
        _loadedAllSoundNames = true;
        return _soundNames;
    }

    /// <summary>
    /// Contains the locator and truncated name of a sound file.
    /// </summary>
    protected readonly struct CachedSound
    {
        /// <summary>
        /// The full name of the sound file, including the file extension and path if applicable. 
        /// </summary>
        public string Locator { get; }
        /// <summary>
        /// The shortened name of the sound file, used for predicates and comparisons.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The primary constructor.
        /// </summary>
        /// <param name="locator">The full, unprocessed name of the sound. Could be the file path or a unique ID.</param>
        /// <param name="name">A truncated name of the sound to be used in predicates.</param>
        public CachedSound(string name, string locator)
        {
            Locator = locator;
            Name = name;
        }
    }
}