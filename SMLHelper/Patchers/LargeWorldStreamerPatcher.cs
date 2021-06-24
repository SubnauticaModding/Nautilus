namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Linq;
    using Logger = Logger;
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
        internal static List<SpawnInfo> savedSpawnInfos = new List<SpawnInfo>();
        
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
                    savedSpawnInfos = JsonConvert.DeserializeObject<List<SpawnInfo>>(reader.ReadToEnd()) ?? new List<SpawnInfo>();
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
            
            IngameMenuHandler.RegisterOneTimeUseOnSaveEvent(() => SaveData(file));

            Initialize();
            Logger.Debug("Coordinated Spawns have been initialized in the current save.");
        }

        private static void SaveData(string file)
        {
            using var writer = new StreamWriter(file);
            try
            {
                string data = JsonConvert.SerializeObject(savedSpawnInfos, Formatting.Indented,
                    new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
                //Logger.Info(data);
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

        private static void CreateSpawner(SpawnInfo sp)
        {
            var keyToCheck = sp.spawnType == SpawnInfo.SpawnType.TechType ? sp.techType.AsString() : sp.classId;
            
            var obj = new GameObject($"{keyToCheck}Spawner");
            obj.EnsureComponent<EntitySpawner>().spawnInfo = sp;
        }
    }
}