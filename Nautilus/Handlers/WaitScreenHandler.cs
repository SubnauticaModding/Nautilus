using System;
using System.Collections;
using Nautilus.Patchers;

namespace Nautilus.Handlers;

/// <summary>
/// A handler for registering mod loading actions to be performed during the loading screen.
/// </summary>
public static class WaitScreenHandler
{
    /// <summary>
    /// Register a task for doing something very early during the loading screen.
    /// <br /><br />
    /// The task will execute almost immediately after the user presses the button to start a game, once the loading
    /// screen fades in. This makes the timing ideal for loading save data or other data that does not directly depend
    /// on objects in the game.
    /// <br />
    /// Do not use this task to set up GameObjects for your mod, as at this point the game is still in the MainMenu
    /// scene and any GameObjects you create now will be destroyed once the in-game Main scene is loaded. Consider using
    /// <see cref="RegisterAsyncLoadTask"/> or <see cref="RegisterLateAsyncLoadTask"/> for that instead.
    /// </summary>
    /// <param name="modName">The name of your mod. Will be displayed while this task is being worked on.</param>
    /// <param name="loadingFunction">The function that will be called.</param>
    /// <param name="description">An optional description to give users detailed information about what your task is
    /// doing. Can be updated even while your task is executing by setting
    /// <see cref="WaitScreenTask"/>.<see cref="WaitScreenTask.Status"/>.</param>
    public static void RegisterEarlyLoadTask(string modName, Action<WaitScreenTask> loadingFunction, string description = null)
    {
        WaitScreenPatcher.EarlyInitTasks.Add(new WaitScreenTask(modName, loadingFunction, description));
    }

    /// <inheritdoc cref="RegisterEarlyLoadTask"/>
    public static void RegisterEarlyAsyncLoadTask(string modName, Func<WaitScreenTask, IEnumerator> loadingFunction,
        string description = null)
    {
        WaitScreenPatcher.EarlyInitTasks.Add(new WaitScreenTask(modName, loadingFunction, description));
    }

    /// <summary>
    /// Register a task for doing something midway through the loading screen.
    /// <br /><br />
    /// The task will execute just after the Main scene has finished loading in, meaning any GameObjects created during
    /// this task will not be destroyed by a unity scene switch.
    /// <br />
    /// However, nothing in the game exists yet. You cannot access in-game concepts like Player.main, EscapePod.main or
    /// similar at this time during the loading process. For that purpose, consider using
    /// <see cref="RegisterLateAsyncLoadTask"/>.
    /// </summary>
    /// <param name="modName">The name of your mod. Will be displayed while this task is being worked on.</param>
    /// <param name="loadingFunction">The function that will be called.</param>
    /// <param name="description">An optional description to give users detailed information about what your task is
    /// doing. Can be updated even while your task is executing by setting
    /// <see cref="WaitScreenTask"/>.<see cref="WaitScreenTask.Status"/>.</param>
    public static void RegisterLoadTask(string modName, Action<WaitScreenTask> loadingFunction, string description = null)
    {
        WaitScreenPatcher.InitTasks.Add(new WaitScreenTask(modName, loadingFunction, description));
    }

    /// <inheritdoc cref="RegisterLoadTask"/>
    public static void RegisterAsyncLoadTask(string modName, Func<WaitScreenTask, IEnumerator> loadingFunction,
        string description = null)
    {
        WaitScreenPatcher.InitTasks.Add(new WaitScreenTask(modName, loadingFunction, description));
    }

    /// <summary>
    /// Register a task for doing something just before the loading screen ends and the player gains control.
    /// <br /><br />
    /// The task will execute at the last possible moment before the loading screen ends. The game has finished all its
    /// setup at this point and is only waiting for any tasks registered through this method. Use this method for tasks
    /// that heavily rely on other GameObjects or singletons to be present, such as the Player, Inventory, or other mods.
    /// </summary>
    /// <param name="modName">The name of your mod. Will be displayed while this task is being worked on.</param>
    /// <param name="loadingFunction">The function that will be called.</param>
    /// <param name="description">An optional description to give users detailed information about what your task is
    /// doing. Can be updated even while your task is executing by setting
    /// <see cref="WaitScreenTask"/>.<see cref="WaitScreenTask.Status"/>.</param>
    public static void RegisterLateLoadTask(string modName, Action<WaitScreenTask> loadingFunction, string description = null)
    {
        WaitScreenPatcher.LateInitTasks.Add(new WaitScreenTask(modName, loadingFunction, description));
    }

    /// <inheritdoc cref="RegisterLateLoadTask"/>
    public static void RegisterLateAsyncLoadTask(string modName, Func<WaitScreenTask, IEnumerator> loadingFunction,
        string description = null)
    {
        WaitScreenPatcher.LateInitTasks.Add(new WaitScreenTask(modName, loadingFunction, description));
    }

    /// <summary>
    /// Represents the unit of work performed by a mod during the loading screen.
    /// </summary>
    public sealed class WaitScreenTask
    {
        /// <summary>
        /// Determines the name displayed while this task is being performed.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        /// Optional detailed information about what the mod is working on during this task. Can be updated during
        /// tasks to give the user detailed feedback about what exactly the mod is doing.
        /// </summary>
        public string Status { get; set; }

        internal readonly Action<WaitScreenTask> ModActionSync;
        internal readonly Func<WaitScreenTask, IEnumerator> ModActionAsync;


        internal WaitScreenTask(string modName, Action<WaitScreenTask> action, string description = null)
        {
            ModName = modName;
            ModActionSync = action;
            Status = description;
        }

        internal WaitScreenTask(string modName, Func<WaitScreenTask, IEnumerator> action, string description = null)
        {
            ModName = modName;
            ModActionAsync = action;
            Status = description;
        }
    }
}