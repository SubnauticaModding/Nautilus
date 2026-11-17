using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Handlers;

public static partial class BiomeHandler
{
    /// <summary>
    /// Defines a reference to a new or existing Sky prefab.
    /// </summary>
    public class SkyReference
    {
        private enum Type
        {
            GameObjectReference,
            StringLookup,
            GetPrefabFromCallback
        }

        private readonly GameObject _obj;
        private readonly string _existingSkyPrefabNameToLookUp;
        private readonly System.Func<GameObject> _prefabCallback;
        private readonly Type _type;

        /// <summary>
        /// Defines a reference to a new or existing Sky prefab.
        /// </summary>
        /// <param name="obj"></param>
        public SkyReference(GameObject obj)
        {
            _obj = obj;
            _type = Type.GameObjectReference;
        }

        /// <summary>
        /// Defines a reference to a base-game Sky prefab.
        /// </summary>
        /// <param name="existingSkyPrefabName"><para>The name of the Sky prefab from the list of base-game Skies, i.e. "SkySafeShallows".</para>
        /// <para>A list of valid inputs can be found on this page: https://subnauticamodding.github.io/Nautilus/tutorials/biomes.html</para></param>
        public SkyReference(string existingSkyPrefabName)
        {
            _existingSkyPrefabNameToLookUp = existingSkyPrefabName;
            _type = Type.StringLookup;
        }
        
        /// <summary>
        /// Defines a reference to a Sky prefab that is created on demand at runtime.
        /// </summary>
        /// <param name="prefabCallback">A deferred callback to retrieve a GameObject containing the <see cref="mset.Sky"/> component.</param>
        public SkyReference(System.Func<GameObject> prefabCallback)
        {
            _prefabCallback = prefabCallback;
            _type = Type.GetPrefabFromCallback;
        }
        
        /// <summary>
        /// Gets the sky prefab for this <see cref="SkyReference"/>.
        /// </summary>
        /// <param name="waterBiomeManager">Used when the type is <see cref="SkyReference.Type.StringLookup">StringLookup</see>.</param>
        /// <returns>
        /// Returns the sky's prefab depending on the given <see cref="SkyReference.Type"/>:
        /// <list type="bullet">
        ///     <item><see cref="SkyReference.Type.GameObjectReference">GameObjectReference</see>: Prefab reference is returned.</item>
        ///     <item><see cref="SkyReference.Type.StringLookup">StringLookup</see>: The sky's name is searched for in the given <paramref name="waterBiomeManager"/>. Returns null if not found.</item>
        ///     <item><see cref="SkyReference.Type.GetPrefabFromCallback">GetPrefabFromCallback</see>: The callback is invoked and the result is returned.</item>
        /// </list>
        /// </returns>
        public virtual GameObject GetSkyPrefabAtRuntime(WaterBiomeManager waterBiomeManager)
        {
            if (_type == Type.StringLookup)
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

            if (_type == Type.GetPrefabFromCallback)
            {
                return _prefabCallback?.Invoke();
            }

            return _obj;
        }
    }
}