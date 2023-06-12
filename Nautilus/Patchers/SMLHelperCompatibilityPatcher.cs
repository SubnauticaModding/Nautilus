using HarmonyLib;
using Nautilus.Utility;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UWE;
using Nautilus.Options;

namespace Nautilus.Patchers;

// Thanks SMLHelper... This disgusting code unpatches most options patches that were done by SMLHelper in order to force compatibility, and also fixes major errors
internal class SMLHelperCompatibilityPatcher
{
    private const string SMLHarmonyInstance = "com.ahk1221.smlhelper";
    private const string SMLAssemblyName = "SMLHelper";

    internal static void Patch(Harmony harmony)
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ahk1221.smlhelper"))
        {
            CoroutineHost.StartCoroutine(WaitOnSMLHelperForPatches(harmony));
        }
    }

    private static IEnumerator WaitOnSMLHelperForPatches(Harmony harmony)
    {
        // This code is arbitrary but was taken from an older version of SMLHelper so that this patch applies only AFTER has been patched

        var chainLoader = typeof(BepInEx.Bootstrap.Chainloader);

        var _loaded = chainLoader.GetField("_loaded", BindingFlags.NonPublic | BindingFlags.Static);
        while (!(bool) _loaded.GetValue(null))
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(2);

        // Waiting an extra frame is CRUCIAL!

        yield return null;

        UnpatchSMLHelperOptionsMethods(harmony);
        FixSMLHelperOptionsException(harmony);
    }

    private static void UnpatchSMLHelperOptionsMethods(Harmony harmony)
    {
        // Here, we unpatch SML's option panel patches to every method EXCEPT the postfix to uGUI_OptionsPanel.AddTabs

        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddTab)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_Binding), nameof(uGUI_Binding.RefreshValue)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddHeading)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.OnEnable)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.RemoveTabs)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.HighlightCurrentTab)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
    }

    // Fix what should have been a compiler error (source can be seen here: https://github.com/SubnauticaModding/Nautilus/blob/f3d5de3e36b61a7f26291ef4725eadcb5c4de2a5/SMLHelper/Options/ModOptions.cs#L157)
    private static void FixSMLHelperOptionsException(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.Method(GetSMLType("SMLHelper.V2.Options.ModOption+ModOptionAdjust"), "Awake"),
            prefix: new HarmonyMethod(typeof(SMLHelperCompatibilityPatcher), nameof(ModOptionAdjustAwakePrefix)));
    }

    [HarmonyPrefix]
    private static bool ModOptionAdjustAwakePrefix(object __instance)
    {
        var asMonoBehaviour = (MonoBehaviour) __instance;
        // use the same logic we use in Nautilus's own version of the component
        var isMainMenu = asMonoBehaviour.gameObject.GetComponentInParent<uGUI_MainMenu>() != null;
        // use good old reflection to avoid having SMLHelper as a dependency
        __instance.SetInstanceField("isMainMenu", isMainMenu);
        return false; // SKIP ORIGINAL TO AVOID TYPE LOAD EXCEPTION
    }

    private static Type GetSMLType(string typeName)
    {
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (a.GetName().Name == SMLAssemblyName)
            {
                foreach (var type in a.GetTypes())
                {
                    if (type.ToString() == typeName)
                    {
                        return type;
                    }
                }
            }
        }
        return null;
    }
}
