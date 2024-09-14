using System.Collections;
using UnityEngine;
using UWE;

#if BELOWZERO
namespace Nautilus.Utility;

public static partial class MaterialUtils_BelowZero
{
    private static IEnumerator PatchInternal()
    {
        yield return LoadIonCubeMaterial();
        yield return LoadPrecursorGlassAndFogMaterial();
        yield return LoadStasisFieldMaterial();
        yield return LoadAirWaterBarrierMaterial();
        yield return LoadForcefieldMaterial();
        yield return LoadGhostMaterial();
    }
}

#endif