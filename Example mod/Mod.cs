using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using UnityEngine;
using UnityEngine.UI;
using Logger = QModManager.Utility.Logger;

namespace SMLHelper.V2.Examples
{
    [QModCore]
    public static class ExampleMod
    {
        internal static Config Config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        [QModPatch]
        public static void Patch()
        {
            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }
    }

    public enum CustomChoice { One, Two, Three }

    [Menu("SMLHelper Example Mod")]
    public class Config : ConfigFile
    {
        [Choice("My index-based choice", "One", "Two", "Three"), OnChange(nameof(MyGenericValueChangedEvent))]
        public int ChoiceIndex;

        [Choice("My string-based choice", "Foo", "Bar"), OnChange(nameof(MyGenericValueChangedEvent))]
        public string ChoiceValue = "Foo";

        [Choice("My enum-based choice"), OnChange(nameof(MyGenericValueChangedEvent))]
        public CustomChoice ChoiceEnum;

        [Choice("My customised enum-based choice", "1", "2", "3"), OnChange(nameof(MyGenericValueChangedEvent))]
        public CustomChoice ChoiceCustomEnum;

        [Keybind("My keybind"), OnChange(nameof(MyGenericValueChangedEvent))]
        public KeyCode KeybindKey;

        [Slider("My slider", 0, 50, DefaultValue = 25, Format = "{0:F2}"), OnChange(nameof(MyGenericValueChangedEvent))]
        public int SliderValue;

        [Slider("My stepped slider", 0, 100, Step = 10), OnChange(nameof(MyGenericValueChangedEvent))]
        public int SteppedSliderValue;

        [Toggle("My checkbox"), OnChange(nameof(MyCheckboxToggleEvent)), OnChange(nameof(MyGenericValueChangedEvent))]
        public bool ToggleValue;

        [Button("My button"), OnGameObjectCreated(nameof(MyGameObjectCreatedEvent))]
        public void MyButtonClickEvent(ButtonClickedEventArgs e)
        {
            Logger.Log(Logger.Level.Info, "Button was clicked!");
            Logger.Log(Logger.Level.Info, $"{e.Id}");
        }

        private void MyCheckboxToggleEvent(ToggleChangedEventArgs e)
        {
            Logger.Log(Logger.Level.Info, "Checkbox value was changed!");
            Logger.Log(Logger.Level.Info, $"{e.Value}");
        }

        private void MyGenericValueChangedEvent(IModOptionEventArgs e)
        {
            Logger.Log(Logger.Level.Info, "Generic value changed!");
            Logger.Log(Logger.Level.Info, $"{e.Id}: {e.GetType()}");

            switch (e)
            {
                case KeybindChangedEventArgs keybindChangedEventArgs:
                    Logger.Log(Logger.Level.Info, keybindChangedEventArgs.KeyName);
                    break;
                case ChoiceChangedEventArgs choiceChangedEventArgs:
                    Logger.Log(Logger.Level.Info, $"{choiceChangedEventArgs.Index}: {choiceChangedEventArgs.Value}");
                    break;
                case SliderChangedEventArgs sliderChangedEventArgs:
                    Logger.Log(Logger.Level.Info, sliderChangedEventArgs.Value.ToString());
                    break;
                case ToggleChangedEventArgs toggleChangedEventArgs:
                    Logger.Log(Logger.Level.Info, toggleChangedEventArgs.Value.ToString());
                    break;
            }
        }

        private void MyGameObjectCreatedEvent(GameObjectCreatedEventArgs e)
        {
            Logger.Log(Logger.Level.Info, "GameObject was created");
            Logger.Log(Logger.Level.Info, $"{e.Id}: {e.GameObject}");
        }
    }
}
