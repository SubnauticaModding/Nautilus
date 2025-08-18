using System;
using System.Collections;
using BepInEx.Logging;
using Nautilus.Extensions;
using Nautilus.Utility;
using TMPro;
using UnityEngine;

namespace Nautilus.Options;

/// <summary>
/// Contains all the information about a keybind changed event.
/// </summary>
public class KeybindChangedEventArgs : ConfigOptionEventArgs<KeyCode>
{
    /// <summary>
    /// Constructs a new <see cref="KeybindChangedEventArgs"/>.
    /// </summary>
    /// <param name="id">The ID of the <see cref="ModKeybindOption"/> that was changed.</param>
    /// <param name="key">The new value for the <see cref="ModKeybindOption"/>.</param>
    public KeybindChangedEventArgs(string id, KeyCode key) : base(id, key) { }
}

/// <summary>
/// A mod option class for handling an option that is a keybind.
/// </summary>
public class ModKeybindOption : ModOption<KeyCode, KeybindChangedEventArgs>
{
    
    /// <summary>
    /// The currently select input source device for the <see cref="ModKeybindOption"/>.
    /// </summary>
    public GameInput.Device Device { get; set; }

    /// <summary>
    /// The tooltip to show when hovering over the option.
    /// </summary>
    public string Tooltip { get; }

    private ModKeybindOption(string id, string label, GameInput.Device device, KeyCode key, string tooltip) : base(label, id, key)
    {
        Device = device;
        Tooltip = tooltip;
#if SUBNAUTICA
        GameInput.OnPrimaryDeviceChanged += () => { Device = GameInput.PrimaryDevice; };
#else
        GameInput.OnPrimaryDeviceChanged += () => { Device = GameInput.GetPrimaryDevice(); };
#endif
        
    }

    /// <summary>
    /// Creates a new <see cref="ModKeybindOption"/> for handling an option that is a keybind.
    /// </summary>
    /// <param name="id">The internal ID for the toggle option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="device">The device name.</param>
    /// <param name="key">The starting keybind value.</param>
    /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
    public static ModKeybindOption Create(string id, string label, GameInput.Device device, KeyCode key, string tooltip = null)
    {
        return new ModKeybindOption(id, label, device, key, tooltip);
    }

    /// <summary>
    /// The base method for adding an object to the options panel
    /// </summary>
    /// <param name="panel">The panel to add the option to.</param>
    /// <param name="tabIndex">Where in the panel to add the option.</param>
    public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
    {
        // Add item
#if SUBNAUTICA
        OptionGameObject = panel.AddItem(tabIndex, panel.controls.prefabBinding);
#else
        OptionGameObject = panel.AddItem(tabIndex, panel.bindingOptionPrefab);
#endif
        

        // Update text
        TextMeshProUGUI text = OptionGameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            OptionGameObject.GetComponentInChildren<TranslationLiveUpdate>().translationKey = Label;
            text.text = Language.main.Get(Label);
        }

        // Add tooltip
        MenuTooltip tooltip = OptionGameObject.EnsureComponent<MenuTooltip>();
        tooltip.key = Tooltip;

        // Create bindings
        uGUI_Bindings bindings = OptionGameObject.GetComponentInChildren<uGUI_Bindings>();
        uGUI_Binding binding = bindings.bindings[0];

        // Destroy secondary bindings
        int last = bindings.bindings.Length - 1;
        UnityEngine.Object.Destroy(bindings.bindings[last].gameObject);
        UnityEngine.Object.Destroy(bindings);

        // Update bindings
        binding.device = Device;
#if SUBNAUTICA
        binding.binding = GameInput.GetDisplayText(Value.KeyCodeToString());
#else
        binding.value = GameInput.GetKeyCodeAsInputName(Value);
#endif
        binding.gameObject.EnsureComponent<ModBindingTag>();
        binding.bindingSet = GameInput.BindingSet.Primary;
#if BELOWZERO
        binding.bindCallback = new Action<GameInput.Device, GameInput.Button, GameInput.BindingSet, string>((_, _1, _2, s) =>
        {
            var keyCode = StringToKeyCode(s);
            binding.value = uGUI.GetDisplayTextForBinding(GameInput.GetInputName(binding.value));
            OnChange(Id, keyCode);
            parentOptions.OnChange<KeyCode, KeybindChangedEventArgs>(Id, StringToKeyCode(s));
            binding.RefreshValue();
        });
#endif

        base.AddToPanel(panel, tabIndex);
    }
    
     private static KeyCode StringToKeyCode(string s)
    {
        switch (s)
        {
            case "0":
                return KeyCode.Alpha0;
            case "1":
                return KeyCode.Alpha1;
            case "2":
                return KeyCode.Alpha2;
            case "3":
                return KeyCode.Alpha3;
            case "4":
                return KeyCode.Alpha4;
            case "5":
                return KeyCode.Alpha5;
            case "6":
                return KeyCode.Alpha6;
            case "7":
                return KeyCode.Alpha7;
            case "8":
                return KeyCode.Alpha8;
            case "9":
                return KeyCode.Alpha9;
            case "MouseButtonLeft":
                return KeyCode.Mouse0;
            case "MouseButtonRight":
                return KeyCode.Mouse1;
            case "MouseButtonMiddle":
                return KeyCode.Mouse2;
            case "ControllerButtonA":
                return KeyCode.JoystickButton0;
            case "ControllerButtonB":
                return KeyCode.JoystickButton1;
            case "ControllerButtonX":
                return KeyCode.JoystickButton2;
            case "ControllerButtonY":
                return KeyCode.JoystickButton3;
            case "ControllerButtonLeftBumper":
                return KeyCode.JoystickButton4;
            case "ControllerButtonRightBumper":
                return KeyCode.JoystickButton5;
            case "ControllerButtonBack":
                return KeyCode.JoystickButton6;
            case "ControllerButtonHome":
                return KeyCode.JoystickButton7;
            case "ControllerButtonLeftStick":
                return KeyCode.JoystickButton8;
            case "ControllerButtonRightStick":
                return KeyCode.JoystickButton9;
            default:
                try
                {
                    return (KeyCode)Enum.Parse(typeof(KeyCode), s);
                }
                catch (Exception)
                {
                    InternalLogger.Log($"Failed to parse {s} as a valid KeyCode!", LogLevel.Error);
                    return 0;
                }
        }
    }

    internal class ModBindingTag : MonoBehaviour
    {
        public ModOptions parentOptions;
    };

    internal class BindingOptionAdjust: ModOptionAdjust
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
            {
                rect.sizeDelta = SetVec2x(rect.sizeDelta, widthAll - widthText - widthBinding);
            }

            Destroy(this);
        }
    }
    /// <summary>
    /// The Adjuster for this <see cref="OptionItem"/>.
    /// </summary>
    public override Type AdjusterComponent => typeof(BindingOptionAdjust);
}