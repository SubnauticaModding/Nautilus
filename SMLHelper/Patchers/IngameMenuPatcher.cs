namespace SMLHelper.V2.Patchers
{
    using System;
    using Harmony;

    internal class IngameMenuPatcher
    {
        internal static Action OnSaveEvents;

        public static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(IngameMenu), nameof(IngameMenu.SaveGame)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(IngameMenuPatcher), nameof(InvokeEvents))));
        }

        internal static void InvokeEvents()
        {
            OnSaveEvents?.Invoke();
        }
    }
}
