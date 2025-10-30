using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nautilus.Patchers;

internal static class WaitScreenPatcher
{
    public const string EarlyModLoadingStage = "ModLoadingEarly";
    public const string ModLoadingStage = "ModLoading";
    public const string LateModLoadingStage = "ModLoadingLate";

    internal static readonly List<WaitScreenHandler.WaitScreenTask> EarlyInitTasks = new();
    internal static readonly List<WaitScreenHandler.WaitScreenTask> InitTasks = new();
    internal static readonly List<WaitScreenHandler.WaitScreenTask> LateInitTasks = new();

    private static TextMeshProUGUI _statusText;

    private static readonly Dictionary<string, string> _statusMap = new()
    {
        // Early Mod Loading
        {"FadeInDummy", "Preparing..."},
        {"SaveFilesLoad", "Loading save files"},
        {"SceneMain", "Loading game environment"},
        // Mod Loading
        {"SceneEssentials", "Loading game environment"},
        {"SceneCyclops", "Loading Cyclops assets"},
        {"SceneEscapePod", "Loading Lifepod assets"},
        {"SceneAurora", "Loading Aurora assets"},
        {"Builder", "Loading world"},
        {"WorldMount", "Loading world"},
        {"WorldTiles", "Loading world"},
        {"Batches", "Loading world"},
        {"Octrees", "Loading world"},
        {"Terrain", "Loading world"},
        {"Clipmap", "Loading world"},
        {"UpdatingVisibility", "Loading world"},
        {"EntityCells", "Loading entities"},
        {"WorldSettle", "Finalising world"},
        {"Equipment", "Loading equipment"}
        // Late Mod Loading
    };

    private static readonly Dictionary<string, string> _modStatusMap = new()
    {
        {EarlyModLoadingStage, "Early mod setup"},
        {ModLoadingStage, "Mod setup"},
        {LateModLoadingStage, "Late mod setup"},
    };

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(WaitScreen), nameof(WaitScreen.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(AddModLoadStageDuration))));
        harmony.Patch(AccessTools.Method(typeof(MainSceneLoading), nameof(MainSceneLoading.Launch)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(LoadEarlyModDataAsync))));

        harmony.Patch(AccessTools.Method(typeof(MainGameController), nameof(MainGameController.StartGame)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(LoadModDataAsync))));
        harmony.Patch(AccessTools.Method(typeof(uGUI_SceneLoading), nameof(uGUI_SceneLoading.SetProgress)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(UpdateStatusText))));
    }

    /// <summary>
    /// Patch in the amount of space that the mod load stages take up in the loading bar. These are relative to all
    /// other stages. The vanilla spacing was likely set based on measured load times (total ~20).
    /// </summary>
    private static void AddModLoadStageDuration()
    {
        LoadingStage.durations[EarlyModLoadingStage] = 4f;
        LoadingStage.durations[ModLoadingStage] = 4f;
        LoadingStage.durations[LateModLoadingStage] = 2f; // Shorter since it will probably see less use.
    }

    /// <summary>
    /// Perform an early, initial load stage for mods. This is as early into the save game launch process as you can
    /// get while still showing the loading screen (and thereby delaying any freezes/stutters to a point where users
    /// expect them to happen).
    /// </summary>
    internal static IEnumerator LoadEarlyModDataAsync(IEnumerator enumerator)
    {
        if (!_statusText)
            _statusText = CreateStatusText();
        _statusText.text = "Early Mod Loading";

        var loadingStage = WaitScreen.Add(EarlyModLoadingStage);
        // In BZ the WaitScreen is part of the Main scene and does not exist yet. Set up a dummy stage instead.
        if (loadingStage == null)
            loadingStage = new WaitScreen.ManualWaitItem(EarlyModLoadingStage);
        yield return ProcessModTasks(EarlyInitTasks, loadingStage);

        // Count the mod loading stage as completed and remove it from the stack.
        loadingStage.SetProgress(1f);
        WaitScreen.Remove(loadingStage);

        // Let the rest of the method continue, which takes it to the end of the loading screen.
        // Patching in the other loading stages here is undesirable because this method simply waits for the one that
        // *actually* does the work to complete.
        yield return enumerator;
    }

    /// <summary>
    /// We gain execution here directly after the game's main scene has finished loading, but before the rest of the
    /// game can continue to load (that's what the rest of the patched method does). This is the earliest point
    /// you can do anything without your GameObjects getting destroyed by the loading of the Main scene.
    /// </summary>
    private static IEnumerator LoadModDataAsync(IEnumerator enumerator)
    {
        var loadingStage = WaitScreen.Add(ModLoadingStage);
        yield return ProcessModTasks(InitTasks, loadingStage);
        // Count the mod loading stage as completed and remove it from the stack.
        loadingStage.SetProgress(1f);
        WaitScreen.Remove(loadingStage);

        // Let the vanilla method continue as normal.
        yield return enumerator;

        // Usually the loading screen would have ended here. Instead, add another opportunity for mods to set up.
        var lateLoading = WaitScreen.Add(LateModLoadingStage);
        yield return ProcessModTasks(LateInitTasks, lateLoading);
        lateLoading.SetProgress(1f);
        WaitScreen.Remove(lateLoading);

        // Destroy the status display. In SN this would have persisted anyway, but in BZ it would get cleaned up
        // on return to main menu.
        Object.Destroy(_statusText.gameObject);
    }

    /// <summary>
    /// Let each mod work through their tasks in turn while catching any errors along the way.
    /// </summary>
    private static IEnumerator ProcessModTasks(List<WaitScreenHandler.WaitScreenTask> tasks, WaitScreen.ManualWaitItem loadingStage)
    {
        var error = false;
        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            InternalLogger.Debug($"Processing mod task by '{task.ModName}' ({i + 1}/{tasks.Count})");
            loadingStage.SetProgress((float)i / tasks.Count);
            SetModStatus(loadingStage, task.ModName, task.Status, i + 1, tasks.Count);

            try
            {
                task.ModActionSync?.Invoke(task);
            }
            catch (Exception ex)
            {
                SetModError(task.ModName, task.Status);
                InternalLogger.Error(
                    $"{task.ModName} has crashed during {_modStatusMap[loadingStage.stage]} task {i + 1}/{tasks.Count}");
                Debug.LogException(ex);
                error = true;
            }
            // Intentionally stall the loading process so that the wait screen gets stuck on the error.
            if (error)
                yield return new WaitWhile(() => true);
            
            if (task.ModActionAsync != null)
            {
                yield return ProcessEnumerator(task.ModActionAsync(task), task, loadingStage, i + 1, tasks.Count);
            }
        }
    }

    private static IEnumerator ProcessEnumerator(IEnumerator taskEnumerator, WaitScreenHandler.WaitScreenTask task,
        WaitScreen.ManualWaitItem loadingStage, int current, int total)
    {
        bool error = false;
        // The try-catch has to be inside the recursive method or exceptions from nested enumerators won't be caught.
        while (true)
        {
            try
            {
                if (!taskEnumerator.MoveNext())
                    break;
            }
            catch (Exception ex)
            {
                SetModError(task.ModName, task.Status);
                InternalLogger.Error(
                    $"{task.ModName} has crashed during {_modStatusMap[loadingStage.stage]} task {current}/{total}");
                Debug.LogException(ex);
                error = true;
            }
            // Intentionally stall the loading process so that the wait screen gets stuck on the error.
            if (error)
                yield return new WaitWhile(() => true);
            
            // Give the mod a chance to update its status label during longer tasks.
            SetModStatus(loadingStage, task.ModName, task.Status, current, total);

            if (taskEnumerator.Current is IEnumerator nestedEnumerator)
            {
                // Recurse nested enumerators. Without this, a method that yield returns another method is unable to
                // update the status label.
                yield return ProcessEnumerator(nestedEnumerator, task, loadingStage, current, total);
            }
            else
            {
                yield return taskEnumerator.Current;
            }
        }
    }

    /// <summary>
    /// Update the text above the loading bar according to the wait screen's status.
    /// Runs every time the loading bar progress updates, i.e. only during update cycles in loading screens.
    /// </summary>
    private static void UpdateStatusText()
    {
        // Cannot update a label that does not exist.
        if (!_statusText || !WaitScreen.main)
            return;

        var waitItems = WaitScreen.main.items;
        if (waitItems == null || waitItems.Count == 0)
            return;

        // The last item on the list is always the newest addition, which in practice means the currently active stage.
        var waitItem = waitItems.Last();
        // Only handle vanilla stages, modded stages need more granular control during their processing.
        if (_statusMap.TryGetValue(waitItem.stage, out string description))
            _statusText.text = description;
    }

    private static void SetModStatus(WaitScreen.ManualWaitItem stage, string modName, string status, int current, int total)
    {
        var language = Language.main;
        var stageDescriptor = _modStatusMap[stage.stage];
        StringBuilder sb = new StringBuilder(stageDescriptor);
        sb.AppendFormat(" ({0}/{1}): {2}", current, total, language.Get(modName));
        if (!string.IsNullOrEmpty(status))
            sb.AppendFormat(" - {0}", language.Get(status));

        var text = sb.ToString();
        if (_statusText.text != text)
        {
            InternalLogger.Debug($"{modName} updated task status to '{status}'");
            _statusText.text = text;
        }
    }

    /// <summary>
    /// Make it very clear to the user that something has gone wrong if a task throws.
    /// </summary>
    private static void SetModError(string modName, string status)
    {
        var language = Language.main;
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("<color=#FF0000FF><b>CRASHED: {0}", language.Get(modName));
        if (!string.IsNullOrEmpty(status))
            sb.AppendFormat(" during '{0}'", language.Get(status));
        sb.AppendLine("</b></color>");
        sb.Append("Please check the log file for more error information.");

        _statusText.text = sb.ToString();
        // The error message is two lines long, make sure both are visible.
        _statusText.overflowMode = TextOverflowModes.Overflow;
    }

    /// <summary>
    /// Set up the text label describing progress above the loading bar.
    /// </summary>
    private static TextMeshProUGUI CreateStatusText()
    {
        var gameObject = new GameObject("WaitScreenStatus");
        // Parent it to the loading screen itself.
        gameObject.transform.SetParent(uGUI.main.loading.loadingBackground.transform, false);

        var textMesh = gameObject.AddComponent<TextMeshProUGUI>();
        textMesh.font = FontUtils.Aller_Rg;
        textMesh.fontSize = 24f;
        textMesh.outlineWidth = 0.3f;
        textMesh.outlineColor = Color.black;
        textMesh.alignment = TextAlignmentOptions.MidlineLeft;
        // Keep this text label a single line that spans across the screen.
        textMesh.autoSizeTextContainer = false;
        textMesh.enableWordWrapping = false;
        // If a mod message somehow gets too long, cut if off with an ellipsis (...)
        textMesh.overflowMode = TextOverflowModes.Ellipsis;
        textMesh.rectTransform.pivot = new Vector2(0f, 1f);
        // Put the text right next to the spinning loading icon.
        textMesh.rectTransform.anchorMin = new Vector2(0.12f, 0.15f);
        // Extend the available space to just before the other edge of the screen.
        textMesh.rectTransform.anchorMax = new Vector2(0.85f, 0.15f);

        return textMesh;
    }
}