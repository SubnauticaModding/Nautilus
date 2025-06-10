using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public abstract class TitleAddon
{
    public bool isEnabled;
    
    public virtual void Initialize(GameObject functionalityRoot) { }
    public abstract void OnEnable();
    public abstract void OnDisable();
}