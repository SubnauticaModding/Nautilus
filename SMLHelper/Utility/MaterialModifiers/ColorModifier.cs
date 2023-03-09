using UnityEngine;

namespace SMLHelper.Utility.MaterialModifiers;

/// <summary>
/// Applies a color
/// </summary>
public class ColorModifier : MaterialModifier 
{
    private Color color;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="color"></param>
    public ColorModifier(Color color)
    {
        this.color = color;
    }

    /// <summary>
    /// Applies changes to the material.
    /// </summary>
    /// <param name="material"></param>
    /// <param name="renderer"></param>
    /// <param name="materialType"></param>
    protected sealed override void ApplyChangesToMaterial(Material material, Renderer renderer, MaterialUtils.MaterialType materialType)
    {
        material.color = color;
    }
}