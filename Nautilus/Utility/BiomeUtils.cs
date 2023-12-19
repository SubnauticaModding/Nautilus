using System;
using mset;
using UnityEngine;
using Object = UnityEngine.Object;

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

    // DO NOT BLAME ME FOR THIS HORRIBLE CODE - SERIALIZE FIELD COMPONENTS HAVE VERY WEIRD BEHAVIOR IN OUR MODS!
    
    private static GameObject _skyPrefabsParent;

    /// <summary>
    /// Creates a new basic Sky prefab.
    /// </summary>
    /// <param name="name">The name of the Sky, can be anything.</param>
    /// <param name="specularCube">The texture of the Sky, VERY important in determining reflections.</param>
    /// <param name="affectedByDayNightCycle">If true, the Sky will appear darker at night and brighter at day.</param>
    /// <param name="outdoors">Whether this sky is outdoors or not (should be false for the interiors of player-made structures).</param>
    /// <returns>The SkyPrefabFixer component for further modification (this is necessary to get around a Unity bug).</returns>
    public static SkyPrefabFixer CreateSkyPrefab(string name, Texture specularCube, bool affectedByDayNightCycle = true,
        bool outdoors = true)
    {
        if (_skyPrefabsParent == null)
        {
            _skyPrefabsParent = new GameObject("Nautilus.SkyPrefabsParent");
            _skyPrefabsParent.AddComponent<SceneCleanerPreserve>();
            _skyPrefabsParent.SetActive(false);
            Object.DontDestroyOnLoad(_skyPrefabsParent);
        }

        var skyObject = new GameObject(name);
        skyObject.transform.parent = _skyPrefabsParent.transform;
        skyObject.AddComponent<SceneCleanerPreserve>();

        var sky = skyObject.AddComponent<Sky>();

        var skyPrefabFixer = skyObject.AddComponent<SkyPrefabFixer>();
        skyPrefabFixer.sky = sky;
        skyPrefabFixer.specularCube = specularCube;
        skyPrefabFixer.affectedByDayNightCycle = affectedByDayNightCycle;
        skyPrefabFixer.outdoors = outdoors;
        
        return skyPrefabFixer;
    }
    
    /// <summary>
    /// Wrapper class that contains all Sky properties, which are automatically assigned. Necessary for our purposes because fields with SerializeField do not have their values saved when they are instantiated. Yes, everything HAS to be public! 
    /// </summary>
    public class SkyPrefabFixer : MonoBehaviour
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Sky sky;
        
        public Texture specularCube;

        public Bounds dimensions = new Bounds(Vector3.zero, Vector3.one);
        
        public bool affectedByDayNightCycle = true;

        public bool outdoors = true;

        public float masterIntensity = 1f;

        public float skyIntensity = 1f;

        public float specIntensity = 1f;

        public float diffIntensity = 1f;

        public float camExposure = 1f;

        public float specIntensityLM = 0.2f;

        public float diffIntensityLM = 0.05f;

        public bool hdrSky = true;

        public bool hdrSpec = true;

        public bool linearSpace = true;

        public bool autoDetectColorSpace = true;

        public bool hasDimensions;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        private void OnEnable()
        {
            if (sky == null) return;
            sky.specularCube = specularCube;
            sky.dimensions = dimensions; 
            sky.affectedByDayNightCycle = affectedByDayNightCycle; 
            sky.outdoors = outdoors; 
            sky.masterIntensity = masterIntensity; 
            sky.skyIntensity = skyIntensity; 
            sky.diffIntensity = diffIntensity; 
            sky.camExposure = camExposure; 
            sky.specIntensity = specIntensity; 
            sky.specIntensityLM = specIntensityLM; 
            sky.diffIntensityLM = diffIntensityLM; 
            sky.hdrSky = hdrSky; 
            sky.hdrSpec = hdrSpec;
            sky.linearSpace = linearSpace;
            sky.autoDetectColorSpace = autoDetectColorSpace;
            sky.hasDimensions = hasDimensions;
        }
    }

}