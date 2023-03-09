using UnityEngine;

namespace SMLHelper.Utility.MaterialModifiers;

/// <summary>
/// Sets the <see cref="Material.color"/> property on all materials.
/// </summary>
public class ColorModifier : MaterialModifier 
{
    private Color color;

    /// <summary>
    /// Sets the <see cref="Material.color"/> property on all materials.
    /// </summary>
    /// <param name="color"></param>
    public ColorModifier(Color color)
    {
        this.color = color;
    }

    /// <summary>
    /// Applies the color changes to the material.
    /// </summary>
    /// <param name="material"></param>
    /// <param name="renderer"></param>
    /// <param name="materialType"></param>
    protected sealed override void ApplyChangesToMaterial(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        material.color = color;
    }
}