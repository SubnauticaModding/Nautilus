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

    private void Start()
    {
        StartCoroutine(SpawnAsync());
    }

    private IEnumerator SpawnAsync()
    {
        string stringToLog = spawnInfo.Type switch
        {
            SpawnInfo.SpawnType.ClassId => spawnInfo.ClassId,
            _ => spawnInfo.TechType.AsString()
        };

        TaskResult<GameObject> task = new();
        yield return GetPrefabAsync(task);

        GameObject prefab = task.Get();
        if (prefab == null)
        {
            InternalLogger.Error($"no prefab found for {stringToLog}; process for Coordinated Spawn canceled.");
            Destroy(gameObject);
            yield break;
        }

        LargeWorldEntity lwe = prefab.GetComponent<LargeWorldEntity>();

        if (!lwe)
        {
            InternalLogger.Error($"No LargeWorldEntity component found for prefab '{stringToLog}'; process for Coordinated Spawn canceled.");
            Destroy(gameObject);
            yield break;
        }

        LargeWorldStreamer lws = LargeWorldStreamer.main;
        yield return new WaitUntil(() => lws != null && lws.IsReady()); // first we make sure the world streamer is initialized

        if (lwe is {cellLevel: not (LargeWorldEntity.CellLevel.Batch or LargeWorldEntity.CellLevel.Global)})
        {
            Int3 batch = lws.GetContainingBatch(spawnInfo.SpawnPosition);
            yield return new WaitUntil(() => lws.IsBatchReadyToCompile(batch)); // then we wait until the terrain is fully loaded (must be checked on each frame for faster spawns)

            var bounds = new Bounds(spawnInfo.SpawnPosition, Vector3.zero);
            
            if (!lws.cellManager.AreCellsLoaded(bounds, lwe.cellLevel))
            {
                // Cells aren't ready yet. We have to wait until they are
                yield return new WaitUntil(() => lws.cellManager.AreCellsLoaded(bounds, lwe.cellLevel));
            }
        }
        
        LargeWorld lw = LargeWorld.main;

        yield return new WaitUntil(() => lw != null && lw.streamer.globalRoot != null); // need to make sure global root is ready too for global spawns.
        
        GameObject obj = Instantiate(prefab, spawnInfo.SpawnPosition, spawnInfo.Rotation);
        obj.transform.localScale = spawnInfo.ActualScale;
        
        obj.SetActive(true);
        
        LargeWorldEntity.Register(obj);

        LargeWorldStreamerPatcher.SavedSpawnInfos.Add(spawnInfo);
        InternalLogger.Debug($"spawned {stringToLog}.");
        
        Destroy(gameObject);
    }

    private IEnumerator GetPrefabAsync(IOut<GameObject> gameObject)
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