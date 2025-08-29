using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UWE;
using ResourceManager = System.Resources.ResourceManager;

namespace Nautilus.Utility;

/// <summary>
/// Allows for quick and simple retrieval of any base-game material throughout both Subnautica, and Subnautica:
/// Below Zero. Materials can either be fetched directly using their names by accessing the <see cref="FetchMaterial"/>
/// method, or applied generically across the entirety of a custom prefab via the <see cref="ReplaceVanillaMats"/> method.
/// </summary>
public static class MaterialLibrary
{
    /// <summary>
    /// Handles loading the material filepath maps from the embedded .resources files, and accessing their contents.
    /// </summary>
    private static ResourceManager _resourceManager;
    
    /// <summary>
    /// The maximum number of times the <see cref="FetchMaterial"/> method is allowed to retry loading a material
    /// from it's designated path. Necessary because .mat files will occasionally fail to load a couple of times before
    /// being successfully retrieved. This cap exists only as a failsafe, and should never actually be reached.
    /// </summary>
    private const int MaxFetchAttempts = 1000;

    /// <summary>
    /// The current amount of entries within the MaterialLibrary.
    /// </summary>
    public static int Size
    {
        get
        {
            if (_resourceManager == null)
                return 0;
            
            var resourceSet = _resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);

            if (resourceSet == null)
            {
                InternalLogger.Error("Failed to get the ResourceSet of the material library.");
                return 0;
            }

            //Sadly, this is actually the simplest way to get the total number of entries in a .resources file.
            int materialEntries = 0;
            foreach (var _ in resourceSet)
                materialEntries++;
            
            return materialEntries;
        }
    }
    
    internal static void Patch()
    {
        #if SUBNAUTICA
                string resourcePath = "Nautilus.Resources.MatFilePathMapSN";
        #elif BELOWZERO
                string resourcePath = "Nautilus.Resources.MatFilePathMapBZ";
        #endif
        
        _resourceManager = new ResourceManager(resourcePath, Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Iterates over every <see cref="Renderer"/> present on the given <see cref="customPrefab"/> and any of its
    /// children, and replaces any custom materials it finds that share the exact same name (case-sensitive) of a
    /// base-game material with its vanilla counterpart.
    /// </summary>
    /// <param name="customPrefab">The custom object you'd like to apply base-game materials to. Children included.</param>
    /// <returns></returns>
    public static IEnumerator ReplaceVanillaMats(GameObject customPrefab)
    {
        if (customPrefab == null)
        {
            InternalLogger.Error("Attempted to apply vanilla materials to a null prefab.");
            yield break;
        }
        
        var loadedVanillaMats = new List<Material>();
        var customMatNames = new List<string>();
        foreach (var renderer in customPrefab.GetAllComponentsInChildren<Renderer>())
        {
            var newMatList = renderer.materials;

            for (int i = 0; i < newMatList.Length; i++)
            {
                if (newMatList[i] == null)
                    continue;

                var currentMatName = MaterialUtils.RemoveInstanceFromMatName(newMatList[i].name);
                
                bool skipMat = customMatNames.Contains(currentMatName);
                if (!skipMat)
                {
                    foreach (var material in loadedVanillaMats)
                    {
                        if (MaterialUtils.RemoveInstanceFromMatName(material.name).Equals(currentMatName))
                        {
                            newMatList[i] = material;
                            skipMat = true;
                            break;
                        }
                    }
                }

                if (skipMat)
                    continue;

                var taskResult = new TaskResult<Material>();
                yield return FetchMaterial(currentMatName, taskResult);

                var foundMaterial = taskResult.value;

                if (foundMaterial == null)
                {
                    customMatNames.Add(currentMatName);
                    continue;
                }

                newMatList[i] = foundMaterial;
                loadedVanillaMats.Add(foundMaterial);
            }
            
            renderer.materials = newMatList;
        }
    }

    /// <summary>
    /// Searches the library for the provided <see cref="materialName"/>, and loads it from its path, if an entry for
    /// it exists. If the material exists within the library, but fails to load, fetching the asset will be reattempted
    /// until the <see cref="MaxFetchAttempts"/> limit is reached.
    /// </summary>
    /// <param name="materialName">The exact name of the material you wish to retrieve as seen in-game. Case-sensitive!</param>
    /// <param name="foundMaterial">The <see cref="TaskResult{Material}"/> to load the found material into. Otherwise, has it's value set to null.</param>
    /// <returns></returns>
    public static IEnumerator FetchMaterial(string materialName, IOut<Material> foundMaterial)
    {
        Material matResult = null;

        string filteredMatName = MaterialUtils.RemoveInstanceFromMatName(materialName);
        string resourcePath = GetPathToMaterial(filteredMatName);
        if (!resourcePath.IsNullOrWhiteSpace())
        {
            int fetchAttempts = 0;
            do
            {
                if (fetchAttempts >= MaxFetchAttempts)
                {
                    InternalLogger.Error($"Max retries limit reached when trying to fetch material: {materialName}.");
                    InternalLogger.Error("Please ensure the material's path is valid, or up the maximum # of retries.");
                    yield break;
                }
                
                fetchAttempts++;
                InternalLogger.Debug($"Attempting to grab material: {materialName}...");
                
                var taskResult = new TaskResult<Material>();
                
                if (resourcePath.EndsWith(".mat"))
                    yield return GetMaterialFromPath(resourcePath, taskResult);
                else if (resourcePath.EndsWith(".prefab"))
                    yield return GetMaterialFromPrefab(filteredMatName, resourcePath, taskResult);
                else if (resourcePath.StartsWith("LightmappedPrefabs/"))
                    yield return GetMaterialFromScene(filteredMatName, resourcePath.Substring(resourcePath.IndexOf('/') + 1), taskResult);
                else
                {
                    InternalLogger.Error($"Invalid path provided for material: {filteredMatName}");
                    break;
                }

                matResult = taskResult.value;
            } while (matResult == null);
        }
        
        foundMaterial.Set(matResult);
    }

    /// <summary>
    /// Loads and returns a material using its path relative to the <see cref="AddressablesUtility"/>.
    /// NOTE: The provided .mat file will occasionally fail to load, resulting in <see cref="matResult"/>'s value
    /// being null after this method is finished running. It is not currently known what causes this, but it
    /// does not happen constantly, and will only occur a handful of times in a row, if it does at all. As such,
    /// the best solution for this problem, for the time being, is simply to try calling this method again, until
    /// a successful result is retrieved.
    /// </summary>
    /// <param name="matPath">The path to the .mat file, relative to the <see cref="AddressablesUtility"/>.</param>
    /// <param name="matResult">The <see cref="TaskResult{Material}"/> to load the found material into. Otherwise, has it's value set to null.</param>
    /// <returns></returns>
    private static IEnumerator GetMaterialFromPath(string matPath, IOut<Material> matResult)
    {
        matResult.Set(null);

        if (!matPath.EndsWith(".mat"))
        {
            InternalLogger.Error($"{matPath} is not a valid path to a material file.");
            yield break;
        }

        var handle = AddressablesUtility.LoadAsync<Material>(matPath);

        yield return handle.Task;
        
        matResult.Set(handle.Result);
    }

    /// <summary>
    /// Finds and returns a material by first loading an associated Prefab, given that prefab's path, and the desired
    /// material's name. Iterates over every <see cref="Renderer"/> on the Prefab's parent object, and any of its
    /// children objects, in order to find the material requested.
    /// </summary>
    /// <param name="matName">The name of the material to search for.</param>
    /// <param name="prefabPath">The path to the reference Prefab, for use in the <see cref="PrefabDatabase"/>.</param>
    /// <param name="matResult">The <see cref="TaskResult{Material}"/> to load the found material into. Otherwise, has it's value set to null.</param>
    /// <returns></returns>
    private static IEnumerator GetMaterialFromPrefab(string matName, string prefabPath, IOut<Material> matResult)
    {
        matResult.Set(null);

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
        
        foreach (var renderer in prefab.GetAllComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                if (material == null)
                    continue;

                if (MaterialUtils.RemoveInstanceFromMatName(material.name).Equals(matName))
                {
                    matResult.Set(material);
                    yield break;
                }
            }
        }
            
        InternalLogger.Error($"Failed to find material: {matName} on prefab at path: {prefabPath}");
    }

    /// <summary>
    /// Finds and returns a material with the specified <see cref="matName"/>, by searching through a scene prefab,
    /// loaded using the given <see cref="sceneName"/>. NOTE: This method won't be able to provide a material result
    /// until the specified Scene is loaded via the <see cref="ScenePrefabDatabase"/>.
    /// </summary>
    /// <param name="matName">The name of the material to search the scene prefab for.</param>
    /// <param name="sceneName">The name of the additive scene prefab to load and iterate through for the desired material.</param>
    /// <param name="matResult">The <see cref="TaskResult{Material}"/> to load the found material into. Otherwise, has it's value set to null.</param>
    /// <returns></returns>
    private static IEnumerator GetMaterialFromScene(string matName, string sceneName, IOut<Material> matResult)
    {
        matResult.Set(null);

        if (!AddressablesUtility.IsAddressableScene(sceneName))
        {
            InternalLogger.Error($"Attempted to get a material from invalid scene: {sceneName}");
            yield break;
        }
        
        yield return new WaitUntil(() => LightmappedPrefabs.main);

        bool materialSet = false;
        bool matCheckFailed = false;
        LightmappedPrefabs.main.RequestScenePrefab(sceneName, scenePrefab =>
        {
            foreach (var renderer in scenePrefab.GetAllComponentsInChildren<Renderer>())
            {
                foreach (var material in renderer.materials)
                {
                    if (material == null)
                        continue;

                    if (MaterialUtils.RemoveInstanceFromMatName(material.name).Equals(matName))
                    {
                        matResult.Set(material);
                        materialSet = true;
                        return;
                    }
                }
            }
            
            matCheckFailed = true;
        });
        
        yield return new WaitUntil(() => materialSet || matCheckFailed);
    }

    /// <summary>
    /// Uses the <see cref="_resourceManager"/> to access the MatFilePathMaps, and retrieve the path associated with
    /// a specified material using the <see cref="materialName"/>, so that it may be loaded when requested.
    /// </summary>
    /// <param name="materialName">The name of the material whose path is being requested.</param>
    /// <returns>The path to the resource which should be loaded in order to retrieve the specified material.
    /// Points to either a mat, prefab, or scene prefab file. Returns an empty string if the provided
    /// <see cref="materialName"/> does not have an entry within the library.</returns>
    private static string GetPathToMaterial(string materialName)
    {
        if (_resourceManager == null)
        {
            InternalLogger.Error("Tried to get material path from library while ResourceManager is null. Please initialize first!");
            return String.Empty;
        }
        
        return _resourceManager.GetString(ConvertNameToKey(materialName));
    }

    /// <summary>
    /// Converts the name of a material to the MatFilePathMap key equivalent. This approach is necessary because files
    /// with the .resources extension do not allow entries with duplicate text to exist, even if the text entries
    /// have different capitalization from one another. The MatFilePathMap contains many entries with the same name,
    /// however, and the only way to differentiate them from one another is by preserving their casing. To get around
    /// this issue, keys within the MatFilePathMap are made to be lowercased versions of the original mat name, with
    /// the number of lowercase and uppercase characters in the original name preserved by being appended to the end
    /// of the key version of the name. I.e. RawTitanium -> rawtitanium_lc9_uc2
    /// </summary>
    /// <param name="matName">The name of the base-game material being searched for.</param>
    /// <returns>The MatFilePathMap key version of the given material name.</returns>
    private static string ConvertNameToKey(string matName)
    {
        var characters = matName.ToCharArray();

        int upperCaseLetters = 0;
        int lowerCaseLetters = 0;
        for (int i = 0; i < characters.Length; i++)
        {
            if (char.IsUpper(characters[i]))
                upperCaseLetters++;
            else
                lowerCaseLetters++;
        }

        return matName.ToLower() + "_lc" + lowerCaseLetters + "_uc" + upperCaseLetters;
    }
    
}