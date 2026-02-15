using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using Nautilus.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace Nautilus.Utility;

/// <summary>
/// Allows for quick and simple retrieval of any base-game material throughout both Subnautica and Subnautica:
/// Below Zero. Materials can either be fetched directly using their names by accessing the <see cref="FetchMaterial"/>
/// method, or applied generically across the entirety of a custom prefab via the <see cref="ReplaceMockMaterials"/> method.
/// </summary>
public static class MaterialLibrary
{
    /// <summary>
    /// Handles loading the appropriate embedded JSON file, used to generate the Filepath map.
    /// </summary>
    #if SUBNAUTICA
        private static readonly Stream _resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nautilus.Resources.MatFilePathMapSN.json");
    #elif BELOWZERO
        private static readonly Stream _resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nautilus.Resources.MatFilePathMapBZ.json");
    #endif
    
    /// <summary>
    /// The parsed JSON data, mapping material names to the best path to load them from.
    /// </summary>
    private static readonly Dictionary<string, string> _filePathMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(new StreamReader(_resourceStream).ReadToEnd());

    /// <summary>
    /// The maximum amount of time, in seconds, that we should wait for materials to finish being retrieved from a scene
    /// prefab before we time out the process. Similarly to the <see cref="MaxFetchAttempts"/> variable, this property
    /// exists exclusively as a failsafe, to prevent neverending wait-times.
    /// </summary>
    private const int MaxSceneLoadTime = 300;

    /// <summary>
    /// Searches the library for the provided <see cref="materialName"/>, and loads it from its path, if an entry for
    /// it exists.
    /// </summary>
    /// <param name="materialName">The exact name of the material you wish to retrieve as seen in-game. Case-sensitive!</param>
    /// <param name="foundMaterial">The <see cref="TaskResult{Material}"/> to load the vanilla material into, if found. Otherwise, has its value set to null.</param>
    /// <param name="warnIfFailed">Whether Nautilus should log a warning if no materials with the provided <see cref="materialName"/> are found.</param>
    public static IEnumerator FetchMaterial(string materialName, IOut<Material> foundMaterial, bool warnIfFailed=true)
    {
        Material matResult = null;

        string filteredMatName = GeneralExtensions.TrimInstance(materialName);
        string resourcePath = GetMaterialPath(filteredMatName);
        if (!resourcePath.IsNullOrWhiteSpace())
        {
            if (resourcePath.EndsWith(".mat"))
            {
                var taskResult = new TaskResult<Material>();
                yield return GetMaterialFromPath(resourcePath, taskResult);
                
                matResult = taskResult.value;
            }
            else if (resourcePath.EndsWith(".prefab"))
            {
                var taskResult = new TaskResult<Material[]>();
                yield return GetMaterialsFromPrefab([filteredMatName], resourcePath, taskResult);
                
                var returnValue = taskResult.value;
                if(returnValue != null)
                    matResult = returnValue[0];
            }
            else if (resourcePath.StartsWith("LightmappedPrefabs/"))
            {
                var taskResult = new TaskResult<Material[]>();
                yield return GetMaterialsFromScene([filteredMatName], resourcePath.Substring(resourcePath.IndexOf('/') + 1), taskResult);
                
                var returnValue = taskResult.value;
                if(returnValue != null)
                    matResult = returnValue[0];
            }
            else
                InternalLogger.Error($"Invalid path provided for material: \"{filteredMatName}\".");
        }else if (warnIfFailed)
            InternalLogger.Warn($"Failed to find material: \"{filteredMatName}\" in MaterialLibrary.");
        
        foundMaterial.Set(matResult);
    }

    /// <summary>
    /// Searches the library for all of the provided <see cref="materialNames"/>, and loads those materials from their
    /// paths, if entries for them exist. If only some of the given mat names have entries in the library, this method
    /// will return all of the vanilla materials it can, and will log warnings for those that it fails to find, by default.
    /// </summary>
    /// <param name="materialNames">An array of the exact material names that you wish to retrieve, as seen in-game. Case-sensitive!</param>
    /// <param name="foundMaterials">The <see cref="TaskResult{Material}"/> to load the vanilla materials into, if found.
    /// Will contain as many of the requested vanilla materials as can be found within the library.</param>
    /// <param name="warnIfFailed">Whether Nautilus should log a warning if one of the requested
    /// <see cref="materialNames"/> can not be found in the library.</param>
    public static IEnumerator FetchMaterials(string[] materialNames, IOut<Material[]> foundMaterials, bool warnIfFailed=true)
    {
        var matResults = new List<Material>();
        
        var materialsWithPath = new Dictionary<string, string[]>();

        foreach (var matName in materialNames)
        {
            string filteredMatName = GeneralExtensions.TrimInstance(matName);
            string resourcePath = GetMaterialPath(filteredMatName);

            if (!resourcePath.IsNullOrWhiteSpace())
            {
                if (resourcePath.EndsWith(".mat"))
                {
                    var taskResult = new TaskResult<Material>();
                    yield return GetMaterialFromPath(resourcePath, taskResult);
                    
                    matResults.Add(taskResult.value);
                    continue;
                }

                if (materialsWithPath.TryGetValue(resourcePath, out var oldMatList))
                {
                    if (Array.IndexOf(oldMatList, filteredMatName) == -1)
                    {
                        var newMatList = new string[oldMatList.Length + 1];
                        
                        Array.Copy(oldMatList, newMatList, oldMatList.Length);
                        newMatList[oldMatList.Length] = filteredMatName;
                        
                        materialsWithPath[resourcePath] = newMatList;
                    }
                }else
                    materialsWithPath.Add(resourcePath, [filteredMatName]);
            }else if (warnIfFailed)
                InternalLogger.Warn($"Failed to find material: \"{filteredMatName}\" in MaterialLibrary.");
        }

        foreach (var path in materialsWithPath.Keys)
        {
            var pathMaterials = materialsWithPath[path];

            var taskResult = new TaskResult<Material[]>();
            
            if (path.EndsWith(".prefab"))
                yield return GetMaterialsFromPrefab(pathMaterials, path, taskResult);
            else if (path.StartsWith("LightmappedPrefabs/"))
                yield return GetMaterialsFromScene(pathMaterials, path.Substring(path.IndexOf('/') + 1), taskResult);
            else
            {
                InternalLogger.Error($"The path: \"{path}\", associated with material: \"{pathMaterials[0]}\"," +
                                     $" is invalid.");
                continue;
            }

            var foundMats = taskResult.value;
            if (foundMats != null)
                foreach (var material in foundMats)
                    matResults.Add(material);
        }
        
        foundMaterials.Set(matResults.ToArray());
    }

    /// <summary>
    /// Loads and returns a material using its path relative to the <see cref="AddressablesUtility"/>.
    /// </summary>
    /// <param name="matPath">The path to the .mat file, relative to the <see cref="AddressablesUtility"/>.</param>
    /// <param name="matResult">The <see cref="TaskResult{Material}"/> to load the found material into. If no material 
    /// is found, has its value set to null.</param>
    private static IEnumerator GetMaterialFromPath(string matPath, IOut<Material> matResult)
    {
        Material loadedMat = null;

        if(matPath.EndsWith(".mat"))
        {
            var handle = AddressablesUtility.LoadAsync<Material>(matPath);
            yield return handle;
                
            loadedMat = handle.Result;
        }
        else
            InternalLogger.Error($"{matPath} is not a valid path to a material file.");
        
        matResult.Set(loadedMat);
    }

    /// <summary>
    /// Finds and returns vanilla materials by first loading an associated Prefab, given that prefab's path, and the
    /// desired vanilla material names. Iterates over every <see cref="Renderer"/> on the Prefab's parent object, and any of its
    /// children objects, in order to find the materials requested.
    /// </summary>
    /// <param name="materialNames">The names of the materials to search for.</param>
    /// <param name="prefabPath">The path to the reference Prefab, for use in the <see cref="PrefabDatabase"/>.</param>
    /// <param name="matResult">The <see cref="TaskResult{Material}"/> to load the found materials into. Otherwise, has its value set to null.</param>
    private static IEnumerator GetMaterialsFromPrefab(string[] materialNames, string prefabPath, IOut<Material[]> matResult)
    {
        var matResults = new List<Material>();
        
        if (!prefabPath.EndsWith(".prefab"))
        {
            InternalLogger.Error($"{prefabPath} is not a valid path to a prefab file.");
            yield break;
        }

        var task = PrefabDatabase.GetPrefabForFilenameAsync(prefabPath);
        yield return task;

        if (!task.TryGetPrefab(out var prefab))
        {
            InternalLogger.Error($"Failed to get prefab at path {prefabPath} from PrefabDatabase.");
            yield break;
        }

        var foundMats = new List<string>();
        bool allMatsFound = false;
        foreach (var renderer in prefab.GetAllComponentsInChildren<Renderer>())
        {
            if (allMatsFound)
                break;
            
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null)
                    continue;

                string filteredMatName = GeneralExtensions.TrimInstance(material.name);

                if (Array.IndexOf(materialNames, filteredMatName) != -1 && !foundMats.Contains(filteredMatName))
                {
                    matResults.Add(material);
                    foundMats.Add(filteredMatName);

                    if (foundMats.Count == materialNames.Length)
                    {
                        allMatsFound = true;
                        break;
                    }
                }
            }
        }
        
        if (!allMatsFound)
        {
            foreach (var matName in materialNames)
            {
                if (!foundMats.Contains(matName))
                    InternalLogger.Error($"Failed to find material: \"{matName}\" on prefab at path: \"{prefabPath}\".");
            }
        }
        
        matResult.Set(matResults.ToArray());
    }

    /// <summary>
    /// Finds and returns vanilla materials with the specified <see cref="materialNames"/>, by searching through a scene prefab,
    /// loaded using the given <see cref="sceneName"/>. NOTE: This method won't be able to return vanilla materials
    /// until the specified Scene is loaded via the <see cref="ScenePrefabDatabase"/>.
    /// </summary>
    /// <param name="materialNames">The names of the materials to search the scene prefab for.</param>
    /// <param name="sceneName">The name of the additive scene prefab to load and iterate through for the desired material.</param>
    /// <param name="matResults">The <see cref="TaskResult{Material}"/> to load the found material into. Otherwise, has its value set to null.</param>
    private static IEnumerator GetMaterialsFromScene(string[] materialNames, string sceneName, IOut<Material[]> matResults)
    {
        var foundMaterials = new List<Material>();

        if (!AddressablesUtility.IsAddressableScene(sceneName))
        {
            InternalLogger.Error($"Attempted to get a material from invalid scene: {sceneName}");
            yield break;
        }
        
        yield return new WaitUntil(() => LightmappedPrefabs.main);

        var loadStartTime = Time.time;

        bool materialsFound = false;
        bool matCheckFailed = false;
        LightmappedPrefabs.main.RequestScenePrefab(sceneName, scenePrefab =>
        {
            var foundMats = new List<string>();
            foreach (var renderer in scenePrefab.GetAllComponentsInChildren<Renderer>())
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null)
                        continue;
                    
                    string filteredMatName = GeneralExtensions.TrimInstance(material.name);

                    if (Array.IndexOf(materialNames, filteredMatName) != -1 && !foundMats.Contains(filteredMatName))
                    {
                        foundMaterials.Add(material);
                        foundMats.Add(filteredMatName);

                        if (foundMats.Count == materialNames.Length)
                        {
                            matResults.Set(foundMaterials.ToArray());
                            materialsFound = true;
                            return;
                        }
                    }
                }
            }

            foreach (var matName in materialNames)
            {
                if (!foundMats.Contains(matName))
                    InternalLogger.Error($"Failed to find material: \"{matName}\" in scene: \"{sceneName}\".");
            }
            
            matResults.Set(foundMaterials.ToArray());
            
            matCheckFailed = true;
        });
        
        yield return new WaitUntil(() => materialsFound || matCheckFailed || Time.time > loadStartTime + MaxSceneLoadTime);
    }

    /// <summary>
    /// Uses the <see cref="_filePathMap"/> to retrieve the path associated with a specified material using the
    /// <see cref="materialName"/>, so that it may be loaded when requested.
    /// </summary>
    /// <param name="materialName">The name of the material whose path is being requested.</param>
    /// <returns>The path to the resource which should be loaded in order to retrieve the specified material.
    /// Points to either a mat, prefab, or scene prefab file. Returns an empty string if the provided
    /// <see cref="materialName"/> does not have an entry within the library.</returns>
    public static string GetMaterialPath(string materialName)
    {
        if (_filePathMap.TryGetValue(materialName, out string path))
            return path;

        return "";
    }
    
}