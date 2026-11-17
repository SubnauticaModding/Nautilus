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
        private readonly GameObject _obj;
        private readonly string _existingSkyPrefabNameToLookUp;
        private readonly System.Func<GameObject> _prefabCallback;
        private readonly ReferenceType _referenceType;

        /// <summary>
        /// Defines a reference to a new or existing Sky prefab.
        /// </summary>
        /// <param name="obj">The GameObject containing the <see cref="mset.Sky"/> component.</param>
        public SkyReference(GameObject obj)
        {
            _obj = obj;
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
        public SkyReference(System.Func<GameObject> prefabCallback)
        {
            _prefabCallback = prefabCallback;
            _referenceType = ReferenceType.GetPrefabFromCallback;
        }
        
        internal GameObject GetSkyPrefabAtRuntime(WaterBiomeManager waterBiomeManager)
        {
            if (_referenceType == ReferenceType.StringLookup)
            {
                foreach (var settings in waterBiomeManager.biomeSettings)
                {
                    if (settings != null && settings.skyPrefab != null && settings.skyPrefab.name == _existingSkyPrefabNameToLookUp)
                    {
                        return settings.skyPrefab;
                    }
                }

                InternalLogger.Error($"No existing Sky prefab found by name '{_existingSkyPrefabNameToLookUp}'");
                return null;
            }

            if (_referenceType == ReferenceType.GetPrefabFromCallback)
            {
                return _prefabCallback?.Invoke();
            }

            return _obj;
        }
        
        private enum ReferenceType
        {
            GameObjectReference,
            StringLookup,
            GetPrefabFromCallback
        }
    }
}