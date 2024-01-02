using System.Collections;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace Nautilus.Assets;

// request for getting ModPrefab asynchronously
internal class ModPrefabRequest: IPrefabRequest
{
    internal bool Done { get; private set; }
    
    private readonly PrefabInfo prefabInfo;
    
    private IEnumerator task;
    
    private TaskResult<GameObject> taskResult;

    public ModPrefabRequest(PrefabInfo prefabInfo)
    {
        this.prefabInfo = prefabInfo;
        ModPrefabCache.Requests[prefabInfo.ClassID] = this;
    }

    private void Init()
    {
        if ((task != null && !Done) || (Done && TryGetPrefab(out _)))
        {
            return;
        }

        taskResult = new TaskResult<GameObject>();
        if (!PrefabHandler.Prefabs.TryGetPrefabForInfo(prefabInfo, out var factory))
        {
            InternalLogger.Error($"Couldn't find a prefab for the following prefab info: {prefabInfo}.");
            return;
        }
            
        task = PrefabHandler.GetPrefabAsync(taskResult, prefabInfo, factory);
    }

    public object Current
    {
        get
        {
            Init();
            return task.Current;
        }
    }

    public bool TryGetPrefab(out GameObject result)
    {
        return ModPrefabCache.TryGetPrefabFromCache(prefabInfo.ClassID, out result) && result != null;
    }

    public bool MoveNext()
    {
        Init();
        if (!task.MoveNext())
        {
            Done = true;
        }
        return !Done;
    }

    public void Reset()
    {
        task.Reset();
        Done = false;
    }

    public void Release()
    {
        taskResult = null;
        task = null;
        Done = false;
    }
}