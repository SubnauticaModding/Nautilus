using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Bootstrap;
using HarmonyLib;
using Nautilus.Utility;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nautilus.Patchers;

internal static class DependencyWarningPatcher
{
    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(DependencyWarningPatcher));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Start))]
    private static void MainMenuStartPostfix(uGUI_MainMenu __instance)
    {
        try
        {
            CreateDependencyWarningUI(__instance);
        }
        catch (System.Exception e)
        {
            InternalLogger.Error("Failed to create main menu dependency warning UI! Exception thrown: " + e);
        }
    }

    private static void CreateDependencyWarningUI(uGUI_MainMenu mainMenu)
    {
        var missingDependencies = GetMissingDependencies();
        if (missingDependencies.Count == 0)
            return;

        var textReference = mainMenu.transform.Find("Panel/MainMenu/PlayerName");
        var textObject = Object.Instantiate(textReference.gameObject, textReference.parent, true);
        textObject.name = "NautilusDependencyWarningText";
        Object.DestroyImmediate(textObject.GetComponent<MainMenuPlayerName>());
        textObject.SetActive(true);
        var transform = textObject.GetComponent<RectTransform>();
        transform.anchorMin = new Vector2(0, 0);
        transform.anchorMax = new Vector2(1, 0);
        transform.pivot = new Vector2(0.5f, 0);
        transform.anchoredPosition = new Vector2(0f, 25);
        transform.sizeDelta = new Vector2(1, 40);
        var text = textObject.GetComponentInChildren<TextMeshProUGUI>();
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.red;
        text.richText = true;
        text.geometrySortingOrder = VertexSortingOrder.Reverse;
        text.fontStyle = FontStyles.Bold;
        text.text = FormatMissingDependencies(missingDependencies);
    }

    private static string FormatMissingDependencies(IList<string> missingDependencies)
    {
        var sb = new StringBuilder();
        sb.Append("Missing ");
        sb.Append(missingDependencies.Count);
        sb.Append(" dependenc");
        sb.Append(missingDependencies.Count == 1 ? "y" : "ies");
        sb.Append(": ");
        for (int i = 0; i < missingDependencies.Count; i++)
        {
            sb.Append(missingDependencies[i]);
            if (i < missingDependencies.Count - 1)
            {
                sb.Append(", ");
            }
        }

        return $"<mark=#000000CC>{sb}</mark>";
    }

    private static List<string> GetMissingDependencies()
    {
        var missingDependencies = new HashSet<string>();
        // Get the list of dependency log warning messages
        // The format is as follows: "Could not load [{0}] because it has missing dependencies: {1}"
        var dependencyErrors = new List<string>(Chainloader.DependencyErrors);
        foreach (var error in dependencyErrors)
        {
            // Plugin GUIDs CANNOT contain colons, so this will only search for the colon in the log warning format
            var indexOfColon = error.LastIndexOf(':');
            // If the message didn't have a colon, it was of the wrong format
            if (indexOfColon <= 0) continue;
            // Otherwise, get the remainder of the string, which should be the dependency's GUID
            var dependencyGuid = error[(indexOfColon + 2)..];
            missingDependencies.Add(dependencyGuid);
        }

        var list = missingDependencies.ToList();
        list.Sort();
        return list;
    }
}