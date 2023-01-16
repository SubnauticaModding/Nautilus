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
            task = new CoroutineTask<GameObject>(prefabInfo.GetGameObjectInternalAsync(taskResult), taskResult);
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
