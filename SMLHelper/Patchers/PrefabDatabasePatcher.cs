using SMLHelper.Handlers;

namespace SMLHelper.Patchers
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
    using Utility;
    using BepInEx.Logging;

    internal static class PrefabDatabasePatcher
    {
        private static class PostPatches
        {
            [PatchUtils.Postfix]
            [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.LoadPrefabDatabase))]
            internal static void LoadPrefabDatabase_Postfix()
            {
                foreach (var prefab in PrefabHandler.Prefabs)
                {
                    PrefabDatabase.prefabFiles[prefab.Key.ClassID] = prefab.Key.PrefabFileName;
                }
            }
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.TryGetPrefabFilename))]
        internal static bool TryGetPrefabFilename_Prefix(string classId, ref string filename, ref bool __result)
        {
            if (!PrefabHandler.Prefabs.TryGetInfoForClassId(classId, out PrefabInfo prefabInfo))
            {
                return true;
            }

            filename = prefabInfo.PrefabFileName;
            __result = true;
            return false;
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(DeferredSpawner.AddressablesTask), nameof(DeferredSpawner.AddressablesTask.SpawnAsync))]
        internal static bool DeferredSpawner_AddressablesTask_Spawn_Prefix(DeferredSpawner.AddressablesTask __instance, ref IEnumerator __result)
        {
            if (!PrefabHandler.Prefabs.TryGetInfoForFileName(__instance.key, out PrefabInfo prefabInfo))
            {
                return true;
            }

            __result = SpawnAsyncReplacement(__instance, prefabInfo);
            return false;
        }

        internal static IEnumerator SpawnAsyncReplacement(DeferredSpawner.AddressablesTask task, PrefabInfo prefabInfo)
        {
            TaskResult<GameObject> prefabResult = new();
            if (!PrefabHandler.Prefabs.TryGetPrefabForInfo(prefabInfo, out var prefabFactory))
            {
                InternalLogger.Error($"Couldn't find a prefab factory for the following prefab info: {prefabInfo}");
                yield break;
            }
            
            yield return prefabFactory.Invoke(prefabResult);
            GameObject prefab = prefabResult.Get();

            if(prefab != null)
            {
                task.spawnedObject = UnityEngine.Object.Instantiate(prefab, task.parent, task.position, task.rotation, task.instantiateActivated);
            }

            if (task.spawnedObject == null)
            {
                task.forceCancelled = true;
            }

            task.HandleLateCancelledSpawn();
        }

        private static IPrefabRequest GetModPrefabAsync(string classId)
        {
            if (!PrefabHandler.Prefabs.TryGetInfoForClassId(classId, out PrefabInfo prefabInfo))
            {
                return null;
            }

            return new ModPrefabRequest(prefabInfo);
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.GetPrefabAsync))]
        internal static bool GetPrefabAsync_Prefix(ref IPrefabRequest __result, string classId)
        {
            __result ??= GetModPrefabAsync(classId);
            return __result == null;
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(AddressablesUtility), nameof(AddressablesUtility.InstantiateAsync), new Type[] 
        { 
            typeof(string), typeof(IOut<GameObject>), typeof(Transform), typeof(Vector3), typeof(Quaternion), typeof(bool)
        })]
        internal static bool InstantiateAsync_Prefix(ref IEnumerator __result,string key, IOut<GameObject> result, Transform parent, Vector3 position, Quaternion rotation, bool awake)
        {
            if(!PrefabHandler.Prefabs.TryGetInfoForFileName(key, out PrefabInfo prefabInfo))
            {
                return true;
            }

            __result = InstantiateAsync(prefabInfo, result, parent, position, rotation, awake);
            return false;
        }

        internal static IEnumerator InstantiateAsync(PrefabInfo prefabInfo, IOut<GameObject> result, Transform parent, Vector3 position, Quaternion rotation, bool awake)
        {
            TaskResult<GameObject> task = new();
            if (!PrefabHandler.Prefabs.TryGetPrefabForInfo(prefabInfo, out var prefabFactory))
            {
                InternalLogger.Error($"Couldn't find a prefab factory for the following prefab info: {prefabInfo}");
                yield break;
            }
            
            yield return prefabFactory.Invoke(task);

            GameObject prefab = task.Get();
            result.Set(GameObject.Instantiate(prefab, parent, position, rotation, awake));
        }

        // transpiler for ProtobufSerializer.DeserializeObjectsAsync
        private static IEnumerable<CodeInstruction> DeserializeObjectsAsync_Transpiler(IEnumerable<CodeInstruction> cins)
        {
            MethodInfo originalMethod = AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.InstantiatePrefabAsync));

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

                // patching iterator method ProtobufSerializer.DeserializeObjectsAsync
                MethodInfo DeserializeObjectsAsync = typeof(ProtobufSerializer).GetMethod(
                    nameof(ProtobufSerializer.DeserializeObjectsAsync), BindingFlags.NonPublic | BindingFlags.Instance);
                harmony.Patch(PatchUtils.GetIteratorMethod(DeserializeObjectsAsync), transpiler:
                    new HarmonyMethod(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(DeserializeObjectsAsync_Transpiler))));

            InternalLogger.Log("PrefabDatabasePatcher is done.", LogLevel.Debug);
        }

        internal static void PostPatch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony, typeof(PostPatches));

            InternalLogger.Log("PrefabDatabasePostPatcher is done.", LogLevel.Debug);
        }
    }
}
