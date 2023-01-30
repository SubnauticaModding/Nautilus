using SMLHelper.Handlers;
using SMLHelper.Utility;

namespace SMLHelper.Assets
{
    using System.Collections;
    using UnityEngine;
    using UWE;

    // request for getting ModPrefab asynchronously
    internal class ModPrefabRequest: IPrefabRequest, IEnumerator
    {
        private readonly PrefabInfo prefabInfo;

        private int state = 0;
        private CoroutineTask<GameObject> task;
        private TaskResult<GameObject> taskResult;

        public ModPrefabRequest(PrefabInfo prefabInfo)
        {
            this.prefabInfo = prefabInfo;
        }

        private void Init()
        {
            if (task != null)
            {
                return;
            }

            taskResult = new TaskResult<GameObject>();
            if (!PrefabHandler.Prefabs.TryGetPrefabForInfo(prefabInfo, out var factory))
            {
                InternalLogger.Error($"Couldn't find a prefab for the following prefab info: {prefabInfo}.");
                return;
            }
            
            task = new CoroutineTask<GameObject>(factory.Invoke(taskResult), taskResult);
        }

        public object Current
        {
            get
            {
                Init();
                return task;
            }
        }

        public bool TryGetPrefab(out GameObject result)
        {
            result = taskResult.Get();
            return result != null;
        }

        public bool MoveNext()
        {
            Init();
            return state++ == 0;
        }

        public void Reset() {}

        public void Release()
        {
        }
    }
}
