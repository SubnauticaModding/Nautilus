using System;
using UnityEngine;

namespace Nautilus.Utility.ThunderkitUtilities;

/// <summary>
/// A MonoBehaviour component designed for Thunderkit which allows for user-friendly modification of material properties
/// on models within modded prefabs, directly from the Unity Editor. Can act either standalone or as a part of custom systems.
/// </summary>
public class ApplyMaterialModification : MonoBehaviour
{
    /// <summary>
    /// If true, this material modification will be applied in Start. In most cases, this should be left false in favor
    /// of custom implementation during prefab creation, because Start is called a frame late (meaning this change is visible to users).
    /// </summary>
    [Tooltip("If true, this material modification will be applied on Start. Otherwise, you can leave this disabled for your own custom implementation.")]
    public bool applyOnStart;

    /// <summary>
    /// Defines the renderer that this material modification is applied to.
    /// </summary>
    [Tooltip("The renderer that this material modification is applied to")]
    public Renderer renderer;

    /// <summary>
    /// If this has a length greater than zero, only materials with the same name as the materials in this array will be modified.
    /// </summary>
    [Tooltip("If this has a length greater than zero, only materials with the same name as the materials in this array will be modified")]
    public Material[] materialFilter;

    // These are for the editor only, they do NOT serialize and will be NULL!
    
    [SerializeField]
    private TextureSetting[] textureProperties = Array.Empty<TextureSetting>();
    [SerializeField]
    private ColorSetting[] colorProperties = Array.Empty<ColorSetting>();
    [SerializeField]
    private FloatSetting[] floatProperties = Array.Empty<FloatSetting>();
    [SerializeField]
    private EnableSetting[] enableProperties = Array.Empty<EnableSetting>();
    [SerializeField]
    private KeywordSetting[] keywordProperties = Array.Empty<KeywordSetting>();
    [SerializeField]
    private VectorSetting[] vectorProperties = Array.Empty<VectorSetting>();

    // The actual values should be accessed from this script through these fields
    
    [SerializeField, HideInInspector] private string[] texturePropertyNames;
    [SerializeField, HideInInspector] private Texture2D[] texturePropertyValues;

    [SerializeField, HideInInspector] private string[] colorPropertyNames;
    [SerializeField, HideInInspector] private Color[] colorPropertyValues;

    [SerializeField, HideInInspector] private string[] floatPropertyNames;
    [SerializeField, HideInInspector] private float[] floatPropertyValues;

    [SerializeField, HideInInspector] private string[] enablePropertyNames;
    [SerializeField, HideInInspector] private bool[] enablePropertyValues;

    [SerializeField, HideInInspector] private string[] keywordPropertyNames;
    [SerializeField, HideInInspector] private bool[] keywordPropertyValues;

    [SerializeField, HideInInspector] private string[] vectorPropertyNames;
    [SerializeField, HideInInspector] private Vector4[] vectorPropertyValues;

    // Editor-only operations to work around bugs

    private void OnValidate()
    {
        texturePropertyNames = new string[textureProperties.Length];
        texturePropertyValues = new Texture2D[textureProperties.Length];

        for (int i = 0; i < textureProperties.Length; i++)
        {
            texturePropertyNames[i] = textureProperties[i].textureProperty.ToString();
            texturePropertyValues[i] = textureProperties[i].texture;
        }

        colorPropertyNames = new string[colorProperties.Length];
        colorPropertyValues = new Color[colorProperties.Length];

        for (int i = 0; i < colorProperties.Length; i++)
        {
            colorPropertyNames[i] = colorProperties[i].colorProperty.ToString();
            colorPropertyValues[i] = colorProperties[i].color;
        }

        floatPropertyNames = new string[floatProperties.Length];
        floatPropertyValues = new float[floatProperties.Length];

        for (int i = 0; i < floatProperties.Length; i++)
        {
            floatPropertyNames[i] = floatProperties[i].floatProperty.ToString();
            floatPropertyValues[i] = floatProperties[i].value;
        }

        enablePropertyNames = new string[enableProperties.Length];
        enablePropertyValues = new bool[enableProperties.Length];

        for (int i = 0; i < enableProperties.Length; i++)
        {
            enablePropertyNames[i] = enableProperties[i].enableProperty.ToString();
            enablePropertyValues[i] = enableProperties[i].enabled;
        }

        keywordPropertyNames = new string[keywordProperties.Length];
        keywordPropertyValues = new bool[keywordProperties.Length];

        for (int i = 0; i < keywordProperties.Length; i++)
        {
            keywordPropertyNames[i] = keywordProperties[i].keywordProperty.ToString();
            keywordPropertyValues[i] = keywordProperties[i].enabled;
        }

        vectorPropertyNames = new string[vectorProperties.Length];
        vectorPropertyValues = new Vector4[vectorProperties.Length];

        for (int i = 0; i < vectorProperties.Length; i++)
        {
            vectorPropertyNames[i] = vectorProperties[i].vectorProperty.ToString();
            vectorPropertyValues[i] = vectorProperties[i].vector;
        }

        if (renderer == null)
            renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        if (applyOnStart && !Application.isEditor)
        {
            ApplyMaterialModifications();
        }
    }

    /// <summary>
    /// Applies all material modifications that are defined in this instance.
    /// </summary>
    /// <remarks>This is automatically called in the behaviour's <c>Start()</c> method if <see cref="applyOnStart"/> is true,
    /// but it is recommended to instead implement your own system to apply this during prefab creation to prevent incorrect materials in the first frame.</remarks>
    /// <exception cref="NullReferenceException"></exception>
    public void ApplyMaterialModifications()
    {
        if (renderer == null) throw new NullReferenceException($"Renderer is null on material setter: {name}!");

        var rendererMaterials = renderer.materials;

        foreach (var gameMaterial in rendererMaterials)
        {
            var useFilter = materialFilter.Length > 0;
            string materialNameForFilter = null;
            
            if (useFilter)
            {
                // Removes the "(Instance)" string from the end of the material name
                materialNameForFilter = gameMaterial.name.EndsWith(" (Instance)") ?
                    gameMaterial.name.Substring(0, gameMaterial.name.Length - 11) : gameMaterial.name;
            }

            if (useFilter)
            {
                var materialExistsInFilter = false;
                foreach (var filterEntry in materialFilter)
                {
                    if (!materialNameForFilter.Equals(filterEntry.name, StringComparison.OrdinalIgnoreCase)) continue;
                    materialExistsInFilter = true;
                    break;
                }
                if (!materialExistsInFilter) continue;
            }
            
            for (var i = 0; i < texturePropertyNames.Length; i++)
            {
                gameMaterial.SetTexture(texturePropertyNames[i], texturePropertyValues[i]);
            }

            for (var i = 0; i < colorPropertyNames.Length; i++)
            {
                gameMaterial.SetColor(colorPropertyNames[i], colorPropertyValues[i]);
            }

            for (var i = 0; i < floatPropertyNames.Length; i++)
            {
                gameMaterial.SetFloat(floatPropertyNames[i], floatPropertyValues[i]);
            }

            for (var i = 0; i < enablePropertyNames.Length; i++)
            {
                gameMaterial.SetFloat(enablePropertyNames[i], enablePropertyValues[i] ? 1f : 0f);
            }

            for (var i = 0; i < keywordPropertyNames.Length; i++)
            {
                if (keywordPropertyValues[i])
                    gameMaterial.EnableKeyword(keywordPropertyNames[i]);
                else
                    gameMaterial.DisableKeyword(keywordPropertyNames[i]);
            }

            for (var i = 0; i < vectorPropertyNames.Length; i++)
            {
                gameMaterial.SetVector(vectorPropertyNames[i], vectorPropertyValues[i]);
            }
        }
    }
    
    // Property holder class definitions
    
    #pragma warning disable CS0649
    [Serializable]
    private class ColorSetting
    {
        public ColorProperty colorProperty;
        [ColorUsage(true, true)] public Color color;
    }

    [Serializable]
    private class FloatSetting
    {
        public FloatProperty floatProperty;
        public float value;
    }

    [Serializable]
    private class EnableSetting
    {
        public EnableProperty enableProperty;
        public bool enabled;
    }

    [Serializable]
    private class KeywordSetting
    {
        public KeywordProperty keywordProperty;
        public bool enabled;
    }

    [Serializable]
    private class VectorSetting
    {
        public VectorProperty vectorProperty;
        public Vector4 vector;
    }

    [Serializable]
    private class TextureSetting
    {
        public TextureProperty textureProperty;
        public Texture2D texture;
    }
    #pragma warning restore CS0649

    // Enum definitions of property names. Must match in-game names exactly.

    // Color properties 
    private enum ColorProperty
    {
        _Color,
        _SpecColor,
        _Color2,
        _Color3,
        _SpecColor2,
        _SpecColor3,
        _GlowColor,
        _ScrollColor,
        _GlowScrollColor,
        _ColorStrength,
        _DetailsColor,
        _SquaresColor,
        _BorderColor
    }

    // Float properties
    private enum FloatProperty
    {
        _AddSrcBlend,
        _AddSrcBlend2,
        _AddDstBlend,
        _AddDstBlend2,
        _Shininess,
        _SpecInt,
        _Fresnel,
        _LightmapStrength,
        _GlowStrength,
        _GlowStrengthNight,
        _EmissionLM,
        _EmissionLMNight,
        _MarmoSpecEnum,
        _BurstStrength,
        _IBLreductionAtNight,
        _Mode,
        _SrcBlend,
        _SrcBlend2,
        _DstBlend,
        _DstBlend2,
        _OverlayStrength,
        _ZOffset,
        _Cutoff,
        _Hypnotize,
        _SquaresTile,
        _SquaresSpeed,
        _SquaresIntensityPow,
        _NoiseThickness,
        _NoiseStr,
        _WaveUpMin,
        _Fallof,
        _minYpos,
        _maxYpos,
        _Displacement,
        _ClipRange,
        _MyCullVariable,
        _RopeGravity,
        _InfectionHeightStrength,
        _InfectionAmount,
        _Built,
        _BuildLinear,
        _FillSack,
        _GlowUVfromVC,
        FX,
        FX_Vertex,
    }

    // These are in-game 'float' properties, which should have a value of either 0 or 1.
    private enum EnableProperty
    {
        _ZWrite
    }

    // Keywords
    private enum KeywordProperty
    {
        MARMO_ALPHA_CLIP,
        MARMO_EMISSION,
        MARMO_NORMALMAP,
        MARMO_SIMPLE_GLASS,
        MARMO_SPECMAP,
        MARMO_VERTEX_COLOR,
        UWE_3COLOR,
        UWE_DETAILMAP,
        UWE_DITHERALPHA,
        UWE_LIGHTMAP,
        UWE_INFECTION,
        UWE_PLAYERINFECTION,
        UWE_SCHOOLFISH,
        UWE_SIG,
        UWE_VR_FADEOUT,
        UWE_WAVING,
        FX_ANIMATEDGLOW,
        FX_BLEEDER,
        FX_BUILDING,
        FX_BURST,
        FX_DEFORM,
        FX_IONCRYSTAL,
        FX_KELP,
        FX_MESMER,
        FX_PROPULSIONCANNON,
        FX_ROPE,
        FX_SINWAVE,
        GLOW_UV_FROM_VERTECCOLOR
    }

    private enum VectorProperty
    {
        _Speed,
        _Scale,
        _Range,
        _Frequency,
        _DetailIntensities,
        _ScrollSpeed,
        _GlowMaskSpeed,
        _NoiseSpeed,
        _FakeSSSSpeed,
        _InfectionSpeed,
        _DeformParams,
        _FakeSSSparams,
        _BuildParams,
        _InfectionScale,
        _InfectionOffset,
        _ObjectUp,
    }

    private enum TextureProperty
    {
        _MainTex,
        _SpecTex,
        _EmissiveTex,
        _NoiseTex,
        _DispTex,
        _ScrollTex,
        _ScrollTex2,
        _DetailDiffuseTex,
        _DetailBumpTex,
        _DetailSpecTex,
        _InfectionNoiseTex,
        _Illum,
        _BumpMap,
        _DeformMap,
        _SIGMap,
        _Lightmap,
        _GlowMask,
        _MultiColorMask,
        _GlowScrollMask,
        _AnimMask,
        _VrFadeMask
    }
}