using System;
using System.Collections;
using System.Collections.Generic;
using SMLHelper.Assets;
using SMLHelper.Patchers;
using SMLHelper.Utility;
using UnityEngine;

namespace SMLHelper.Handlers;

/// <summary>
/// A handler for registering prefabs into the game.
/// </summary>
public static class PrefabHandler
{
    /// <summary>
    /// A collection of custom prefabs to add to the game.
    /// </summary>
    public static PrefabCollection Prefabs { get; } = new();
}

/// <summary>
/// Represents extension methods for the <see cref="PrefabCollection"/> class.
/// </summary>
public static class PrefabCollectionExtensions
{
    /// <summary>
    /// Registers a <see cref="CustomPrefab"/> into the game.
    /// </summary>
    /// <param name="collection">The collection to register to.</param>
    /// <param name="customPrefab">The custom prefab to register.</param>
    public static void RegisterPrefab(this PrefabCollection collection, CustomPrefab customPrefab)
    {
        collection.Add(customPrefab.Info, customPrefab.Prefab);
    }
}

/// <summary>
/// Represents a collection of <see cref="PrefabInfo"/> as keys and prefab factory as values.
/// </summary>
public class PrefabCollection : IEnumerable<KeyValuePair<PrefabInfo, Func<TaskResult<GameObject>, IEnumerator>>>
{
    private readonly Dictionary<PrefabInfo, Func<TaskResult<GameObject>, IEnumerator>> _prefabs = new();
    
    private readonly Dictionary<string, PrefabInfo> _classIdPrefabs = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, PrefabInfo> _fileNamePrefabs = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, PrefabInfo> _techTypePrefabs = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Adds a prefab info with the function that constructs the game object into the game.
    /// </summary>
    /// <param name="info">The prefab info to register.</param>
    /// <param name="prefabFactory">The function that constructs the game object for this prefab info.</param>
    public void Add(PrefabInfo info, Func<TaskResult<GameObject>, IEnumerator> prefabFactory)
    {
        if (_prefabs.ContainsKey(info) || info.TechType == TechType.None)
        {
            InternalLogger.Error($"Another modded prefab already registered the following prefab: {info}");
            return;
        }
        
        _prefabs.Add(info, prefabFactory);
        _classIdPrefabs.Add(info.ClassID, info);
        _fileNamePrefabs.Add(info.PrefabFileName, info);
        _techTypePrefabs.Add(info.TechType.AsString(), info);
        CraftDataPatcher.ModPrefabsPatched = false;
    }

    /// <summary>
    /// Determines whether the provided prefab info is registered.
    /// </summary>
    /// <param name="info">The prefab info to look for</param>
    /// <returns>true if found; otherwise false.</returns>
    public bool ContainsPrefabInfo(PrefabInfo info)
    {
        return _prefabs.ContainsKey(info);
    }

    /// <summary>
    /// Gets the prefab factory associated with the provided info.
    /// </summary>
    /// <param name="info">The info of the prefab factory to get.</param>
    /// <param name="prefabFactory">The returned prefab factory. If nothing was found for the prefab info specified, this will be set to the default initialization instead.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetPrefabForInfo(PrefabInfo info, out Func<TaskResult<GameObject>, IEnumerator> prefabFactory)
    {
        return _prefabs.TryGetValue(info, out prefabFactory);
    }

    /// <summary>
    /// Gets the prefab info associated with the provided class ID.
    /// </summary>
    /// <param name="classId">The class ID of the prefab info to get.</param>
    /// <param name="info">The returned prefab info. If nothing was found for the class ID specified, this will be set to the default initialization instead.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetInfoForClassId(string classId, out PrefabInfo info)
    {
        if (string.IsNullOrEmpty(classId))
        {
            info = default;
            return false;
        }

        return _classIdPrefabs.TryGetValue(classId, out info);
    }
    
    /// <summary>
    /// Gets the prefab info associated with the provided file name.
    /// </summary>
    /// <param name="fileName">The file name of the prefab info to get.</param>
    /// <param name="info">The returned prefab info. If nothing was found for the file name specified, this will be set to the default initialization instead.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetInfoForFileName(string fileName, out PrefabInfo info)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            info = default;
            return false;
        }

        return _fileNamePrefabs.TryGetValue(fileName, out info);
    }
    
    /// <summary>
    /// Gets the prefab info associated with the provided tech type.
    /// </summary>
    /// <param name="techType">The tech type of the prefab info to get.</param>
    /// <param name="info">The returned prefab info. If nothing was found for the tech type specified, this will be set to the default initialization instead.</param>
    /// <returns>True if found; otherwise false.</returns>
    public bool TryGetInfoForTechType(string techType, out PrefabInfo info)
    {
        if (string.IsNullOrEmpty(techType))
        {
            info = default;
            return false;
        }

        return _techTypePrefabs.TryGetValue(techType, out info);
    }

    IEnumerator<KeyValuePair<PrefabInfo, Func<TaskResult<GameObject>, IEnumerator>>> IEnumerable<KeyValuePair<PrefabInfo, Func<TaskResult<GameObject>, IEnumerator>>>.GetEnumerator()
    {
        return _prefabs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _prefabs.GetEnumerator();
    }
}
