using System;
using System.Collections;
using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

/// <summary>
/// Enables and disables a custom GameObject in the main menu depending on what mod theme is selected
/// </summary>
public class WorldObjectTitleAddon : TitleAddon
{
    private Func<GameObject> _spawnObject;
    private GameObject _worldObject;

    /// <summary>
    /// Spawns in the specified <see cref="GameObject"/> when your mod is selected
    /// </summary>
    /// <param name="spawnObject">A function to get the object to enable. It is recommended to spawn your object in this method to ensure
    /// returning to the main menu from a save does not cause NREs</param>
    /// <param name="requiredGUIDs">The required mod GUIDs for this addon to enable. Each required mod must approve
    /// this addon by using <see cref="TitleScreenHandler.ApproveTitleCollaboration"/></param>
    public WorldObjectTitleAddon(Func<GameObject> spawnObject, params string[] requiredGUIDs) : base (requiredGUIDs)
    {
        _spawnObject = spawnObject;
    }

    /// <summary>
    /// Sets the correct sky appliers on the managed object
    /// </summary>
    public override void Initialize()
    {
        _worldObject = _spawnObject();
        UWE.CoroutineHost.StartCoroutine(SetupObjectSkyAppliers());
    }

    private IEnumerator SetupObjectSkyAppliers()
    {
        var menuLogo = GameObject.FindObjectOfType<MenuLogo>();

        yield return new WaitUntil(() => menuLogo.logoObject);

        var customSky = menuLogo.logoObject.GetComponent<SkyApplier>().customSkyPrefab;
        foreach (var applier in _worldObject.GetComponentsInChildren<SkyApplier>(true))
        {
            applier.customSkyPrefab = customSky;
            
            applier.Start();
        }
    }
    
    /// <summary>
    /// Enables the managed object
    /// </summary>
    public override void OnEnable()
    {
        _worldObject.SetActive(true);
    }

    /// <summary>
    /// Disables the managed object
    /// </summary>
    public override void OnDisable()
    {
        _worldObject.SetActive(false);
    }
}