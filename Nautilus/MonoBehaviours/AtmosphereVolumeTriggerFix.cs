using UnityEngine;

namespace Nautilus.MonoBehaviours;

// In the base game, atmosphere volumes will not trigger while you're inside a vehicle. This file fixes that issue on custom atmosphere volumes (can be opted out).
internal class AtmosphereVolumeTriggerFix : MonoBehaviour
{
    public AtmosphereVolume atmosphereVolume;
    
    private void Start()
    {
        InvokeRepeating(nameof(CheckTriggerEnter), Random.value, 3f);
    }

    private void CheckTriggerEnter()
    {
        if (atmosphereVolume.settingsActive || !isActiveAndEnabled)
        {
            return;
        }
        var playerObject = Player.mainObject;
        if (playerObject == null) return;
        if (atmosphereVolume.Contains(playerObject.transform.position))
        {
            atmosphereVolume.PushSettings();
        }
    }
}