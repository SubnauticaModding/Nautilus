using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Utility.ThunderkitUtilities;

internal class ApplySNMaterial : MonoBehaviour
{
    [Tooltip("How to apply the material")]
    public MaterialApplicationMode applicationMode;

    [Tooltip("What material to apply")]
    public MaterialType materialType;

    [Tooltip("Run at start, or be manually called?")]
    public bool runAtStart = true;

    [Header("Single Object Settings:")]
    public Renderer renderer;
    public int[] materialIndices;

    private static Material _glassMaterial;
    private static Material _exteriorGlassMaterial;
    private static Material _shinyGlassMaterial;
    private static Material _interiorWindowGlassMaterial;
    private static Material _holographicUIMaterial;
    private static bool _cyclopsLoaded;

    private void OnValidate()
    {
        if (!renderer) TryGetComponent(out renderer);
    }

    private IEnumerator Start()
    {
        yield return LoadMaterialsAsync();

        if (!runAtStart) yield break;

        yield return AssignMaterials();
    }

    /// <summary>
    /// Applies the set material using the specified <see cref="MaterialApplicationMode"/>
    /// A coroutine is required to ensure all material references are retrieved before being applied
    /// </summary>
    public IEnumerator AssignMaterials()
    {
        if (!_cyclopsLoaded)
        {
            yield return LoadMaterialsAsync();
        }

        switch(applicationMode)
        {
            case MaterialApplicationMode.SingleObject:
                ApplyMaterialsOnSingleRend();
                break;
            case MaterialApplicationMode.AllChildObjects:
                ApplyMaterialsOnChildren(false);
                break;
            case MaterialApplicationMode.AllChildObjectsIncludeInactive:
                ApplyMaterialsOnChildren(true);
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
            MaterialType.HolographicUI => _holographicUIMaterial,
#endif
            MaterialType.Glass => _glassMaterial,
            MaterialType.ExteriorGlass => _exteriorGlassMaterial,
            MaterialType.ShinyGlass => _shinyGlassMaterial,
            MaterialType.InteriorWindowGlass => _interiorWindowGlassMaterial,
            _ => null
        };

        return mat;
    }

    private IEnumerator LoadMaterialsAsync()
    {
        CoroutineTask<GameObject> task = null;
#if BZ_STABLE
        task = CraftData.GetPrefabForTechTypeAsync(TechType.SeaTruck);
#elif SN_STABLE
        task = CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
#endif

        yield return task;

        string path = "";
#if BZ_STABLE
        path = "model/seatruck_anim/Seatruck_cabin_exterior_glass_geo";
#elif SN_STABLE
        path = "Model/Submersible_SeaMoth/Submersible_seaMoth_geo/Submersible_SeaMoth_glass_geo";
#endif

        var glassMaterial = task.GetResult()
            .transform.Find("Model/Submersible_SeaMoth/Submersible_seaMoth_geo/Submersible_SeaMoth_glass_geo")
            .GetComponent<Renderer>().material;

        _glassMaterial = new Material(glassMaterial);

        _exteriorGlassMaterial = new Material(glassMaterial);
        _exteriorGlassMaterial.SetFloat("_SpecInt", 100);
        _exteriorGlassMaterial.SetFloat("_Shininess", 6.3f);
        _exteriorGlassMaterial.SetFloat("_Fresnel", 0.85f);
        _exteriorGlassMaterial.SetColor("_Color", new Color(0.33f, 0.58f, 0.71f, 0.1f));
        _exteriorGlassMaterial.SetColor("_SpecColor", new Color(0.5f, 0.76f, 1f, 1f));

        _shinyGlassMaterial = new Material(glassMaterial);
        _shinyGlassMaterial.SetColor("_Color", new Color(1, 1, 1, 0.2f));
        _shinyGlassMaterial.SetFloat("_SpecInt", 3);
        _shinyGlassMaterial.SetFloat("_Shininess", 8);
        _shinyGlassMaterial.SetFloat("_Fresnel", 0.78f);

        _interiorWindowGlassMaterial = new Material(glassMaterial);
        _interiorWindowGlassMaterial.SetColor("_Color", new Color(0.67f, 0.71f, 0.76f, 0.56f));
        _interiorWindowGlassMaterial.SetFloat("_SpecInt", 2);
        _interiorWindowGlassMaterial.SetFloat("_Shininess", 6f);
        _interiorWindowGlassMaterial.SetFloat("_Fresnel", 0.88f);

#if SN_STABLE
        yield return new WaitUntil(() => LightmappedPrefabs.main);

        LightmappedPrefabs.main.RequestScenePrefab("Cyclops", new LightmappedPrefabs.OnPrefabLoaded(OnCyclopsLoaded));

        yield return new WaitUntil(() => _cyclopsLoaded);
#endif
    }

    private void OnCyclopsLoaded(GameObject cyclops)
    {
        _cyclopsLoaded = true;

        var holoMat = cyclops.transform.Find("HelmHUD/HelmHUDVisuals/Canvas_LeftHUD/EngineOnUI/EngineOff_Button")
            .GetComponent<Image>().material;

        _holographicUIMaterial = new Material(holoMat);
    }

    internal enum MaterialType
    {
        Glass,
        ExteriorGlass,
        ShinyGlass,
        InteriorWindowGlass,
        // Kinda icky but these underlying values shouldn't change between versions
#if SN_STABLE
        WaterBarrier,
        ForceField,
        StasisField,
        HolographicUI
#endif
    }
}
