using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    internal static readonly List<WaitScreenHandler.WaitScreenTask> EarlyInitTasks = new();
    internal static readonly List<WaitScreenHandler.WaitScreenTask> InitTasks = new();

    private static TextMeshProUGUI _statusText;

    private static readonly Dictionary<string, string> _statusMap = new()
    {
        {"FadeInDummy", "Preparing..."},
        {"SaveFilesLoad", "Loading save files"},
        {"SceneMain", "Loading main game"},
        {"SceneEssentials", "Loading main game"},
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
    };
    
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(WaitScreen), nameof(WaitScreen.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(WaitScreenPatcher), nameof(AddModLoadStageDuration))));
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
    }

    /// <summary>
    /// Perform an early, initial load stage for mods. This is as early into the save game launch process as you can
    /// get while still showing the loading screen (and thereby delaying any freezes/stutters to a point where users
    /// expect them to happen).
    /// </summary>
    internal static IEnumerator LoadEarlyModDataAsync()
    {
        _statusText ??= CreateStatusText();
        _statusText.text = "Early Mod Loading";
        
        var loadingStage = WaitScreen.Add(EarlyModLoadingStage);
        yield return ProcessModTasks(EarlyInitTasks, loadingStage);
        
        // Count the mod loading stage as completed and remove it from the stack.
        loadingStage.SetProgress(1f);
        WaitScreen.Remove(loadingStage);
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

        // The entire loading screen remains loaded at all times anyway so this isn't super necessary, but it also
        // doesn't hurt.
        Object.Destroy(_statusText.gameObject);
    }

    private static IEnumerator ProcessModTasks(List<WaitScreenHandler.WaitScreenTask> tasks, WaitScreen.ManualWaitItem loadingStage)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            loadingStage.SetProgress((float)i / tasks.Count);
            SetModStatus(task.ModName, task.Status, i, tasks.Count);
            
            task.ModActionSync?.Invoke(task);
            if (task.ModActionAsync == null)
                continue;
            
            var taskEnumerator = task.ModActionAsync(task);
            while (taskEnumerator.MoveNext())
            {
                // Give the mod a chance to update its status label during longer tasks.
                SetModStatus(task.ModName, task.Status, i, tasks.Count);
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
        if (!_statusText)
            return;

        // Only handle vanilla stages, modded stages need more granular control during their processing.
        if (_statusMap.TryGetValue(WaitScreen.main.items.Last().stage, out string description))
            _statusText.text = description;
    }
    
    private static void SetModStatus(string modName, string status, int current, int total)
    {
        var text = $"Loading Mod ({current}/{total}): {modName}";
        if (!string.IsNullOrEmpty(status))
            text += " - " + status;
        _statusText.text = text;
    }

    /// <summary>
    /// Set up the text label describing progress above the loading bar.
    /// </summary>
    private static TextMeshProUGUI CreateStatusText()
    {
        var gameObject = new GameObject("WaitScreenStatus");
        // Parent it to the loading screen itself.
        gameObject.transform.SetParent(uGUI.main.loading.loadingBackground.transform, false);
        // Put the text right next to the spinning loading icon.
        gameObject.transform.localPosition = new Vector3(-685f, -395f);
        
        var textMesh = gameObject.AddComponent<TextMeshProUGUI>();
        textMesh.font = FontUtils.Aller_Rg;
        textMesh.fontSize = 24f;
        textMesh.alignment = TextAlignmentOptions.MidlineLeft;
        // Keep this text label a single line that spans across the screen.
        textMesh.autoSizeTextContainer = false;
        textMesh.enableWordWrapping = false;
        // If a mod message somehow gets too long, cut if off with an ellipsis (...)
        textMesh.overflowMode = TextOverflowModes.Ellipsis;
        textMesh.rectTransform.pivot = new Vector2(0f, 1f);
        // Extend the available space to just before the other edge of the screen.
        textMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1600f);

        return textMesh;
    }
}