using Nautilus.Utility.MaterialModifiers;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using Nautilus.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UWE;

namespace Nautilus.Utility;

/// <summary>
/// Utilities related to Materials and Shaders.
/// </summary>
public static partial class MaterialUtils
{
    private static bool _sceneEventAdded;
    private static bool _startedLoadingMaterials;
    
    // Emission:
    private static readonly int _unityEmissionMapProperty = Shader.PropertyToID("_EmissionMap");
    private const string UnityEmissionKeyword = "_EMISSION";
    private const string MarmosetEmissionKeyword = "MARMO_EMISSION";
    
    // Specularity:
    private static readonly int _specInt = Shader.PropertyToID("_SpecInt");
    private static readonly int _shininess = Shader.PropertyToID("_Shininess");
    private static readonly int _fresnel = Shader.PropertyToID("_Fresnel");
    private const string UnitySpecGlossMapKeyword = "_SPECGLOSSMAP";
    private const string MarmosetSpecMapKeyword = "MARMO_SPECMAP";
    private const float DefaultFresnel = 0.24f;
    
    // Normal maps:
    // The same property name is used in both the Standard shader and MarmosetUBER
    private static readonly int _bumpMap = Shader.PropertyToID("_BumpMap");
    private const string UnityNormalMapKeyword = "_NORMALMAP";
    private const string MarmosetNormalMapKeyword = "MARMO_NORMALMAP";
    
    // Detail maps:
    // Unity standard shaders
    private static readonly int _unityDetailAlbedoMap = Shader.PropertyToID("_DetailAlbedoMap");
    private static readonly int _unityDetailNormalMap = Shader.PropertyToID("_DetailNormalMap");
    private static readonly int _unityDetailAlbedoMapSt = Shader.PropertyToID("_DetailAlbedoMap_ST");
    private static readonly int _unityDetailNormalMapScale = Shader.PropertyToID("_DetailNormalMapScale");
    
    // Subnautica shader detail map properties
    private static readonly int _detailDiffuseTex = Shader.PropertyToID("_DetailDiffuseTex");
    private static readonly int _detailDiffuseTexSt = Shader.PropertyToID("_DetailDiffuseTex_ST");
    private static readonly int _detailBumpTex = Shader.PropertyToID("_DetailBumpTex");
    private static readonly int _detailBumpTexSt = Shader.PropertyToID("_DetailBumpTex_ST");
    private static readonly int _detailIntensities = Shader.PropertyToID("_DetailIntensities");
    private const string DetailMapKeyword = "UWE_DETAILMAP";
    private const float DefaultDetailDiffuseIntensity = 1.0f;
    
    // Transparency, sorting and miscellaneous
    private const string ZWriteKeyword = "_ZWRITE_ON";
    private const string AlphaClipKeyword = "MARMO_ALPHA_CLIP";
    private const string WboitKeyword = "WBOIT";

    internal static void Patch()
    {
        if (!_sceneEventAdded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _sceneEventAdded = true;
        }
    }
    
    private static IEnumerator LoadReferences()
    {
        yield return PatchInternal();

        IsReady = true;
    }

    /// <summary>
    /// Only returns true once all relevant materials/shaders have been loaded by the MaterialUtils class and are safe to be used.
    /// </summary>
    public static bool IsReady { get; private set; }

    /// <summary>
    /// Gets the basic glass material
    /// </summary>
    public static Material GlassMaterial { get; private set; }

    /// <summary>
    /// Gets the material for the outside of glass, such as for base windows
    /// </summary>
    public static Material ExteriorGlassMaterial { get; private set; }

    /// <summary>
    /// Gets the material for the inside of glass, such as the inside of the Cyclops windshield
    /// </summary>
    public static Material InteriorGlassMaterial { get; private set; }

    /// <summary>
    /// Gets a shiny glass material
    /// </summary>
    public static Material ShinyGlassMaterial { get; private set; }

    /// <summary>
    /// Contains references to various Shaders.
    /// </summary>
    public static class Shaders
    {
        /// <summary>
        /// The <see cref="Shader"/> that is used for most materials in the game.
        /// </summary>
        public static Shader MarmosetUBER
        {
            get
            {
                // Shader.Find("MarmosetUBER") returns the wrong shader, so we need to get it in this way:
                if (field == null)
                {
                    field = AddressablesUtility.LoadAsync<Shader>("Assets/Marmoset/Shader/Uber/MarmosetUber.shader").WaitForCompletion();
                }
                return field;
            }
        }

        /// <summary>
        /// The <see cref="Shader"/> that is used for most particle systems.
        /// </summary>
        public static Shader ParticlesUBER
        {
            get
            {
                if (field == null)
                {
                    field = Shader.Find("UWE/Particles/UBER");
                }
                return field;
            }
        }

        /// <summary>
        /// The <see cref="Shader"/> that is used for Ion Cubes.
        /// </summary>
        public static Shader IonCube
        {
            get
            {
                if (field == null)
                {
                    field = Shader.Find("UWE/Marmoset/IonCrystal");
                }
                return field;
            }
        }
    }

    /// <summary>
    /// Enum for some generic types of Materials.
    /// </summary>
    public enum MaterialType
    {
        /// <summary>
        /// Solid material with no transparency.
        /// </summary>
        Opaque,
        /// <summary>
        /// See-through material.
        /// </summary>
        Transparent,
        /// <summary>
        /// Transparent pixels on the texture are not renderered. Useful for decals.
        /// </summary>
        Cutout
    }

    /// <summary>
    /// Render queue used by opaque objects.
    /// </summary>
    public const int kOpaqueRenderQueue = 2000;
    /// <summary>
    /// Render queue used by transparent objects.
    /// </summary>
    public const int kTransparencyRenderQueue = 3101;

    /// <summary>
    /// <para>Applies the necessary settings for Subnautica's standard shader (<see cref="Shaders.MarmosetUBER"/>) to the passed <see cref="GameObject"/>.</para>
    /// <para><b>The specific changes to each material are influenced by certain (case-insensitive) keywords in their asset names:</b>
    /// <br/>"TRANSPARENT": Enables transparency.
    /// <br/>"CUTOUT": Enables alpha clipping.</para>
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to apply the shaders to (includes all children).</param>
    /// <param name="shininess">'_Shininess' value of the shader. Recommended range of 1.0f-8.0f.</param>
    /// <param name="specularIntensity">'_SpecularInt' value of the shader. Values around 1f are recommended.</param>
    /// <param name="glowStrength">'_GlowStrength' and '_GlowStrengthNight' value of the shader. Should not be absurdly high.</param>
    /// <param name="modifiers">Optional array of classes that inherit from the <see cref="MaterialModifier"/> class. This allows for extensive customization of the method. Called in ascending order on every material.</param>
    public static void ApplySNShaders(GameObject gameObject, float shininess = 4f, float specularIntensity = 1f, float glowStrength = 1f, params MaterialModifier[] modifiers)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            for (var j = 0; j < renderers[i].materials.Length; j++)
            {
                var material = renderers[i].materials[j];

                var matNameLower = material.name.ToLower();
                bool transparent = matNameLower.Contains("transparent");
                bool alphaClip = matNameLower.Contains("cutout");

                var materialType = MaterialType.Opaque;
                if (transparent)
                    materialType = MaterialType.Transparent;
                else if (alphaClip)
                    materialType = MaterialType.Cutout;

                // check each material modifier first. determine if any want to block the shader conversion
                bool blockShaderConversion = false;
                if (modifiers != null)
                {
                    foreach (var modifier in modifiers)
                    {
                        if (modifier.BlockShaderConversion(material, renderers[i], materialType))
                        {
                            blockShaderConversion = true;
                        }
                    }
                }

                // apply the shader conversions, if possible
                if (!blockShaderConversion)
                {
                    ApplyUBERShader(material, shininess, specularIntensity, glowStrength, materialType);
                }

                // apply all modifiers
                if (modifiers != null)
                {
                    foreach (var modifier in modifiers)
                    {
                        modifier.EditMaterial(material, renderers[i], j, materialType);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Applies the <see cref="Shaders.MarmosetUBER"/> Shader to the given material.
    /// </summary>
    /// <param name="material">The material to apply changes to.</param>
    /// <param name="shininess">'_Shininess' value of the shader. Recommended range of 1.0f-8.0f.</param>
    /// <param name="specularIntensity">'_SpecularInt' value of the shader. Values around 1f are recommended.</param>
    /// <param name="glowStrength">'_GlowStrength' and '_GlowStrengthNight' value of the shader. Should not be absurdly high.</param>
    /// <param name="materialType">Controls various settings including alpha clipping and transparency.</param>
    public static void ApplyUBERShader(Material material, float shininess, float specularIntensity, float glowStrength, MaterialType materialType)
    {
        // Grab existing references to textures & colors using property names from the standard shader
        var specularTexture = material.HasProperty(ShaderPropertyID._SpecGlossMap)
            ? material.GetTexture(ShaderPropertyID._SpecGlossMap)
            : null;
        var emissionTexture = material.HasProperty(_unityEmissionMapProperty)
            ? material.GetTexture(_unityEmissionMapProperty)
            : null;
        var emissionColor = material.HasProperty(ShaderPropertyID._EmissionColor)
            ? material.GetColor(ShaderPropertyID._EmissionColor)
            : Color.black;
        // Check for existing detail map related properties
        var detailAlbedoMap = material.HasProperty(_unityDetailAlbedoMap)
            ? material.GetTexture(_unityDetailAlbedoMap)
            : null;
        var detailNormalMap = material.HasProperty(_unityDetailNormalMap)
            ? material.GetTexture(_unityDetailNormalMap)
            : null;
        var detailNormalMapIntensity = material.HasProperty(_unityDetailNormalMapScale)
            ? material.GetFloat(_unityDetailNormalMapScale)
            : 0;
        var detailMapsSt = material.HasProperty(_unityDetailAlbedoMapSt)
            ? material.GetVector(_unityDetailAlbedoMapSt)
            : new Vector4(1, 1, 0, 0);

        // Change the shader to MarmosetUBER
        material.shader = Shaders.MarmosetUBER;

        // Disable keywords that were added by Unity
        material.DisableKeyword(UnitySpecGlossMapKeyword);
        material.DisableKeyword(UnityNormalMapKeyword);

        // Apply specular settings if there is a specular texture (otherwise it will appear bright white)
        if (specularTexture != null)
        {
            material.SetTexture(ShaderPropertyID._SpecTex, specularTexture);
            material.SetFloat(_specInt, specularIntensity);
            material.SetFloat(_shininess, shininess);
            material.EnableKeyword(ZWriteKeyword);
            material.EnableKeyword(MarmosetSpecMapKeyword);
            material.SetColor(ShaderPropertyID._SpecColor, Color.white);
            material.SetFloat(_fresnel, DefaultFresnel);
        }

        // Apply emission if it was enabled in the standard shader
        if (material.IsKeywordEnabled(UnityEmissionKeyword))
        {
            material.EnableKeyword(MarmosetEmissionKeyword);
            material.SetFloat(ShaderPropertyID._EnableGlow, 1f);
            material.SetTexture(ShaderPropertyID._Illum, emissionTexture);
            material.SetColor(ShaderPropertyID._GlowColor, emissionColor);
            material.SetFloat(ShaderPropertyID._GlowStrength, glowStrength);
            material.SetFloat(ShaderPropertyID._GlowStrengthNight, glowStrength);
        }

        // Apply normal map if it was applied in the standard shader
        if (material.GetTexture(_bumpMap))
        {
            material.EnableKeyword(MarmosetNormalMapKeyword);
        }
        
        // Apply detail maps
        bool hasDetailAlbedoMap = detailAlbedoMap != null;
        bool hasDetailNormalMap = detailNormalMap != null;
        if (hasDetailAlbedoMap || hasDetailNormalMap)
        {
            material.EnableKeyword(DetailMapKeyword);
            
            // Set textures
            if (hasDetailAlbedoMap)
                material.SetTexture(_detailDiffuseTex, detailAlbedoMap);
            if (hasDetailNormalMap)
                material.SetTexture(_detailBumpTex, detailNormalMap);

            // Set intensities
            material.SetVector(_detailIntensities,
                new Vector3(DefaultDetailDiffuseIntensity, detailNormalMapIntensity, 0));
            
            // Set tiling
            material.SetVector(_detailDiffuseTexSt, detailMapsSt);
            material.SetVector(_detailBumpTexSt, detailMapsSt);
        }
        
        // Miscellaneous
        material.enableInstancing = true;

        // Apply material type
        switch (materialType)
        {
            case MaterialType.Transparent:
                SetMaterialCutout(material, false);
                SetMaterialTransparent(material, true);
                break;
            case MaterialType.Cutout:
                SetMaterialTransparent(material, false);
                SetMaterialCutout(material, true);
                break;
            default:
                SetMaterialTransparent(material, false);
                SetMaterialCutout(material, false);
                break;
        }
    }
    
    /// <summary>
    /// Iterates over every <see cref="Renderer"/> present on the given <see cref="prefab"/> and any of its
    /// children, and replaces any custom materials it finds that share the exact same name (case-sensitive) of a
    /// base-game material with its vanilla counterpart.
    /// </summary>
    /// <param name="prefab">The custom object you'd like to apply base-game materials to. Children included.</param>
    public static IEnumerator ReplaceMockMaterials(GameObject prefab)
    {
        if (prefab == null)
        {
            InternalLogger.Error("Attempted to apply vanilla materials to a null prefab.");
            yield break;
        }

        yield return ReplaceMockMaterials(prefab.GetAllComponentsInChildren<Renderer>());
    }
    
    /// <summary>
    /// Iterates over every <see cref="Renderer"/> present within the <see cref="renderers"/> collection, and replaces
    /// any custom materials it finds that share the exact same name (case-sensitive) of a base-game material with its
    /// vanilla counterpart.
    /// </summary>
    /// <param name="renderers">The generic collection of <see cref="Renderer"/> objects that you would like
    /// to apply base-game materials to.</param>
    public static IEnumerator ReplaceMockMaterials(Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0)
        {
            InternalLogger.Error("Attempted to call ReplaceMockMaterials with no valid Renderers!");
            yield break;
        }
        
        var relevantRenderers = new List<Renderer>();
        var requiredMaterials = new List<string>();

        foreach (var renderer in renderers)
        {
            int vanillaMats = 0;
            
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null)
                    continue;

                // This probably isn't necessary here anymore, but removing it feels dangerous to me for some reason...
                string filteredMatName = GeneralExtensions.TrimInstance(material.name);
                
                if (!MaterialLibrary.GetMaterialPath(filteredMatName).IsNullOrWhiteSpace())
                {
                    if(!requiredMaterials.Contains(filteredMatName))
                        requiredMaterials.Add(filteredMatName);
                    
                    vanillaMats++;
                }
            }

            if (vanillaMats > 0)
                relevantRenderers.Add(renderer);
        }

        if (requiredMaterials.Count == 0)
        {
            InternalLogger.Warn($"Called ReplaceMockMaterials without any vanilla materials! Consider removing" +
                                $" your call to MaterialUtils.");
            yield break;
        }

        var taskResult = new TaskResult<Material[]>();
        yield return MaterialLibrary.FetchMaterials(requiredMaterials.ToArray(), taskResult);

        var foundMaterials = taskResult.value;

        if (foundMaterials == null)
        {
            InternalLogger.Error($"Failed to load vanilla materials from MatLibrary for renderer collection, with " +
                                 $"leading renderer on object: \"{renderers[0].gameObject.name}\".");
            yield break;
        }

        var nameToVanillaMat = new Dictionary<string, Material>();
        foreach (var matName in requiredMaterials)
        {
            for (int i = 0; i < foundMaterials.Length; i++)
            {
                if (matName.Equals(GeneralExtensions.TrimInstance(foundMaterials[i].name)))
                {
                    nameToVanillaMat.Add(matName, foundMaterials[i]);
                    break;
                }
            }
        }

        foreach (var renderer in relevantRenderers)
        {
            var newMatList = renderer.sharedMaterials;

            for (int i = 0; i < newMatList.Length; i++)
            {
                if (newMatList[i] == null)
                    continue;

                if (nameToVanillaMat.TryGetValue(GeneralExtensions.TrimInstance(newMatList[i].name), out var vanillaMat))
                    newMatList[i] = vanillaMat;
            }

            renderer.sharedMaterials = newMatList;
        }
    }

    /// <summary>
    /// Toggle the transparency on a material.
    /// </summary>
    /// <param name="material"></param>
    /// <param name="transparent"></param>
    /// <returns></returns>
    public static void SetMaterialTransparent(Material material, bool transparent)
    {
        if (transparent)
        {
            material.EnableKeyword(WboitKeyword);
            material.SetInt(ShaderPropertyID._ZWrite, 0);
            material.SetInt(ShaderPropertyID._Cutoff, 0);
            material.SetFloat(ShaderPropertyID._SrcBlend, 1f);
            material.SetFloat(ShaderPropertyID._DstBlend, 1f);
            material.SetFloat(ShaderPropertyID._SrcBlend2, 0f);
            material.SetFloat(ShaderPropertyID._DstBlend2, 10f);
            material.SetFloat(ShaderPropertyID._AddSrcBlend, 1f);
            material.SetFloat(ShaderPropertyID._AddDstBlend, 1f);
            material.SetFloat(ShaderPropertyID._AddSrcBlend2, 0f);
            material.SetFloat(ShaderPropertyID._AddDstBlend2, 10f);
        }
        else
        {
            material.SetInt(ShaderPropertyID._ZWrite, 1);
            material.SetFloat(ShaderPropertyID._SrcBlend, 1f);
            material.SetFloat(ShaderPropertyID._DstBlend, 0f);
            material.SetFloat(ShaderPropertyID._SrcBlend2, 1f);
            material.SetFloat(ShaderPropertyID._DstBlend2, 0f);
            material.SetFloat(ShaderPropertyID._AddSrcBlend, 1f);
            material.SetFloat(ShaderPropertyID._AddDstBlend, 0f);
            material.SetFloat(ShaderPropertyID._AddSrcBlend2, 1f);
            material.SetFloat(ShaderPropertyID._AddDstBlend2, 0f);
            material.DisableKeyword(WboitKeyword);
        }
        material.renderQueue = transparent ? kTransparencyRenderQueue : kOpaqueRenderQueue;
    }

    /// <summary>
    /// Toggle alpha clipping on a material. Incompatbile with transparency.
    /// </summary>
    /// <param name="material"></param>
    /// <param name="cutout"></param>
    /// <returns></returns>
    public static void SetMaterialCutout(Material material, bool cutout)
    {
        if (cutout)
        {
            material.EnableKeyword(AlphaClipKeyword);
        }
        else
        {
            material.DisableKeyword(AlphaClipKeyword);
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MenuEnvironment") return;

        // Following the 2025 patch, it is possible (and normal) for Nautilus to load before any scene has loaded
        // So we delay it until the main menu
        if (!_startedLoadingMaterials)
        {
            _startedLoadingMaterials = true;
            CoroutineHost.StartCoroutine(LoadReferences());
            return;
        }
        
        CoroutineHost.StartCoroutine(ReloadStaleReferences());
    }
}
