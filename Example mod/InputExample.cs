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
        .SetAsBindable(GameInput.Device.Keyboard, GameInput.Device.Controller);

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
            ErrorMessage.AddDebug("Button was pressed!");
        }
    }
}
#endif