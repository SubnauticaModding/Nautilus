using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SMLHelper.V2.Options
{
    internal class ModOptionTooltip : MonoBehaviour, ITooltip
    {
        public string Tooltip;

        void Awake() => Destroy(GetComponent<LayoutElement>());

        public void GetTooltip(out string tooltipText, List<TooltipIcon> _) => tooltipText = Tooltip;
    }
}
