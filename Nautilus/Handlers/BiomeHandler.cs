using Nautilus.Patchers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for registering new biome types.
/// </summary>
public static class BiomeHandler
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
    /// Defines a reference to a new or existing Sky prefab.
    /// </summary>
    public class SkyReference
    {
        private enum Type
        {
            GameObjectReference,
            StringLookup,
            GetPrefabFromCallback
        }

        private readonly GameObject _obj;
        private readonly string _existingSkyPrefabNameToLookUp;
        private System.Func<GameObject> _prefabCallback;
        private readonly Type _type;


        /// <summary>
        /// Defines a reference to a new or existing Sky prefab.
        /// </summary>
        /// <param name="obj"></param>
        public SkyReference(GameObject obj)
        {
            _obj = obj;
            _type = Type.GameObjectReference;
        }

        /// <summary>
        /// Defines a reference to a base-game Sky prefab.
        /// </summary>
        /// <param name="existingSkyPrefabName"><para>The name of the Sky prefab from the list of base-game Skies, i.e. "SkySafeShallows".</para>
        /// <para>A list of valid inputs can be found on this page: https://subnauticamodding.github.io/Nautilus/tutorials/biomes.html</para></param>
        public SkyReference(string existingSkyPrefabName)
        {
            _existingSkyPrefabNameToLookUp = existingSkyPrefabName;
            _type = Type.StringLookup;
        }
        
        /// <summary>
        /// Defines a reference to a Sky prefab created from a callback at runtime.
        /// </summary>
        public SkyReference(System.Func<GameObject> prefabCallback)
        {
            _prefabCallback = prefabCallback;
            _type = Type.GetPrefabFromCallback;
        }
        
        public virtual GameObject GetSkyPrefabAtRuntime(WaterBiomeManager waterBiomeManager)
        {
            if (_type == Type.StringLookup)
            {
                foreach (var settings in waterBiomeManager.biomeSettings)
                {
                    if (settings != null && settings.skyPrefab != null && settings.skyPrefab.name == _existingSkyPrefabNameToLookUp)
                    {
                        return settings.skyPrefab;
                    }
                }

                InternalLogger.Error($"No existing Sky prefab found by name '{_existingSkyPrefabNameToLookUp}'");
                return null;
            }

            if (_type == Type.GetPrefabFromCallback)
            {
                return _prefabCallback?.Invoke();
            }

            return _obj;
        }
    }
}