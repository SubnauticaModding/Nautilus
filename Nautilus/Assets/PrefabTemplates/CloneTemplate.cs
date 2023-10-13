namespace Nautilus.Assets.PrefabTemplates;

using System.Collections;
using System.Collections.Generic;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UWE;

/// <summary>
/// Represents a prefab clone template.
/// </summary>
public class CloneTemplate : PrefabTemplate
{
    private string _classIdToClone;
    private AssetReferenceGameObject _prefabToClone;
    private TechType _techTypeToClone;
    private SpawnType _spawnType;

    /// <summary>
    /// Reskinning model data to apply to the clone.
    /// </summary>
    public List<CustomModelData> ModelDatas { get; } = new();
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more.
    /// </summary>
    public System.Action<GameObject> ModifyPrefab { get; set; }
    
    /// <summary>
    /// Callback that will get called after the prefab is retrieved. Use this to modify or process your prefab further more asynchronously.
    /// </summary>
    public System.Func<GameObject, IEnumerator> ModifyPrefabAsync { get; set; }
    
    /// <summary>
    /// Creates a <see cref="CloneTemplate"/> instance.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="techTypeToClone">The tech type to clone and use for this template.</param>
    public CloneTemplate(PrefabInfo info, TechType techTypeToClone) : this(info, techTypeToClone, null, null) {}
    
    /// <summary>
    /// Creates a <see cref="CloneTemplate"/> instance.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="classIdToClone">The class ID to clone and use for this template.</param>
    public CloneTemplate(PrefabInfo info, string classIdToClone) : this(info, TechType.None, classIdToClone, null) {}

    /// <summary>
    /// Creates a <see cref="CloneTemplate"/> instance.
    /// </summary>
    /// <param name="info">The prefab info to base this template off of.</param>
    /// <param name="prefabToClone">The AssetReferenceGameObject with a valid key to clone and use for this template.</param>
    public CloneTemplate(PrefabInfo info, AssetReferenceGameObject prefabToClone) : this(info, TechType.None, null, prefabToClone) { }



    private CloneTemplate(PrefabInfo info, TechType techTypeToClone, string classIdToClone, AssetReferenceGameObject prefabToClone) : base(info)
    {
        _techTypeToClone = techTypeToClone;
        _classIdToClone = classIdToClone;
        _prefabToClone = prefabToClone;
        _spawnType = techTypeToClone is not TechType.None ? SpawnType.TechType
            : !string.IsNullOrWhiteSpace(classIdToClone) ? SpawnType.ClassId
            : prefabToClone is not null ? SpawnType.Prefab
            : throw new System.Exception("CloneTemplate Missing valid identifier.");
    }

    /// <summary>
    /// Gets the appropriate cloned prefab.
    /// </summary>
    /// <param name="gameObject">The cloned prefab is set into this argument.<br/>
    /// If the provided task result already has a game object, this method will only call the <see cref="ModifyPrefab"/> callback on it.
    /// Otherwise; Creates a prefab clone, then runs the <see cref="ModifyPrefab"/> callback.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    public override IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject)
    {
        // If the provided task result already has a game object set to it, only modify it instead.
        GameObject obj = gameObject.Get();
        if (obj)
        {
            ApplySkin(obj);
            ModifyPrefab?.Invoke(obj);
            if(ModifyPrefabAsync is { })
                yield return ModifyPrefabAsync(obj);

            yield break;
        }

        if (_spawnType == SpawnType.TechType)
        {
            yield return CraftData.InstantiateFromPrefabAsync(_techTypeToClone, gameObject);
            obj = gameObject.Get();
        }
        else if(_spawnType == SpawnType.Prefab)
        {
            var task = _prefabToClone.InstantiateAsync();
            yield return task;
            
            if (task.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                InternalLogger.Error($"Couldn't find prefab with key: '{_prefabToClone.RuntimeKey}'.");
                yield break;
            }

            obj = task.Result;
        }
        else if(_spawnType == SpawnType.ClassId)
        {
            var task = PrefabDatabase.GetPrefabAsync(_classIdToClone);
            yield return task;
            
            if (!task.TryGetPrefab(out var prefab))
            {
                InternalLogger.Error($"Couldn't find class ID: '{_classIdToClone}'.");
                yield break;
            }

            obj = Object.Instantiate(prefab);
        }

        ApplySkin(obj);
        ModifyPrefab?.Invoke(obj);
        if (ModifyPrefabAsync is { })
            yield return ModifyPrefabAsync(obj);

        gameObject.Set(obj);
    }

    private void ApplySkin(GameObject obj)
    {
        if (ModelDatas.Count <= 0) 
            return;

        foreach (var modelData in ModelDatas)
        {
            var renderers = string.IsNullOrWhiteSpace(modelData.TargetPath)
                ? obj.GetAllComponentsInChildren<Renderer>()
                : obj.transform.Find(modelData.TargetPath)?.gameObject.GetComponentsInChildren<Renderer>();
            
            if (renderers == null)
            {
                InternalLogger.Warn($"{info.ClassID} unable to find {modelData.TargetPath} on {obj.name} when trying to apply CustomModelData.");
                continue;
            }
            
            foreach (var renderer in renderers)
            {
                if (modelData.CustomTexture != null)
                    renderer.material.SetTexture(ShaderPropertyID._MainTex, modelData.CustomTexture);

                if (modelData.CustomNormalMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._BumpMap, modelData.CustomNormalMap);

                if (modelData.CustomSpecMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._SpecTex, modelData.CustomSpecMap);

                if (modelData.CustomIllumMap != null)
                {
                    renderer.material.SetTexture(ShaderPropertyID._Illum, modelData.CustomIllumMap);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrength, modelData.CustomIllumStrength);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, modelData.CustomIllumStrength);
                }
            }
        }
    }

    private enum SpawnType
    {
        TechType,
        ClassId,
        Prefab
    }
}