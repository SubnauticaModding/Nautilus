using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;

namespace Nautilus.Patchers;

internal class SaveUtilsPatcher
{
    private static readonly List<Action> oneTimeUseOnSaveEvents = new();
    private static readonly List<Action> oneTimeUseOnLoadEvents = new();
    private static readonly List<Action> oneTimeUseOnQuitEvents = new();

    internal static Action OnSaveEvents;
    internal static Action OnFinishLoadingEvents;
    internal static Action OnStartLoadingEvents;
    internal static Action OnQuitEvents;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(IngameMenu), nameof(IngameMenu.CaptureSaveScreenshot)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(SaveUtilsPatcher), nameof(InvokeSaveEvents))));
        harmony.Patch(AccessTools.Method(typeof(MainSceneLoading), nameof(MainSceneLoading.Launch)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(SaveUtilsPatcher), nameof(InvokeLoadEvents))));
        harmony.Patch(AccessTools.Method(typeof(IngameMenu), nameof(IngameMenu.QuitGameAsync)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(SaveUtilsPatcher), nameof(InvokeQuitEvents))));
    }

    internal static void AddOneTimeUseSaveEvent(Action onSaveAction)
    {
        oneTimeUseOnSaveEvents.Add(onSaveAction);
    }

    internal static void AddOneTimeUseLoadEvent(Action onLoadAction)
    {
        oneTimeUseOnLoadEvents.Add(onLoadAction);
    }

    internal static void AddOneTimeUseQuitEvent(Action onQuitAction)
    {
        oneTimeUseOnQuitEvents.Add(onQuitAction);
    }

    internal static void InvokeSaveEvents()
    {
        OnSaveEvents?.Invoke();
            
        if (oneTimeUseOnSaveEvents.Count > 0)
        {
            foreach (Action action in oneTimeUseOnSaveEvents)
            {
                action.Invoke();
            }

            oneTimeUseOnSaveEvents.Clear();
        }
    }

    internal static IEnumerator InvokeLoadEvents(IEnumerator enumerator)
    {
        yield return WaitScreenPatcher.LoadEarlyModDataAsync();

        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
        
#if SUBNAUTICA
        OnLoad();
#elif BELOWZERO
        uGUI_MainMenu.main.StartCoroutine(WaitUntilLoaded());
        
        IEnumerator WaitUntilLoaded()
        {
            if (uGUI.main.loading.isLoading)
            {
                yield return new WaitWhile(() => uGUI.main.loading.isLoading);
            }
            
            if (WaitScreen.main.Exists()?.isShown is true)
            {
                yield return new WaitWhile(() => WaitScreen.main.isShown);
            }
            
            OnLoad();
        }
#endif
        void OnLoad()
        {
            OnFinishLoadingEvents?.Invoke();
            
            if (oneTimeUseOnLoadEvents.Count > 0)
            {
                foreach (Action action in oneTimeUseOnLoadEvents)
                {
                    action.Invoke();
                }
                
                oneTimeUseOnLoadEvents.Clear();
            }
        }
    }

    internal static IEnumerator InvokeQuitEvents(IEnumerator enumerator)
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
            
        OnQuitEvents?.Invoke();

        if (oneTimeUseOnQuitEvents.Count > 0)
        {
            foreach (Action action in oneTimeUseOnQuitEvents)
            {
                action.Invoke();
            }

            oneTimeUseOnQuitEvents.Clear();
        }
    }
}