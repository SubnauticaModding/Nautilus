using UnityEngine;

namespace SMLHelper.Utility.MaterialModifiers;

/// <summary>
/// Basic material modifier that sets the <see cref="Material.color"/> property on all materials.
/// </summary>
public sealed class ColorModifier : MaterialModifier 
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
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        material.color = color;
    }
}