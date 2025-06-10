namespace Nautilus.Handlers.TitleScreen;

public class MusicTitleAddon : TitleAddon
{
    private readonly FMOD_CustomEmitter _emitter;
    
    public MusicTitleAddon(FMOD_CustomEmitter emitter)
    {
        _emitter = emitter;
    }

    public override void OnEnable()
    {
        _emitter.Play();
    }

    public override void OnDisable()
    {
        _emitter.Stop();
    }
}