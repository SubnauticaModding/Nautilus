using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FMOD;
using Nautilus.Utility;

namespace Nautilus.FMod;

/// <summary>
/// Enables loading sounds from a folder by the name of each sound file, EXCLUDING extensions.
/// </summary>
public class ModFolderSoundSource : CustomSoundSourceBase
{
    private readonly string _soundFolderPath;

    /// <summary>
    /// Enables an <see cref="FModSoundBuilder"/> to load its sounds from the given folder path inside the mod directory.
    /// </summary>
    /// <param name="soundFileFolder">The name of or path to the folder that contains the sound files.
    /// Relative to the mod folder (the folder containing <paramref name="overrideAssembly"/>).</param>
    /// <param name="overrideAssembly">The assembly of the mod, used for locating the mod folder.
    /// If left null, this is automatically resolved to the calling assembly.
    /// Using a cached reference can be slightly more performant.</param>
    /// <remarks>Recursive folder structures are not supported.</remarks>
    public ModFolderSoundSource(string soundFileFolder, Assembly overrideAssembly = null) : base(
        (overrideAssembly == null ? Assembly.GetExecutingAssembly() : overrideAssembly).GetName().Name)
    {
        var assembly = overrideAssembly ?? Assembly.GetCallingAssembly();
        _soundFolderPath = Path.Combine(Path.GetDirectoryName(assembly.Location), soundFileFolder);
    }

    internal override Sound LoadSound(string soundName, MODE mode)
    {
        var cached = GetAllCachedSounds();
        if (!cached.TryGetValue(soundName, out var sound))
        {
            throw new Exception(
                $"Failed to find sound file by name '{soundName}' (did you forget to exclude the file extension?)");
        }

        return AudioUtils.CreateSound(GetFilePathOfSoundByLocator(sound.Locator), mode);
    }

    internal override Sound[] LoadSounds(string[] soundNames, MODE mode)
    {
        var cached = GetAllCachedSounds();
        var filePaths = new string[soundNames.Length];
        
        for (int i = 0; i < soundNames.Length; i++)
        {
            if (!cached.TryGetValue(soundNames[i], out var sound))
            {
                throw new Exception(
                    $"Failed to find sound file by name '{soundNames[i]}' (did you forget to exclude the file extension?)");
            }
            filePaths[i] = GetFilePathOfSoundByLocator(sound.Locator);
        }

        return AudioUtils.CreateSounds(filePaths, mode).ToArray();
    }

    internal override Sound[] LoadSoundsWithPredicate(Func<string, bool> predicate, MODE mode)
    {
        var cached = GetAllCachedSounds();
        var soundLocators = new List<string>();
        foreach (var soundName in cached)
        {
            if (predicate.Invoke(soundName.Value.Name))
            {
                soundLocators.Add(GetFilePathOfSoundByLocator(soundName.Value.Locator));
            }
        }

        return AudioUtils.CreateSounds(soundLocators, mode).ToArray();
    }

    /// <summary>
    /// Gets all files in the folder and caches their names.
    /// </summary>
    /// <returns>A list of all sound names from the folder.</returns>
    protected override Dictionary<string, CachedSound> InitializeCachedSounds()
    {
        var files = Directory.GetFiles(_soundFolderPath);
        var names = new Dictionary<string, CachedSound>();
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            names.Add(name, new CachedSound(name, Path.GetFileName(file)));
        }

        return names;
    }

    private string GetFilePathOfSoundByLocator(string locator)
    {
        return Path.Combine(_soundFolderPath, locator);
    }
}