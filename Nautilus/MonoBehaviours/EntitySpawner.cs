namespace Nautilus.MonoBehaviours;

using System.Collections;
using System.Collections.Generic;
using Nautilus.Handlers;
using Nautilus.Patchers;
using Nautilus.Utility;
using UnityEngine;
using UWE;

internal class EntitySpawner : MonoBehaviour
{
    internal Int3 batchId;
    internal IReadOnlyCollection<SpawnInfo> spawnInfos;
    internal bool global;
    private List<SpawnInfo> delayedSpawns = new List<SpawnInfo>();

    private IEnumerator Start()
    {
        yield return SpawnAsync();
        yield return new WaitUntil(() => delayedSpawns.Count == 0);
        Destroy(gameObject);
    }

    private IEnumerator SpawnAsync()
    {
        LargeWorldStreamer lws = LargeWorldStreamer.main;
        yield return new WaitUntil(() => lws != null && lws.IsReady()); // first we make sure the world streamer is initialized

        if (!global)
        {
            // then we wait until the terrain is fully loaded (must be checked on each frame for faster spawns)
            yield return new WaitUntil(() => lws.IsBatchReadyToCompile(batchId));
        }

        LargeWorld lw = LargeWorld.main;
        
        yield return new WaitUntil(() => lw != null && lw.streamer.globalRoot != null); // need to make sure global root is ready too for global spawns.

        foreach (var spawnInfo in spawnInfos)
        {
            string stringToLog = spawnInfo.Type switch
            {
                SpawnInfo.SpawnType.ClassId => spawnInfo.ClassId,
                _ => spawnInfo.TechType.AsString()
            };
            
            InternalLogger.Debug($"Spawning {stringToLog}");
            
            TaskResult<GameObject> task = new();
            yield return GetPrefabAsync(spawnInfo, task);

            GameObject prefab = task.Get();
            if (prefab == null)
            {
                InternalLogger.Error($"no prefab found for {stringToLog}; process for Coordinated Spawn canceled.");
                continue;
            }

            LargeWorldEntity lwe = prefab.GetComponent<LargeWorldEntity>();

            if (!lwe)
            {
                // @Metious: I don't think this is necessary because LargeWorldEntity.Register(obj) Ensures that the object has a LargeWorldEntity component so we could replace 
                // lwe.cellLevel < LargeWorldEntity.CellLevel.Batch with (lwe == null || lwe.cellLevel < LargeWorldEntity.CellLevel.Batch) and it should work fine.
                InternalLogger.Error($"No LargeWorldEntity component found for prefab '{stringToLog}'; process for Coordinated Spawn canceled.");
                continue;
                // 😎 Nice.
            }

            if (lwe.cellLevel < LargeWorldEntity.CellLevel.Batch && !lws.IsRangeActiveAndBuilt(new Bounds(spawnInfo.SpawnPosition, Vector3.one)))
            {
                // Cells aren't ready yet. We have to wait until they are.
                StartCoroutine(WaitForCellLoaded(lws, prefab, spawnInfo, stringToLog));
                continue;
            }
            
            Spawn(prefab, spawnInfo, stringToLog);
        }
    }

    private IEnumerator WaitForCellLoaded(LargeWorldStreamer lws, GameObject prefab, SpawnInfo spawnInfo, string stringToLog)
    {
        delayedSpawns.Add(spawnInfo);
        yield return new WaitUntil(() => lws.IsRangeActiveAndBuilt(new Bounds(spawnInfo.SpawnPosition, Vector3.one * 5)));
        Spawn(prefab, spawnInfo, stringToLog);
        delayedSpawns.Remove(spawnInfo);
    }

    private void Spawn(GameObject prefab, SpawnInfo spawnInfo, string stringToLog)
    {
        GameObject obj = Instantiate(prefab, spawnInfo.SpawnPosition, spawnInfo.Rotation);
        obj.transform.localScale = spawnInfo.ActualScale;

        obj.SetActive(true);

        LargeWorldEntity.Register(obj);

        LargeWorldStreamerPatcher.SavedSpawnInfos.Add(spawnInfo);
        InternalLogger.Debug($"spawned {stringToLog}.");
    }

    private IEnumerator GetPrefabAsync(SpawnInfo spawnInfo, IOut<GameObject> gameObject)
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