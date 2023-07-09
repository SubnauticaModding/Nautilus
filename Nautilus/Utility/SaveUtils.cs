using System;
using Nautilus.Patchers;

namespace Nautilus.Utility;

/// <summary>
/// A small collection of save data related utilities.
/// </summary>
public static class SaveUtils
{
    /// <summary>
    /// Returns the path to the current save slot's directory.
    /// </summary>
    public static string GetCurrentSaveDataDir()
    {
        return SaveLoadManager.GetTemporarySavePath();
    }
    /// <summary>
    /// Registers a simple <see cref="Action"/> method to invoke whenever the player saves the game via the in game menu.
    /// </summary>
    /// <param name="onSaveAction">The method to invoke.</param>
    public static void RegisterOnSaveEvent(Action onSaveAction)
    {
        SaveUtilsPatcher.OnSaveEvents += onSaveAction;
    }

    /// <summary>
    /// Registers a simple <see cref="Action"/> method to invoke the <c>first time</c> the player loads a saved game via the in game menu.
    /// This is only invoked after the game (including most objects around the player) has FULLY loaded. For an earlier alternative, see <see cref="RegisterOnStartLoadingEvent"/>.
    /// </summary>
    /// <param name="onFinishLoadingAction">The method to invoke. This action will not be invoked a second time.</param>
    public static void RegisterOnFinishLoadingEvent(Action onFinishLoadingAction)
    {
        SaveUtilsPatcher.OnFinishLoadingEvents += onFinishLoadingAction;
    }

    /// <summary>
    /// Registers a simple <see cref="Action"/> method to invoke immediately after the <c>first time</c> the player loads a saved game via the in game menu.
    /// </summary>
    /// <param name="onStartLoadingAction">The method to invoke. This action will not be invoked a second time.</param>
    public static void RegisterOnStartLoadingEvent(Action onStartLoadingAction)
    {
        SaveUtilsPatcher.OnStartLoadingEvents += onStartLoadingAction;
    }

    /// <summary>
    /// Registers a simple <see cref="Action"/> method to invoke whenever the player quits the game via the in game menu.
    /// </summary>
    /// <param name="onQuitAction">The method to invoke.</param>
    public static void RegisterOnQuitEvent(Action onQuitAction)
    {
        SaveUtilsPatcher.OnQuitEvents += onQuitAction;
    }

    /// <summary>
    /// Removes a method previously added through <see cref="RegisterOnSaveEvent(Action)"/> so it is no longer invoked when saving the game.<para/>
    /// If you plan on using this, do not register an anonymous method.
    /// </summary>
    /// <param name="onSaveAction">The method invoked.</param>
    public static void UnregisterOnSaveEvent(Action onSaveAction)
    {
        SaveUtilsPatcher.OnSaveEvents -= onSaveAction;
    }

    /// <summary>
    /// Removes a method previously added through <see cref="RegisterOnFinishLoadingEvent(Action)"/> so it is no longer invoked when loading the game.<para/>
    /// If you plan on using this, do not register an anonymous method.
    /// </summary>
    /// <param name="onLoadAction">The method invoked.</param>
    public static void UnregisterOnFinishLoadingEvent(Action onLoadAction)
    {
        SaveUtilsPatcher.OnFinishLoadingEvents -= onLoadAction;
    }

    /// <summary>
    /// Removes a method previously added through <see cref="RegisterOnStartLoadingEvent(Action)"/> so it is no longer invoked when loading the game.<para/>
    /// If you plan on using this, do not register an anonymous method.
    /// </summary>
    /// <param name="onLoadAction">The method invoked.</param>
    public static void UnregisterOnStartLoadingEvent(Action onLoadAction)
    {
        SaveUtilsPatcher.OnStartLoadingEvents -= onLoadAction;
    }

    /// <summary>
    /// Removes a method previously added through <see cref="RegisterOnSaveEvent(Action)"/> so it is no longer invoked when quiting the game.<para/>
    /// If you plan on using this, do not register an anonymous method.
    /// </summary>
    /// <param name="onQuitAction">The method invoked.</param>
    public static void UnregisterOnQuitEvent(Action onQuitAction)
    {
        SaveUtilsPatcher.OnQuitEvents -= onQuitAction;
    }

    /// <summary>
    /// Registers a simple <see cref="Action"/> method to invoke the <c>first time</c> the player saves the game via the in game menu.
    /// </summary>
    /// <param name="onSaveAction">The method to invoke. This action will not be invoked a second time.</param>
    public static void RegisterOneTimeUseOnSaveEvent(Action onSaveAction)
    {
        SaveUtilsPatcher.AddOneTimeUseSaveEvent(onSaveAction);
    }

    /// <summary>
    /// Registers a simple <see cref="Action"/> method to invoke the <c>first time</c> the player loads a saved game via the in game menu.
    /// </summary>
    /// <param name="onLoadAction">The method to invoke. This action will not be invoked a second time.</param>
    public static void RegisterOneTimeUseOnLoadEvent(Action onLoadAction)
    {
        SaveUtilsPatcher.AddOneTimeUseLoadEvent(onLoadAction);
    }

    /// <summary>
    /// Registers a simple <see cref="Action"/> method to invoke the <c>first time</c> the player quits the game via the in game menu.
    /// </summary>
    /// <param name="onQuitAction">The method to invoke. This action will not be invoked a second time.</param>
    public static void RegisterOneTimeUseOnQuitEvent(Action onQuitAction)
    {
        SaveUtilsPatcher.AddOneTimeUseQuitEvent(onQuitAction);
    }
}