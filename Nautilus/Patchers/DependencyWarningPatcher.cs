using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Bootstrap;
using HarmonyLib;
using Nautilus.MonoBehaviours;
using Nautilus.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        catch (Exception e)
        {
            InternalLogger.Error("Failed to create main menu dependency warning UI! Exception thrown: " + e);
        }
    }

    private static void CreateDependencyWarningUI(uGUI_MainMenu mainMenu)
    {
        var dependencyErrors = Chainloader.DependencyErrors;
        if (dependencyErrors.Count == 0)
            return;

        var missingDependencies = GetMissingDependencies();
        var formattedMissingDependencies = FormatMissingDependencies(missingDependencies);

        // -- Add warning text --
        var textReference = mainMenu.transform.Find("Panel/MainMenu/PlayerName").gameObject;
        var textObject = Object.Instantiate(textReference, textReference.transform.parent, true);
        textObject.name = "NautilusDependencyWarningText";
        Object.DestroyImmediate(textObject.GetComponent<MainMenuPlayerName>());
        textObject.SetActive(true);
        var transform = textObject.GetComponent<RectTransform>();
        transform.anchorMin = new Vector2(0, 1);
        transform.anchorMax = new Vector2(1, 1);
        transform.pivot = new Vector2(0.5f, 0);
        transform.sizeDelta = new Vector2(1, 40);
        transform.offsetMin = new Vector2(-0.5f, 0);
        transform.offsetMax = new Vector2(-0.5f, 570);
        var text = textObject.GetComponentInChildren<TextMeshProUGUI>();
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.red;
        text.richText = true;
        text.geometrySortingOrder = VertexSortingOrder.Reverse;
        text.fontStyle = FontStyles.Bold;
        text.text = "<mark=#000000CC>Some mods failed to load! Please view the mod load errors for more details.\n" +
                    formattedMissingDependencies + "</mark>";

        // -- Add mod load errors window --
        var optionsWindowReference = mainMenu.transform.Find("Panel/Options");
        var errorsMenuPanel =
            Object.Instantiate(optionsWindowReference.gameObject, optionsWindowReference.parent, true);
        errorsMenuPanel.name = "NautilusModErrorsWindow";
        var menuBehaviour = errorsMenuPanel.AddComponent<PluginLoadErrorMenu>();
        errorsMenuPanel.SetActive(false);
        var optionsPanel = errorsMenuPanel.GetComponentInChildren<uGUI_OptionsPanel>();
        var panePrefab = optionsPanel.panePrefab;
        Object.DestroyImmediate(optionsPanel);
        var errorsMenuHeader = errorsMenuPanel.transform.Find("Header");
        errorsMenuHeader.gameObject.GetComponent<TextMeshProUGUI>().text = "BepInEx plugin load errors";
        Object.DestroyImmediate(errorsMenuHeader.GetComponent<TranslationLiveUpdate>());
        errorsMenuPanel.transform.Find("Middle/TabsHolder").gameObject.SetActive(false);
        var panesHolder = errorsMenuPanel.transform.Find("Middle/PanesHolder");
        var mainPaneTransform = Object.Instantiate(panePrefab, panesHolder).transform;
        var mainPaneContent = mainPaneTransform.Find("Viewport/Content");
        // Create a list of all error messages that should be displayed
        var errorsToDisplay = new List<string> { formattedMissingDependencies };
        errorsToDisplay.AddRange(dependencyErrors.Where(ShouldDisplayError));
        // Add error messages to menu
        foreach (var error in errorsToDisplay)
        {
            var errorEntryTextObj = Object.Instantiate(textReference, mainPaneContent, true);
            Object.DestroyImmediate(errorEntryTextObj.GetComponent<MainMenuPlayerName>());
            var errorEntryText = errorEntryTextObj.GetComponent<TextMeshProUGUI>();
            errorEntryText.text = error;
            errorEntryText.enableWordWrapping = true;
            errorEntryText.fontSize = 25;
            errorEntryTextObj.SetActive(true);
            errorEntryTextObj.transform.localScale = Vector3.one;
        }

        // Fix up buttons and add close window button
        var closeWindowButton = errorsMenuPanel.transform.Find("Bottom/ButtonBack").gameObject;
        Object.DestroyImmediate(closeWindowButton.GetComponentInChildren<TranslationLiveUpdate>());
        closeWindowButton.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
        var closeWindowButtonComponent = closeWindowButton.GetComponent<Button>();
        closeWindowButtonComponent.onClick.RemoveAllListeners();
        closeWindowButtonComponent.onClick.AddListener(menuBehaviour.OnButtonClose);
        errorsMenuPanel.transform.Find("Bottom/ButtonApply").gameObject.SetActive(false);

        // -- Add open button --
        var openButtonParent = mainMenu.transform.Find("Panel/MainMenu");
        var openButtonReference = mainMenu.transform.Find("Panel/Options/Bottom/ButtonBack");
        var openButton = Object.Instantiate(openButtonReference.gameObject, openButtonParent, true);
        openButton.name = "NautilusViewModLoadErrorsButton";
        var buttonTransform = openButton.GetComponent<RectTransform>();
        buttonTransform.SetSiblingIndex(0);
        buttonTransform.anchorMin = new Vector2(0.65f, 0.01f);
        buttonTransform.anchorMax = new Vector2(0.99f, 0.12f);
        buttonTransform.offsetMin = new Vector2(-0.5f, -0.5f);
        buttonTransform.offsetMax = new Vector2(0.5f, 0.5f);
        buttonTransform.sizeDelta = Vector2.one;
        Object.DestroyImmediate(openButton.GetComponentInChildren<TranslationLiveUpdate>());
        buttonTransform.GetComponentInChildren<TextMeshProUGUI>().text =
            $"View mod load errors ({dependencyErrors.Count})";
        var openWindowButtonComponent = openButton.GetComponent<Button>();
        openWindowButtonComponent.onClick.RemoveAllListeners();
        openWindowButtonComponent.onClick.AddListener(menuBehaviour.OnButtonOpen);
        openButton.SetActive(true);
    }

    private static bool ShouldDisplayError(string errorMessage)
    {
        // These errors occur for perfectly working mods, don't do any good for people, and will confuse users
        if (errorMessage.Contains("targets a wrong version of BepInEx"))
            return false;
        return true;
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

        return sb.ToString();
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
            // If the message contained the word "incompatible," it was an incompatibility error and should be ignored
            var indexOfIncompatible = error.IndexOf("incompatible", StringComparison.Ordinal);
            if (indexOfIncompatible >= 0 && indexOfIncompatible < indexOfColon) continue;
            // Otherwise, get the remainder of the string, which should be the dependency's GUID
            var dependencyGuidText = error[(indexOfColon + 2)..];
            var dependencyGuids = dependencyGuidText.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var guid in dependencyGuids)
            {
                missingDependencies.Add(guid);
            }
        }

        var list = missingDependencies.ToList();
        list.Sort();
        return list;
    }
}