using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.MonoBehaviours;
using UnityEngine;

namespace SMLHelper.V2.Patchers
{
    [HarmonyPatch(typeof(LargeWorldStreamer))]
    internal class LargeWorldStreamerPatcher
    {
        internal static void Patch(Harmony harmony)
        {
            var initializeOriginal = AccessTools.Method(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.Initialize));
            var postfix = new HarmonyMethod(AccessTools.Method(typeof(LargeWorldStreamerPatcher), nameof(InitializePostfix)));

            harmony.Patch(initializeOriginal, postfix: postfix);
        }
        
        internal static List<SpawnInfo> spawnInfos = new List<SpawnInfo>();
        
        static void InitializePostfix()
        {
            var file = Path.Combine(SaveLoadManager.GetTemporarySavePath(), "CoordinatedSpawnsInitialized.smlhelper");
            if (File.Exists(file))
            {
                // already initialized, return to prevent from spawn duplications.
                Logger.Debug("Coordinated Spawns already been Initialized in the current save.");
                return;
            }
            
            Initialize();
            
            File.Create(file);
            
            Logger.Debug("Coordinated Spawns have been initialized in the current save.");
        }

        static void Initialize()
        {
            foreach (var spawnInfo in spawnInfos)
            {
                if (spawnInfo.techType != TechType.None)
                {
                    CreateSpawner(true, spawnInfo);
                }
                else if (!string.IsNullOrEmpty(spawnInfo.classId))
                {
                    CreateSpawner(false, spawnInfo);
                }
            }
        }

        static void CreateSpawner(bool checkTechType, SpawnInfo sp)
        {
            var keyToCheck = checkTechType ? sp.techType.ToString() : sp.classId;
            
            var obj = new GameObject($"{keyToCheck}Spawner");
            obj.EnsureComponent<EntitySpawner>().spawnInfo = sp;
        }
    }
}