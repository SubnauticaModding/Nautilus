namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;

    internal class IngameMenuPatcher
    {
        private static readonly List<Action> oneTimeUseOnSaveEvents = new List<Action>();
        private static readonly List<Action> oneTimeUseOnQuitEvents = new List<Action>();

        internal static Action OnSaveEvents;
        internal static Action OnQuitEvents;

        public static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(IngameMenu), nameof(IngameMenu.SaveGame)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(IngameMenuPatcher), nameof(InvokeSaveEvents))));
            harmony.Patch(AccessTools.Method(typeof(IngameMenu), nameof(IngameMenu.QuitGame)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(IngameMenuPatcher), nameof(InvokeQuitEvents))));
        }

        internal static void AddOneTimeUseSaveEvent(Action onSaveAction)
        {
            oneTimeUseOnSaveEvents.Add(onSaveAction);
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
                    action.Invoke();

                oneTimeUseOnSaveEvents.Clear();
            }
        }

        internal static void InvokeQuitEvents()
        {
            OnQuitEvents?.Invoke();

            if (oneTimeUseOnQuitEvents.Count > 0)
            {
                foreach (Action action in oneTimeUseOnQuitEvents)
                    action.Invoke();

                oneTimeUseOnQuitEvents.Clear();
            }
        }
    }
}
