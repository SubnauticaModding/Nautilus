using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers.TitleScreen;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Patchers;

internal static class MainMenuPatcher
{
    internal static readonly SelfCheckingDictionary<string, TitleScreenHandler.CustomTitleData> TitleObjectDatas = new("TitleObjectData");

    private static string _activeModGUID;
    
    internal static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(MainMenuPatcher.AwakePostfix))));

        harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Start)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(MainMenuPatcher.StartPostfix))));
        
        InternalLogger.Log("MainMenuPatcher is done.", LogLevel.Debug);
    }

    private static void AwakePostfix()
    {
        foreach (var titleObjectData in TitleObjectDatas.Values)
        {
            foreach (var addon in titleObjectData.addons)
            {
                addon.Value.Initialize();
                addon.Value.OnDisable();
            }
        }
    }

    private static void StartPostfix(uGUI_MainMenu __instance)
    {
        CreateSelectionUI(__instance);
    }

    private static void CreateSelectionUI(uGUI_MainMenu mainMenu)
    {
        var optionsMenu = mainMenu.GetComponentInChildren<uGUI_OptionsPanel>(true);
        var background = optionsMenu.choiceOptionPrefab.transform.Find("Choice/Background");
        
        var selector = GameObject.Instantiate(background, mainMenu.transform, false);
        selector.gameObject.name = "ActiveModSelector";
        selector.localPosition = new Vector3(-750, -450, 0);

        var choice = selector.GetComponent<uGUI_Choice>();
        List<string> options = new() { "Subnautica" };
        foreach (var titleData in TitleObjectDatas.Values)
        {
            options.Add(titleData.localizationKey);
        }
        
        choice.SetOptions(options.ToArray());

        float maxWidth = float.MinValue;
        choice.currentText.transform.localPosition = Vector3.zero;
        for (int i = 0; i <= choice.options.Count; i++)
        {
            if (choice.currentText.preferredWidth > maxWidth)
            {
                maxWidth = choice.currentText.preferredWidth;
            }

            choice.NextChoice();
        }
        
        var nextButtonRect = choice.nextButton.GetComponent<RectTransform>();
        var prevButtonRect = choice.previousButton.GetComponent<RectTransform>();
        float offset = maxWidth * 0.5f + nextButtonRect.sizeDelta.x + 10;
        
        nextButtonRect.localPosition = new Vector3(offset, nextButtonRect.localPosition.y, nextButtonRect.localPosition.z);
        prevButtonRect.localPosition = new Vector3(-offset, prevButtonRect.localPosition.y, prevButtonRect.localPosition.z);

        UWE.CoroutineHost.StartCoroutine(AddButtonListeners(choice));
    }

    private static IEnumerator AddButtonListeners(uGUI_Choice choice)
    {
        yield return new WaitForEndOfFrame();
        
        var nextButton = choice.nextButton.GetComponent<Button>();
        var prevButton = choice.previousButton.GetComponent<Button>();
        
        nextButton.onClick.AddListener(() => OnActiveModChanged(choice));
        prevButton.onClick.AddListener(() => OnActiveModChanged(choice));
    }

    internal static void OnActiveModChanged(uGUI_Choice choice)
    {
        int index = 0;
        foreach (var titleData in TitleObjectDatas)
        {
            if (index == choice.currentIndex - 1)
            {
                foreach (var addon in titleData.Value.addons.Values)
                {
                    addon.OnEnable();
                }
            }
            else
            {
                foreach (var addon in titleData.Value.addons.Values)
                {
                    addon.OnDisable();
                }
            }
            
            index++;
        }
    }
}