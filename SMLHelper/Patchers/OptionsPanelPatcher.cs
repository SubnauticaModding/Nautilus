namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using Options;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    internal class OptionsPanelPatcher
    {
        internal static SortedList<string, ModOptions> modOptions = new SortedList<string, ModOptions>();

        internal static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(uGUI_OptionsPanel), "AddTabs"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(OptionsPanelPatcher), "AddTabs_Postfix")));

            // check if already patched (there will be some conflicts if we patch this twice)
            MethodInfo setVisibleTabMethod = AccessTools.Method(typeof(uGUI_TabbedControlsPanel), "SetVisibleTab");
            Patches patches = harmony.GetPatchInfo(setVisibleTabMethod);
            
            if (patches == null)
            {
                harmony.Patch(setVisibleTabMethod,
                    new HarmonyMethod(AccessTools.Method(typeof(OptionsPanelPatcher), "SetVisibleTab_Prefix")),
                    new HarmonyMethod(AccessTools.Method(typeof(OptionsPanelPatcher), "SetVisibleTab_Postfix")));
                
                V2.Logger.Log("Options.SetVisibleTab is patched", V2.LogLevel.Debug);
            }
            else
                V2.Logger.Log("Options.SetVisibleTab is already patched. Check if ModOptionsAdjusted mod is active.", V2.LogLevel.Warn);
        }

        internal static void AddTabs_Postfix(uGUI_OptionsPanel __instance)
        {
            uGUI_OptionsPanel optionsPanel = __instance;
            
            // Start the modsTab index at a value of -1
            var modsTab = -1;
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



        // do not show tab if it is already visible (to prevent scroll position resetting)
        internal static bool SetVisibleTab_Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex)
        {
            return !(tabIndex >= 0 && tabIndex < __instance.tabs.Count && __instance.tabs[tabIndex].pane.activeSelf);
        }

        // adjusting ui elements
        internal static void SetVisibleTab_Postfix(uGUI_TabbedControlsPanel __instance, int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex < __instance.tabs.Count)
            {
                try
                {
                    Transform options = __instance.tabs[tabIndex].container.transform;

                    for (int i = 0; i < options.childCount; ++i)
                    {
                        Transform option = options.GetChild(i);

                        if (option.localPosition.x == 0) // ui layout didn't touch this element yet
                            continue;

                        if (option.name.Contains("uGUI_ToggleOption"))
                            ProcessToggleOption(option);
                        else
                        if (option.name.Contains("uGUI_SliderOption"))
                            ProcessSliderOption(option);
                        else
                        if (option.name.Contains("uGUI_ChoiceOption"))
                            ProcessChoiceOption(option);
                        else
                        if (option.name.Contains("uGUI_BindingOption"))
                            ProcessBindingOption(option);
                    }
                }
                catch (System.Exception e)
                {
                    V2.Logger.Log($"Exception while adjusting mod options: {e.GetType()}\t{e.Message}", LogLevel.Error);
                }
            }
        }

        static void ProcessToggleOption(Transform option)
        {
            Transform check = option.Find("Toggle/Background");
            Text text = option.GetComponentInChildren<Text>();

            // :)
            if (text.text == "Enable AuxUpgradeConsole                        (Restart game)")
                text.text = "Enable AuxUpgradeConsole (Restart game)";

            int textWidth = GetTextWidth(text) + 20;
            Vector3 pos = check.localPosition;

            if (textWidth > pos.x)
            {
                pos.x = textWidth;
                check.localPosition = pos;
            }
        }

        static void ProcessSliderOption(Transform option)
        {
            const float sliderValueWidth = 85f;

            // changing width for slider value label
            RectTransform sliderValueRect = option.Find("Slider/Value").GetComponent<RectTransform>();
            Vector2 valueSize = sliderValueRect.sizeDelta;
            valueSize.x = sliderValueWidth;
            sliderValueRect.sizeDelta = valueSize;

            // changing width for slider
            Transform slider = option.Find("Slider/Background");
            Text text = option.GetComponentInChildren<Text>();

            RectTransform rect = slider.GetComponent<RectTransform>();

            float widthAll = option.GetComponent<RectTransform>().rect.width;
            float widthSlider = rect.rect.width;
            float widthText = GetTextWidth(text) + 25;

            if (widthText + widthSlider + sliderValueWidth > widthAll)
            {
                Vector2 size = rect.sizeDelta;
                size.x = widthAll - widthText - sliderValueWidth - widthSlider;
                rect.sizeDelta = size;
            }
        }

        static void ProcessChoiceOption(Transform option)
        {
            Transform choice = option.Find("Choice/Background");
            Text text = option.GetComponentInChildren<Text>();

            RectTransform rect = choice.GetComponent<RectTransform>();

            float widthAll = option.GetComponent<RectTransform>().rect.width;
            float widthChoice = rect.rect.width;

            float widthText = GetTextWidth(text) + 10;

            if (widthText + widthChoice > widthAll)
            {
                Vector2 size = rect.sizeDelta;
                size.x = widthAll - widthText - widthChoice;
                rect.sizeDelta = size;
            }
        }

        static void ProcessBindingOption(Transform option)
        {
            // changing width for keybinding option
            Transform binding = option.Find("Bindings");
            Text text = option.GetComponentInChildren<Text>();

            RectTransform rect = binding.GetComponent<RectTransform>();

            float widthAll = option.GetComponent<RectTransform>().rect.width;
            float widthBinding = rect.rect.width;

            float widthText = GetTextWidth(text) + 10;

            if (widthText + widthBinding > widthAll)
            {
                Vector2 size = rect.sizeDelta;
                size.x = widthAll - widthText - widthBinding;
                rect.sizeDelta = size;
            }

            // fixing bug when all keybinds show 'D' (after reselecting tab)
            Transform primaryBinding = binding.Find("Primary Binding"); // bug only on primary bindings
            Text bindingText = primaryBinding.Find("Label").GetComponent<Text>();

            if (bindingText.text == "D")
            {
                string buttonRawText = primaryBinding.GetComponent<uGUI_Binding>().value;

                if (uGUI.buttonCharacters.TryGetValue(buttonRawText, out string buttonText))
                    bindingText.text = buttonText;
                else
                    bindingText.text = buttonRawText;
            }
        }

        static int GetTextWidth(Text text)
        {
            int width = 0;

            Font font = text.font;
            font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);

            foreach (char c in text.text)
            {
                font.GetCharacterInfo(c, out CharacterInfo charInfo, text.fontSize, text.fontStyle);
                width += charInfo.advance;
            }

            return width;
        }
    }
}
