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

            if (task.Get() is null)
            {
                Logger.Error($"no prefab found for {stringToLog}; process for Coordinated Spawn canceled.");
                Destroy(gameObject);
            }

            var obj = Utils.InstantiateDeactivated(task.Get(), spawnInfo.spawnPosition, spawnInfo.rotation);

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