using System;
using System.Collections;
using Nautilus.Patchers;

namespace Nautilus.Handlers;

public static class WaitScreenHandler
{
    public static void RegisterLoadTask(string modName, Action<WaitScreenTask> loadingFunction)
    {
        WaitScreenPatcher.InitEvents.Add(new WaitScreenTask(modName, loadingFunction));
    }

    public static void RegisterAsyncLoadTask(string modName, Func<WaitScreenTask, IEnumerator> asyncLoadingFunction)
    {
        WaitScreenPatcher.InitEvents.Add(new WaitScreenTask(modName, asyncLoadingFunction));
    }

    public sealed class WaitScreenTask
    {
        public readonly string ModName;

        public string Status { get; private set; }

        internal Action<WaitScreenTask> ModActionSync;
        internal Func<WaitScreenTask, IEnumerator> ModActionAsync;
        
        
        internal WaitScreenTask(string modName, Action<WaitScreenTask> action)
        {
            ModName = modName;
            ModActionSync = action;
        }
        
        internal WaitScreenTask(string modName, Func<WaitScreenTask, IEnumerator> action)
        {
            ModName = modName;
            ModActionAsync = action;
        }

        public void SetStatus(string status)
        {
            Status = status;
        }
    }
}