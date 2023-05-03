using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace Nautilus.Patchers;

internal static class PrefabDatabasePatcher
{
    private static class PostPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.LoadPrefabDatabase))]
        internal static void LoadPrefabDatabase_Postfix()
        {
            foreach (var prefab in PrefabHandler.Prefabs)
            {
                PrefabDatabase.prefabFiles[prefab.Key.ClassID] = prefab.Key.PrefabFileName;
            }
        }
    }

    [HarmonyPrefix]
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

    [HarmonyPrefix]
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

        yield return PrefabHandler.ProcessPrefabAsync(prefabResult, prefabInfo, prefabFactory);
        GameObject prefab = prefabResult.Get();

        if(prefab != null)
        {
            task.spawnedObject = EditorModifications.Instantiate(prefab, task.parent, task.position, task.rotation, task.instantiateActivated);
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PrefabDatabase), nameof(PrefabDatabase.GetPrefabAsync))]
    internal static bool GetPrefabAsync_Prefix(ref IPrefabRequest __result, string classId)
    {
        __result ??= GetModPrefabAsync(classId);
        return __result == null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AddressablesUtility), nameof(AddressablesUtility.InstantiateAsync), new Type[] 
    { 
        typeof(string), typeof(IOut<GameObject>), typeof(Transform), typeof(Vector3), typeof(Quaternion), typeof(bool)
    })]
    internal static bool InstantiateAsync_Prefix(ref IEnumerator __result,string key, IOut<GameObject> result, Transform parent, Vector3 position, Quaternion rotation, bool awake)
    {
        if (!PrefabHandler.Prefabs.TryGetInfoForFileName(key, out var prefabInfo) && !PrefabHandler.Prefabs.TryGetInfoForClassId(key, out prefabInfo))
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

        yield return PrefabHandler.ProcessPrefabAsync(task, prefabInfo, prefabFactory);

        GameObject prefab = task.Get();
        result.Set(EditorModifications.Instantiate(prefab, parent, position, rotation, awake));
    }

    // calling this instead of InstantiatePrefabAsync in ProtobufSerializer.DeserializeObjectsAsync
    private static IEnumerator InstantiatePrefabAsync_Postfix(IEnumerator enumerator, ProtobufSerializer.GameObjectData gameObjectData, IOut<UniqueIdentifier> result)
    {
        if (GetModPrefabAsync(gameObjectData.ClassId) is { } request)
        {
            yield return request;

            if (request.TryGetPrefab(out GameObject prefab))
            {
                result.Set(UnityEngine.Object.Instantiate(prefab).GetComponent<UniqueIdentifier>());
                yield break;
            }
        }

        yield return enumerator;
    }

    internal static void PrePatch(Harmony harmony)
    {
        harmony.PatchAll(typeof(PrefabDatabasePatcher));
        
        MethodInfo instantiatePrefabAsync = AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.InstantiatePrefabAsync));
        harmony.Patch(instantiatePrefabAsync, postfix: new HarmonyMethod(AccessTools.Method(typeof(PrefabDatabasePatcher), nameof(InstantiatePrefabAsync_Postfix))));

        InternalLogger.Log("PrefabDatabasePatcher is done.", LogLevel.Debug);
    }

    internal static void PostPatch(Harmony harmony)
    {
        harmony.PatchAll(typeof(PostPatches));

        InternalLogger.Log("PrefabDatabasePostPatcher is done.", LogLevel.Debug);
    }
}