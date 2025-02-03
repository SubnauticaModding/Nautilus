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
        var dependencyErrors = new List<string>(Chainloader.DependencyErrors.Where(ShouldDisplayError));
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
        foreach (Transform child in panesHolder)
        {
            Object.Destroy(child.gameObject);
        }

        var mainPaneTransform = Object.Instantiate(panePrefab, panesHolder).transform;
        var mainPaneContent = mainPaneTransform.Find("Viewport/Content");
#if BELOWZERO
        var layoutGroup = mainPaneContent.gameObject.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 65;
        layoutGroup.padding = new RectOffset(15, 15, 40, 15);
#endif
        // Create a list of all error messages that should be displayed
        var errorsToDisplay = new List<string>
        {
            $"<color=#FF0000>{formattedMissingDependencies}</color>",
            "<color=#FFFFFF>Mod load errors:</color>"
        };
        errorsToDisplay.AddRange(dependencyErrors);
        // Add error messages to menu
        foreach (var error in errorsToDisplay)
        {
            var errorEntryTextObj = Object.Instantiate(textReference, mainPaneContent, true);
            Object.DestroyImmediate(errorEntryTextObj.GetComponent<MainMenuPlayerName>());
            var errorEntryText = errorEntryTextObj.GetComponent<TextMeshProUGUI>();
            errorEntryText.text = error;
            errorEntryText.enableWordWrapping = true;
            errorEntryText.fontSize = 25;
            errorEntryText.richText = true;
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
        var missingDependencies = new Dictionary<string, Version>();
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
            // Split up separate GUIDs if there are multiple
            var dependencyGuids = dependencyGuidText.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries);
            // Format individual GUIDs and add them to the list
            foreach (var formattedGuid in dependencyGuids)
            {
                // Separate the name and version of each formatted GUID string
                var dependencyData = SeparateDependencyNameAndVersion(formattedGuid);
                // Update the dictionary of dependencies, avoiding duplicates and displaying the highest version
                if (missingDependencies.TryGetValue(dependencyData.Item1, out var previousHighestVersion))
                {
                    // Make sure only the highest version requirement is present
                    // E.g., if we have Nautilus (1.0 or newer) and Nautilus (2.0 or newer), only display the latter
                    if (previousHighestVersion == null ||
                        dependencyData.Item2 != null && dependencyData.Item2 > previousHighestVersion)
                    {
                        missingDependencies[dependencyData.Item1] = dependencyData.Item2;
                    }
                }
                else
                {
                    missingDependencies.Add(dependencyData.Item1, dependencyData.Item2);
                }
            }
        }

        // Finally, make a list of formatted strings:
        var formattedDependencyStrings = new List<string>();
        foreach (var dependency in missingDependencies)
        {
            var formattedString = dependency.Key;
            // Only add the version number if the requirement exists and isn't 0.0.0
            if (dependency.Value != default)
            {
                formattedString += $" ({dependency.Value} or newer)";
            }

            formattedDependencyStrings.Add(formattedString);
        }

        formattedDependencyStrings.Sort();
        return formattedDependencyStrings;
    }

    // Attempts to find a version from the given dependency name and returns the GUID name by itself
    // Example string of a formatted dependency name with version:
    // "com.snmodding.nautilus (v1.0.0.34 or newer)"
    private static (string, Version) SeparateDependencyNameAndVersion(string formattedDependency)
    {
        try
        {
            // Parentheses cannot exist in GUIDs, so this is ONLY present if the GUID is accompanied by a version
            if (!formattedDependency.Contains("("))
            {
                return (formattedDependency, default);
            }
            
            // Surpass the "(v"
            var versionBeginIndex = formattedDependency.IndexOf('(') + 2;
            var length = formattedDependency.LastIndexOf(" or newer)", StringComparison.Ordinal) + 1 -
                         versionBeginIndex;
            var rawGuidName = formattedDependency.Substring(0, versionBeginIndex - 3);
            return (rawGuidName, Version.Parse(formattedDependency.Substring(versionBeginIndex, length)));
        }
        catch (Exception e)
        {
            InternalLogger.Warn($"Exception getting version from string '{formattedDependency}'" + e);
            return (formattedDependency, default);
        }
    }
}