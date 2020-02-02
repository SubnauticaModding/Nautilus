namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using Options;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    internal class OptionsPanelPatcher
    {
        internal static SortedList<string, ModOptions> modOptions = new SortedList<string, ModOptions>();

        private static int  modsTab = -1; // index of 'Mods' tab
        private static bool isMainMenu = true; // is options was opened in main menu or in game

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTabs)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(OptionsPanelPatcher), nameof(OptionsPanelPatcher.AddTabs_Postfix))));

            ModOptionsAdjuster.Init(harmony);
        }

        internal static void AddTabs_Postfix(uGUI_OptionsPanel __instance)
        {
            uGUI_OptionsPanel optionsPanel = __instance;
            isMainMenu = (optionsPanel.GetComponent<MainMenuOptions>() != null);

            // Start the modsTab index at a value of -1
            modsTab = -1;
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
            public static void Init(HarmonyInstance harmony)
            {
                MethodInfo uGUITabbedControlsPanel_AddItem =
                    AccessTools.Method(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddItem), new Type[] { typeof(int), typeof(GameObject) });

                if (harmony.GetPatchInfo(uGUITabbedControlsPanel_AddItem) == null) // check that it is not already patched
                {
                    harmony.Patch(uGUITabbedControlsPanel_AddItem,
                        postfix: new HarmonyMethod(AccessTools.Method(typeof(ModOptionsAdjuster), nameof(ModOptionsAdjuster.AddItem_Postfix))));

                    V2.Logger.Log("ModOptionsAdjuster is inited", LogLevel.Debug);
                }
                else
                    V2.Logger.Log("ModOptionsAdjuster is not inited", LogLevel.Warn);
            }

            private static readonly Tuple<string, Type>[] optionTypes = new Tuple<string, Type>[]
            {
                Tuple.Create("uGUI_ToggleOption",  typeof(AdjustToggleOption)),
                Tuple.Create("uGUI_SliderOption",  typeof(AdjustSliderOption)),
                Tuple.Create("uGUI_ChoiceOption",  typeof(AdjustChoiceOption)),
                Tuple.Create("uGUI_BindingOption", typeof(AdjustBindingOption))
            };

            // postfix for uGUI_TabbedControlsPanel.AddItem
            private static void AddItem_Postfix(int tabIndex, GameObject __result)
            {
                if (__result == null || tabIndex != modsTab)
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
    }
}
