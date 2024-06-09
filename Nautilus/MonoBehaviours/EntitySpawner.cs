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

    private IEnumerator Start()
    {
        yield return SpawnAsync();
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
                InternalLogger.Error($"No LargeWorldEntity component found for prefab '{stringToLog}'; process for Coordinated Spawn canceled.");
                continue;
            }

            GameObject obj = Instantiate(prefab, spawnInfo.SpawnPosition, spawnInfo.Rotation);
            obj.transform.localScale = spawnInfo.ActualScale;

            obj.SetActive(true);

            spawnInfo.OnSpawned?.Invoke(obj);

            LargeWorldEntity.Register(obj);

            LargeWorldStreamerPatcher.SavedSpawnInfos.Add(spawnInfo);
            InternalLogger.Debug($"spawned {stringToLog}.");
        }
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