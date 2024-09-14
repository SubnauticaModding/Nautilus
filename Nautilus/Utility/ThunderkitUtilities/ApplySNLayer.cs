using System.Reflection;
using UnityEngine;

namespace Nautilus.Utility.ThunderkitUtilities;

internal class ApplySNLayer : MonoBehaviour
{
    [Tooltip("The name of the layer you want to apply")]
    public LayerName layerName;

    [Tooltip("How to apply the layer")]
    public GeneralSetMode applicationMode;

    private void Start()
    {
        FieldInfo field = typeof(LayerID).GetField(layerName.ToString(), BindingFlags.Static | BindingFlags.Public);
        int layer = (int) field.GetValue(null);

        switch(applicationMode)
        {
            case GeneralSetMode.SingleObject:
                gameObject.layer = layer;
                break;
            case GeneralSetMode.AllChildObjects:
                GetComponentsInChildren<GameObject>().ForEach(g => g.layer = layer);
                break;
            case GeneralSetMode.AllChildObjectsIncludeInactive:
                GetComponentsInChildren<GameObject>(true).ForEach(g => g.layer = layer);
                break;
        }
    }

    /// <summary>
    /// These are taken from <see cref="LayerID"/>, and are retrieved using reflection
    /// </summary>
    public enum LayerName
    {
        Default,
        Useable,
        NotUseable,
        Player,
        TerrainCollider,
        UI,
        Trigger,
        BaseClipProxy,
        OnlyVehicle,
        Vehicle,
#if SN_STABLE
        DefaultCollisionMask,
        SubRigidbodyExclude,
#elif BZ_STABLE
        Interior,
        AllowPlayerAndVehicle
#endif
    }
}
