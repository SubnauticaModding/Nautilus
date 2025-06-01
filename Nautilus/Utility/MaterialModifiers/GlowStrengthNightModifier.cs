using UnityEngine;

namespace Nautilus.Utility.MaterialModifiers;

/// <summary>
/// A material modifier that changes the glow strength of materials with options related to the time of day.
/// </summary>
public class GlowStrengthNightModifier : MaterialModifier
{
    private readonly float _dayGlowStrength = -1f;
    private readonly float _nightGlowStrength;

    /// <summary>
    /// Changes the glow strength of the materials when rendered at nighttime.
    /// </summary>
    /// <param name="nightGlowStrength">New nighttime glows strength. Corresponds to the '_GlowStrengthNight' property.</param>
    /// <remarks>Default glow strength values are 1.</remarks>
    public GlowStrengthNightModifier(float nightGlowStrength)
    {
        _nightGlowStrength = nightGlowStrength;
    }

    /// <summary>
    /// Changes the glow strength of the materials when rendered at daytime and nighttime, individually.
    /// </summary>
    /// <param name="nightGlowStrength">Nighttime glow strength. Corresponds to the '_GlowStrengthNight' property.</param>
    /// <param name="dayGlowStrength">Daytime glow strength. Corresponds to the '_GlowStrength' property.</param>
    /// <remarks>These two properties do not interfere with each other, aside from at "dawn" and "dusk" when a mix of the two is used.</remarks>

    public GlowStrengthNightModifier(float nightGlowStrength, float dayGlowStrength) : this(nightGlowStrength)
    {
        _dayGlowStrength = dayGlowStrength;
    }

    /// <summary>
    /// Applies changes to the specified material.
    /// </summary>
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        if (_dayGlowStrength >= 0)
        {
            material.SetFloat(ShaderPropertyID._GlowStrength, _dayGlowStrength);
        }
        if (_nightGlowStrength >= 0)
        {
            material.SetFloat(ShaderPropertyID._GlowStrengthNight, _nightGlowStrength);
        }
    }
}