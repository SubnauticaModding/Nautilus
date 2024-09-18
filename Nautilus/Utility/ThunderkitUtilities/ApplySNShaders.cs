using UnityEngine;

namespace Nautilus.Utility.ThunderkitUtilities;

internal class ApplySNShaders : MonoBehaviour
{
    [Tooltip("Which GameObject to apply the shaders to (All children will also be affected)")]
    public GameObject applyTo;

    private void OnValidate()
    {
        if (applyTo == null) applyTo = gameObject;
    }

    private void Start()
    {
        MaterialUtils.ApplySNShaders(applyTo);
    }
}
