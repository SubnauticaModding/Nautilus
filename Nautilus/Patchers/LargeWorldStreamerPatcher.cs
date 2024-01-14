namespace Nautilus.Patchers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json.Converters;
using Nautilus.MonoBehaviours;
using Nautilus.Utility;
using Newtonsoft.Json;
using UnityEngine;
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
                InternalLogger.Error($"Failed to load Saved spawn data from {file}\nSkipping static spawning until fixed!\n{ex}");
                reader.Close();
                return;
            }
        }

        SpawnInfos.RemoveWhere(s => SavedSpawnInfos.Contains(s));
        InternalLogger.Debug("Coordinated Spawns have been initialized in the current save.");

        // Preload all the prefabs for faster spawning.
        SpawnInfos.Do((info) =>
        {
            // Ensures the prefab is loaded in the cache for faster spawning.
            CoroutineHost.StartCoroutine(EntitySpawner.GetPrefabAsync(new TaskResult<GameObject>(), info));
        });
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

    private static void OnBatchFullyLoadedPostfix(LargeWorldStreamer __instance, Int3 batchId)
    {
        var spawned = new HashSet<SpawnInfo>();
        foreach (SpawnInfo spawnInfo in SpawnInfos)
        {
            if (__instance.GetContainingBatch(spawnInfo.SpawnPosition) == batchId)
            {
                if (CreateSpawner(spawnInfo, __instance))
                {
                    spawned.Add(spawnInfo);
                }
            }
        }

        spawned.Do((info) =>
        {
            SavedSpawnInfos.Add(info);
            SpawnInfos.Remove(info);
        });
    }

    private static bool CreateSpawner(SpawnInfo spawnInfo, LargeWorldStreamer streamer)
    {
        string keyToCheck = spawnInfo.Type switch
        {
            SpawnInfo.SpawnType.TechType => spawnInfo.TechType.AsString(),
            _ => spawnInfo.ClassId
        };

        InternalLogger.Debug($"Creating Spawner for {keyToCheck}");
        GameObject obj = new($"{keyToCheck}Spawner");

        obj.SetActive(false);

        obj.transform.SetPositionAndRotation(spawnInfo.SpawnPosition, spawnInfo.Rotation);
        LargeWorldEntity lwe = obj.EnsureComponent<LargeWorldEntity>();
        lwe.cellLevel = LargeWorldEntity.CellLevel.Batch;
        obj.EnsureComponent<EntitySpawner>().spawnInfo = spawnInfo;

        if (!streamer.cellManager.RegisterEntity(lwe))
        {
            GameObject.Destroy(obj);
            return false;
        }

        obj.SetActive(true);
        return true;
    }
}