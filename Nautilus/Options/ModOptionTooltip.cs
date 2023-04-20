using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Options;

internal class ModOptionTooltip : MonoBehaviour, ITooltip
{
    public string Tooltip;

    void Awake()
    {
        Destroy(GetComponent<LayoutElement>());
    }

    public bool showTooltipOnDrag => true;

    public void GetTooltip(TooltipData tooltip)
    {
        tooltip.prefix.Append(Tooltip);
    }
}