namespace SMLHelper.V2.MonoBehaviours
{
    using System.Collections;
    using Handlers;
    using Patchers;
    using UnityEngine;
    using UWE;
    using Logger = Logger;

    internal class EntitySpawner : MonoBehaviour
    {
        internal SpawnInfo spawnInfo;
        

        void Start() 
        { 
            StartCoroutine(SpawnAsync());
        }

        IEnumerator SpawnAsync()
        {
            var stringToLog = spawnInfo.spawnType == SpawnInfo.SpawnType.ClassId
                ? spawnInfo.classId
                : spawnInfo.techType.ToString();
            
            var task = new TaskResult<GameObject>();
            yield return GetPrefabAsync(task);

            var prefab = task.Get();

            if (prefab == null)
            {
                Logger.Error($"no prefab found for {stringToLog}; process for Coordinated Spawn canceled.");
                Destroy(gameObject);
            }


            var obj = Utils.InstantiateDeactivated(prefab, spawnInfo.spawnPosition, spawnInfo.rotation);

            var lwe = obj.GetComponent<LargeWorldEntity>();

            // non-global objects cannot be spawned in unloaded terrain so we need to wait
            if (lwe == null || lwe?.cellLevel is not (LargeWorldEntity.CellLevel.Batch or LargeWorldEntity.CellLevel.Global))
            {
                var lws = LargeWorldStreamer.main;
                yield return new WaitUntil(() => lws.IsReady()); // first we make sure the world streamer is initialized
                var batch = lws.GetContainingBatch(spawnInfo.spawnPosition);
                yield return new WaitUntil(() => lws.IsBatchReadyToCompile(batch)); // then we wait until the terrain is fully loaded (must be checked on each frame for faster spawns)
            }

            LargeWorld.main.streamer.cellManager.RegisterEntity(obj);
            
            obj.SetActive(true);

            LargeWorldStreamerPatcher.savedSpawnInfos.Add(spawnInfo);
            
            Destroy(gameObject);
        }

        IEnumerator GetPrefabAsync(IOut<GameObject> gameObject)
        {
            GameObject obj;
            
            if (spawnInfo.spawnType == SpawnInfo.SpawnType.ClassId) // Spawn is via ClassID.
            {
                var request = PrefabDatabase.GetPrefabAsync(spawnInfo.classId);
                yield return request;

                request.TryGetPrefab(out obj);
            }
            else // spawn is via TechType.
            {
                var task = CraftData.GetPrefabForTechTypeAsync(spawnInfo.techType);
                yield return task;

                obj = task.GetResult();
            }
            
            gameObject.Set(obj);
        }
    }
}
