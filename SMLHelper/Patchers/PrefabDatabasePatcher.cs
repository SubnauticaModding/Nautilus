namespace SMLHelper.V2.Patchers
{
    using Assets;
    using HarmonyLib;
    using System.Reflection;
    using UnityEngine;
    using UWE;
    using Logger = V2.Logger;

    internal class PrefabDatabasePatcher
    {
        private static MethodInfo tryGetPrefabFilename = AccessTools.Method(typeof(PrefabDatabase), nameof(PrefabDatabase.TryGetPrefabFilename));

        internal static void LoadPrefabDatabase_Postfix()
        {
            foreach (ModPrefab prefab in ModPrefab.Prefabs)
            {
                PrefabDatabase.prefabFiles[prefab.ClassID] = prefab.PrefabFileName;
            }

            Initializer.harmony.Unpatch(tryGetPrefabFilename, HarmonyPatchType.Prefix, Initializer.harmony.Id);
        }

        internal static bool GetPrefabForFilename_Prefix(string filename, ref GameObject __result)
        {
            if (ModPrefab.TryGetFromFileName(filename, out ModPrefab prefab))
            {
                GameObject go = prefab.GetGameObjectInternal();
                __result = go;

                return false;
            }

            return true;
        }

        internal static bool TryGetPrefabFilename_Prefix(string classId, ref string filename, ref bool __result)
        {
            if (ModPrefab.TryGetFromClassId(classId, out ModPrefab prefab))
            {
                filename = prefab.PrefabFileName;
                __result = true;
                return false;
            }
            return true;
        }

        internal static bool GetPrefabAsync_Prefix(ref IPrefabRequest __result, string classId)
        {
            if (ModPrefab.TryGetFromClassId(classId, out ModPrefab prefab))
            { 
                GameObject go = prefab.GetGameObjectInternal();
                __result = new LoadedPrefabRequest(go);

                return false;
            }

            return true;
        }

        internal static void PrePatch(Harmony harmony)
        {
#if !SUBNAUTICA_EXP // TODO
            harmony.Patch(AccessTools.Method(typeof(PrefabDatabase), nameof(PrefabDatabase.GetPrefabForFilename)), 
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(PrefabDatabasePatcher.GetPrefabForFilename_Prefix))));
#endif
            harmony.Patch(tryGetPrefabFilename, prefix: new HarmonyMethod(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(PrefabDatabasePatcher.TryGetPrefabFilename_Prefix))));

            harmony.Patch(AccessTools.Method(typeof(PrefabDatabase), nameof(PrefabDatabase.GetPrefabAsync)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(PrefabDatabasePatcher.GetPrefabAsync_Prefix))));

            Logger.Log("PrefabDatabasePatcher is done.", LogLevel.Debug);
        }

        internal static void PostPatch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PrefabDatabase), nameof(PrefabDatabase.LoadPrefabDatabase)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(PrefabDatabasePatcher.LoadPrefabDatabase_Postfix))));


            Logger.Log("PrefabDatabasePostPatcher is done.", LogLevel.Debug);
        }
    }
}
