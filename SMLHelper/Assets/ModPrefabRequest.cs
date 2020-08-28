namespace SMLHelper.V2.Assets
{
    using System.Collections;
    using UnityEngine;
    using UWE;

    // request for getting ModPrefab asynchronously
    internal class ModPrefabRequest: IPrefabRequest, IEnumerator
    {
        private readonly ModPrefab modPrefab;

        private CoroutineTask<GameObject> task;
        private TaskResult<GameObject> taskResult;

        public ModPrefabRequest(ModPrefab modPrefab)
        {
            this.modPrefab = modPrefab;
        }

        private void Init()
        {
            if (task != null)
                return;

            taskResult = new TaskResult<GameObject>();
            task = new CoroutineTask<GameObject>(modPrefab.GetGameObjectInternalAsync(taskResult), taskResult);
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
            return task.GetResult() == null; // TODO: possible infinite loop ?
        }

        public void Reset() {}
    }
}
