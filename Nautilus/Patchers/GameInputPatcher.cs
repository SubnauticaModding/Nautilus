#if SUBNAUTICA
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Nautilus.Utility;
using UnityEngine.InputSystem;
using Hotkey = (GameInput.Button button, GameInput.Device device);

namespace Nautilus.Patchers;

[HarmonyPatch(typeof(GameInputSystem))]
internal static class GameInputPatcher
{
    public record InputBinding(GameInput.Device Device, GameInput.BindingSet BindingSet, string Path);
    
    public static Dictionary<GameInput.Button, InputAction> CustomButtons = new();
    public static Dictionary<GameInput.Button, List<InputBinding>> Bindings = new();
    public static HashSet<Hotkey> BindableButtons = new();
    public static List<InputActionMap> CustomActionMaps = new();

    public static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(GameInputPatcher));
        InternalLogger.Info("Patched GameInputPatcher.");
    }
    
    public static void RegisterActions(InputActionAsset actionAsset)
    {
        CustomActionMaps.AddRange(actionAsset.actionMaps);
    }
    
    [HarmonyPatch(nameof(GameInputSystem.Initialize))]
    [HarmonyPostfix]
    private static void InitializePostfix(GameInputSystem __instance)
    {
        foreach (var kvp in CustomButtons)
        {
            var button = kvp.Key;
            var action =  kvp.Value;
            __instance.actions[button] = action;
            action.started += __instance.OnActionStarted;
            action.Enable();
            
            if (Bindings.TryGetValue(button, out var bindings))
            {
                bindings.ForEach(binding => action.AddBinding(binding.Path, groups: __instance.GetCompositeGroup(binding.Device, binding.BindingSet)));
            }
        }
        
        InternalLogger.Debug("Added all custom buttons.");
    }

    [HarmonyPatch(nameof(GameInputSystem.Deinitialize))]
    [HarmonyPostfix]
    private static void DeinitializePostfix(GameInputSystem __instance)
    {
        foreach (var customButton in CustomButtons)
        {
            customButton.Value.started -= __instance.OnActionStarted;
            customButton.Value.Disable();
        }
    }

    [HarmonyPatch(typeof(GameInput))]
    [HarmonyPatch(nameof(GameInput.IsBindable))]
    [HarmonyPostfix]
    private static void IsBindablePostfix(GameInput.Device device, GameInput.Button action, ref bool __result)
    {
        if (CustomButtons.ContainsKey(action) && BindableButtons.Contains((action, device)))
        {
            __result = true;
        }
    }
    
    [HarmonyPatch(nameof(GameInputSystem.SerializeSettings))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SerializeSettingsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(GameInput), nameof(GameInput.AllActions))))
            .Advance(1)
            .Insert(Transpilers.EmitDelegate(IncludeCustomButtons));
        
        return matcher.InstructionEnumeration();
        
        GameInput.Button[] IncludeCustomButtons(GameInput.Button[] buttons)
        {
            var result = buttons.ToHashSet();
            result.AddRange(BindableButtons.Select(b => b.button));
            return result.ToArray();
        }
    }
    
    [HarmonyPatch(typeof(GameInput))]
    [HarmonyPatch(nameof(GameInput.PrimaryDevice), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool PrimaryDevicePrefix(ref GameInput.Device __result)
    {
        if (GameInput.input is not null)
        {
            return true;
        }
        __result = GameInput.Device.Keyboard;
        return false;

    }
}
#endif