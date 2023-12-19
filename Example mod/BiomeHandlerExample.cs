using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.biomehandler", "Nautilus Biome Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class BiomeHandlerExample : BaseUnityPlugin
{
    private void Awake()
    {
        // Register the new biome into the game
        var lilyPadsFogSettings = BiomeUtils.CreateBiomeSettings(new Vector3(20, 5, 6), 0.6f, Color.white, 0.45f,
            new Color(0.18f, 0.604f, 0.404f), 0.05f, 20, 1, 1.25f, 20);
#if SUBNAUTICA
        BiomeHandler.RegisterBiome("nautilusexamplebiome", lilyPadsFogSettings, new BiomeHandler.SkyReference("SkyKelpForest"));
#elif BELOWZERO
        BiomeHandler.RegisterBiome("nautilusexamplebiome", lilyPadsFogSettings, new BiomeHandler.SkyReference("SkyLilyPads"));
#endif
        
        #if SUBNAUTICA
        // Add wreck ambience & music
        BiomeHandler.AddBiomeMusic("nautilusexamplebiome", AudioUtils.GetFmodAsset("event:/env/music/wreak_ambience_big_music"));
        BiomeHandler.AddBiomeAmbience("nautilusexamplebiome", AudioUtils.GetFmodAsset("event:/env/background/wreak_ambience_big"), FMODGameParams.InteriorState.OnlyOutside);
        #endif

        // Create an atmosphere volume for the biome
        PrefabInfo volumePrefabInfo = PrefabInfo.WithTechType("NautilusExampleBiomeSphereVolume");
        CustomPrefab volumePrefab = new CustomPrefab(volumePrefabInfo);
        AtmosphereVolumeTemplate volumeTemplate = new AtmosphereVolumeTemplate(volumePrefabInfo, AtmosphereVolumeTemplate.VolumeShape.Sphere, "nautilusexamplebiome");
        volumePrefab.SetGameObject(volumeTemplate);
        volumePrefab.Register();
        
        // Add the biome somewhere to the world
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(volumePrefabInfo.ClassID, new Vector3(-1400, -80, 600), Quaternion.identity, new Vector3(50, 50, 50)));
    }
}