using UnityEngine;
using TMPro;

namespace Nautilus.Utility.ThunderkitUtilities;

internal class ApplySNFont : MonoBehaviour
{
    [Tooltip("How to apply the font")]
    public GeneralApplicationMode applicationMode;

    private void Start()
    {
        switch (applicationMode)
        {
            case GeneralApplicationMode.SingleObject:
                GetComponent<TextMeshProUGUI>().font = FontUtils.Aller_Rg;
                break;
            case GeneralApplicationMode.AllChildObjects:
                GetComponentsInChildren<TextMeshProUGUI>().ForEach(t => t.font = FontUtils.Aller_Rg);
                break;
            case GeneralApplicationMode.AllChildObjectsIncludeInactive:
                GetComponentsInChildren<TextMeshProUGUI>(true).ForEach(t => t.font = FontUtils.Aller_Rg);
                break;
        }

    }
}
