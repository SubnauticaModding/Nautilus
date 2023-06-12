using System;
using System.Collections;
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
        GameInput.OnPrimaryDeviceChanged += () => { Device = GameInput.GetPrimaryDevice(); };
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
    /// Creates a new <see cref="ModKeybindOption"/> for handling an option that is a keybind.
    /// </summary>
    /// <param name="id">The internal ID for the toggle option.</param>
    /// <param name="label">The display text to use in the in-game menu.</param>
    /// <param name="device">The device name.</param>
    /// <param name="key">The starting keybind value.</param>
    /// /// <param name="tooltip">The tooltip to show when hovering over the option.</param>
    public static ModKeybindOption Create(string id, string label, GameInput.Device device, string key, string tooltip = null)
    {
        return Create(id, label, device, KeyCodeUtils.StringToKeyCode(key), tooltip);
    }

    /// <summary>
    /// The base method for adding an object to the options panel
    /// </summary>
    /// <param name="panel">The panel to add the option to.</param>
    /// <param name="tabIndex">Where in the panel to add the option.</param>
    public override void AddToPanel(uGUI_TabbedControlsPanel panel, int tabIndex)
    {
        // Add item
        OptionGameObject = panel.AddItem(tabIndex, panel.bindingOptionPrefab);

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
        binding.value = KeyCodeUtils.GetDisplayTextForKeyCode(Value);
        binding.gameObject.EnsureComponent<ModBindingTag>();
        binding.bindingSet = GameInput.BindingSet.Primary;
        binding.bindCallback = new Action<GameInput.Device, GameInput.Button, GameInput.BindingSet, string>((_, _1, _2, s) =>
        {
            var keyCode = KeyCodeUtils.StringToKeyCode(s);
            binding.value = KeyCodeUtils.GetDisplayTextForKeyCode(keyCode);
            OnChange(Id, keyCode);
            parentOptions.OnChange<KeyCode, KeybindChangedEventArgs>(Id, KeyCodeUtils.StringToKeyCode(s));
            binding.RefreshValue();
        });

        base.AddToPanel(panel, tabIndex);
    }

    internal class ModBindingTag: MonoBehaviour { };

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