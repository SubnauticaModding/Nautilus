using System.Collections;

namespace Nautilus.Patchers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Json.Converters;
using Nautilus.MonoBehaviours;
using Nautilus.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UWE;

internal static class LargeWorldStreamerPatcher
{
    internal static void Patch(Harmony harmony)
    {
        MethodInfo initializeOrig = AccessTools.Method(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.Initialize));
        HarmonyMethod initPostfix = new(AccessTools.Method(typeof(LargeWorldStreamerPatcher), nameof(InitializePostfix)));
        harmony.Patch(initializeOrig, postfix: initPostfix);

        MethodInfo onBatchFullyLoadedOrig = AccessTools.Method(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.OnBatchFullyLoaded));
        HarmonyMethod onBatchFullyLoadedPostfix = new(AccessTools.Method(typeof(LargeWorldStreamerPatcher), nameof(OnBatchFullyLoadedPostfix)));
        harmony.Patch(onBatchFullyLoadedOrig, postfix: onBatchFullyLoadedPostfix);
    }
    
    internal static readonly HashSet<SpawnInfo> SpawnInfos = new();
    internal static readonly HashSet<SpawnInfo> SavedSpawnInfos = new();
    internal static readonly Dictionary<Int3, HashSet<SpawnInfo>> BatchToSpawnInfos = new();

    private static readonly HashSet<SpawnInfo> _initialSpawnInfos = new();

    private static bool _initialized;

    private static void InitializePostfix()
    {
        InitializeSpawnInfos();

        string file = Path.Combine(SaveLoadManager.GetTemporarySavePath(), "CoordinatedSpawnsInitialized.nautilus");
        if (File.Exists(file))
        {
            InternalLogger.Debug("Coordinated Spawns already been spawned in the current save. Loading Data");

            using StreamReader reader = new(file);
            try
            {
                List<SpawnInfo> deserializedList = JsonConvert.DeserializeObject<List<SpawnInfo>>(reader.ReadToEnd(), new Vector3Converter(), new QuaternionConverter());
                if (deserializedList is not null)
                {
                    SavedSpawnInfos.AddRange(deserializedList);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                InternalLogger.Error($"Failed to load Saved spawn data from {file}\nSkipping Coordinated spawning until fixed!\n{ex}");
                reader.Close();
                return;
            }
        }

        SpawnInfos.RemoveWhere(s => SavedSpawnInfos.Contains(s));
        InternalLogger.Debug("Coordinated Spawns have been initialized in the current save.");

        var globalSpawns = new HashSet<SpawnInfo>();

        // Preload all the prefabs for faster spawning.
        new List<SpawnInfo>(SpawnInfos).Do((info) =>
        {
            string keyToCheck = info.Type switch
            {
                SpawnInfo.SpawnType.TechType => CraftData.GetClassIdForTechType(info.TechType),
                _ => info.ClassId
            };
            if (!PrefabDatabase.TryGetPrefabFilename(keyToCheck, out string prefabName))
            {
                InternalLogger.Error($"Failed to get prefab name for {keyToCheck}; process for Coordinated Spawn canceled.");
                SpawnInfos.Remove(info);
                return;
            }

            if (PrefabHandler.Prefabs.TryGetInfoForFileName(prefabName, out var prefabInfo))
            {
                InternalLogger.Debug($"Preloading {keyToCheck}");
                CoroutineHost.StartCoroutine(PreloadModdedPrefab(info, prefabInfo));
            }
            else
            {
                var task = new AssetReferenceGameObject(prefabName).LoadAssetAsync();
                task.Completed += (t) =>
                {
                    if (t.Status != AsyncOperationStatus.Succeeded)
                    {
                        InternalLogger.Error($"Failed to preload {keyToCheck} with error: {t.OperationException}");
                        return;
                    }

                    var prefab = t.Result;
                    if (prefab == null)
                    {
                        InternalLogger.Error($"no prefab found for {keyToCheck}; process for Coordinated Spawn canceled.");
                        SpawnInfos.Remove(info);
                        return;
                    }

                    LargeWorldEntity lwe = prefab.GetComponent<LargeWorldEntity>();
                    if (lwe is null)
                    {
                        InternalLogger.Error($"No LargeWorldEntity found on {keyToCheck}; Please ensure the prefab has a LargeWorldEntity component when using Coordinated Spawns.");
                        lwe = prefab.AddComponent<LargeWorldEntity>();
                    }

                    if (lwe is { cellLevel: LargeWorldEntity.CellLevel.Global })
                    {
                        globalSpawns.Add(info);
                        InternalLogger.Debug($"Init: Created spawner for {keyToCheck} at the Global level.");
                        SpawnInfos.Remove(info);
                    }

                    InternalLogger.Debug($"Preloaded {keyToCheck}");
                };
            }
        });

        CreateSpawner(LargeWorldStreamer.main.GetContainingBatch(Vector3.zero), globalSpawns, true);

        foreach (var spawnInfo in SpawnInfos)
        {
            var batch = LargeWorldStreamer.main.GetContainingBatch(spawnInfo.SpawnPosition);

            if (globalSpawns.Contains(spawnInfo))
            {
                continue;
            }
            
            BatchToSpawnInfos.GetOrAddNew(batch).Add(spawnInfo);
        }
    }
    
    private static void OnBatchFullyLoadedPostfix(Int3 batchId)
    {
        if (BatchToSpawnInfos.TryGetValue(batchId, out var spawnInfos))
        {
            CreateSpawner(batchId, spawnInfos, false);
            SpawnInfos.RemoveRange(spawnInfos);
            BatchToSpawnInfos.Remove(batchId);
        }
    }

    private static IEnumerator PreloadModdedPrefab(SpawnInfo info, PrefabInfo prefabInfo)
    {
        var request = new ModPrefabRequest(prefabInfo);
        yield return request;

        if (!request.TryGetPrefab(out var prefab))
        {
            InternalLogger.Error($"no prefab found for {prefabInfo.ClassID}; process for Coordinated Spawn canceled.");
            SpawnInfos.Remove(info);
            yield break;
        }
        LargeWorldEntity lwe = prefab.GetComponent<LargeWorldEntity>();
        if (lwe is null)
        {
            InternalLogger.Error($"No LargeWorldEntity found on {prefabInfo.ClassID}; Please ensure the prefab has a LargeWorldEntity component when using Coordinated Spawns.");
            lwe = prefab.AddComponent<LargeWorldEntity>();
        }
        if (lwe is { cellLevel: LargeWorldEntity.CellLevel.Global })
        {
            var batch = LargeWorldStreamer.main.GetContainingBatch(info.SpawnPosition);
            CreateSpawner(batch, new []{info}, true);
            InternalLogger.Debug($"Preload: Created spawner for {info.ClassId} at the Global level.");
            SpawnInfos.Remove(info);
            if (BatchToSpawnInfos.TryGetValue(batch, out var spawnInfos))
            {
                spawnInfos.Remove(info);
            }
        }
    }

    private static void SaveData()
    {
        string file = Path.Combine(SaveLoadManager.GetTemporarySavePath(), "CoordinatedSpawnsInitialized.nautilus");
        using StreamWriter writer = new(file);
        try
        {
            string data = JsonConvert.SerializeObject(SavedSpawnInfos, Formatting.Indented, new Vector3Converter(), new QuaternionConverter());
            writer.Write(data);
            writer.Flush();
            writer.Close();
        }
        catch (Exception ex)
        {
            InternalLogger.Error($"Failed to save spawn data to {file}\n{ex}");
            writer.Close();
        }
    }

    // We keep an initial copy of the spawn infos so Coordinated Spawns also works if you quit to main menu.
    private static void InitializeSpawnInfos()
    {
        if (_initialized)
        {
            // we already have an initialSpawnInfos initialized, refresh our spawnInfos List.
            SavedSpawnInfos.Clear();
            foreach (SpawnInfo spawnInfo in _initialSpawnInfos.Where(spawnInfo => !SpawnInfos.Contains(spawnInfo)))
            {
                SpawnInfos.Add(spawnInfo);
            }
            return;
        }

        _initialSpawnInfos.AddRange(SpawnInfos);
        SaveUtils.RegisterOnSaveEvent(SaveData);
        _initialized = true;
    }

    private static void CreateSpawner(Int3 batch, IReadOnlyCollection<SpawnInfo> spawnInfos, bool global)
    {
        var centerPosition = LargeWorldStreamer.main.GetBatchCenter(batch);
        InternalLogger.Debug($"Creating Spawner for batch: {batch} at {centerPosition}");
        GameObject obj = new($"{batch} Spawner");
        obj.transform.position = centerPosition;
        var spawner = obj.EnsureComponent<EntitySpawner>();
        spawner.batchId = batch;
        spawner.spawnInfos = spawnInfos;
        spawner.global = global;
    }
}