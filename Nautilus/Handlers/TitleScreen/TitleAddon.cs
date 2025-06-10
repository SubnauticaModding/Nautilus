using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public abstract class TitleAddon
{
    public bool isEnabled;
    
    public virtual void Initialize() { }
    public abstract void OnEnable();
    public abstract void OnDisable();
}