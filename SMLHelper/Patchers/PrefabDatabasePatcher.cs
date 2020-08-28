namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Collections;
    using System.Collections.Generic;

    using Assets;
    using HarmonyLib;
    using UnityEngine;
    using UWE;
    using Logger = V2.Logger;

    internal static class PrefabDatabasePatcher
    {
        private static class PostPatches
        {
            [PatchUtils.Postfix]
            [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.LoadPrefabDatabase))]
            internal static void LoadPrefabDatabase_Postfix()
            {
                foreach (ModPrefab prefab in ModPrefab.Prefabs)
                {
                    PrefabDatabase.prefabFiles[prefab.ClassID] = prefab.PrefabFileName;
                }

                var tryGetPrefabFilename = AccessTools.Method(typeof(PrefabDatabase), nameof(PrefabDatabase.TryGetPrefabFilename));
                Initializer.harmony.Unpatch(tryGetPrefabFilename, HarmonyPatchType.Prefix, Initializer.harmony.Id);
            }
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.TryGetPrefabFilename))]
        internal static bool TryGetPrefabFilename_Prefix(string classId, ref string filename, ref bool __result)
        {
            if (!ModPrefab.TryGetFromClassId(classId, out ModPrefab prefab))
                return true;

            filename = prefab.PrefabFileName;
            __result = true;
            return false;
        }

        [PatchUtils.Prefix] // SUBNAUTICA_EXP TODO: remove for SN after async update
        [HarmonyPatch(typeof(PrefabDatabase), "GetPrefabForFilename")] // method can be absent
        internal static bool GetPrefabForFilename_Prefix(string filename, ref GameObject __result)
        {
            if (!ModPrefab.TryGetFromFileName(filename, out ModPrefab prefab))
                return true;

            __result = prefab.GetGameObjectInternal();
            return false;
        }

        private static IPrefabRequest GetModPrefabAsync(string classId)
        {
            if (!ModPrefab.TryGetFromClassId(classId, out ModPrefab prefab))
                return null;

            try
            {
                // trying sync method for backward compatibility
                if (prefab.GetGameObjectInternal() is GameObject go) // SUBNAUTICA_EXP TODO: remove for SN after async update
                    return new LoadedPrefabRequest(go);
            }
            catch (Exception) {}

            return new ModPrefabRequest(prefab);
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.GetPrefabAsync))]
        internal static bool GetPrefabAsync_Prefix(ref IPrefabRequest __result, string classId)
        {
            __result ??= GetModPrefabAsync(classId);
            return __result == null;
        }


        // transpiler for ProtobufSerializer.DeserializeObjectsAsync
        private static IEnumerable<CodeInstruction> DeserializeObjectsAsync_Transpiler(IEnumerable<CodeInstruction> cins)
        {
            var originalMethod = AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.InstantiatePrefabAsync));

            return new CodeMatcher(cins).
                MatchForward(false, new CodeMatch(OpCodes.Call, originalMethod)).
                SetOperandAndAdvance(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(_InstantiatePrefabAsync))).
                InstructionEnumeration();
        }

        // calling this instead of InstantiatePrefabAsync in ProtobufSerializer.DeserializeObjectsAsync
        private static IEnumerator _InstantiatePrefabAsync(ProtobufSerializer.GameObjectData gameObjectData, IOut<UniqueIdentifier> result)
        {
            if (GetModPrefabAsync(gameObjectData.ClassId) is IPrefabRequest request)
            {
                yield return request;

                if (request.TryGetPrefab(out GameObject prefab))
                {
                    result.Set(UnityEngine.Object.Instantiate(prefab).GetComponent<UniqueIdentifier>());
                    yield break;
                }
            }

            yield return ProtobufSerializer.InstantiatePrefabAsync(gameObjectData, result);
        }


        internal static void PrePatch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony);

            var obsoleteInstantiatePrefabAsync = AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.InstantiatePrefabAsync),
                new[] { typeof(ProtobufSerializer.GameObjectData) });

            if (obsoleteInstantiatePrefabAsync == null) // it means that we have async-only prefabs now, otherwise patch will fail
            {
                // patching iterator method ProtobufSerializer.DeserializeObjectsAsync
                MethodInfo DeserializeObjectsAsync = typeof(ProtobufSerializer).GetMethod(
                    nameof(ProtobufSerializer.DeserializeObjectsAsync), BindingFlags.NonPublic | BindingFlags.Instance);
                harmony.Patch(PatchUtils.GetIteratorMethod(DeserializeObjectsAsync), transpiler:
                    new HarmonyMethod(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(DeserializeObjectsAsync_Transpiler))));
            }

            Logger.Log("PrefabDatabasePatcher is done.", LogLevel.Debug);
        }

        internal static void PostPatch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony, typeof(PostPatches));

            Logger.Log("PrefabDatabasePostPatcher is done.", LogLevel.Debug);
        }
    }
}
