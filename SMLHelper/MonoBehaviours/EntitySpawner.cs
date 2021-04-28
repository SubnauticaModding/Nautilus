using System.Collections;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;

namespace SMLHelper.V2.MonoBehaviours
{
    public class EntitySpawner : MonoBehaviour
    {
        public SpawnInfo spawnInfo;

        void Start() 
        { 
            StartCoroutine(SpawnAsync());
        }

        IEnumerator SpawnAsync()
        {
            var task = new TaskResult<GameObject>();
            yield return GetPrefabAsync(task);

            var obj = UWE.Utils.InstantiateDeactivated(task.Get(), spawnInfo.spawnPosition, Quaternion.identity);
            
            LargeWorld.main.streamer.cellManager.RegisterEntity(obj);
            
            obj.SetActive(true);
            
            Destroy(gameObject);
        }

        IEnumerator GetPrefabAsync(IOut<GameObject> gameObject)
        {
            GameObject obj = null;
            
            if (!string.IsNullOrEmpty(spawnInfo.classId)) // if classid isn't null, then we get the prefab using it
            {
                var request = PrefabDatabase.GetPrefabAsync(spawnInfo.classId);
                yield return request;

                if (!request.TryGetPrefab(out obj))
                    Logger.Error($"no prefab found for ClassID: {spawnInfo.classId}");
            }
            else if (spawnInfo.techType != TechType.None) // if techtype isn't null, then we get the prefab using it
            {
                var task = CraftData.GetPrefabForTechTypeAsync(spawnInfo.techType);
                yield return task;

                obj = task.GetResult();
            }
            
            gameObject.Set(obj);
        }
    }
}