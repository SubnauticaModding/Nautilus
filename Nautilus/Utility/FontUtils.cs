using TMPro;
using UnityEngine;

namespace Nautilus.Utility;

/// <summary>
/// <para>Contains references to frequently used Font Assets for use in the <see cref="TextMeshProUGUI"/> component, which is the preferred component for rendering text.</para>
/// <para>The fonts referenced in this class should not be expected to exist until after the Main Menu scene has been loaded and both the <see cref="uGUI"/> and <see cref="uGUI_MainMenu"/> components have been initialized.</para>
/// </summary>
public static class FontUtils
{
    /// <summary>
    /// Returns the regular version of the Aller font, referred to internally as 'Aller_Rg SDF'.
    /// </summary>
    public static TMP_FontAsset Aller_Rg { get; internal set; }
    /// <summary>
    /// Returns a bold alternative of the Aller font, referred to internally as 'Aller_W_Bd SDF'.
    /// </summary>
    public static TMP_FontAsset Aller_W_Bd { get; internal set; }

    /// <summary>
    /// Applies the given font to every <see cref="TextMeshProUGUI"/> component within <paramref name="rootGameObject"/> and its children (recursive).
    /// </summary>
    /// <param name="rootGameObject">The parent of all affected <see cref="TextMeshProUGUI"/> componentss.</param>
    /// <param name="font">The Font Asset to be applied.</param>
    public static void SetFontInChildren(GameObject rootGameObject, TMP_FontAsset font)
    {
        var textComponents = rootGameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var textComponent in textComponents)
        {
            textComponent.font = font;
        }
    }
}