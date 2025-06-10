using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public class MusicTitleAddon : TitleAddon
{
    protected readonly FMODAsset _asset;
    protected FMOD_CustomEmitter _customEmitter;
    
    public MusicTitleAddon(FMODAsset asset)
    {
        _asset = asset;
    }

    public override void Initialize()
    {
        var functionalityRoot = MainMenuMusic.main.gameObject;
        _customEmitter = functionalityRoot.AddComponent<FMOD_CustomLoopingEmitter>();
        _customEmitter.asset = _asset;
        _customEmitter.restartOnPlay = true;
    }

    public override void OnEnable()
    {
        _customEmitter.Play();
    }

    public override void OnDisable()
    {
        _customEmitter.Stop();
    }
}