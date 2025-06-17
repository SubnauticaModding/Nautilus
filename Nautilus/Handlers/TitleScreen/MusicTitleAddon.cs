namespace Nautilus.Handlers.TitleScreen;

/// <summary>
/// Enables and disables custom music depending on what mod theme is selected.
/// </summary>
public class MusicTitleAddon : TitleAddon
{
    /// <summary>
    /// The FMOD asset passed into the instance when created.
    /// </summary>
    protected readonly FMODAsset _asset;
    
    /// <summary>
    /// The custom emitter created in <see cref="MusicTitleAddon.Initialize"/>.
    /// </summary>
    protected FMOD_CustomEmitter _customEmitter;
    
    /// <summary>
    /// Creates a new <see cref="MusicTitleAddon"/>. The music will be enabled when your mod is selected and disabled when the game starts or
    /// if your theme is unselected.
    /// </summary>
    /// <param name="asset">The FMOD asset for the music you want to play. A fade out time is HIGHLY recommended.</param>
    /// <param name="requiredGUIDs">The required mod GUIDs for this addon to enable. Each required mod must approve
    /// this addon by using <see cref="TitleScreenHandler.ApproveTitleCollaboration"/>.</param>
    public MusicTitleAddon(FMODAsset asset, params string[] requiredGUIDs) : base (requiredGUIDs)
    {
        _asset = asset;
    }

    /// <summary>
    /// Sets up the FMOD looping emitter with the provided asset.
    /// </summary>
    public override void Initialize()
    {
        var functionalityRoot = MainMenuMusic.main.gameObject;
        _customEmitter = functionalityRoot.AddComponent<FMOD_CustomLoopingEmitter>();
        _customEmitter.asset = _asset;
        _customEmitter.restartOnPlay = true;
    }

    /// <summary>
    /// Enables the music.
    /// </summary>
    public override void OnEnable()
    {
        _customEmitter.Play();
    }

    /// <summary>
    /// Disables the music.
    /// </summary>
    public override void OnDisable()
    {
        _customEmitter.Stop();
    }
}