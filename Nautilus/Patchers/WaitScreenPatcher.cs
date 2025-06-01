using System.Collections;
using HarmonyLib;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class WaitScreenPatcher
{
    public const string ModLoadingStage = "ModLoading";
    
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(LoadingStage), nameof(LoadingStage.GetDuration)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(AddModLoadStageDuration))));
    }

    private static bool AddModLoadStageDuration(ref float __result, string stage)
    {
        if (stage != ModLoadingStage)
            return true;

        // Roughly equal to the width of the bar used for terrain loading. Should be around 20%.
        __result = 5f;
        return false;
    }

    internal static IEnumerator LoadEarlyModDataAsync()
    {
        var loadingStage = WaitScreen.Add(ModLoadingStage);
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
        
        loadingStage.SetProgress(1f);
        WaitScreen.Remove(loadingStage);
    }
}