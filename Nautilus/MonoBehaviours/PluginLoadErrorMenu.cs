using UnityEngine;

namespace Nautilus.MonoBehaviours;

internal class PluginLoadErrorMenu : MonoBehaviour
{
    public void OnButtonOpen() => gameObject.SetActive(true);
    public void OnButtonClose() => gameObject.SetActive(false);
}