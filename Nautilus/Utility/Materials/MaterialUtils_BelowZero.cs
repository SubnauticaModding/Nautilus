using System.Collections;
using UnityEngine;

#if BELOWZERO
namespace Nautilus.Utility;

public static partial class MaterialUtils
{
    private static IEnumerator PatchInternal()
    {
        yield return LoadGlassMaterials();
    }

    private static IEnumerator LoadGlassMaterials()
    {
        var seamothTask = CraftData.GetPrefabForTechTypeAsync(TechType.SeaTruck);

        yield return seamothTask;

        var glassMaterial = seamothTask.GetResult()
            .transform.Find("model/seatruck_anim/Seatruck_cabin_exterior_glass_geo")
            .GetComponent<Renderer>().material;

        GlassMaterial = new Material(glassMaterial);

        ExteriorGlassMaterial = new Material(glassMaterial);
        ExteriorGlassMaterial.SetFloat("_SpecInt", 100);
        ExteriorGlassMaterial.SetFloat("_Shininess", 6.3f);
        ExteriorGlassMaterial.SetFloat("_Fresnel", 0.85f);
        ExteriorGlassMaterial.SetColor("_Color", new Color(0.33f, 0.58f, 0.71f, 0.1f));
        ExteriorGlassMaterial.SetColor("_SpecColor", new Color(0.5f, 0.76f, 1f, 1f));

        ShinyGlassMaterial = new Material(glassMaterial);
        ShinyGlassMaterial.SetColor("_Color", new Color(1, 1, 1, 0.2f));
        ShinyGlassMaterial.SetFloat("_SpecInt", 3);
        ShinyGlassMaterial.SetFloat("_Shininess", 8);
        ShinyGlassMaterial.SetFloat("_Fresnel", 0.78f);

        InteriorGlassMaterial = new Material(glassMaterial);
        InteriorGlassMaterial.SetColor("_Color", new Color(0.67f, 0.71f, 0.76f, 0.56f));
        InteriorGlassMaterial.SetFloat("_SpecInt", 2);
        InteriorGlassMaterial.SetFloat("_Shininess", 6f);
        InteriorGlassMaterial.SetFloat("_Fresnel", 0.88f);
    }

    private static IEnumerator ReloadStaleReferences()
    {
        Object.Destroy(GlassMaterial);
        Object.Destroy(ExteriorGlassMaterial);
        Object.Destroy(ShinyGlassMaterial);
        Object.Destroy(InteriorGlassMaterial);

        yield return LoadGlassMaterials();
    }
}

#endif