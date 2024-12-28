using UnityEngine;

namespace Nautilus.Utility.MaterialModifiers;

/// <summary>
/// A simple material modifier that prevents materials on ParticleSystems from being converted to MarmosetUBER. 
/// </summary>
public class IgnoreParticleSystemsModifier : MaterialModifier
{
    /// <summary>
    /// Returns true if the given <see cref="Renderer"/> is a <see cref="ParticleSystemRenderer"/>.
    /// </summary>
    public override bool BlockShaderConversion(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        return renderer is ParticleSystemRenderer;
    }

    /// <summary>
    /// Applies changes to the specified material.
    /// </summary>
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        
    }
}