using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Nautilus.Extensions;
using UnityEngine;

namespace Nautilus.Patchers;

internal class SaveUtilsPatcher
{
    private static readonly List<Action> oneTimeUseOnSaveEvents = new();
    private static readonly List<Action> oneTimeUseOnLoadEvents = new();
    private static readonly List<Action> oneTimeUseOnQuitEvents = new();

    internal static Action OnSaveEvents;
    internal static List<Func<IEnumerator>> OnSaveAsyncEvents = new();
    internal static Action OnFinishLoadingEvents;
    internal static Action OnStartLoadingEvents;
    internal static Action OnQuitEvents;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(IngameMenu), nameof(IngameMenu.SaveGameAsync)),
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

    internal static IEnumerator InvokeSaveEvents(IEnumerator enumerator)
    {
        // Progress the saving function up to the point where a screenshot has been taken and saved to temporary storage.
        // This puts us at a point where the game knows it is saving but has not yet started saving scene objects.
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
            if (enumerator.Current is CoroutineTask<SaveLoadManager.SaveResult>)
                break;
        }
        
        OnSaveEvents?.Invoke();
        foreach (Func<IEnumerator> task in OnSaveAsyncEvents)
        {
            yield return task.Invoke();
        }
            
        if (oneTimeUseOnSaveEvents.Count > 0)
        {
            foreach (Action action in oneTimeUseOnSaveEvents)
            {
                action.Invoke();
            }

            oneTimeUseOnSaveEvents.Clear();
        }

        // Finish the vanilla function.
        yield return enumerator;
    }

    internal static IEnumerator InvokeLoadEvents(IEnumerator enumerator)
    {
        OnStartLoadingEvents.Invoke();

        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
        
        OnLoad();
        
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