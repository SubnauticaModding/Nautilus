using HarmonyLib;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UWE;
using Nautilus.Options;

namespace Nautilus.Patchers;

// Thanks SMLHelper... This disgusting code fixes many of the bugs caused by SMLHelper with patching, in order to force compatibility.
// This class can be SAFELY removed if we ever decide to make Nautilus incompatible with SMLHelper (which it already kinda is...)
internal class SMLHelperCompatibilityPatcher
{
    private const string SMLHarmonyInstance = "com.ahk1221.smlhelper"; // This string is both the harmony instance & plugin GUID.
    private const string SMLAssemblyName = "SMLHelper";

    private static Assembly _smlHelperAssembly;

    internal static void Patch(Harmony harmony)
    {
        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(SMLHarmonyInstance))
        {
            CoroutineHost.StartCoroutine(WaitOnSMLHelperForPatches(harmony));
        }
    }

    private static IEnumerator WaitOnSMLHelperForPatches(Harmony harmony)
    {
        // This code is arbitrary but was taken from an older version of SMLHelper so that this patch applies only AFTER has been patched.

        var chainLoader = typeof(BepInEx.Bootstrap.Chainloader);

        var _loaded = chainLoader.GetField("_loaded", BindingFlags.NonPublic | BindingFlags.Static);
        while (!(bool) _loaded.GetValue(null))
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(2);

        // Waiting an extra frame is CRUCIAL to avoid race conditions.

        yield return null;

        InternalLogger.Log("Patching SMLHelper compatibility fixes", BepInEx.Logging.LogLevel.Info);

        UnpatchSMLOptionsMethods(harmony);
        FixSMLOptionsException(harmony);
    }

    private static void UnpatchSMLOptionsMethods(Harmony harmony)
    {
        /* Here, we unpatch SML's option panel patches to every method EXCEPT the following:
        * The postfix to uGUI_OptionsPanel.AddTabs (needed for SML options to actually appear)
        * The prefix to uGUI_Binding.RefreshValue (needed for keybinds)
        */

        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddTab)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddHeading)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.OnEnable)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.RemoveTabs)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.HighlightCurrentTab)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab)), HarmonyPatchType.Prefix, SMLHarmonyInstance);
        harmony.Unpatch(AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab)), HarmonyPatchType.Postfix, SMLHarmonyInstance);
    }

    // Fix what should have been a compiler error (cause of error is this line: https://github.com/SubnauticaModding/Nautilus/blob/f3d5de3e36b61a7f26291ef4725eadcb5c4de2a5/SMLHelper/Options/ModOptions.cs#L157)
    // Here we just use Nautilus's version of the ModOptionAdjust components, instead of the old SMLHelper ones.
    // We can't patch that single Awake line because it references a missing class, and is therefore unpatchable.
    private static void FixSMLOptionsException(Harmony harmony)
    {
        var modOptionBaseClass = GetSMLType("SMLHelper.V2.Options.ModOption");
        var typesToPatch = new Type[]
        {
            GetSMLType("SMLHelper.V2.Options.ModChoiceOption"),
            GetSMLType("SMLHelper.V2.Options.ModKeybindOption"),
            GetSMLType("SMLHelper.V2.Options.ModSliderOption"),
            GetSMLType("SMLHelper.V2.Options.ModToggleOption")
        };
        var modChoiceOptionType = GetSMLType("SMLHelper.V2.Options.ModChoiceOption");
        foreach (var type in typesToPatch)
        {
            harmony.Patch(
                AccessTools.PropertyGetter(type, "AdjusterComponent"),
                prefix: new HarmonyMethod(typeof(SMLHelperCompatibilityPatcher), nameof(ChangeAdjusterComponentPrefix))
                );
        }
    }

    // This method swaps out the old (broken) ModOption.ModOptionAdjust components with the new Nautilus ones.
    // __instance is of type SMLHelper.V2.Options.ModOption
    private static bool ChangeAdjusterComponentPrefix(object __instance, ref Type __result)
    {
        var typeName = __instance.GetType().Name;
        switch (typeName)
        {
            // The cases are the SMLHelper option classes, and __result is set equal to the new Nautilus adjust types.
            case "ModToggleOption":
                __result = typeof(ModToggleOption.ToggleOptionAdjust);
                break;
            case "ModChoiceOption":
                __result = typeof(ChoiceOptionAdjust);
                break;
            case "ModSliderOption":
                __result = typeof(ModSliderOption.SliderOptionAdjust);
                break;
            case "ModKeybindOption":
                __result = typeof(ModKeybindOption.BindingOptionAdjust);
                break;
        }
        return false;
    }

    private static Assembly GetSMLAssembly()
    {
        if (_smlHelperAssembly != null)
        {
            return _smlHelperAssembly;
        }
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == SMLAssemblyName)
            {
                _smlHelperAssembly = assembly;
            }
        }
        return _smlHelperAssembly;
    }

    private static Type GetSMLType(string typeName)
    {
        var assembly = GetSMLAssembly();
        return assembly.GetType(typeName);
    }

    // Might be necessary for types that are subclasses like "SMLHelper.V2.Options.ModOption+ModOptionAdjust", notice the + symbol there
    private static Type GetSMLTypeByFullName(string typeName)
    {
        var assembly = GetSMLAssembly();
        foreach (var type in assembly.GetTypes())
        {
            if (type.ToString() == typeName)
            {
                return type;
            }
        }
        return null;
    }
}
