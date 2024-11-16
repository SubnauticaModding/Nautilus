using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Utility.ThunderkitUtilities;

internal class ApplySNMaterial : MonoBehaviour
{
    [Tooltip("How to apply the material")]
    public MaterialSetMode materialSetMode;

    [Tooltip("What material to apply")]
    public MaterialType materialType;

    [Tooltip("Run at start, or be manually called?")]
    public bool runAtStart = true;

    [Header("Single Object Settings:")]
    public Renderer renderer;
    public int[] materialIndices = new[] { 0 };

    private void OnValidate()
    {
        if (!renderer) TryGetComponent(out renderer);
    }

    private void Start()
    {
        if (!runAtStart) return;

        AssignMaterials();
    }

    /// <summary>
    /// Applies the set material using the specified <see cref="MaterialSetMode"/>
    /// </summary>
    public void AssignMaterials()
    {
        switch(materialSetMode)
        {
            case MaterialSetMode.SingleObject:
                ApplyMaterialsOnSingleRend();
                break;
            case MaterialSetMode.AllChildObjects:
                ApplyMaterialsOnChildren(false);
                break;
            case MaterialSetMode.AllChildObjectsIncludeInactive:
                ApplyMaterialsOnChildren(true);
                break;
            case MaterialSetMode.AllChildGraphics:
                foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                {
                    graphic.material = GetMaterial(materialType);
                }
                break;
        }
    }

    private void ApplyMaterialsOnSingleRend()
    {
        if (renderer == null) throw new System.Exception($"The renderer is null on {gameObject} when SN materials were trying to be applied");

        var mats = renderer.materials;
        foreach (var index in materialIndices)
        {
            mats[index] = GetMaterial(materialType);
        }

        renderer.materials = mats;
    }

    private void ApplyMaterialsOnChildren(bool includeInactive)
    {
        var rends = GetComponentsInChildren<Renderer>(includeInactive);
        foreach (var rend in rends)
        {
            var materials = rend.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = GetMaterial(materialType);
            }

            rend.materials = materials;
        }
    }

    private Material GetMaterial(MaterialType type)
    {
        Material mat = type switch
        {
#if SN_STABLE
            MaterialType.WaterBarrier => MaterialUtils.AirWaterBarrierMaterial,
            MaterialType.ForceField => MaterialUtils.ForceFieldMaterial,
            MaterialType.StasisField => MaterialUtils.StasisFieldMaterial,
            MaterialType.HolographicUI => MaterialUtils.HolographicUIMaterial,
#endif
            MaterialType.Glass => MaterialUtils.GlassMaterial,
            MaterialType.ExteriorGlass => MaterialUtils.ExteriorGlassMaterial,
            MaterialType.ShinyGlass => MaterialUtils.ShinyGlassMaterial,
            MaterialType.InteriorWindowGlass => MaterialUtils.InteriorGlassMaterial,
            _ => null
        };

        return mat;
    }

    internal enum MaterialType
    {
        Glass,
        ExteriorGlass,
        ShinyGlass,
        InteriorWindowGlass,
#if SN_STABLE
        WaterBarrier,
        ForceField,
        StasisField,
        HolographicUI
#endif
    }
}
