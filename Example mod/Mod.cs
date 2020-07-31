using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using UnityEngine;
using UnityEngine.UI;
using Logger = QModManager.Utility.Logger;
using Tooltip = SMLHelper.V2.Options.TooltipAttribute;

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
        [Label("My index-based choice"), Tooltip("A simple tooltip"), Choice("Choice 1", "Choice 2", "Choice 3"), OnChange(nameof(MyGenericValueChangedEvent))]
        public int ChoiceIndex;

        [Label("My string-based choice"), Choice("Foo", "Bar"), OnChange(nameof(MyGenericValueChangedEvent))]
        public string ChoiceString = "Foo";

        [Label("My enum-based choice"), OnChange(nameof(MyGenericValueChangedEvent))]
        public CustomChoice ChoiceEnum;

        [Label("My customised enum-based choice"), Choice("1", "2", "3"), OnChange(nameof(MyGenericValueChangedEvent))]
        public CustomChoice ChoiceEnumCustomValues;

        [Label("My keybind"), OnChange(nameof(MyGenericValueChangedEvent))]
        public KeyCode KeybindKey;

        [Label("My slider"), Slider(0, 50, DefaultValue = 25, Format = "{0:F2}"), OnChange(nameof(MyGenericValueChangedEvent))]
        public int SliderValue;

        [Label("My checkbox"), OnChange(nameof(MyCheckboxToggleEvent)), OnChange(nameof(MyGenericValueChangedEvent))]
        public bool ToggleValue;

        [Label("My button"), OnGameObjectCreated(nameof(MyGameObjectCreatedEvent))]
        public void MyButtonClickEvent(ButtonClickedEventArgs e)
        {
            Logger.Log(Logger.Level.Info, "Button was clicked!");
            Logger.Log(Logger.Level.Info, $"{e.Id}");
        }

        public void MyCheckboxToggleEvent(ToggleChangedEventArgs e)
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
