namespace SMLHelper.Handlers
{
    using SMLHelper.Patchers.EnumPatching;
    using UnityEngine;

    /// <summary>
    /// A handler related to PingTypes
    /// </summary>
    public static class PingHandler
    {

#if SUBNAUTICA
        /// <summary>
        /// Registers a ping type for use when creating a beacon
        /// </summary>
        /// <param name="pingName">The name of the new ping type</param>
        /// <param name="sprite">The sprite that is associated with the ping</param>
        /// <returns>The newly registered PingType</returns>
        public static PingType RegisterNewPingType(string pingName, Atlas.Sprite sprite)
        {
            return PingTypePatcher.AddPingType(pingName, sprite);
        }
#endif

        /// <summary>
        /// Registers a ping type for use when creating a beacon
        /// </summary>
        /// <param name="pingName">The name of the new ping type</param>
        /// <param name="sprite">The sprite that is associated with the ping</param>
        /// <returns>The newly registered PingType</returns>
        public static PingType RegisterNewPingType(string pingName, Sprite sprite)
        {
            return PingTypePatcher.AddPingType(pingName, sprite);   
        }

        /// <summary>
        /// Safely looks for a modded ping type in the SMLHelper PingTypeCache and outputs its <see cref="PingType"/> value when found.
        /// </summary>
        /// <param name="pingTypeString">The string used to define the modded PingType</param>
        /// <param name="moddedPingType">The PingType enum value. Defaults to <see cref="PingType.None"/> when the PingType was not found.</param>
        /// <returns><c>True</c> if the PingType was found; Otherwise <c>false</c></returns>
        public static bool TryGetModdedPingType(string pingTypeString, out PingType moddedPingType)
        {
            Utility.EnumTypeCache cache = PingTypePatcher.cacheManager.RequestCacheForTypeName(pingTypeString, false);
            if (cache != null)
            {
                moddedPingType = (PingType) cache.Index;
                return true;
            }

            moddedPingType = PingType.None;
            return false;
        }
    }
}
