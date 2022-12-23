using SMLHelper.V2.Utility;

namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Linq;
    using InternalLogger = InternalLogger;
    using Json.Converters;
    using System.Collections.Generic;
    using System.IO;
    using HarmonyLib;
    using Handlers;
    using MonoBehaviours;
    using UnityEngine;
    using Newtonsoft.Json;

    internal class LargeWorldStreamerPatcher
    {
        internal static void Patch(Harmony harmony)
        {
            var initializeOrig = AccessTools.Method(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.Initialize));
            var initPostfix = new HarmonyMethod(AccessTools.Method(typeof(LargeWorldStreamerPatcher), nameof(InitializePostfix)));
            harmony.Patch(initializeOrig, postfix: initPostfix);
        }

        internal static readonly List<SpawnInfo> spawnInfos = new List<SpawnInfo>();
        internal static readonly List<SpawnInfo> savedSpawnInfos = new List<SpawnInfo>();
        
        private static readonly List<SpawnInfo> initialSpawnInfos = new List<SpawnInfo>();

        private static bool initialized;

        private static void InitializePostfix()
        {
            InitializeSpawnInfos();
            
            var file = Path.Combine(SaveLoadManager.GetTemporarySavePath(), "CoordinatedSpawnsInitialized.smlhelper");
            if (File.Exists(file))
            {
                InternalLogger.Debug("Coordinated Spawns already been spawned in the current save. Loading Data");

                using var reader = new StreamReader(file);
                try
                {
                    var deserializedList = JsonConvert.DeserializeObject<List<SpawnInfo>>(reader.ReadToEnd(), new Vector3Converter(), new QuaternionConverter());
                    if (deserializedList is not null)
                        savedSpawnInfos.AddRange(deserializedList);

                    reader.Close();
                }
                catch (Exception ex)
                {
                    InternalLogger.Error($"Failed to load Saved spawn data from {file}\nSkipping static spawning until fixed!\n{ex}");
                    reader.Close();
                    return;
                }
            }

            foreach (var savedSpawnInfo in savedSpawnInfos)
            {
                if (spawnInfos.Contains(savedSpawnInfo))
                    spawnInfos.Remove(savedSpawnInfo);
            }

            InitializeSpawners();
            InternalLogger.Debug("Coordinated Spawns have been initialized in the current save.");
        }

        private static void SaveData()
        {
            var file = Path.Combine(SaveLoadManager.GetTemporarySavePath(), "CoordinatedSpawnsInitialized.smlhelper");
            using var writer = new StreamWriter(file);
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
                foreach (var spawnInfo in initialSpawnInfos.Where(spawnInfo => !spawnInfos.Contains(spawnInfo)))
                {
                    spawnInfos.Add(spawnInfo);
                }
                return;
            }
            
            initialSpawnInfos.AddRange(spawnInfos);
            IngameMenuHandler.RegisterOnSaveEvent(SaveData);
            initialized = true;
        }

        private static void InitializeSpawners()
        {
            foreach (var spawnInfo in spawnInfos)
            {
                CreateSpawner(spawnInfo);
            }
        }

        private static void CreateSpawner(SpawnInfo spawnInfo)
        {
            var keyToCheck = spawnInfo.Type switch
            {
                SpawnInfo.SpawnType.TechType => spawnInfo.TechType.AsString(),
                _ => spawnInfo.ClassId
            };

            var obj = new GameObject($"{keyToCheck}Spawner");
            obj.EnsureComponent<EntitySpawner>().spawnInfo = spawnInfo;
        }
    }
}
