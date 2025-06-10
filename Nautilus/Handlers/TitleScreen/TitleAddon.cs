using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public abstract class TitleAddon
{
    public string modGUID;
    public bool isEnabled;
    public string[] requiredGUIDs;

    protected TitleAddon()
    {
        
    }
    
    protected TitleAddon(string[] requiredGUIDs)
    {
        this.requiredGUIDs = requiredGUIDs;
    }

    public virtual void Initialize() { }
    public abstract void OnEnable();
    public abstract void OnDisable();
}