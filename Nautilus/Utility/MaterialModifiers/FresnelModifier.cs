using UnityEngine;

namespace Nautilus.Utility.MaterialModifiers;

/// <summary>
/// A material modifier that changes the '_Fresnel' property of all materials.
/// </summary>
public class FresnelModifier : MaterialModifier
{
    private readonly float _fresnel;
    private static readonly int Fresnel = Shader.PropertyToID("_Fresnel");

    /// <summary>
    /// Changes the fresnel of all materials based on the '<paramref name="fresnel"/>' parameter.
    /// </summary>
    /// <param name="fresnel">This factor determines how much the reflection intensity changes based on the angle of incidence.</param>
    /// <remarks>
    /// Example values:
    /// <list type="bullet">
    /// <item>0.00: no fresnel; uniform reflection intensity regardless of view direction.</item>
    /// <item>0.25: average fresnel; edges are shinier.</item>
    /// <item>0.70: high fresnel; ideal for transparent membranes (ghost leviathans use this value).</item>
    /// <item>1.00: full fresnel. Only geometry at a 90° angle is shiny, which results in essentially no reflections.</item>
    /// </list></remarks>
    public FresnelModifier(float fresnel)
    {
        _fresnel = fresnel;
    }

    /// <summary>
    /// Applies changes to the specified material.
    /// </summary>
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex,
        MaterialUtils.MaterialType materialType)
    {
        material.SetFloat(Fresnel, _fresnel);
    }
}