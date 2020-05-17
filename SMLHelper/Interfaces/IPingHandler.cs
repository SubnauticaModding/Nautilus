using System;

namespace SMLHelper.V2.Interfaces
{
    /// <summary>
    /// A handler related to PingTypes
    /// </summary>
    public interface IPingHandler
    {
        /// <summary>
        /// Registers a ping type for use when creating a beacon
        /// </summary>
        /// <param name="pingName">The name of the new ping type</param>
        /// <param name="sprite">The sprite that is associated with the ping</param>
        /// <returns>The newly registered PingType</returns>
        PingType RegisterNewPingType(string pingName, UnityEngine.Sprite sprite);
        
        /// <summary>
        /// Registers a ping type for use when creating a beacon
        /// </summary>
        /// <param name="pingName">The name of the new ping type</param>
        /// <param name="sprite">The sprite that is associated with the ping</param>
        /// <returns>The newly registered PingType</returns>
        PingType RegisterNewPingType(string pingName, Atlas.Sprite sprite);

        /// <summary>
        /// Returns nullable containing a ping type associated with a name, if it exists.
        /// </summary>
        /// <param name="pingName">The name associated with the PingType you want.</param>
        /// <returns>A nullable containing a PingType if one was associated with the given pingName</returns>
        PingType? GetPingType(string pingName);
    }
}
