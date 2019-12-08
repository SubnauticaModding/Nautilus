namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Collections.Generic;
    using Harmony;

    internal class IngameMenuPatcher
    {
        private static readonly List<Action> oneTimeUseOnSaveEvents = new List<Action>();

        internal static Action OnSaveEvents;

        public static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(IngameMenu), nameof(IngameMenu.SaveGame)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(IngameMenuPatcher), nameof(InvokeEvents))));
        }

        internal static void AddOneTimeUseSaveEvent(Action onSaveAction)
        {
            oneTimeUseOnSaveEvents.Add(onSaveAction);
        }

        internal static void InvokeEvents()
        {
            OnSaveEvents?.Invoke();

            if (oneTimeUseOnSaveEvents.Count > 0)
            {
                foreach (Action action in oneTimeUseOnSaveEvents)
                    action.Invoke();

                oneTimeUseOnSaveEvents.Clear();
            }
        }
    }
}
