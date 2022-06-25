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
    using System.Reflection.Emit;
    using System.Reflection;
#if SUBNAUTICA_STABLE
    using Oculus.Newtonsoft.Json;
    using System.Collections;
    using System.Text.RegularExpressions;
#else
    using Newtonsoft.Json;
#endif
    [HarmonyPatch]
    internal class LargeWorldStreamerPatcher
    {
        [HarmonyPatch(typeof(LargeWorldStreamer),nameof(LargeWorldStreamer.LoadBatchThreaded))]
        internal static class LargeWorldStreamer_LoadBatchThreaded_Patch
        {
            [HarmonyPrefix]
            internal static bool Prefix(BatchCells batchCells)
            {
                var shouldContinue = false;
                BiomeThings.Biome containingBiome = null;
                for (var e = 0; e < BiomeThings.Variables.biomes.Count; e++)
                {
                    var biome = BiomeThings.Variables.biomes[e];
                    if (biome.batchIds.Contains(batchCells.batch))
                    {
                        shouldContinue = true;
                        containingBiome = biome;
                        break;
                    }
                }
                if (shouldContinue)
                {
                    var instantiatedgo = UnityEngine.GameObject.Instantiate(containingBiome.batchroots[batchCells.batch]);
                    LargeWorldStreamer.main.OnBatchObjectsLoaded(batchCells.batch, instantiatedgo);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(LargeWorldStreamer),nameof(LargeWorldStreamer.CheckBatch))]
        internal static class LargeWorldStreamer_CheckBatch_Patch
        {
            [HarmonyPostfix]
            internal static void Postfix(ref bool __result,Int3 batch)
            {
                for(int e = 0;e < BiomeThings.Variables.biomes.Count; e++)
                {
                    var biome = BiomeThings.Variables.biomes[e];
                    if(biome.batchIds.Contains(batch))
                    {
                        __result = true;
                        break;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(LargeWorldStreamer),nameof(LargeWorldStreamer.FinalizeLoadBatchObjectsAsync))]
        internal static class LargeWorldStreamer_FinializeLoadBatchObjectsAsync_Patch
        {
            [HarmonyPrefix]
            internal static bool Prefix(Int3 index)
            {
                var shouldContinue = false;
                BiomeThings.Biome containingBiome = null;
                for (var e = 0; e < BiomeThings.Variables.biomes.Count; e++)
                {
                    var biome = BiomeThings.Variables.biomes[e];
                    if (biome.batchIds.Contains(index))
                    {
                        shouldContinue = true;
                        containingBiome = biome;
                        break;
                    }
                }
                if (shouldContinue)
                {
                    var instantiatedgo = UnityEngine.GameObject.Instantiate(containingBiome.batchroots[index]);
                    LargeWorldStreamer.main.OnBatchObjectsLoaded(index, instantiatedgo);
                    return false;
                }
                return true;
            }
        }
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
            foreach(var biome in BiomeThings.Variables.biomes)
            {
                Patchers.EnumPatching.BiomeTypePatcher.AddBiomeType(biome.BiomeName);
                for (var e = 0; e < biome.batchTerrains.Count; e++)
                    {
                    var BatchTerrain = biome.batchTerrains.ElementAt(e);
                   var prefab = BatchTerrain.Value;
                    
                        var largeworldbatchroot = prefab.EnsureComponent<LargeWorldBatchRoot>();
                        largeworldbatchroot.atmospherePrefabClassId = biome.BiomeName;
                   
                        var AtmoVolume = prefab.EnsureComponent<AtmosphereVolume>();
                        AtmoVolume.amb = biome.amblightsettings;
                        AtmoVolume.sun = biome.sunsettings;
                        AtmoVolume.fog = biome.fogsettings;
                        AtmoVolume.fadeRate = 0.1f;
                        AtmoVolume.overrideBiome = biome.BiomeName;
                    AtmoVolume.affectsVisuals = true;                        
                        largeworldbatchroot.batchId = BatchTerrain.Key;
                        largeworldbatchroot.amb = biome.amblightsettings;
                        largeworldbatchroot.sun = biome.sunsettings;
                        largeworldbatchroot.fog = biome.fogsettings;
                        largeworldbatchroot.fadeRate = 0.1f;
                    var collider = biome.GetCollider().GetComponent<Collider>();
                    if(collider is SphereCollider)
                    {
                        var sphereCollider = collider as SphereCollider;
                        var sphereCollider_ = prefab.EnsureComponent<SphereCollider>();
                        sphereCollider_.center = sphereCollider.center;
                        sphereCollider_.radius = sphereCollider.radius;
                        sphereCollider_.isTrigger = true;
                    }else if(collider is BoxCollider)
                    {
                        var boxCollider = collider as BoxCollider;
                        var boxCollider_ = prefab.EnsureComponent<BoxCollider>();
                        boxCollider_.center = boxCollider.center;
                        boxCollider_.size = boxCollider.size;
                        boxCollider_.isTrigger = true;
                    }else if(collider is CapsuleCollider)
                    {
                        var capsuleCollider = collider as CapsuleCollider;
                        var capsuleCollider_ = prefab.EnsureComponent<CapsuleCollider>();
                        capsuleCollider_.center = capsuleCollider.center;
                        capsuleCollider_.direction = capsuleCollider.direction;
                        capsuleCollider_.radius = capsuleCollider.radius;
                        capsuleCollider_.height = capsuleCollider.height;
                        capsuleCollider_.isTrigger = true;
                    }
                    spawnInfos.AddRange(biome.SpawnInfos);
                    biome.batchroots.Add(BatchTerrain.Key,prefab);
                    }
            }
            InitializeSpawners();
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
                Logger.Error($"Failed to save spawn data to {file}\n{ex}");
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
