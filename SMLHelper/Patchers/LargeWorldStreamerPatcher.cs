namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Linq;
    using Logger = Logger;
    using Json.Converters;
    using System.Collections.Generic;
    using System.IO;
    using HarmonyLib;
    using Handlers;
    using MonoBehaviours;
    using UnityEngine;
#if SUBNAUTICA_STABLE
    using Oculus.Newtonsoft.Json;
#else
    using Newtonsoft.Json;
#endif

    internal class LargeWorldStreamerPatcher
    {
        internal static void Patch(Harmony harmony)
        {
            var initializeOriginal = AccessTools.Method(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.Initialize));
            var postfix = new HarmonyMethod(AccessTools.Method(typeof(LargeWorldStreamerPatcher), nameof(InitializePostfix)));

            harmony.Patch(initializeOriginal, postfix: postfix);
        }

        internal static readonly List<SpawnInfo> spawnInfos = new List<SpawnInfo>();
        internal static readonly List<SpawnInfo> savedSpawnInfos = new List<SpawnInfo>();

        private static void InitializePostfix()
        {
            var file = Path.Combine(SaveLoadManager.GetTemporarySavePath(), "CoordinatedSpawnsInitialized.smlhelper");
            if (File.Exists(file))
            {
                // already initialized, return to prevent from spawn duplications.
                Logger.Debug("Coordinated Spawns already been spawned in the current save. Loading Data");

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
                    Logger.Error($"Failed to load Saved spawn data from {file}\nSkipping static spawning until fixed!\n{ex}");
                    reader.Close();
                    return;
                }
            }

            foreach (var savedSpawnInfo in savedSpawnInfos)
            {
                if (spawnInfos.Contains(savedSpawnInfo))
                    spawnInfos.Remove(savedSpawnInfo);
            }

            IngameMenuHandler.RegisterOneTimeUseOnSaveEvent(() => SaveData());

            Initialize();
            Logger.Debug("Coordinated Spawns have been initialized in the current save.");
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
                Logger.Error($"Failed to save spawn data to {file}\nSkipping static spawning until fixed!\n{ex}");
                writer.Close();
            }
        }

        private static void Initialize()
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
