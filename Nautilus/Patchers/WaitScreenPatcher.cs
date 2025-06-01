using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class WaitScreenPatcher
{
    public const string EarlyModLoadingStage = "ModLoadingEarly";
    public const string ModLoadingStage = "ModLoading";

    internal static List<WaitScreenHandler.WaitScreenTask> InitEvents = new();
    
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(LoadingStage), nameof(LoadingStage.GetDuration)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(AddModLoadStageDuration))));
        harmony.Patch(AccessTools.Method(typeof(MainGameController), nameof(MainGameController.StartGame)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(LoadModDataAsync))));
    }

    private static bool AddModLoadStageDuration(ref float __result, string stage)
    {
        switch (stage)
        {
            case EarlyModLoadingStage:
                // Roughly equal to the width of the bar used for terrain loading. Should be around 20%.
                __result = 4f;
                return false;
            case ModLoadingStage:
                __result = 4f;
                return false;
            default:
                return true;
        }
    }

    internal static IEnumerator LoadEarlyModDataAsync()
    {
        var loadingStage = WaitScreen.Add(EarlyModLoadingStage);
        var delegates = SaveUtilsPatcher.OnStartLoadingEvents.GetInvocationList();
        for (int i = 0; i < delegates.Length; i++)
        {
            var startTime = UWE.Utils.GetSystemTime();
            loadingStage.SetProgress((float)i / delegates.Length);
            delegates[i].DynamicInvoke();
            
            InternalLogger.Debug($"Took {(UWE.Utils.GetSystemTime() - startTime)}ms for '{delegates[i].Target}'");
            // Wait for a frame before the next mod to reduce the chance of stutter.
            yield return null;
        }
        
        // Count the mod loading stage as completed and remove it from the stack.
        loadingStage.SetProgress(1f);
        WaitScreen.Remove(loadingStage);
    }

    private static IEnumerator LoadModDataAsync(IEnumerator enumerator)
    {
        // We gain execution here directly after SceneMain has finished loading. This is the earliest point you can
        // be without getting scooped up by the SceneCleaner.
        var loadingStage = WaitScreen.Add(ModLoadingStage);

        for (int i = 0; i < InitEvents.Count; i++)
        {
            var task = InitEvents[i];
            loadingStage.SetProgress((float)i / InitEvents.Count);
            
            // Later on, set current status in UI.
            InternalLogger.Debug($"{task.ModName}: {task.Status}");
            task.ModActionSync?.Invoke(task);
            yield return task.ModActionAsync?.Invoke(task);
        }

        // Count the mod loading stage as completed and remove it from the stack.
        loadingStage.SetProgress(1f);
        WaitScreen.Remove(loadingStage);
        // Let the vanilla method continue as normal.
        yield return enumerator;
    }
}