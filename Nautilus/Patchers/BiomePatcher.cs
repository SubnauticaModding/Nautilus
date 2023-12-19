using System.Collections.Generic;
using HarmonyLib;
using mset;
using Nautilus.Handlers;
using UnityEngine;

namespace Nautilus.Patchers;

internal static class BiomePatcher
{
    private static readonly List<CustomBiomeData> CustomBiomes = new();
    private static readonly List<CustomBiomeSoundData> CustomBiomeSoundDatas = new();

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(BiomePatcher));
    }

    internal static void RegisterBiome(CustomBiomeData biome)
    {
        CustomBiomes.Add(biome);

        var manager = WaterBiomeManager.main;
        if (manager != null)
        {
            AddBiomeToWaterBiomeManager(manager, biome);
        }
    }

    internal static void RegisterBiomeSoundData(CustomBiomeSoundData biomeSoundData)
    {
        CustomBiomeSoundDatas.Add(biomeSoundData);

        // Water ambience is the parent of all biome sounds. If it exists, we missed our chance to add it automatically, so add it now.
        if (Player.main == null)
            return;
        var waterAmbienceComponent = Player.main.GetComponentInChildren<WaterAmbience>();
        if (waterAmbienceComponent != null)
        {
            AddBiomeSoundEmitterToWaterAmbience(waterAmbienceComponent, biomeSoundData);
        }
    }

    [HarmonyPatch(typeof(WaterBiomeManager), nameof(WaterBiomeManager.Start))]
    [HarmonyPostfix]
    internal static void WaterBiomeManagerStartPostfix(WaterBiomeManager __instance)
    {
        foreach (var customBiome in CustomBiomes)
        {
            AddBiomeToWaterBiomeManager(__instance, customBiome);
        }
    }

    [HarmonyPatch(typeof(WaterAmbience), nameof(WaterAmbience.Start))]
    [HarmonyPostfix]
    internal static void WaterAmbienceStartPostfix(WaterAmbience __instance)
    {
        foreach (var soundData in CustomBiomeSoundDatas)
        {
            AddBiomeSoundEmitterToWaterAmbience(__instance, soundData);
        }
    }

    internal static void AddBiomeToWaterBiomeManager(WaterBiomeManager manager, CustomBiomeData biome)
    {
        var skyPrefab = biome.Sky.GetSkyPrefabAtRuntime(manager);
        manager.biomeSettings.Add(new WaterBiomeManager.BiomeSettings()
            {name = biome.Name, settings = biome.Settings, skyPrefab = skyPrefab});
        var newIndexInBiomeLookup = manager.biomeLookup.Count;
        Sky biomeSky = null;
        if (skyPrefab != null && MarmoSkies.main != null)
        {
            biomeSky = MarmoSkies.main.GetSky(skyPrefab);
        }

        manager.biomeSkies.Add(biomeSky);
        if (manager.biomeLookup.ContainsKey(biome.Name))
        {
            Debug.LogWarningFormat(
                "WaterBiomeManager: biomeSettings contains multiple instances of the same biome name: {0}", new object[]
                {
                    biome.Name
                });
        }
        else
        {
            manager.biomeLookup.Add(biome.Name, newIndexInBiomeLookup);
        }
    }

    internal static void AddBiomeSoundEmitterToWaterAmbience(WaterAmbience waterAmbience, CustomBiomeSoundData biomeSoundData)
    {
        var parent = waterAmbience.transform.Find(biomeSoundData.SoundType == CustomBiomeSoundData.Type.Music ? "music" : "background");
        var emitterObject = new GameObject(biomeSoundData.BiomeName + (biomeSoundData.SoundType == CustomBiomeSoundData.Type.Music ? "Music" : "Ambience"));
        emitterObject.transform.parent = parent;
        var emitter = emitterObject.AddComponent<FMOD_CustomLoopingEmitter>();
        emitter.SetAsset(biomeSoundData.SoundAsset);
#if SUBNAUTICA
        emitter.stopImmediatelyOnDisable = true;
#endif
        var gameParams = emitterObject.AddComponent<FMODGameParams>();
        gameParams.loopingEmitter = emitter;
        gameParams.onlyInBiome = biomeSoundData.BiomeName;
        gameParams.interiorState = biomeSoundData.InteriorState;
    }

    internal class CustomBiomeData
    {
        public string Name { get; }
        public WaterscapeVolume.Settings Settings { get; }
        public BiomeHandler.SkyReference Sky { get; }

        public CustomBiomeData(string name, WaterscapeVolume.Settings settings, BiomeHandler.SkyReference sky)
        {
            Name = name;
            Settings = settings;
            Sky = sky;
        }
    }

    internal class CustomBiomeSoundData
    {
        public Type SoundType { get; }
        public string BiomeName { get; }
        public FMODAsset SoundAsset { get; }
        public FMODGameParams.InteriorState InteriorState { get; }

        public CustomBiomeSoundData(Type soundType, string biomeName, FMODAsset soundAsset,
            FMODGameParams.InteriorState interiorState)
        {
            SoundType = soundType;
            BiomeName = biomeName;
            SoundAsset = soundAsset;
            InteriorState = interiorState;
        }

        public enum Type
        {
            Ambience,
            Music
        }
    }
}