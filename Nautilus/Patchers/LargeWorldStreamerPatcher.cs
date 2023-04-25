using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Json.Converters;
using Nautilus.MonoBehaviours;
using Nautilus.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace Nautilus.Patchers;

internal class LargeWorldStreamerPatcher
{
    internal static void Patch(Harmony harmony)
    {
        System.Reflection.MethodInfo initializeOrig = AccessTools.Method(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.Initialize));
        HarmonyMethod initPostfix = new(AccessTools.Method(typeof(LargeWorldStreamerPatcher), nameof(InitializePostfix)));
        harmony.Patch(initializeOrig, postfix: initPostfix);
    }

    internal static readonly HashSet<SpawnInfo> spawnInfos = new();
    internal static readonly HashSet<SpawnInfo> savedSpawnInfos = new();
        
    private static readonly HashSet<SpawnInfo> initialSpawnInfos = new();

    private static bool initialized;

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
                    savedSpawnInfos.AddRange(deserializedList);
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

        spawnInfos.RemoveWhere(s => savedSpawnInfos.Contains(s));

        InitializeSpawners();
        InternalLogger.Debug("Coordinated Spawns have been initialized in the current save.");
    }

    private static void SaveData()
    {
        string file = Path.Combine(SaveLoadManager.GetTemporarySavePath(), "CoordinatedSpawnsInitialized.nautilus");
        using StreamWriter writer = new(file);
        try
        {
            string data = JsonConvert.SerializeObject(savedSpawnInfos, Formatting.Indented, new Vector3Converter(), new QuaternionConverter());
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
        if (initialized)
        {
            // we already have an initialSpawnInfos initialized, refresh our spawnInfos List.
            savedSpawnInfos.Clear();
            foreach (SpawnInfo spawnInfo in initialSpawnInfos.Where(spawnInfo => !spawnInfos.Contains(spawnInfo)))
            {
                spawnInfos.Add(spawnInfo);
            }
            return;
        }
            
        initialSpawnInfos.AddRange(spawnInfos);
        SaveUtils.RegisterOnSaveEvent(SaveData);
        initialized = true;
    }

    private static void InitializeSpawners()
    {
        foreach (SpawnInfo spawnInfo in spawnInfos)
        {
            CreateSpawner(spawnInfo);
        }
    }

    internal static void CreateSpawner(SpawnInfo spawnInfo)
    {
        string keyToCheck = spawnInfo.Type switch
        {
            SpawnInfo.SpawnType.TechType => spawnInfo.TechType.AsString(),
            _ => spawnInfo.ClassId
        };

        GameObject obj = new($"{keyToCheck}Spawner");
        obj.EnsureComponent<EntitySpawner>().spawnInfo = spawnInfo;
    }
}