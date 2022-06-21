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
        private static readonly Regex pattern
                = new Regex($@"^(?:[^\0]*(?:\/|\\))?(compiled-batch-(?<x>-?\d+)-(?<y>-?\d+)-(?<z>-?\d+)\.optoctrees)$");
        [HarmonyPatch(typeof(LargeWorldStreamer),nameof(LargeWorldStreamer.GetCompiledOctreesCachePath))]
        internal static class LargeWorldStreamer_GetCompiledOctreesCachePath_Patch
        {
            internal static bool Prefix(string filename,ref string __result)
            {
                var match = pattern.Match(filename);
                Int3 batchId;
                try
                {
                    int parse(string g) => int.Parse(match.Groups[g].Value);
                    batchId = new Int3(parse("x"), parse("y"), parse("z"));
                }
                catch
                {
                    Debug.LogError($"Game accessed batch file with invalid filename: '{filename}'");
                    return false;
                }
                for(int i = 0;i < BiomeThings.Variables.biomes.Count;i++)
                {
                    var biome = BiomeThings.Variables.biomes[i];
                    if(biome.batchIds.Contains(batchId))
                    {
                        __result = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"BiomeOctreeCache");
                    }
                    else
                    {
                        return true;
                    }
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

            InitializeSpawners();
            Logger.Debug("Coordinated Spawns have been initialized in the current save.");
            foreach(var biome in BiomeThings.Variables.biomes)
            {
                    for (var e = 0; e < biome.batchIds.Count(); e++)
                    {
                    var prefab = new GameObject($"Batch {biome.batchIds[e].x},{biome.batchIds[e].y},{biome.batchIds[e].z}");
                        var largeworldbatchroot = prefab.EnsureComponent<LargeWorldBatchRoot>();
                        largeworldbatchroot.atmospherePrefabClassId = biome.BiomeName;
                        var AtmoVolume = prefab.EnsureComponent<AtmosphereVolume>();
                        AtmoVolume.amb = biome.amblightsettings;
                        AtmoVolume.sun = biome.sunsettings;
                        AtmoVolume.fog = biome.fogsettings;
                        AtmoVolume.fadeRate = 0.1f;
                        AtmoVolume.overrideBiome = biome.BiomeName;
                        largeworldbatchroot.batchId = biome.batchIds[e];
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
                    int x = LargeWorldStreamer.main.blocksPerBatch.x;
                    Int3 @int = biome.batchIds[e] * x + new Int3(x / 2, x - 3, x / 2);
                    largeworldbatchroot.transform.position = LargeWorldStreamer.main.land.transform.TransformPoint(@int.ToVector3());
                        biome.batchroots.Add(prefab);
                        for (int i = 0; i < Directory.GetFiles(biome.OctreesPath).Count(); i++)
                        {
                            var filename = Path.GetFileName(Directory.GetFiles(biome.OctreesPath)[i]);
                            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BiomeOctreeCache", filename)))
                            {
                                File.Copy(Directory.GetFiles(biome.OctreesPath)[i], Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BiomeOctreeCache", filename));
                            }
                        }
                    }
            }
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
