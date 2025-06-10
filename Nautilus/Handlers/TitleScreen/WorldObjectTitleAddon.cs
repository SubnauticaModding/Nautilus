using System;
using Nautilus.Assets;
using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public class WorldObjectTitleAddon : TitleAddon
{
    private readonly Func<GameObject> _worldObjectPrefab;
    private readonly SpawnLocation _spawnLocation;
    private GameObject _worldObject;
    
    public WorldObjectTitleAddon(Func<GameObject> worldObject, SpawnLocation spawnLocation)
    {
        _worldObjectPrefab = worldObject;
        if (spawnLocation.Scale == default)
        {
            spawnLocation = new SpawnLocation(spawnLocation.Position, spawnLocation.EulerAngles, Vector3.one);
        }
        
        _spawnLocation = spawnLocation;
    }

    public override void Initialize(GameObject functionalityRoot)
    {
        _worldObject = GameObject.Instantiate(_worldObjectPrefab());
        _worldObject.transform.position = _spawnLocation.Position;
        _worldObject.transform.rotation = Quaternion.Euler(_spawnLocation.EulerAngles);
        _worldObject.transform.localScale = _spawnLocation.Scale;
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