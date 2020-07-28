using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using UnityEngine;
using UnityEngine.UI;

namespace SMLHelper.V2.Examples
{
    public static class ExampleMod
    {
        internal static Config Config { get; private set; } = new Config();

        public static void Patch()
        {
            Config.Load();
            OptionsPanelHandler.RegisterModOptions(new Options());
        }
    }

    internal class Config : ConfigFile
    {
        public int ChoiceIndex;
        public KeyCode KeybindKey;
        public float SliderValue;
        public bool ToggleValue;
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
            ExampleMod.Config.ChoiceIndex = e.Index;
            ExampleMod.Config.Save();
        }
        public void Options_KeybindChanged(object sender, KeybindChangedEventArgs e)
        {
            if (e.Id != "exampleKeybind") return;
            ExampleMod.Config.KeybindKey = e.Key;
            ExampleMod.Config.Save();
        }
        public void Options_SliderChanged(object sender, SliderChangedEventArgs e)
        {
            if (e.Id != "exampleSlider") return;
            ExampleMod.Config.SliderValue = e.Value;
            ExampleMod.Config.Save();
        }
        public void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        {
            if (e.Id != "exampleToggle") return;
            ExampleMod.Config.ToggleValue = e.Value;
            ExampleMod.Config.Save();
        }

        public override void BuildModOptions()
        {
            AddChoiceOption("exampleChoice", "Choice", new string[] { "Choice 1", "Choice 2", "Choice 3" }, ExampleMod.Config.ChoiceIndex);
            AddKeybindOption("exampleKeybind", "Keybind", GameInput.Device.Keyboard, ExampleMod.Config.KeybindKey);
            AddSliderOption("exampleSlider", "Slider", 0, 100, ExampleMod.Config.SliderValue, 50f); // do not specify format here if you going to use custom SliderValue
            AddToggleOption("exampleToggle", "Toggle", ExampleMod.Config.ToggleValue);
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

        private class Tooltip : MonoBehaviour, ITooltip
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
        private class CustomSliderValue : ModSliderOption.SliderValue
        {
            protected override void UpdateLabel()
            {
                slider.value = Mathf.Round(slider.value / 5.0f) * 5.0f;
                base.UpdateLabel();
            }
        }
    }
}
