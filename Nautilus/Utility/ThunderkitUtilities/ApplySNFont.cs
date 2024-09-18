using UnityEngine;
using TMPro;

namespace Nautilus.Utility.ThunderkitUtilities;

internal class ApplySNFont : MonoBehaviour
{
    [Tooltip("How to apply the font")]
    public GeneralSetMode fontSetMode;

    private void Start()
    {
        switch (fontSetMode)
        {
            case GeneralSetMode.SingleObject:
                GetComponent<TextMeshProUGUI>().font = FontUtils.Aller_Rg;
                break;
            case GeneralSetMode.AllChildObjects:
                GetComponentsInChildren<TextMeshProUGUI>().ForEach(t => t.font = FontUtils.Aller_Rg);
                break;
            case GeneralSetMode.AllChildObjectsIncludeInactive:
                GetComponentsInChildren<TextMeshProUGUI>(true).ForEach(t => t.font = FontUtils.Aller_Rg);
                break;
        }

    }
}
