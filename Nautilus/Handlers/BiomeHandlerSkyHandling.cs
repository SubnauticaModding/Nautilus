using System;
using mset;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Handlers;

public static partial class BiomeHandler
{
    /// <summary>
    /// Defines a reference to a new or existing Sky prefab.
    /// </summary>
    public sealed class SkyReference
    {
        private readonly ReferenceType _referenceType;
        private readonly Func<GameObject> _prefabCallback;
        private GameObject _cachedSkyPrefab;
        
        private readonly string _existingSkyPrefabNameToLookUp;
        private readonly string _newSkyName;
        private readonly Action<Sky> _modifyCallback;

        /// <summary>
        /// Defines a reference to a new or existing Sky prefab.
        /// </summary>
        /// <param name="obj">The GameObject containing the <see cref="mset.Sky"/> component.</param>
        public SkyReference(GameObject obj)
        {
            _cachedSkyPrefab = obj;
            _referenceType = ReferenceType.GameObjectReference;
        }

        /// <summary>
        /// Defines a reference to a base-game Sky prefab.
        /// </summary>
        /// <param name="existingSkyPrefabName"><para>The name of the Sky prefab from the list of base-game Skies, i.e. "SkySafeShallows".</para>
        /// <para>A list of valid inputs can be found on this page: https://subnauticamodding.github.io/Nautilus/tutorials/biome-setting-references.html</para></param>
        public SkyReference(string existingSkyPrefabName)
        {
            _existingSkyPrefabNameToLookUp = existingSkyPrefabName;
            _referenceType = ReferenceType.StringLookup;
        }
        
        /// <summary>
        /// Defines a reference to a Sky prefab that is created on demand at runtime.
        /// </summary>
        /// <param name="prefabCallback">A deferred callback to retrieve a GameObject containing the <see cref="mset.Sky"/> component.</param>
        /// <remarks>The object returned by the callback will not be cached.</remarks>
        public SkyReference(Func<GameObject> prefabCallback)
        {
            _prefabCallback = prefabCallback;
            _referenceType = ReferenceType.GetPrefabFromCallback;
        }
        
        /// <summary>
        /// Defines a reference to a new custom Sky prefab that is based on an existing one.
        /// </summary>
        /// <param name="existingSkyPrefabName"><para>The name of the Sky prefab from the list of base-game Skies, i.e. "SkySafeShallows".</para>
        /// <para>A list of valid inputs can be found on this page: https://subnauticamodding.github.io/Nautilus/tutorials/biome-setting-references.html</para></param>
        /// <param name="newSkyName">The name of the new Sky object.</param>
        /// <param name="modify">A callback that is called to allow for modifying the Sky.</param>
        public SkyReference(string existingSkyPrefabName, string newSkyName, Action<Sky> modify)
        {
            _existingSkyPrefabNameToLookUp = existingSkyPrefabName;
            _newSkyName = newSkyName;
            _modifyCallback = modify;
            
            _referenceType = ReferenceType.ClonedSky;
        }
        
        internal GameObject GetSkyPrefabAtRuntime(WaterBiomeManager waterBiomeManager)
        {
            if (_referenceType == ReferenceType.ClonedSky && _cachedSkyPrefab != null)
            {
                return _cachedSkyPrefab;
            }
            
            if (_referenceType is ReferenceType.StringLookup or ReferenceType.ClonedSky)
            {
                var skyPrefab = GetExistingSkyPrefab(waterBiomeManager);
                
                if (_referenceType == ReferenceType.ClonedSky && skyPrefab != null)
                {
                    skyPrefab = CloneSkyPrefab(skyPrefab);
                }

                return skyPrefab;
            }

            if (_referenceType == ReferenceType.GetPrefabFromCallback)
            {
                return _prefabCallback?.Invoke();
            }

            return _cachedSkyPrefab;
        }

        private GameObject GetExistingSkyPrefab(WaterBiomeManager waterBiomeManager)
        {
            foreach (var settings in waterBiomeManager.biomeSettings)
            {
                if (settings == null || settings.skyPrefab == null ||
                    !string.Equals(settings.skyPrefab.name, _existingSkyPrefabNameToLookUp, StringComparison.OrdinalIgnoreCase))
                    continue;
                
                if (settings.skyPrefab.GetComponent<Sky>() == null)
                {
                    InternalLogger.Warn($"The sky prefab found by name '{_existingSkyPrefabNameToLookUp}' appears to be missing a Sky component.");
                    return null;
                }
                        
                return settings.skyPrefab;
            }

            InternalLogger.Error($"No existing Sky prefab found by name '{_existingSkyPrefabNameToLookUp}'");
            return null;
        }

        private GameObject CloneSkyPrefab(GameObject originalSkyPrefab)
        {
            var skyPrefab = UWE.Utils.InstantiateDeactivated(originalSkyPrefab);
            skyPrefab.name = _newSkyName;
            _cachedSkyPrefab = skyPrefab;
            var sky = skyPrefab.GetComponent<Sky>();
            _modifyCallback?.Invoke(sky);
            sky.Dirty = true; // force dirty because this is an instantiated clone
            skyPrefab.SetActive(true);
            return skyPrefab;
        }
        
        private enum ReferenceType
        {
            GameObjectReference,
            StringLookup,
            GetPrefabFromCallback,
            ClonedSky
        }
    }
}