using UnityEngine;

namespace Nautilus.Utility.MaterialModifiers;

/// <summary>
/// A material modifier that enables a "waving" effect on all materials, commonly used for plants.
/// </summary>
public class WavingEffectModifier : MaterialModifier
{
    private static readonly int _waveUpMinParam = Shader.PropertyToID("_WaveUpMin");
    private static readonly int _scaleParam = Shader.PropertyToID("_Scale");
    private static readonly int _frequencyParam = Shader.PropertyToID("_Frequency");
    private static readonly int _speedParam = Shader.PropertyToID("_Speed");
    
    private readonly float _waveUpMin;
    
    /// <summary>
    /// The scale of the waving effect. X, Y and Z values refer to respective directions while W refers to "rustling".
    /// </summary>
    public Vector4 Scale { get; init; } = new Vector4(0.12f, 0.05f, 0.12f, 0.1f);
    /// <summary>
    /// The frequency of the waving effect. X, Y and Z values refer to respective directions while W refers to "rustling".
    /// </summary>
    public Vector4 Frequency { get; init; } = new Vector4(0.6f, 0.5f, 0.75f, 0.8f);
    /// <summary>
    /// The speed of the waving effect. The X value refers to speed while the Y value refers to rustling speed.
    /// </summary>
    public Vector2 Speed { get; init; } = new Vector2(0.6f, 0.3f);

    /// <summary>
    /// Creates a <see cref="WavingEffectModifier"/> that enables the plant waving effect on all materials.
    /// </summary>
    /// <param name="waveUpMin">Also referred to as the "influence cutoff y". Range is 0-1. Higher values tend to lead
    /// to higher intensities of the waving effect towards the "base" of the plant.</param>
    public WavingEffectModifier(float waveUpMin)
    {
        _waveUpMin = waveUpMin;
    }
    
    /// <summary>
    /// Applies changes to the specified material.
    /// </summary>
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        material.EnableKeyword("UWE_WAVING");
        material.SetFloat(_waveUpMinParam, _waveUpMin);
        material.SetVector(_scaleParam, Scale);
        material.SetVector(_frequencyParam, Frequency);
        material.SetVector(_speedParam, Speed);
    }
}