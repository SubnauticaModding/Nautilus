using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Logging;
using FMOD.Studio;
using HarmonyLib;
using Nautilus.Handlers.LoadingScreen;
using Nautilus.Handlers.TitleScreen;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Nautilus.Patchers;

internal static class MainMenuPatcher
{
    #if SUBNAUTICA
    private const string GameName = "Subnautica";
    #elif BELOWZERO
    private const string GameName = "Below Zero";
    #endif
    
    internal static readonly SelfCheckingDictionary<string, TitleScreenHandler.CustomTitleData> TitleObjectDatas = new("TitleObjectData");

    internal static readonly SelfCheckingDictionary<string, TitleScreenHandler.CollaborationData> CollaborationData =
        new("TitleCollaborationData");

    internal static event Action onActiveModChanged;
    private static ConfigEntry<string> _activeModGuid;
    private static uGUI_Choice _choiceOption;
    
    internal static void Patch(Harmony harmony, ConfigFile config)
    {
        harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(AwakePostfix))));

        harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.Start)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(StartPostfix))));
        
        harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.StartNewGame)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(CallAddonCleanups))));
        
        harmony.Patch(AccessTools.Method(typeof(uGUI_MainMenu), nameof(uGUI_MainMenu.LoadGameAsync)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(CallAddonCleanups))));

        harmony.Patch(AccessTools.Method(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(SceneLoadingAwakePostfix))));
        
        harmony.Patch(AccessTools.Method(typeof(MainMenuMusic), nameof(MainMenuMusic.Stop)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(MainMenuPatcher), nameof(MainMenuMusicStopPostfix))));
        
        _activeModGuid = config.Bind("Nautilus", "ActiveModTheme", GameName);
        InternalLogger.Log("MainMenuPatcher is done.", LogLevel.Debug);
    }

    private static void AwakePostfix()
    {
        foreach (var titleObjectData in TitleObjectDatas)
        {
            InitializeAddons(titleObjectData.Key, titleObjectData.Value);
        }
    }

    private static void SceneLoadingAwakePostfix(uGUI_SceneLoading __instance)
    {
        __instance.gameObject.EnsureComponent<LoadingScreenSetter>();
    }

    private static void MainMenuMusicStopPostfix()
    {
        foreach (var titleObjectData in TitleObjectDatas)
        {
            foreach (var addon in titleObjectData.Value.addons)
            {
                if (addon is MusicTitleAddon)
                {
                    addon.Disable();
                }
            }
        }
    }

    private static void InitializeAddons(string guid, TitleScreenHandler.CustomTitleData titleData)
    {
        foreach (var addon in titleData.addons)
        {
            addon.ModGuid = guid;
            addon.Initialize();
            addon.Disable();
        }
    }

    private static void StartPostfix(uGUI_MainMenu __instance)
    {
        CreateSelectionUI(__instance);
    }
    
    private static void CallAddonCleanups()
    {
        if (!TitleObjectDatas.TryGetValue(_activeModGuid.Value, out var titleData)) return;
        
        foreach (var addon in titleData.addons)
        {
            addon.CleanupUponLoadScreen();
        }
    }

    private static void CreateSelectionUI(uGUI_MainMenu mainMenu)
    {
        var optionsMenu = mainMenu.GetComponentInChildren<uGUI_OptionsPanel>(true);
#if SUBNAUTICA
        var background = optionsMenu.controls.prefabChoice.transform.Find("Choice/Background");
#else
        var background = optionsMenu.choiceOptionPrefab.transform.Find("Choice/Background");
#endif
        
        
        var selector = GameObject.Instantiate(background, mainMenu.transform, false);
        selector.gameObject.name = "ActiveModSelector";

        _choiceOption = selector.GetComponent<uGUI_Choice>();
        _choiceOption.currentText.raycastTarget = false;
        UpdateSelectorAnchors(_choiceOption);
        RefreshModOptions(_choiceOption);
        
        if (TitleObjectDatas.TryGetValue(_activeModGuid.Value, out var data))
        {
            int currentIndex = _choiceOption.options.IndexOf(data.localizationKey);
            if (currentIndex >= 0)
            {
                _choiceOption.value = currentIndex;
                OnActiveModChanged();
            }
        }
        
        optionsMenu.transform.Find("Bottom/ButtonApply").GetComponent<Button>().onClick.AddListener(() =>
        {
            UpdateSelectorAnchors(_choiceOption);
            RefreshModOptions(_choiceOption);
        });
        
        Language.main.onLanguageChanged += () => UpdateButtonPositions(_choiceOption);

        UWE.CoroutineHost.StartCoroutine(AddListenersDelayed());
    }

    private static void UpdateSelectorAnchors(uGUI_Choice choice)
    {
        choice.transform.localPosition = new Vector3(-750, -440, 0);
        var rect = choice.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0);
        rect.anchorMax = new Vector2(0.2f, 0);
        rect.pivot = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 50);
        choice.transform.localPosition = new Vector3(-750, -440, 0);
    }

    private static IEnumerator AddListenersDelayed()
    {
        // Wait for choice option to register its events first, so that the selection option is changed before we look at it
        yield return new WaitForEndOfFrame();
        
        var nextButton = _choiceOption.nextButton.GetComponent<Button>();
        var prevButton = _choiceOption.previousButton.GetComponent<Button>();
        
        nextButton.onClick.AddListener(OnActiveModChanged);
        prevButton.onClick.AddListener(OnActiveModChanged);
    }

    private static void RefreshModOptions(uGUI_Choice choice)
    {
        List<string> options = new() { GameName };
        foreach (var titleData in TitleObjectDatas)
        {
            if (!titleData.Value.addons.Any(AddonApprovedForCollab))
            {
                InternalLogger.Log(
                    $"{titleData.Key} did not have any addons that were approved for collaboration. Skipping option registration",
                    LogLevel.Warning);
                continue;
            }
            
            options.Add(titleData.Value.localizationKey);
        }
        
        choice.SetOptions(options.ToArray());
        
        UpdateButtonPositions(choice);
    }

    private static void UpdateButtonPositions(uGUI_Choice choice)
    {
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
        var textRect = choice.currentText.rectTransform;
        float offset = maxWidth * 0.5f + nextButtonRect.sizeDelta.x - 10;
        
        // Stop buttons from going offscreen with really long mod names
        offset = Mathf.Min(offset, 180);

        var localPosPrev = new Vector3(-offset, 0, 0);
        prevButtonRect.localPosition = localPosPrev;
        prevButtonRect.pivot = new Vector2(1, 0.5f);
        prevButtonRect.anchorMin = new Vector2(0, 0.5f);
        prevButtonRect.anchorMax = new Vector2(0, 0.5f);
        prevButtonRect.offsetMin = Vector2.zero;
        prevButtonRect.offsetMax = Vector2.zero;
        prevButtonRect.sizeDelta = new Vector2(25, 45);
        prevButtonRect.localPosition = localPosPrev;

        var localPosNext = new Vector3(offset, 0, 0);
        nextButtonRect.localPosition = localPosNext;
        nextButtonRect.pivot = new Vector2(0, 0.5f);
        nextButtonRect.anchorMin = new Vector2(1, 0.5f);
        nextButtonRect.anchorMax = new Vector2(1, 0.5f);
        nextButtonRect.offsetMin = Vector2.zero;
        nextButtonRect.offsetMax = Vector2.zero;
        nextButtonRect.sizeDelta = new Vector2(25, 45);
        nextButtonRect.localPosition = localPosNext;
        
        textRect.localScale = Vector3.one;
        textRect.localPosition = Vector3.zero;
        textRect.anchorMin = new Vector2(0.1f, 0);
        textRect.anchorMax = new Vector2(0.9f, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        textRect.localPosition = Vector3.zero;
        
        choice.gameObject.SetActive(choice.options.Count > 1);
    }

    private static void OnActiveModChanged()
    {
        var choice = _choiceOption;
        
        int index = 0;
        foreach (var titleData in TitleObjectDatas)
        {
            if (index == choice.currentIndex - 1)
            {
                foreach (var addon in titleData.Value.addons)
                {
                    if (!AddonApprovedForCollab(addon)) continue;

                    if (!addon.IsEnabled)
                    {
                        addon.Enable();
                    }

                    if (addon is MusicTitleAddon)
                    {
                        EventInstance musicEvent;
                        #if SUBNAUTICA
                        musicEvent = MainMenuMusic.main.evt;
                        #elif BELOWZERO
                        musicEvent = MainMenuMusic.main.eventMusic;
                        #endif
                        
                        musicEvent.stop(STOP_MODE.ALLOWFADEOUT);
                    }
                }
            }
            else
            {
                foreach (var addon in titleData.Value.addons)
                {
                    if (addon.IsEnabled)
                    {
                        addon.Disable();
                    }
                }
            }
            
            index++;
        }
        
        bool customMusicActive = TitleObjectDatas.Values.Any(data =>
            data.addons.Any(addon => addon is MusicTitleAddon && addon.IsEnabled));
        if (!customMusicActive)
        {
            EventInstance musicEvent;
            #if SUBNAUTICA
            musicEvent = MainMenuMusic.main.evt;
            #elif BELOWZERO
            musicEvent = MainMenuMusic.main.eventMusic;
            #endif
            
            musicEvent.getPlaybackState(out var state);
            if (state is not (PLAYBACK_STATE.PLAYING or PLAYBACK_STATE.SUSTAINING))
            {
                musicEvent.start();
            }
        }

        var data = TitleObjectDatas.FirstOrDefault(d => d.Value.localizationKey == choice.options[choice.currentIndex]);
        _activeModGuid.Value = choice.currentIndex == 0 ? GameName : data.Key;

        onActiveModChanged?.Invoke();
    }

    private static bool AddonApprovedForCollab(TitleAddon addon)
    {
        bool hasRequiredMods = true;
        foreach (var guid in addon.RequiredGUIDs)
        {
            hasRequiredMods = false;

            if (!CollaborationData.TryGetValue(guid, out var collabData))
            {
                InternalLogger.Log(
                    $"{guid} was not installed/registered for the addon {addon.GetType()} from {addon.ModGuid}. Not allowing addon to be enabled", 
                    LogLevel.Debug);
                break;
            }

            if (!collabData.modApprovedAddons.TryGetValue(addon.ModGuid, out var approvedTypes))
            {
                InternalLogger.Log(
                    $"{guid} did not have {addon.ModGuid} listed as a collaborator. Not allowing {addon.GetType()} to be enabled", 
                    LogLevel.Debug);
                break;
            }

            bool approvesAll = approvedTypes[0] == typeof(ApproveAllAddons);
            if (!approvesAll && !approvedTypes.Contains(addon.GetType()))
            {
                InternalLogger.Log(
                    $"{guid} had {addon.ModGuid} listed as a collaborator, but {addon.GetType()} was not a whitelisted type. Not allowing addon to be enabled", 
                    LogLevel.Debug);
                break;
            }

            hasRequiredMods = true;
        }

        return hasRequiredMods;
    }

    internal static void RegisterTitleObjectData(string key, TitleScreenHandler.CustomTitleData data)
    {
        if (TitleObjectDatas.ContainsKey(key))
        {
            InternalLogger.Log($"MainMenuPatcher already contain title data for {key}! Skipping.", LogLevel.Error);
            return;
        }
        
        TitleObjectDatas.Add(key, data);

        if (!_choiceOption) return;
        
        InitializeAddons(key, data);
        RefreshModOptions(_choiceOption);
    }

    internal static string GetActiveModGuid() => _activeModGuid.Value;
}