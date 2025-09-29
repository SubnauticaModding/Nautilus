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
    /// <param name="actionType">Determines the behavior with which an <see cref="InputAction"/> triggers.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputActionType.html"/>
    public static EnumBuilder<Button> CreateInput(this EnumBuilder<Button> builder, InputActionType actionType = InputActionType.Button)
    {
        Button button = builder;
        GameInputPatcher.CustomButtons[button] = new InputAction(button.ToString(), actionType);
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
    /// /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputControlPath.html"/>
    public static EnumBuilder<Button> WithBinding(this EnumBuilder<Button> builder, GameInput.Device device,
        GameInput.BindingSet bindingSet, string bindingPath)
    {
        Button button = builder;
        GameInputPatcher.Bindings.GetOrAddNew(button).Add(new(device, bindingSet, bindingPath));
        if (device == GameInput.Device.Keyboard)
        {
            GameInputSystem.bindingsKeyboard[(button, bindingSet)] = bindingPath;
        }
        else
        {
            GameInputSystem.bindingsController[(button, bindingSet)] = bindingPath;
        }
        
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
        
        return builder;
    }

    /// <summary>
    /// Sets the default bindings for keyboard controls for this button.
    /// </summary>
    /// <param name="builder">The current custom enum object instance.</param>
    /// <param name="primaryBindingPath">The binding path to bind the primary hotkey of this button.</param>
    /// <param name="secondaryBindingPath">The binding path to bind the secondary hotkey of this button. If null or empty, the button will have an empty secondary binding.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputControlPath.html"/>
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
    /// /// <seealso href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.InputControlPath.html"/>
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
    public static EnumBuilder<Button> SetAsBindable(this EnumBuilder<Button> builder, params GameInput.Device[] devices)
    {
        foreach (var device in devices)
        {
            GameInputPatcher.BindableButtons.Add((builder, device));
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
}
#endif