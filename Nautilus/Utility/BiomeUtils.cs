using mset;
using UnityEngine;

namespace Nautilus.Utility;

/// <summary>
/// Utility class containing methods related to creating custom biomes.
/// </summary>
public static class BiomeUtils
{
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

    private static GameObject _skyPrefabsParent;
    
    /// <summary>
    /// Creates a new basic Sky prefab.
    /// </summary>
    /// <param name="name">The name of the Sky, can be anything.</param>
    /// <param name="specularCube">The texture of the Sky, VERY important in determining reflections.</param>
    /// <param name="affectedByDayNightCycle">If true, the Sky will appear darker at night and brighter at day.</param>
    /// <param name="outdoors">Whether this sky is outdoors or not (should be false for the interiors of player-made structures).</param>
    /// <returns></returns>
    public static Sky CreateSkyPrefab(string name, Texture specularCube, bool affectedByDayNightCycle = true, bool outdoors = true)
    {
        if (_skyPrefabsParent == null)
        {
            _skyPrefabsParent = new GameObject("SkyPrefabsParent");
            _skyPrefabsParent.AddComponent<SceneCleanerPreserve>();
            _skyPrefabsParent.SetActive(false);
            Object.DontDestroyOnLoad(_skyPrefabsParent);
        }

        var skyObject = new GameObject(name);
        skyObject.transform.parent = _skyPrefabsParent.transform;
        skyObject.AddComponent<SceneCleanerPreserve>();
        
        var sky = skyObject.AddComponent<Sky>();
        sky.specularCube = specularCube;
        sky.affectedByDayNightCycle = affectedByDayNightCycle;
        sky.outdoors = outdoors;
        
        return sky;
    }
}