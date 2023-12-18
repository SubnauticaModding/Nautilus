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
    /// <param name="settings">The fog settings of the biome. See <see cref="CreateBiomeSettings"/>.</param>
    /// <param name="sky">The Sky of the biome, which determines reflections and general lighting.</param>
    public static void RegisterBiome(string name, WaterscapeVolume.Settings settings, SkyReference sky)
    {
        BiomePatcher.RegisterBiome(new BiomePatcher.CustomBiomeData(name, settings, sky));
    }

    /// <summary>
    /// <para>A shorthand for creating an instance of the <see cref="WaterscapeVolume.Settings"/> class.</para>
    /// <para>A list of base game values can be found on this page: https://subnauticamodding.github.io/Nautilus/tutorials/biomes.html</para>
    /// </summary>
    /// <param name="absorption">Attenuation coefficients of light (1/cm), red green and blue respectively. The default value is 100f, 18.29155f, 3.531373f.</param>
    /// <param name="scattering">The strength of light scattering, 1f by default.</param>
    /// <param name="scatteringColor">The color of light scattering. Default value is white.</param>
    /// <param name="murkiness">The murkiness of the water. Default value is 1f, recommended range is 0f-20f.</param>
    /// <param name="emissive">The emission color of the fog. Default value is black.</param>
    /// <param name="emissiveScale">The emission strength of the fog. Default value is 1f.</param>
    /// <param name="startDistance">The starting distance of the fog. Default value is 25f. Don't make this too high.</param>
    /// <param name="sunlightScale">The strength of sunlight in this biome. Default value is 1f.</param>
    /// <param name="ambientScale">The strength of ambient light in this biome. Default value is 1f.</param>
    /// <param name="temperature">The temperature of this biome in Celsius. Default value is 24f.</param>
    /// <returns>An instance of the class with all settings applied.</returns>
    public static WaterscapeVolume.Settings CreateBiomeSettings(Vector3 absorption, float scattering,
        Color scatteringColor, float murkiness, Color emissive, float emissiveScale = 1, float startDistance = 25,
        float sunlightScale = 1, float ambientScale = 1, float temperature = 24)
    {
        return new WaterscapeVolume.Settings()
        {
            absorption = absorption,
            scattering = scattering,
            scatteringColor = scatteringColor,
            murkiness = murkiness,
            emissive = emissive,
            emissiveScale = emissiveScale,
            startDistance = startDistance,
            sunlightScale = sunlightScale,
            ambientScale = ambientScale,
            temperature = temperature
        };
    }

    /// <summary>
    /// Defines a reference to a new or existing Sky prefab.
    /// </summary>
    public class SkyReference
    {
        private enum Type
        {
            GameObjectReference,
            StringLookup
        }

        private readonly GameObject _obj;
        private readonly string _existingSkyPrefabNameToLookUp;
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

        public virtual GameObject GetSkyPrefabAtRuntime(WaterBiomeManager waterBiomeManager)
        {
            if (_type == Type.StringLookup)
            {
                foreach (var settings in waterBiomeManager.biomeSettings)
                {
                    if (settings.skyPrefab.name == _existingSkyPrefabNameToLookUp)
                    {
                        return settings.skyPrefab;
                    }
                }

                InternalLogger.Error($"No existing Sky prefab found by name '{_existingSkyPrefabNameToLookUp}'");
                return null;
            }

            return _obj;
        }
    }
}