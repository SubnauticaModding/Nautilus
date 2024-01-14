using System.Collections;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Patchers;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace Nautilus.MonoBehaviours;

internal class EntitySpawner : MonoBehaviour
{
    internal SpawnInfo spawnInfo;

    void Start()
    {
        StartCoroutine(SpawnAsync());
    }

    IEnumerator SpawnAsync()
    {
        string stringToLog = spawnInfo.Type switch
        {
            SpawnInfo.SpawnType.ClassId => spawnInfo.ClassId,
            _ => spawnInfo.TechType.AsString()
        };

        TaskResult<GameObject> task = new();
        yield return GetPrefabAsync(task, spawnInfo);

        GameObject prefab = task.Get();
        if (prefab == null)
        {
            InternalLogger.Error($"no prefab found for {stringToLog}; process for Coordinated Spawn canceled.");
            Destroy(gameObject);
            yield break;
        }

        LargeWorldEntity lwe = prefab.GetComponent<LargeWorldEntity>();

        if (lwe == null)
        {
            InternalLogger.Error($"no LargeWorldEntity found for {stringToLog}; process for Coordinated Spawn canceled.");
            Destroy(gameObject);
            yield break;
        }

        GameObject obj = UWE.Utils.InstantiateDeactivated(prefab, spawnInfo.SpawnPosition, spawnInfo.Rotation, spawnInfo.ActualScale);
        lwe = obj.GetComponent<LargeWorldEntity>();

        yield return new WaitUntil(()=> LargeWorld.main?.streamer?.cellManager?.RegisterEntity(lwe)?? false); // then we register the entity to the cell manager

        obj.SetActive(true);
        Destroy(gameObject);
    }

    internal static IEnumerator GetPrefabAsync(IOut<GameObject> gameObject, SpawnInfo spawnInfo)
    {
        GameObject obj;

        if (spawnInfo.Type == SpawnInfo.SpawnType.ClassId) // Spawn is via ClassID.
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(spawnInfo.ClassId);
            yield return request;

            request.TryGetPrefab(out obj);
        }
        else // spawn is via TechType.
        {
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(spawnInfo.TechType);
            yield return task;

            obj = task.GetResult();
        }

        gameObject.Set(obj);
    }
}