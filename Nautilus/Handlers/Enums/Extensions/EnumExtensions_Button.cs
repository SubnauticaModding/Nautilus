#if SUBNAUTICA
using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Nautilus.Extensions;
using Nautilus.Handlers.Internals;
using Nautilus.Patchers;
using Nautilus.Utility;
using Newtonsoft.Json.Utilities;
using UnityEngine.InputSystem;

namespace Nautilus.Handlers;

using Button = GameInput.Button;

public static partial class EnumExtensions
{
    [OnEnumRegister<Button>]
    private static void OnButtonRegistered(EnumBuilder<Button> builder)
    {
        Button button = builder;
        var name = button.ToString();
        GameInputSystem.nameToAction[name] = button;
        if (!GameInput.ActionNames.valueToString.ContainsKey(button))
        {
            GameInput.ActionNames.valueToString.Add(button, name);
        }
    }

    /// <summary>
    /// Initializes the action input for this button.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="tooltip">The display name of the button, can be anything. If null or empty, this will use the language line "Option{enumName}" instead.</param>
    /// <param name="language">The language for the display name. Defaults to English.</param>
    /// <param name="actionType">Determines the behavior with which an <see cref="InputAction"/> triggers.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputActionType.html"/>
    public static EnumBuilder<Button> CreateInput(this EnumBuilder<Button> builder, string tooltip = "", string language = "English", InputActionType actionType = InputActionType.Button)
    {
        Button button = builder;
        var buttonName = button.AsString();
        var fullName = $"Option{buttonName}";
        GameInputPatcher.CustomButtons[button] = new InputAction(buttonName, actionType);
        if (!string.IsNullOrEmpty(tooltip))
        {
            LanguageHandler.SetLanguageLine(fullName, tooltip, language);
            return builder;
        }
        
        var friendlyName = Language.main.Get(fullName);
        if (string.IsNullOrEmpty(friendlyName))
        {
            InternalLogger.Warn($"Display name for Button '{buttonName}' is not specified and no language key has been found. Setting display name to 'Option{buttonName}'.");
        }
        
        return builder;
    }

    /// <summary>
    /// Sets the default bindings for this button.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="device">The device this binding is for.</param>
    /// <param name="bindingSet">Whether this is the primary binding or the secondary binding.</param>
    /// <param name="bindingPath">The binding path to bind this button to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>By default, the binding will be bindable in the Mod Input tab. If you wish to make the button unbindable, consider using <see cref="SetUnbindable"/>.</remarks>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputControlPath.html"/>
    public static EnumBuilder<Button> WithBinding(this EnumBuilder<Button> builder, GameInput.Device device,
        GameInput.BindingSet bindingSet, string bindingPath)
    {
        Button button = builder;
        if (!IsBindingPathValid(builder, bindingPath, device))
        {
            return null;
        }
        GameInputPatcher.Bindings.GetOrAddNew(button).Add(new(device, bindingSet, bindingPath));
        SetBindingDefinition(button, device, bindingSet, bindingPath);
        SetBindable(builder, device);
        
        return builder;
    }
    
    /// <summary>
    /// Sets the default bindings for this button.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="device">The device this binding is for.</param>
    /// <param name="primaryBindingPath">The binding path to bind the primary hotkey of this button.</param>
    /// <param name="secondaryBindingPath">The binding path to bind the secondary hotkey of this button. If null or empty, the button will have an empty secondary binding.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>By default, the binding will be bindable in the Mod Input tab. If you wish to make the button unbindable, consider using <see cref="SetUnbindable"/>.</remarks>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputControlPath.html"/>
    public static EnumBuilder<Button> WithBinding(this EnumBuilder<Button> builder, GameInput.Device device,
        string primaryBindingPath, string secondaryBindingPath = null)
    {
        Button button = builder;
        
        if (string.IsNullOrWhiteSpace(primaryBindingPath))
        {
            InternalLogger.Error($"Primary binding path cannot be empty for button: '{button.AsString()}'");
            return builder;
        }

        if (!IsBindingPathValid(builder, primaryBindingPath, device))
        {
            return null;
        }
        GameInputPatcher.Bindings.GetOrAddNew(button).Add(new(device, GameInput.BindingSet.Primary, primaryBindingPath));
        SetBindingDefinition(button, device, GameInput.BindingSet.Primary, primaryBindingPath);

        if (!string.IsNullOrWhiteSpace(secondaryBindingPath))
        {
            if (!IsBindingPathValid(builder, primaryBindingPath, device))
            {
                return null;
            }
            GameInputPatcher.Bindings.GetOrAddNew(button).Add(new(device,  GameInput.BindingSet.Secondary, secondaryBindingPath));
            SetBindingDefinition(button, device, GameInput.BindingSet.Secondary, secondaryBindingPath);
        }

        SetBindable(builder, device);
        
        return builder;
    }

    

    /// <summary>
    /// Sets the default bindings for keyboard controls for this button.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="primaryBindingPath">The binding path to bind the primary hotkey of this button.</param>
    /// <param name="secondaryBindingPath">The binding path to bind the secondary hotkey of this button. If null or empty, the button will have an empty secondary binding.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>By default, the binding will be bindable in the Mod Input tab. If you wish to make the button unbindable, consider using <see cref="SetUnbindable"/>.</remarks>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputControlPath.html"/>
    public static EnumBuilder<Button> WithKeyboardBinding(this EnumBuilder<Button> builder, string primaryBindingPath, string secondaryBindingPath = null)
    {
        return WithBinding(builder, GameInput.Device.Keyboard, primaryBindingPath, secondaryBindingPath);
    }
    
    /// <summary>
    /// Sets the default bindings for controller controls for this button.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="primaryBindingPath">The binding path to bind the primary hotkey of this button.</param>
    /// <param name="secondaryBindingPath">The binding path to bind the secondary hotkey of this button. If null or empty, the button will have an empty secondary binding.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>By default, the binding will be bindable in the Mod Input tab. If you wish to make the button unbindable, consider using <see cref="SetUnbindable"/>.</remarks>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputControlPath.html"/>
    public static EnumBuilder<Button> WithControllerBinding(this EnumBuilder<Button> builder, string primaryBindingPath, string secondaryBindingPath = null)
    {
        return WithBinding(builder, GameInput.Device.Controller, primaryBindingPath, secondaryBindingPath);
    }

    /// <summary>
    /// Assigns this button to be bindable in the Mod Input tab.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="devices">The devices that will have customized bindings.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>By default, all custom buttons are bindable, unless explicitly set as unbindable by calling <see cref="SetUnbindable"/>.</remarks>
    /// <seealso cref="SetUnbindable"/>
    public static EnumBuilder<Button> SetBindable(this EnumBuilder<Button> builder, params GameInput.Device[] devices)
    {
        foreach (var device in devices)
        {
            GameInputPatcher.BindableButtons.Add((builder, device));
        }
        
        return builder;
    }

    /// <summary>
    /// Removes this button from being bindable in the Mod Input tab.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="devices">The devices in which this button cannot be bound.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<Button> SetUnbindable(this EnumBuilder<Button> builder, params GameInput.Device[] devices)
    {
        foreach (var device in devices)
        {
            GameInputPatcher.BindableButtons.Remove((builder, device));
        }
        
        return builder;
    }

    /// <summary>
    /// Assigns this button to force its bindings to work regardless of whether another vanilla button or custom button have similar bindings.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="devices">The devices in which this button avoids conflicts.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static EnumBuilder<Button> AvoidConflicts(this EnumBuilder<Button> builder, params GameInput.Device[] devices)
    {
        foreach (var device in devices)
        {
            GameInputPatcher.ConflictEvaders.Add((builder, device));
        }
        
        return builder;
    }

    private static void SetBindingDefinition(Button button, GameInput.Device device, GameInput.BindingSet bindingSet, string bindingPath)
    {
        if (device == GameInput.Device.Keyboard)
        {
            GameInputSystem.bindingsKeyboard[(button, bindingSet)] = bindingPath;
        }
        else
        {
            GameInputSystem.bindingsController[(button, bindingSet)] = bindingPath;
        }
    }
    
    private static bool IsBindingPathValid(EnumBuilder<Button> builder, string bindingPath, GameInput.Device device)
    {
        Button button = builder;
        var deviceName = device.AsString();
        
        if (string.IsNullOrWhiteSpace(bindingPath))
        {
            InternalLogger.Error($"Binding cannot be empty or null for button: '{button}'.");
            return false;
        }

        var controlsForBinding = InputSystem.FindControls(bindingPath);
        if (controlsForBinding.Count <= 0)
        {
            InternalLogger.Error($"Binding path '{bindingPath}' is invalid for button: '{button}'.");
            return false;
        }

        var actionType = GameInputPatcher.CustomButtons[button].type;

        // pass through buttons can have any binding
        if (actionType == InputActionType.PassThrough)
        {
            if (controlsForBinding.Any(c => !IsDeviceValid(c, device)))
            {
                InternalLogger.Error(
                    $"Binding path '{bindingPath}' represents more than one binding, which is invalid for input action type '{actionType} for pass through button: '{button}'.");
                return false;
            }
            return true;
        }

        if (controlsForBinding.Count > 1)
        {
            InternalLogger.Error($"Binding path '{bindingPath}' represents more than one binding, which is invalid for input action type '{actionType} for button: '{button}'.");
            return false;
        }

        foreach (var inputControl in controlsForBinding)
        {
            if (!IsDeviceValid(inputControl, device))
            {
                InternalLogger.Error($"Binding path '{bindingPath}' is not compatible with device: '{device}' for button: '{button}'.");
                return false;
            }

            if (inputControl is InputDevice)
            {
                InternalLogger.Error($"Binding path '{bindingPath}' cannot be a device for button: '{button}'.");
                return false;
            }
        }

        return true;
    }

    private static bool IsDeviceValid(InputControl inputControl, GameInput.Device device)
    {
        return device switch
        {
            GameInput.Device.Keyboard => inputControl.device is Keyboard or Mouse,
            GameInput.Device.Controller => inputControl.device is Gamepad,
            _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
        };
    }
}
#endif