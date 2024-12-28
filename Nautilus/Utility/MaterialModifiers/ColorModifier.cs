using UnityEngine;

namespace Nautilus.Utility.MaterialModifiers;

/// <summary>
/// Basic material modifier that sets the <see cref="Material.color"/> property on all materials.
/// </summary>
public sealed class ColorModifier : MaterialModifier 
{
    private readonly Color _color;

    /// <summary>
    /// Sets the <see cref="Material.color"/> property on all materials.
    /// </summary>
    /// <param name="color">The new color to be used.</param>
    public ColorModifier(Color color)
    {
        _color = color;
    }

    /// <summary>
    /// Applies the color changes to the material.
    /// </summary>
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        material.color = _color;
    }
}