#if SUBNAUTICA
using System;
using System.IO;
using BepInEx;
using Nautilus.Handlers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.inputexample", "Nautilus Input Example Mod", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class InputExample : BaseUnityPlugin
{
    private GameInput.Button PrintButton = EnumHandler.AddEntry<GameInput.Button>("MyPrint")
        .CreateInput()
        .WithKeyboardBinding("<Keyboard>/l", "<Keyboard>/z")
        .WithControllerBinding("<Gamepad>/dpad/right")
        .AvoidConflicts(GameInput.Device.Keyboard)
        .WithCategory("NautilusExamplePrintCategory");
    
    private GameInput.Button PrintHelloButton = EnumHandler.AddEntry<GameInput.Button>("PrintHello")
        .CreateInput()
        .WithKeyboardBinding("<Keyboard>/r")
        .WithControllerBinding("<Gamepad>/dpad/left")
        .AvoidConflicts(GameInput.Device.Keyboard)
        .WithCategory("NautilusExamplePrintCategory");

    private GameInput.Button LogButton = EnumHandler.AddEntry<GameInput.Button>("MyLog")
        .CreateInput()
        .WithKeyboardBinding("<Keyboard>/l")
        .WithControllerBinding("<Gamepad>/dpad/up")
        .AvoidConflicts(GameInput.Device.Keyboard)
        .WithCategory("NautilusExampleLogCategory");

    private void Awake()
    {
        LanguageHandler.RegisterLocalizationFolder();
    }

    private void Update()
    {
        if (!GameInput.IsInitialized)
        {
            return;
        }
        
        if (GameInput.GetButtonDown(PrintButton))
        {
            ErrorMessage.AddDebug("Button 1 was pressed!");
        }
        
        if (GameInput.GetButtonDown(PrintHelloButton))
        {
            ErrorMessage.AddDebug("Button 2 was pressed!");
        }
        
        if (GameInput.GetButtonDown(LogButton))
        {
            Logger.LogMessage("Log button was pressed!");
        }
    }
}
#endif