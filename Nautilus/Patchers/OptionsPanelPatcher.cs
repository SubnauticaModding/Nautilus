using System.Text;

namespace Nautilus.Patchers;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Options;
using Utility;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if SUBNAUTICA
using UnityEngine.InputSystem;
using MonoBehaviours;
using BepInEx;
using Handlers;
#endif

internal class OptionsPanelPatcher
{
    internal static SortedList<string, ModOptions> modOptions = new();

    private static int _modsTabIndex = -1;

    private static Color _headerColor = new(1f, 0.777f, 0f);

    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(OptionsPanelPatcher));
        harmony.PatchAll(typeof(ScrollPosKeeper));
        harmony.PatchAll(typeof(ModOptionsHeadingsToggle));
    }

    // 'Mods' tab also added in QModManager, so we can't rely on 'modsTab' in AddTabs_Postfix
    [HarmonyPostfix]
    [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddTab))]
    internal static void AddTab_Postfix(uGUI_TabbedControlsPanel __instance, string label, int __result)
    {
        if(__instance is not uGUI_OptionsPanel)
            return;

        if (label == "Mods")
        {
            _modsTabIndex = __result;
        }
    }

#if BELOWZERO
    [HarmonyPrefix]
    [HarmonyPatch(typeof(uGUI_Binding), nameof(uGUI_Binding.RefreshValue))]
    internal static bool RefreshValue_Prefix(uGUI_Binding __instance)
    {
        if (__instance.gameObject.GetComponent<ModKeybindOption.ModBindingTag>() is null)
        {
            return true;
        }
        
        __instance.currentText.text = (__instance.active || __instance.value == null) ? "" : __instance.value;
        __instance.UpdateState();
        return false;
    }
#endif

    [HarmonyPostfix]
    [HarmonyPatch(typeof(uGUI_OptionsPanel), nameof(uGUI_OptionsPanel.AddTabs))]
    internal static void AddTabs_Postfix(uGUI_OptionsPanel __instance)
    {
        uGUI_OptionsPanel optionsPanel = __instance;

        // Start the modsTab index at a value of -1
        int modsTab = -1;
        // Loop through all of the tabs
        for (int i = 0; i < optionsPanel.tabsContainer.childCount; i++)
        {
            // Check if they are named "Mods"
            TextMeshProUGUI text = optionsPanel.tabsContainer.GetChild(i).GetComponentInChildren<TextMeshProUGUI>(true);

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

        var optionsToAdd = new List<ModOptions>(modOptions.Values);

        var nautilusOptions = optionsToAdd.FirstOrDefault(options => options.Name == "Nautilus");
        optionsToAdd.Remove(nautilusOptions);

        nautilusOptions.AddOptionsToPanel(optionsPanel, modsTab);

        // adding all other options here
        optionsToAdd.ForEach(options => options.AddOptionsToPanel(optionsPanel, modsTab));

#if SUBNAUTICA
        var inputTab = optionsPanel.AddTab("Mod Input");
        PopulateBindings(optionsPanel, inputTab, GameInput.Device.Keyboard);

        // Add dividing line
        optionsPanel.AddHeading(inputTab, new string('\u2500', 34));
        
        PopulateBindings(optionsPanel, inputTab, GameInput.Device.Controller);
#endif
    }

#if SUBNAUTICA
    private static void PopulateBindings(uGUI_OptionsPanel panel, int tab, GameInput.Device device)
    {
        var bindingsHeader = panel.AddBindingsHeader(tab);
        bindingsHeader.transform.Find("Caption").GetComponent<TextMeshProUGUI>().text =
            $"<color=#FFAC09><b>{device switch
            {
                GameInput.Device.Keyboard => "Keyboard",
                GameInput.Device.Controller => "Controller",
                _ => "Unknown device"
            }}</b></color>";

        /*
         * Category logic order:
         * 1. all buttons with the same category name go under one category
         * 2. If a button doesn't have a category, it's automatically assigned to a category with the same name as the plugin name
         * 3. If the plugin name couldn't be fetched, use assembly name.
         */
        
        
        // find buttons with no categories and assign plugin name or assembly name as their category
        var buttonsWithCategories = GameInputPatcher.Categories.Values.SelectMany(b => b).ToHashSet();
        var buttonsWithoutCategories = GameInputPatcher.BindableButtons
            .Where(hotkey => !buttonsWithCategories.Contains(hotkey.button) && hotkey.device == device)
            .GroupBy(b =>
            {
                if (EnumHandler.TryGetOwnerAssembly(b.button, out var assembly))
                    return assembly;
                
                InternalLogger.Error($"Couldn't find the assembly associated with bindable button '{b}'.");
                return null;
            }).Where(g => g.Key is not null)
            .ToDictionary(
                g => FindPluginNameForAssembly(g.Key),
                g => g.Select(h => h.button).ToHashSet());
        
        
        // merge buttons that have categories with buttons that don't have categories
        var categorizedButtons = GameInputPatcher.Categories.Concat(buttonsWithoutCategories).ToList();
        
        if (categorizedButtons.Count == 0)
        {
            panel.AddHeading(tab, "[No custom input]");
            return;
        }
        
        panel.AddButton(tab, "ResetToDefault", () => RemoveAllBindingOverrides(device));
        var sb = new StringBuilder();
        foreach (var kvp in categorizedButtons)
        {
            var category = kvp.Key;
            var buttons = kvp.Value;
            panel.AddHeading(tab, category);
            foreach (var button in buttons)
            {
                if (!GameInputPatcher.BindableButtons.Contains((button, device)))
                {
                    InternalLogger.Error($"Button '{button}' has a category but wasn't set to be bindable. Please set the button to be bindable first.");
                    continue;
                }
                
                var bindings = panel.AddBindingOption(tab, $"Option{button.AsString()}", device, button);
                (GameInput.input as GameInputSystem)!.bindingOptions.Add(bindings);
                if (!EnumHandler.TryGetOwnerAssembly(button, out var assembly))
                {
                    InternalLogger.Error($"Couldn't find the assembly associated with bindable button '{button}', category '{category}' was skipped.'");
                    break;
                }
                
                
                if (Language.main.TryGet($"OptionDesc_{button.AsString()}", out var tooltip))
                {
                    sb.AppendLine(tooltip);
                    sb.AppendLine();
                }

                var pluginName = FindPluginNameForAssembly(assembly);
                sb.AppendLine($"Added by <b><color=#FFAC09>{pluginName}</color></b>");
                bindings.transform.parent.Find("Caption").gameObject.AddComponent<ModBindingTooltip>().tooltip = sb.ToString();
                sb.Clear();
            }
        }
    }

    private static string FindPluginNameForAssembly(Assembly assembly)
    {
        var plugin = assembly
            .GetTypes()
            .FirstOrDefault(t => t.GetCustomAttribute<BepInPlugin>() is not null)?.GetCustomAttribute<BepInPlugin>();
        
        return plugin?.Name ?? assembly.GetName().Name;
    }

    private static void RemoveAllBindingOverrides(GameInput.Device device)
    {
        using (GameInputSystem.DeferBindingResolution())
        {
            GameInput.SetBindingsChanged();
            var deviceName = device.AsString();
            foreach (var button in GameInputPatcher.CustomButtons)
            {
                var action = button.Value;
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var groups = action.bindings[i].groups;
                    if (!string.IsNullOrEmpty(groups) && groups.Contains(deviceName))
                    {
                        action.ApplyBindingOverride(i, default(InputBinding));
                    }
                }
            }
        }
    }
#endif

    // Class for collapsing/expanding options in 'Mods' tab
    // Options can be collapsed/expanded by clicking on mod's title or arrow button
    private static class ModOptionsHeadingsToggle
    {
        private enum HeadingState { Collapsed, Expanded };

        private static GameObject _headingPrefab = null;

        private static class StoredHeadingStates
        {
            private static readonly string _configPath = Path.Combine(Path.Combine(BepInEx.Paths.ConfigPath, Assembly.GetExecutingAssembly().GetName().Name), "headings_states.json");

            private class StatesConfig
            {
                [JsonProperty]
                private readonly Dictionary<string, HeadingState> _states = new();

                public HeadingState this[string name]
                {
                    get => _states.TryGetValue(name, out HeadingState state) ? state : HeadingState.Expanded;

                    set
                    {
                        _states[name] = value;
                        File.WriteAllText(_configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
                    }
                }
            }
            private static readonly StatesConfig _statesConfig = CreateConfig();

            private static StatesConfig CreateConfig()
            {
                if (File.Exists(_configPath))
                {
                    return JsonConvert.DeserializeObject<StatesConfig>(File.ReadAllText(_configPath));
                }
                else
                {
                    return new StatesConfig();
                }
            }

            public static HeadingState get(string name)
            {
                return _statesConfig[name];
            }

            public static void store(string name, HeadingState state)
            {
                _statesConfig[name] = state;
            }
        }

        // we add arrow button from Choice ui element to the options headings for collapsing/expanding 
        private static void InitHeadingPrefab(uGUI_TabbedControlsPanel panel)
        {
            if (_headingPrefab)
            {
                return;
            }

#if SUBNAUTICA
            _headingPrefab = Object.Instantiate(panel.controls.prefabHeading);
#else
            _headingPrefab = Object.Instantiate(panel.headingPrefab);
#endif
            _headingPrefab.name = "OptionHeadingToggleable";
            _headingPrefab.AddComponent<HeadingToggle>();

            var option = _headingPrefab.AddComponent<uGUI_OptionSelection>();
#if SUBNAUTICA
            option.selectionBackground = Object.Instantiate(panel.controls.prefabToggle.GetComponentInChildren<uGUI_OptionSelection>(true).selectionBackground);
#else
            option.selectionBackground = Object.Instantiate(panel.toggleOptionPrefab.GetComponentInChildren<uGUI_OptionSelection>(true).selectionBackground);
#endif
            option.selectionBackground.transform.SetParent(_headingPrefab.transform);
            option.hoverSound = AudioUtils.GetFmodAsset("event:/tools/pda/select");

            Transform captionTransform = _headingPrefab.transform.Find("Caption");
            captionTransform.localPosition = new Vector3(45f, 0f, 0f);
            captionTransform.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

#if SUBNAUTICA
            GameObject button = Object.Instantiate(panel.controls.prefabChoice.transform.Find("Choice/Background/NextButton").gameObject);
#else
            GameObject button = Object.Instantiate(panel.choiceOptionPrefab.transform.Find("Choice/Background/NextButton").gameObject);
#endif
            button.name = "HeadingToggleButton";
            button.AddComponent<ToggleButtonClickHandler>();
            Object.Destroy(button.GetComponent<Button>());

            var textComponent = captionTransform.GetComponent<TextMeshProUGUI>();
            textComponent.fontStyle = FontStyles.Bold;

            RectTransform buttonTransform = button.transform as RectTransform;
            buttonTransform.SetParent(_headingPrefab.transform);
            buttonTransform.SetAsFirstSibling();
            buttonTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
            buttonTransform.localPosition = new Vector3(15f, -13f, 0f);
            buttonTransform.pivot = new Vector2(0.25f, 0.5f);
            buttonTransform.anchorMin = buttonTransform.anchorMax = new Vector2(0f, 0.5f);
        }

        #region components
        // main component for headings toggling
        private class HeadingToggle: Selectable, IPointerClickHandler
        {
            private HeadingState _headingState = HeadingState.Expanded;
            private string _headingName = null;
            private List<GameObject> _childOptions = null;
            
            protected override void OnEnable()
            {
                base.OnEnable();
                transform.Find("Caption").GetComponent<TextMeshProUGUI>().color = _headerColor;
            }

            private void Init()
            {
                if (_childOptions != null)
                {
                    return;
                }

                _headingName = transform.Find("Caption")?.GetComponent<TextMeshProUGUI>()?.text ?? "";

                _childOptions = new List<GameObject>();

                for (int i = transform.GetSiblingIndex() + 1; i < transform.parent.childCount; i++)
                {
                    GameObject option = transform.parent.GetChild(i).gameObject;

                    if (option.GetComponent<HeadingToggle>())
                    {
                        break;
                    }

                    _childOptions.Add(option);
                }
            }

            public void EnsureState() // for setting previously saved state
            {
                Init();

                HeadingState storedState = StoredHeadingStates.get(_headingName);

                if (_headingState != storedState)
                {
                    SetState(storedState);
                    GetComponentInChildren<ToggleButtonClickHandler>()?.SetStateInstant(storedState);
                }
            }

            public void SetState(HeadingState state)
            {
                Init();

                _childOptions.ForEach(option => option.SetActive(state == HeadingState.Expanded));
                _headingState = state;

                StoredHeadingStates.store(_headingName, state);
            }
            
            public void OnPointerClick(PointerEventData _)
            {
                var buttonHandler = GetComponentInChildren<ToggleButtonClickHandler>();
                if (buttonHandler.isRotating)
                    return;

                _headingState = _headingState == HeadingState.Expanded ? HeadingState.Collapsed : HeadingState.Expanded;
                StartCoroutine(buttonHandler.SetState(_headingState));

                SetState(_headingState);
            }
        }

        // change handler for arrow button
        private class ToggleButtonClickHandler: MonoBehaviour
        {
            private const float TimeRotate = 0.1f;

            public bool isRotating = false;

            public void SetStateInstant(HeadingState state)
            {
                transform.localEulerAngles = new Vector3(0, 0, state == HeadingState.Expanded? -90: 0);
            }

            internal IEnumerator SetState(HeadingState state)
            {
                isRotating = true;

                float angle = state == HeadingState.Expanded ? -90 : 90;

                Quaternion startRotation = transform.localRotation;
                Quaternion endRotation = Quaternion.Euler(new Vector3(0f, 0f, angle)) * startRotation;

                float timeStart = Time.realtimeSinceStartup; // Time.deltaTime works only in main menu

                while (timeStart + TimeRotate > Time.realtimeSinceStartup)
                {
                    transform.localRotation = Quaternion.Lerp(startRotation, endRotation, (Time.realtimeSinceStartup - timeStart) / TimeRotate);
                    yield return null;
                }

                transform.localRotation = endRotation;
                isRotating = false;
            }
        }
#endregion

        #region patches for uGUI_TabbedControlsPanel
        [HarmonyPrefix]
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.AddHeading))]
        private static bool AddHeading_Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex, string label)
        {
            if (tabIndex != _modsTabIndex || __instance is not uGUI_OptionsPanel)
                return true;

#if SUBNAUTICA
            var item = __instance.AddItem(tabIndex, _headingPrefab);
            uGUI_Controls.AssignLabel(item, label);
#else
            var item = __instance.AddItem(tabIndex, _headingPrefab, label);
#endif
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.OnEnable))]
        private static void Awake_Postfix(uGUI_TabbedControlsPanel __instance)
        {
            if(__instance is not uGUI_OptionsPanel)
                return;

            InitHeadingPrefab(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab))]
        private static void SetVisibleTab_Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex)
        {
            if (tabIndex != _modsTabIndex || __instance is not uGUI_OptionsPanel)
                return;

            // just in case, for changing vertical spacing between ui elements
            //__instance.tabs[tabIndex].container.GetComponent<VerticalLayoutGroup>().spacing = 15f; // default is 15f

            Transform options = __instance.tabs[tabIndex].container.transform;

            for (int i = 0; i < options.childCount; i++)
            {
                options.GetChild(i).GetComponent<HeadingToggle>()?.EnsureState();
            }
        }
        #endregion
    }


    // Patch class for saving scroll positions for tabs in options menu
    // Restores positions after switching between tabs and after reopening menu
    private static class ScrollPosKeeper
    {
        // key - tab index, value - scroll position
        private static readonly Dictionary<int, float> _devMenuScrollPos = new();
        private static readonly Dictionary<int, float> _optionsScrollPos = new();

        private static void StorePos(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            Dictionary<int, float> scrollPos = panel is uGUI_DeveloperPanel ? _devMenuScrollPos : _optionsScrollPos;
            if (tabIndex >= 0 && tabIndex < panel.tabs.Count)
            {
                scrollPos[tabIndex] = panel.tabs[tabIndex].pane.GetComponent<ScrollRect>().verticalNormalizedPosition;
            }
        }

        private static void RestorePos(uGUI_TabbedControlsPanel panel, int tabIndex)
        {
            Dictionary<int, float> scrollPos = panel is uGUI_DeveloperPanel ? _devMenuScrollPos : _optionsScrollPos;
            if (tabIndex >= 0 && tabIndex < panel.tabs.Count && scrollPos.TryGetValue(tabIndex, out float pos))
            {
                panel.tabs[tabIndex].pane.GetComponent<ScrollRect>().verticalNormalizedPosition = pos;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.RemoveTabs))]
        private static void RemoveTabs_Prefix(uGUI_TabbedControlsPanel __instance)
        {
            if(__instance is not uGUI_OptionsPanel)
                return;
            StorePos(__instance, __instance.currentTab);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.HighlightCurrentTab))]
        private static void HighlightCurrentTab_Postfix(uGUI_TabbedControlsPanel __instance)
        {
            if(__instance is not uGUI_OptionsPanel)
                return;
            __instance.StartCoroutine(_restorePos());

            IEnumerator _restorePos()
            {
                yield return null;
                RestorePos(__instance, __instance.currentTab);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab))]
        private static void SetVisibleTab_Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex)
        {
            if(__instance is not uGUI_OptionsPanel)
                return;
            if (tabIndex != __instance.currentTab)
            {
                StorePos(__instance, __instance.currentTab);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(uGUI_TabbedControlsPanel), nameof(uGUI_TabbedControlsPanel.SetVisibleTab))]
        private static void SetVisibleTab_Postfix(uGUI_TabbedControlsPanel __instance, int tabIndex)
        {
            if(__instance is not uGUI_OptionsPanel)
                return;
            RestorePos(__instance, tabIndex);
        }
    }
}