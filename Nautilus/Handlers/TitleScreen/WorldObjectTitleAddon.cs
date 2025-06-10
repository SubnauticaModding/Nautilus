using System.Collections;
using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public class WorldObjectTitleAddon : TitleAddon
{
    private GameObject _worldObject;

    public WorldObjectTitleAddon(GameObject worldObject)
    {
        _worldObject = worldObject;
    }

    public override void Initialize()
    {
        UWE.CoroutineHost.StartCoroutine(SetupObjectSkyAppliers());
    }

    protected virtual IEnumerator SetupObjectSkyAppliers()
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
    
    public override void OnEnable()
    {
        _worldObject.SetActive(true);
    }

    public override void OnDisable()
    {
        _worldObject.SetActive(false);
    }
}