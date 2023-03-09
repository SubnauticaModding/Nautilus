namespace SMLHelper.Utility.MaterialModifiers;

using UnityEngine;

/// <summary>
/// Basic Material Modifier that only affects Particle Systems. By default ignores these materials completely. Can be overriden to implement custom behaviour.
/// </summary>
public class MaterialModifier
{
    /// <summary>
    /// Basic Material Modifier that only affects Particle Systems. By default ignores these materials completely. Can be overriden to implement custom behaviour.
    /// </summary>
    public MaterialModifier()
    {
    }

    /// <summary>
    /// Method called after all other material conversions have finished.
    /// </summary>
    /// <param name="material">The material being modified.</param>
    /// <param name="renderer">The renderer using the <paramref name="material"/>.</param>
    /// <param name="materialType">The type of material that this can be expected to be. Determined in <see cref="MaterialUtils.ApplySNShaders"/> based on specific keywords.</param>
    public void EditMaterial(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        ApplyChangesToMaterial(material, renderer, materialType);
    }

    /// <summary>
    /// Only called for Materials on ParticleRenderers. Override to implement your own custom behaviour, such as shader conversions.
    /// </summary>
    protected virtual void ApplyChangesToMaterial(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {

    }

    /// <summary>
    /// Method called before any shader conversions and material modifiers are applied. By default returns false.<br/>
    /// If <see langword="true"/> is returned from ANY MaterialModifier, the shader of <paramref name="material"/> will <i>not</i> be converted to MarmosetUBER. However, all modifiers will still be applied normally.
    /// </summary>
    /// <param name="material">The material being evalauted.</param>
    /// <param name="renderer">The renderer using the <paramref name="material"/>.</param>
    /// <param name="materialType">The type of material that this can be expected to be. Determined in <see cref="MaterialUtils.ApplySNShaders"/> based on specific keywords.</param>
    /// <returns></returns>
    public virtual bool BlockShaderConversion(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        return false;
    }
}