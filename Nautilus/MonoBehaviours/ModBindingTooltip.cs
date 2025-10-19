using UnityEngine;

namespace Nautilus.MonoBehaviours;

internal class ModBindingTooltip : MonoBehaviour, ITooltip
{
    public string tooltip;

    public bool showTooltipOnDrag => true;

    public void GetTooltip(TooltipData tooltipData)
    {
        tooltipData.prefix.Append(tooltip);
    }
}