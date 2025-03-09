using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FMOD;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.FMod;

public class FModSoundBuilderLoader
{
    private readonly Mode _mode;
    
    // Asset Bundle mode

    private readonly AssetBundle _bundle;
    
    // Mod Folder mode

    private readonly string _directoryName;
    private readonly Assembly _assembly;
    
    // Constructors
    
    public FModSoundBuilderLoader(AssetBundle bundle)
    {
        _mode = Mode.AssetBundle;
        _bundle = bundle;
    }

    public FModSoundBuilderLoader(string directoryName, Assembly overrideAssembly = null)
    {
        _mode = Mode.AssetBundle;
        _directoryName = directoryName;
        _assembly = overrideAssembly ?? Assembly.GetCallingAssembly();
    }
    
    // Main methods

    internal Sound LoadSound(string soundName, MODE mode)
    {
        switch (_mode)
        {
            case Mode.AssetBundle:
                return AudioUtils.CreateSound(_bundle.LoadAsset<AudioClip>(soundName), mode);
            case Mode.ModFolder:
                throw new NotImplementedException();
            default:
                throw new Exception("Invalid load mode used: " + _mode);
        }
    }
    
    internal Sound[] LoadSounds(string[] soundNames, MODE mode)
    {
        switch (_mode)
        {
            case Mode.AssetBundle:
                var clipList = new List<AudioClip>();
                soundNames.ForEach(clipName => clipList.Add(_bundle.LoadAsset<AudioClip>(clipName)));
                var sounds = AudioUtils.CreateSounds(clipList, mode).ToArray();
                return sounds.ToArray();
            case Mode.ModFolder:
                throw new NotImplementedException();
            default:
                throw new Exception("Invalid load mode used: " + _mode);
        }
    }
    
    // Other definitions

    private enum Mode
    {
        Undefined,
        AssetBundle,
        ModFolder
    }
}