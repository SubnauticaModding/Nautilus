﻿#if SUBNAUTICA
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
        
        GameInputPatcher.Bindings.GetOrAddNew(button).Add(new(device, GameInput.BindingSet.Primary, primaryBindingPath));
        SetBindingDefinition(button, device, GameInput.BindingSet.Primary, primaryBindingPath);

        if (!string.IsNullOrWhiteSpace(secondaryBindingPath))
        {
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
    /// Defines a custom category in which this button will appear in the Mod Input tab.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="category">The category name.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>If the specified category was already added by another mod, both buttons will be merged under one category.</remarks>
    public static EnumBuilder<Button> WithCategory(this EnumBuilder<Button> builder, string category)
    {
        GameInputPatcher.Categories.GetOrAddNew(category).Add(builder);
        return builder;
    }

    /// <summary>
    /// Assigns this button to be bindable for all devices in the Mod Input tab.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>By default, all custom buttons are bindable, unless explicitly set as unbindable by calling <see cref="SetUnbindable(EnumBuilder{Button})"/>.</remarks>
    /// <seealso cref="SetBindable(EnumBuilder{Button}, GameInput.Device)"/>
    public static EnumBuilder<Button> SetBindable(this EnumBuilder<Button> builder)
    {
        foreach (var device in GameInput.AllDevices)
        {
            SetBindable(builder, device);
        }
        
        return builder;
    }

    /// <summary>
    /// Assigns this button to be bindable for a device in the Mod Input tab.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="device">The device that was set to be bindable.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <remarks>By default, all custom buttons are bindable, unless explicitly set as unbindable by calling <see cref="SetUnbindable(EnumBuilder{Button}, GameInput.Device)"/>.</remarks>
    /// <seealso cref="SetBindable(EnumBuilder{Button})"/>
    public static EnumBuilder<Button> SetBindable(this EnumBuilder<Button> builder, GameInput.Device device)
    {
        GameInputPatcher.BindableButtons.Add((builder, device));
        return builder;
    }

    /// <summary>
    /// Removes this button from being bindable in the Mod Input tab.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="SetUnbindable(EnumBuilder{Button}, GameInput.Device)"/>
    public static EnumBuilder<Button> SetUnbindable(this EnumBuilder<Button> builder)
    {
        foreach (var device in GameInput.AllDevices)
        {
            SetUnbindable(builder, device);
        }
        
        return builder;
    }

    /// <summary>
    /// Removes this button from being bindable for a specific device in the Mod Input tab.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="device">The device in which this button cannot be bound.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="SetUnbindable(EnumBuilder{Button})"/>
    public static EnumBuilder<Button> SetUnbindable(this EnumBuilder<Button> builder, GameInput.Device device)
    {
        GameInputPatcher.BindableButtons.Remove((builder, device));
        return builder;
    }

    /// <summary>
    /// Assigns this button to force its bindings to work regardless of whether another vanilla button or custom button have similar bindings.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="AvoidConflicts(EnumBuilder{Button}, GameInput.Device)"/>
    public static EnumBuilder<Button> AvoidConflicts(this EnumBuilder<Button> builder)
    {
        foreach (var device in GameInput.AllDevices)
        {
            AvoidConflicts(builder, device);
        }
        
        return builder;
    }
    
    /// <summary>
    /// Assigns this button to force its bindings to work regardless of whether another vanilla button or custom button have similar bindings in a device.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="device">The devices in which this button avoids conflicts.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso cref="AvoidConflicts(EnumBuilder{Button})"/>
    public static EnumBuilder<Button> AvoidConflicts(this EnumBuilder<Button> builder, GameInput.Device device)
    {
        GameInputPatcher.ConflictEvaders.Add((builder, device));
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
}
#endif