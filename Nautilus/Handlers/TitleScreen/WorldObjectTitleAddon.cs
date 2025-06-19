using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Handlers.TitleScreen;

/// <summary>
/// Enables and disables a custom GameObject in the main menu depending on what mod theme is selected.
/// </summary>
public class WorldObjectTitleAddon : TitleAddon, IManagedUpdateBehaviour
{
    /// <summary>
    /// The func to spawn the world object. Set in the constructor.
    /// </summary>
    protected readonly Func<GameObject> SpawnObject;
    
    /// <summary>
    /// The duration that it takes for an object to fade in when enabled/disabled.
    /// </summary>
    protected readonly float FadeInTime;
    
    /// <summary>
    /// The object that is spawned in the world and managed.
    /// </summary>
    protected GameObject WorldObject;

    private Renderer[] _fadeRenderers;
    private Graphic[] _fadeGraphics;
    private float _currentFadeInTime;
    private bool _fadingIn;
    
    /// <summary>
    /// Spawns in the specified <see cref="GameObject"/> when your mod is selected.
    /// </summary>
    /// <param name="spawnObject">A function to get the object to enable. It is recommended to spawn your object in this method to ensure
    /// returning to the main menu from a save does not cause NREs.</param>
    /// <param name="fadeInTime">The duration of the fade in for enabling/disabling objects.
    /// The spawnObject must have SN shaders applied for this to work.</param>
    /// <param name="requiredGUIDs">The required mod GUIDs for this addon to enable. Each required mod must approve
    /// this addon by using <see cref="TitleScreenHandler.ApproveTitleCollaboration"/>.</param>
    public WorldObjectTitleAddon(Func<GameObject> spawnObject, float fadeInTime = 1f, params string[] requiredGUIDs) : base (requiredGUIDs)
    {
        SpawnObject = spawnObject;
        FadeInTime = fadeInTime;
    }
 
    /// <summary>
    /// Sets the correct sky appliers on the managed object.
    /// </summary>
    protected override void OnInitialize()
    {
        WorldObject = SpawnObject();
        UWE.CoroutineHost.StartCoroutine(SetupObjectSkyAppliers());
        WorldObject.SetActive(false);

        _fadeRenderers = WorldObject.GetComponentsInChildren<Renderer>(true);
        _fadeGraphics = WorldObject.GetComponentsInChildren<Graphic>(true);
    }

    private IEnumerator SetupObjectSkyAppliers()
    {
        var menuLogo = GameObject.FindObjectOfType<MenuLogo>();

        yield return new WaitUntil(() => menuLogo.logoObject);

        var customSky = menuLogo.logoObject.GetComponent<SkyApplier>().customSkyPrefab;
        foreach (var applier in WorldObject.GetComponentsInChildren<SkyApplier>(true))
        {
            applier.anchorSky = Skies.Custom;
            applier.customSkyPrefab = customSky;
            
            applier.Start();
        }
    }
    
    /// <summary>
    /// Enables the managed object.
    /// </summary>
    protected override void OnEnable()
    {
        BehaviourUpdateUtils.Register(this);
        _currentFadeInTime = 0;
        _fadingIn = true;
    }

    /// <summary>
    /// Disables the managed object.
    /// </summary>
    protected override void OnDisable()
    {
        if (!_fadingIn)
            _currentFadeInTime = 0;
        else
            _currentFadeInTime = FadeInTime - _currentFadeInTime;
        _fadingIn = false;
    }

    private void UpdateObjectOpacities(float alpha)
    {
        if (!WorldObject) return;

        foreach (var rend in _fadeRenderers)
        {
            rend.SetFadeAmount(alpha);
        }
        
        foreach (var graphic in _fadeGraphics)
        {
            var col = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            graphic.color = col;
        }
    }

    string IManagedBehaviour.GetProfileTag()
    {
        return "WorldObjectTitleAddon";
    }

    /// <summary>
    /// Called every frame while registered.
    /// </summary>
    public virtual void ManagedUpdate()
    {
        if (!WorldObject)
        {
            BehaviourUpdateUtils.Deregister(this);
            return;
        }
        
        if (_currentFadeInTime < FadeInTime)
        {
            _currentFadeInTime += Time.deltaTime;
            float normalizedProgress = _currentFadeInTime / Mathf.Max(FadeInTime, float.Epsilon);
            UpdateObjectOpacities(_fadingIn ? normalizedProgress : 1 - normalizedProgress);

            if (!_fadingIn && normalizedProgress >= 1)
            {
                WorldObject.SetActive(false);
            }
            else if (_fadingIn && normalizedProgress > 0)
            {
                WorldObject.SetActive(true);
            }
        }
        else if (!_fadingIn)
        {
            BehaviourUpdateUtils.Deregister(this);
        }
    }

    int IManagedUpdateBehaviour.managedUpdateIndex { get; set; }
}