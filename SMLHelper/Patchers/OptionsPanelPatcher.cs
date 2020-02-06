namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using Options;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using QModManager.API;

    using Object = UnityEngine.Object;

    internal class OptionsPanelPatcher
    {
        internal static SortedList<string, ModOptions> modOptions = new SortedList<string, ModOptions>();

        private static int  modsTabIndex = -1;
        private static bool isMainMenu = true; // is options was opened in main menu or in game

        internal static void Patch(HarmonyInstance harmony)
        {
            PatchUtils.PatchClass(harmony);

            if (QModServices.Main.FindModById("ModsOptionsAdjusted")?.Enable ?? false)
                V2.Logger.Log("ModOptionsAdjuster is not inited (ModsOptionsAdjusted mod is active)", LogLevel.Warn);
            else
            {
                PatchUtils.PatchClass(harmony, typeof(ModOptionsAdjuster));
                PatchUtils.PatchClass(harmony, typeof(ModOptionsHeadingsToggle));
            }
        }


        // 'Mods' tab also added in QModManager, so we can't rely on 'modsTab' in AddTabs_Postfix
        [HarmonyPostfix]
        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTab))]
        internal static void AddTab_Postfix(string label, int __result)
        {
            if (label == "Mods")
                modsTabIndex = __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTabs))]
        internal static void AddTabs_Postfix(uGUI_OptionsPanel __instance)
        {
            uGUI_OptionsPanel optionsPanel = __instance;
            isMainMenu = (optionsPanel.GetComponent<MainMenuOptions>() != null);

            // Start the modsTab index at a value of -1
            int modsTab = -1;
            // Loop through all of the tabs
            for (int i = 0; i < optionsPanel.tabsContainer.childCount; i++)
            {
                // Check if they are named "Mods"
                var text = optionsPanel.tabsContainer.GetChild(i).GetComponentInChildren<Text>(true);
                if (text != null && text.text == "Mods")
                {
                    // Set the tab index to the found one and break
                    modsTab = i;
                    break;
                }
            }
            // If no tab was found, create one
            if (modsTab == -1)
            {
                modsTab = optionsPanel.AddTab("Mods");
            }

            // Maybe this could be split into its own file to handle smlhelper options, or maybe it could be removed alltogether
            optionsPanel.AddHeading(modsTab, "SMLHelper");
            optionsPanel.AddToggleOption(modsTab, "Enable debug logs", V2.Logger.EnableDebugging, V2.Logger.SetDebugging);
            optionsPanel.AddChoiceOption(modsTab, "Extra item info", new string[]
            {
                "Mod name (default)",
                "Mod name and item ID",
                "Nothing"
            }, (int)TooltipPatcher.ExtraItemInfoOption, (i) => TooltipPatcher.SetExtraItemInfo((TooltipPatcher.ExtraItemInfo)i));

            foreach (ModOptions modOption in modOptions.Values)
            {
                optionsPanel.AddHeading(modsTab, modOption.Name);

                foreach (ModOption option in modOption.Options)
                {
                    switch (option.Type)
                    {
                        case ModOptionType.Slider:
                            var slider = (ModSliderOption)option;

                            optionsPanel.AddSliderOption(modsTab, slider.Label, slider.Value, slider.MinValue, slider.MaxValue, slider.Value,
                                new UnityAction<float>((float sliderVal) =>
                                    modOption.OnSliderChange(slider.Id, sliderVal)));
                            break;
                        case ModOptionType.Toggle:
                            var toggle = (ModToggleOption)option;

                            optionsPanel.AddToggleOption(modsTab, toggle.Label, toggle.Value,
                                new UnityAction<bool>((bool toggleVal) =>
                                    modOption.OnToggleChange(toggle.Id, toggleVal)));
                            break;
                        case ModOptionType.Choice:
                            var choice = (ModChoiceOption)option;

                            optionsPanel.AddChoiceOption(modsTab, choice.Label, choice.Options, choice.Index,
                                new UnityAction<int>((int index) =>
                                    modOption.OnChoiceChange(choice.Id, index, choice.Options[index])));
                            break;
                        case ModOptionType.Keybind:
                            var keybind = (ModKeybindOption)option;

                            ModKeybindOption.AddBindingOptionWithCallback(optionsPanel, modsTab, keybind.Label, keybind.Key, keybind.Device,
                                new UnityAction<KeyCode>((KeyCode key) => 
                                    modOption.OnKeybindChange(keybind.Id, key)));
                            break;
                        default:
                            V2.Logger.Log($"Invalid ModOptionType detected for option: {option.Id} ({option.Type.ToString()})", LogLevel.Error);
                            break;
                    }
                }
            }
        }

        // Adjusting mod options ui elements so they don't overlap with their text labels.
        // We add corresponding 'adjuster' components to each ui element in mod options tab.
        // Reason for using components is to skip one frame before manually adjust ui elements to make sure that Unity UI Layout components is updated
        private static class ModOptionsAdjuster
        {
            private static readonly Tuple<string, Type>[] optionTypes = new Tuple<string, Type>[]
            {
                Tuple.Create("uGUI_ToggleOption",  typeof(AdjustToggleOption)),
                Tuple.Create("uGUI_SliderOption",  typeof(AdjustSliderOption)),
                Tuple.Create("uGUI_ChoiceOption",  typeof(AdjustChoiceOption)),
                Tuple.Create("uGUI_BindingOption", typeof(AdjustBindingOption))
            };

            [HarmonyPostfix]
            [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddItem), new Type[] { typeof(int), typeof(GameObject) })]
            private static void AddItem_Postfix(int tabIndex, GameObject __result)
            {
                if (__result == null || tabIndex != modsTabIndex)
                    return;

                foreach (var type in optionTypes)
                {
                    if (__result.name.Contains(type.Item1))
                    {
                        __result.EnsureComponent(type.Item2);
                        break;
                    }
                }
            }

            // base class for 'adjuster' components
            // we add ContentSizeFitter component to text label so it will change width in its Update() based on text
            // that's another reason to skip one frame
            private abstract class AdjustModOption: MonoBehaviour
            {
                private const float minCaptionWidth_MainMenu = 480f;
                private const float minCaptionWidth_InGame   = 360f;
                private GameObject caption = null;

                protected float CaptionWidth { get => caption?.GetComponent<RectTransform>().rect.width ?? 0f; }

                protected static Vector2 SetVec2x(Vector2 vec, float val)  { vec.x = val; return vec; }

                protected void SetCaptionGameObject(string gameObjectPath)
                {
                    caption = gameObject.transform.Find(gameObjectPath)?.gameObject;

                    if (!caption)
                    {
                        V2.Logger.Log($"AdjustModOption: caption gameobject '{gameObjectPath}' not found", V2.LogLevel.Warn);
                        return;
                    }

                    caption.AddComponent<LayoutElement>().minWidth = isMainMenu? minCaptionWidth_MainMenu: minCaptionWidth_InGame;
                    caption.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // for autosizing captions

                    RectTransform transform = caption.GetComponent<RectTransform>();
                    transform.SetAsFirstSibling(); // for HorizontalLayoutGroup
                    transform.pivot = SetVec2x(transform.pivot, 0f);
                    transform.anchoredPosition = SetVec2x(transform.anchoredPosition, 0f);
                }
            }

            // in case of ToggleOption there is no need to manually move elements
            // other option types don't work well with HorizontalLayoutGroup :(
            private class AdjustToggleOption: AdjustModOption
            {
                private const float spacing = 20f;

                public void Awake()
                {
                    HorizontalLayoutGroup hlg = gameObject.transform.Find("Toggle").gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childControlWidth = false;
                    hlg.childForceExpandWidth = false;
                    hlg.spacing = spacing;

                    SetCaptionGameObject("Toggle/Caption");

                    Destroy(this);
                }
            }

            private class AdjustSliderOption: AdjustModOption
            {
                private const float spacing = 25f;
                private const float sliderValueWidth = 85f;

                public IEnumerator Start()
                {
                    SetCaptionGameObject("Slider/Caption");
                    yield return null; // skip one frame

                    // for some reason sliders don't update their handle positions sometimes
                    uGUI_SnappingSlider slider = gameObject.GetComponentInChildren<uGUI_SnappingSlider>();
                    AccessTools.Method(typeof(Slider), "UpdateVisuals")?.Invoke(slider, null);

                    // changing width for slider value label
                    RectTransform sliderValueRect = gameObject.transform.Find("Slider/Value") as RectTransform;
                    sliderValueRect.sizeDelta = SetVec2x(sliderValueRect.sizeDelta, sliderValueWidth);

                    // changing width for slider
                    RectTransform rect = gameObject.transform.Find("Slider/Background") as RectTransform;

                    float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
                    float widthSlider = rect.rect.width;
                    float widthText = CaptionWidth + spacing;

                    if (widthText + widthSlider + sliderValueWidth > widthAll)
                        rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - sliderValueWidth - widthSlider);

                    Destroy(this);
                }
            }

            private class AdjustChoiceOption: AdjustModOption
            {
                private const float spacing = 10f;

                public IEnumerator Start()
                {
                    SetCaptionGameObject("Choice/Caption");
                    yield return null; // skip one frame

                    RectTransform rect = gameObject.transform.Find("Choice/Background") as RectTransform;

                    float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
                    float widthChoice = rect.rect.width;
                    float widthText = CaptionWidth + spacing;

                    if (widthText + widthChoice > widthAll)
                        rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - widthChoice);

                    Destroy(this);
                }
            }

            private class AdjustBindingOption: AdjustModOption
            {
                private const float spacing = 10f;

                public IEnumerator Start()
                {
                    SetCaptionGameObject("Caption");
                    yield return null; // skip one frame

                    RectTransform rect = gameObject.transform.Find("Bindings") as RectTransform;

                    float widthAll = gameObject.GetComponent<RectTransform>().rect.width;
                    float widthBinding = rect.rect.width;
                    float widthText = CaptionWidth + spacing;

                    if (widthText + widthBinding > widthAll)
                        rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - widthBinding);

                    Destroy(this);
                }
            }
        }


        // Class for collapsing/expanding options in 'Mods' tab
        // Options can be collapsed/expanded by clicking on mod's title or arrow button
        private static class ModOptionsHeadingsToggle
        {
            private enum HeadingState { Collapsed, Expanded };

            private static GameObject headingPrefab = null;

            private static class StoredHeadingStates
            {
                private class StatesConfig
                {
                    private readonly Dictionary<string, HeadingState> states = new Dictionary<string, HeadingState>();

                    public HeadingState this[string name]
                    {
                        get => states.TryGetValue(name, out HeadingState state)? state: HeadingState.Expanded;

                        set
                        {
                            states[name] = value;
                            // TODO: save
                        }
                    }
                }
                private static readonly StatesConfig statesConfig = new StatesConfig(); // TODO: load

                public static HeadingState get(string name) => statesConfig[name];
                public static void store(string name, HeadingState state) => statesConfig[name] = state;
            }

            // we add arrow button from Choice ui element to the options headings for collapsing/expanding 
            private static void InitHeadingPrefab(uGUI_TabbedControlsPanel panel)
            {
                if (headingPrefab)
                    return;

                headingPrefab = Object.Instantiate(panel.headingPrefab);
                headingPrefab.name = "OptionHeadingToggleable";
                headingPrefab.AddComponent<HeadingToggle>();

                Transform captionTransform = headingPrefab.transform.Find("Caption");
                captionTransform.localPosition = new Vector3(45f, 0f, 0f);
                captionTransform.gameObject.AddComponent<HeadingClickHandler>();
                captionTransform.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                GameObject button = Object.Instantiate(panel.choiceOptionPrefab.transform.Find("Choice/Background/NextButton").gameObject);
                button.name = "HeadingToggleButton";
                button.AddComponent<ToggleButtonClickHandler>();

                RectTransform buttonTransform = button.transform as RectTransform;
                buttonTransform.SetParent(headingPrefab.transform);
                buttonTransform.SetAsFirstSibling();
                buttonTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
                buttonTransform.localPosition = new Vector3(15f, -13f, 0f);
                buttonTransform.pivot = new Vector2(0.25f, 0.5f);
                buttonTransform.anchorMin = buttonTransform.anchorMax = new Vector2(0f, 0.5f);
            }

            #region components
            // main component for headings toggling
            private class HeadingToggle: MonoBehaviour
            {
                private HeadingState headingState = HeadingState.Expanded;
                private string headingName = null;
                private List<GameObject> childOptions = null;

                private void Init()
                {
                    if (childOptions != null)
                        return;

                    headingName = transform.Find("Caption")?.GetComponent<Text>()?.text;

                    childOptions = new List<GameObject>();

                    for (int i = transform.GetSiblingIndex() + 1; i < transform.parent.childCount; i++)
                    {
                        GameObject option = transform.parent.GetChild(i).gameObject;

                        if (option.GetComponent<HeadingToggle>())
                            break;

                        childOptions.Add(option);
                    }
                }

                public void EnsureState() // for setting previously saved state
                {
                    Init();

                    HeadingState storedState = StoredHeadingStates.get(headingName);

                    if (headingState != storedState)
                    {
                        SetState(storedState);
                        GetComponentInChildren<ToggleButtonClickHandler>()?.SetStateInstant(storedState);
                    }
                }

                public void SetState(HeadingState state)
                {
                    Init();

                    childOptions.ForEach(option => option.SetActive(state == HeadingState.Expanded));
                    headingState = state;

                    StoredHeadingStates.store(headingName, state);
                }
            }

            // click handler for arrow button
            private class ToggleButtonClickHandler: MonoBehaviour, IPointerClickHandler
            {
                private const float timeRotate = 0.1f;
                private HeadingState headingState = HeadingState.Expanded;
                private bool isRotating = false;

                public void SetStateInstant(HeadingState state)
                {
                    headingState = state;
                    transform.localEulerAngles = new Vector3(0, 0, headingState == HeadingState.Expanded? -90: 0);
                }

                public void OnPointerClick(PointerEventData _)
                {
                    if (isRotating)
                        return;

                    headingState = headingState == HeadingState.Expanded? HeadingState.Collapsed: HeadingState.Expanded;
                    StartCoroutine(SmoothRotate(headingState == HeadingState.Expanded? -90: 90));

                    GetComponentInParent<HeadingToggle>()?.SetState(headingState);
                }

                private IEnumerator SmoothRotate(float angles)
                {
                    isRotating = true;

                    Quaternion startRotation = transform.localRotation;
                    Quaternion endRotation = Quaternion.Euler(new Vector3(0f, 0f, angles)) * startRotation;

                    float timeStart = Time.realtimeSinceStartup; // Time.deltaTime works only in main menu

                    while (timeStart + timeRotate > Time.realtimeSinceStartup)
                    {
                        transform.localRotation = Quaternion.Lerp(startRotation, endRotation, (Time.realtimeSinceStartup - timeStart) / timeRotate);
                        yield return null;
                    }

                    transform.localRotation = endRotation;
                    isRotating = false;
                }
            }

            // click handler for title, just redirects clicks to button click handler
            private class HeadingClickHandler: MonoBehaviour, IPointerClickHandler
            {
                public void OnPointerClick(PointerEventData eventData) =>
                    transform.parent.GetComponentInChildren<ToggleButtonClickHandler>()?.OnPointerClick(eventData);
            }
            #endregion

            #region patches for uGUI_TabbedControlsPanel
            [HarmonyPrefix]
            [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddHeading))]
            private static bool AddHeading_Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex, string label)
            {
                if (tabIndex != modsTabIndex)
                    return true;

                __instance.AddItem(tabIndex, headingPrefab, label);
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.Awake))]
            private static void Awake_Postfix(uGUI_TabbedControlsPanel __instance)
            {
                InitHeadingPrefab(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab))]
            private static void SetVisibleTab_Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex)
            {
                if (tabIndex != modsTabIndex)
                    return;

                // just in case, for changing vertical spacing between ui elements
                //__instance.tabs[tabIndex].container.GetComponent<VerticalLayoutGroup>().spacing = 15f; // default is 15f

                Transform options = __instance.tabs[tabIndex].container.transform;

                for (int i = 0; i < options.childCount; i++)
                    options.GetChild(i).GetComponent<HeadingToggle>()?.EnsureState();
            }
            #endregion
        }
    }
}
