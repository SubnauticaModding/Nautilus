using System.Collections.Generic;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace SMLHelper.V2.Examples
{
    [QModCore]
    public static class ExampleMod
    {
        [QModPatch]
        public static void Patch()
        {
            Config.Load();
            OptionsPanelHandler.RegisterModOptions(new Options());
        }
    }

    public static class Config
    {
        public static int ChoiceIndex;
        public static KeyCode KeybindKey;
        public static float SliderValue;
        public static bool ToggleValue;

        public static void Load()
        {
            ChoiceIndex = PlayerPrefs.GetInt("SMLHelperExampleModChoice", 0);
            KeybindKey = PlayerPrefsExtra.GetKeyCode("SMLHelperExampleModKeybind", KeyCode.X);
            SliderValue = PlayerPrefs.GetFloat("SMLHelperExampleModSlider", 50f);
            ToggleValue = PlayerPrefsExtra.GetBool("SMLHelperExampleModToggle", true);
        }
    }

    public class Options : ModOptions
    {
        public Options() : base("SMLHelper Example Mod")
        {
            ChoiceChanged += Options_ChoiceChanged;
            KeybindChanged += Options_KeybindChanged;
            SliderChanged += Options_SliderChanged;
            ToggleChanged += Options_ToggleChanged;

            GameObjectCreated += Options_GameObjectCreated;
        }

        public void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            if (e.Id != "exampleChoice") return;
            Config.ChoiceIndex = e.Index;
            PlayerPrefs.SetInt("SMLHelperExampleModChoice", e.Index);
        }
        public void Options_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            if (e.Id != "exampleKeybind") return;
            Config.KeybindKey = e.Key;
            PlayerPrefsExtra.SetKeyCode("SMLHelperExampleModKeybind", e.Key);
        }
        public void Options_SliderChanged(object sender, SliderChangedEventArgs e)
        {
            if (e.Id != "exampleSlider") return;
            Config.SliderValue = e.Value;
            PlayerPrefs.SetFloat("SMLHelperExampleModSlider", e.Value);
        }
        public void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            if (e.Id != "exampleToggle") return;
            Config.ToggleValue = e.Value;
            PlayerPrefsExtra.SetBool("SMLHelperExampleModToggle", e.Value);
        }

        public override void BuildModOptions()
        {
            AddChoiceOption("exampleChoice", "Choice", new string[] { "Choice 1", "Choice 2", "Choice 3" }, Config.ChoiceIndex);
            AddKeybindOption("exampleKeybind", "Keybind", GameInput.Device.Keyboard, Config.KeybindKey);
            AddSliderOption("exampleSlider", "Slider", 0, 100, Config.SliderValue, 50f); // do not specify format here if you going to use custom SliderValue
            AddToggleOption("exampleToggle", "Toggle", Config.ToggleValue);
        }

        // some optional and advanced stuff
        public void Options_GameObjectCreated(object sender, GameObjectCreatedEventArgs e)
        {
            GameObject go = e.GameObject;

            if (e.Id == "exampleChoice")
            {
                // adding tooltip to the choice's label
                GameObject label = go.GetComponentInChildren<Text>().gameObject;
                label.AddComponent<Tooltip>().tooltip = "This is tooltip for choice";
            }
            else
            if (e.Id == "exampleSlider")
            {
                // adding custom value handler
                // if you need just custom value format, you don't need custom component, just add format to AddSliderOption params
                GameObject slider = go.transform.Find("Slider").gameObject;
                slider.AddComponent<CustomSliderValue>().ValueFormat = "{0:F2}"; // ValueFormat is optional
            }
        }

        private class Tooltip: MonoBehaviour, ITooltip
        {
            public string tooltip;

            public void Start()
            {
                Destroy(gameObject.GetComponent<LayoutElement>()); // so tooltip will be only for the text and not for empty space after the text
            }

            public void GetTooltip(out string tooltipText, List<TooltipIcon> _)
            {
                tooltipText = tooltip;
            }
        }

        // simple example, just changing slider's value with step of 5.0
        private class CustomSliderValue: ModSliderOption.SliderValue
        {
            protected override void UpdateLabel()
            {
                slider.value = Mathf.Round(slider.value / 5.0f) * 5.0f;
                base.UpdateLabel();
            }
        }
    }
}
