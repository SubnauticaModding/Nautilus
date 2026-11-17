using UnityEngine;

namespace Nautilus.Utility.MaterialModifiers;

/// <summary>
/// A material modifier that changes materials of the specified type to be double-sided.
/// </summary>
public class DoubleSidedModifier : MaterialModifier
{
    private readonly MaterialUtils.MaterialType[] _typesToAffect;
    
    /// <summary>
    /// Changes all materials of the types defined in <paramref name="typesToAffect"/> to be double-sided.
    /// </summary>
    /// <param name="typesToAffect">Only materials with these types will be affected.</param>
    public DoubleSidedModifier(params MaterialUtils.MaterialType[] typesToAffect)
    {
        _typesToAffect = typesToAffect;
    }
    
    /// <summary>
    /// Applies changes to the specified material.
    /// </summary>
    public override void EditMaterial(Material material, Renderer renderer, int materialIndex, MaterialUtils.MaterialType materialType)
    {
        var affect = false;
        
        foreach (var type in _typesToAffect)
        {
            if (materialType == type)
            {
                affect = true;
            }
        }

        if (affect)
        {
            material.SetFloat(ShaderPropertyID._MyCullVariable, 0);
        }
    }
}