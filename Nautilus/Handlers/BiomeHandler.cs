using Nautilus.Assets.PrefabTemplates;
using Nautilus.Patchers;
using Nautilus.Utility;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for registering new biomes.
/// </summary>
public static partial class BiomeHandler
{
    /// <summary>
    /// Registers a new biome type into the game.
    /// </summary>
    /// <param name="name">The name of the biome, as seen in the F1 menu.</param>
    /// <param name="settings">The fog settings of the biome. See <see cref="BiomeUtils.CreateBiomeSettings"/>.</param>
    /// <param name="sky">The Sky of the biome, which determines reflections and general lighting.</param>
    public static void RegisterBiome(string name, WaterscapeVolume.Settings settings, SkyReference sky)
    {
        BiomePatcher.RegisterBiome(new BiomePatcher.CustomBiomeData(name, settings, sky));
    }

#if SUBNAUTICA
    /// <summary>
    /// Adds music that plays in the given biome(s). The sound emitter is played when the given conditions are ended, until those conditions are no longer true, and then a fadeout is allowed.
    /// </summary>
    /// <param name="biomeName">The name of the biome that this music can play in. Prefix matching and case insensitive, so using "canyon" for this value would affect biomes named "canyon_one" and "canyon_TWO".</param>
    /// <param name="musicAsset">The sound asset that plays in this biome.</param>
    /// <param name="interiorState">Determines how this sound is affected by being indoors or outside.</param>
    public static void AddBiomeMusic(string biomeName, FMODAsset musicAsset, FMODGameParams.InteriorState interiorState = FMODGameParams.InteriorState.Always)
    {
        BiomePatcher.RegisterBiomeSoundData(new BiomePatcher.CustomBiomeSoundData(BiomePatcher.CustomBiomeSoundData.Type.Music, biomeName, musicAsset, interiorState));
    }
#endif
    
    /// <summary>
    /// Adds an ambient sound that plays in the given biome(s). The sound emitter is played when the given conditions are ended, until those conditions are no longer true, and then a fadeout is allowed.
    /// </summary>
    /// <param name="biomeName">The name of the biome that this ambient sound can play in. Prefix matching and case insensitive, so using "canyon" for this value would affect biomes named "canyon_one" and "canyon_TWO".</param>
    /// <param name="ambienceAsset">The sound asset that plays in this biome.</param>
    /// <param name="interiorState">Determines how this sound is affected by being indoors or outside.</param>
    public static void AddBiomeAmbience(string biomeName, FMODAsset ambienceAsset, FMODGameParams.InteriorState interiorState)
    {
        BiomePatcher.RegisterBiomeSoundData(new BiomePatcher.CustomBiomeSoundData(BiomePatcher.CustomBiomeSoundData.Type.Ambience, biomeName, ambienceAsset, interiorState));
    }

    /// <summary>
    /// Overrides the atmosphere volume prefab of the batch that is located at the provided coordinates.
    /// This can be used to set the entire biome throughout a single batch.
    /// </summary>
    /// <param name="batch">The coordinates of the batch to override. Example batch coordinates: (12, 18, 12).
    /// The current batch can be found in-game using the F1 menu.</param>
    /// <param name="atmosphereVolumeClassID">The Class ID of the atmosphere volume prefab that settings are pulled from.
    /// See the <see cref="AtmosphereVolumeTemplate"/> class for making your own.</param>
    /// <remarks>
    /// Biomes defined at the batch level can override the 2D biome map, but will NOT override Atmosphere Volumes.
    /// Batch biomes are typically used by the game for large caves such as the Jellyshroom Cave, Lost River, and Lava Zone.
    /// </remarks>
    public static void SetBatchAtmospherePrefab(Int3 batch, string atmosphereVolumeClassID)
    {
        BiomePatcher.SetBatchAtmosphereVolume(batch, atmosphereVolumeClassID);
    }

    /// <summary>
    /// Removes batch atmosphere volume prefab overrides set by <see cref="SetBatchAtmospherePrefab"/>,
    /// reverting it to the default biome set by the game.
    /// </summary>
    /// <param name="batch">The batch that will have its biome override unregistered and reset to the game's default biome.</param>
    /// <remarks>If this batch is currently loaded, this change will not apply until the batch has been unloaded.</remarks>
    public static void ResetBatchAtmospherePrefab(Int3 batch)
    {
        BiomePatcher.ResetBatchAtmosphereVolume(batch);
    }
}