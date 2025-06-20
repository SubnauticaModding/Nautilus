using System;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.FMod;

/// <summary>
/// Enables loading sounds from an asset bundle by the name of AudioClips.
/// </summary>
public class AssetBundleSoundSource : CustomSoundSourceBase
{
    private readonly AssetBundle _bundle;

    private bool _cachedClips;
    private Dictionary<string, AudioClip> _cachedAudioClips;

    /// <summary>
    /// Creates a sound source that loads sounds from the given asset bundle. 
    /// </summary>
    /// <param name="bundle">The bundle containing the audio clips to be accessed.</param>
    public AssetBundleSoundSource(AssetBundle bundle) : base(bundle != null ? bundle.name : "AssetBundle")
    {
        _bundle = bundle;
    }

    internal override Sound LoadSound(string soundName, MODE mode)
    {
        return AudioUtils.CreateSound(GetAudioClipByLocator(soundName), mode);
    }

    internal override Sound[] LoadSounds(string[] soundNames, MODE mode)
    {
        var clipList = new List<AudioClip>();
        soundNames.ForEach(clipName => clipList.Add(GetAudioClipByLocator(clipName)));
        var sounds = AudioUtils.CreateSounds(clipList, mode).ToArray();
        return sounds.ToArray();
    }

    internal override Sound[] LoadSoundsWithPredicate(Func<string, bool> predicate, MODE mode)
    {
        var soundNames = GetAllCachedSounds();
        var clipList = new List<AudioClip>();
        foreach (var soundName in soundNames)
        {
            if (predicate.Invoke(soundName.Value.Name))
            {
                clipList.Add(GetAudioClipByLocator(soundName.Value.Locator));
            }
        }

        var sounds = AudioUtils.CreateSounds(clipList, mode).ToArray();
        return sounds.ToArray();
    }

    private AudioClip GetAudioClipByLocator(string locator)
    {
        if (!_cachedClips)
        {
            return _bundle.LoadAsset<AudioClip>(locator);
        }

        if (!_cachedAudioClips.TryGetValue(locator, out var clip))
        {
            throw new Exception($"Failed to find Audio Clip by locator '{locator}'");
        }

        return clip;
    }

    /// <summary>
    /// Creates and returns a list of all AudioClip names in the asset bundle.
    /// </summary>
    /// <returns>A list of all sound names.</returns>
    protected override Dictionary<string, CachedSound> InitializeCachedSounds()
    {
        var soundNames = new Dictionary<string, CachedSound>();
        _cachedAudioClips = new Dictionary<string, AudioClip>();
        var allClips = _bundle.LoadAllAssets<AudioClip>();
        foreach (var clip in allClips)
        {
            _cachedAudioClips.Add(clip.name, clip);
            soundNames.Add(clip.name, new CachedSound(clip.name, clip.name));
        }

        _cachedClips = true;
        return soundNames;
    }
}