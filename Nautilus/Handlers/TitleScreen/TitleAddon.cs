namespace Nautilus.Handlers.TitleScreen;

/// <summary>
/// A custom title addon for the main menu. Inherit from this class to create your own main menu functionality.
/// </summary>
public abstract class TitleAddon
{
    /// <summary>
    /// The GUID of the mod that owns this addon.
    /// </summary>
    protected internal string ModGuid { get; internal set; }
    
    /// <summary>
    /// Whether the addon is currently enabled.
    /// </summary>
    protected internal bool IsEnabled { get; internal set; }
    
    /// <summary>
    /// The required mod GUIDs for this addon to be enabled.
    /// </summary>
    protected internal string[] RequiredGUIDs { get; }

    /// <summary>
    /// Creates a new instance of TitleAddon.
    /// </summary>
    protected TitleAddon()
    {
        
    }
    
    /// <summary>
    /// Creates a new instance of TitleAddon.
    /// </summary>
    /// <param name="requiredGUIDs">The required mod GUIDs for this addon to enable. Each required mod must approve
    /// this addon by using <see cref="TitleScreenHandler.ApproveTitleCollaboration"/>.</param>
    protected TitleAddon(string[] requiredGUIDs)
    {
        RequiredGUIDs = requiredGUIDs;
    }

    /// <summary>
    /// Runs initialization code once before the main menu is loaded to set up required fields.
    /// </summary>
    public virtual void Initialize() { }
    
    /// <summary>
    /// Called when the addon is enabled. This occurs when your mod is selected as the current theme.
    /// </summary>
    public abstract void OnEnable();
    
    /// <summary>
    /// Called when the addon is disabled. This occurs when your mod is deselected as the current theme, or
    /// if the addon inherits from <see cref="MusicTitleAddon"/> and the game starts.
    /// </summary>
    public abstract void OnDisable();
}