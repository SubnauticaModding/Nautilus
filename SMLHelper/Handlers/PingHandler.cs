using SMLHelper.V2.Patchers;
using UnityEngine;

namespace SMLHelper.V2.Handlers
{
    using Interfaces;

    /// <summary>
    /// A handler related to PingTypes
    /// </summary>
    public class PingHandler : IPingHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IPingHandler Main { get; } = new PingHandler();

        private PingHandler()
        {
        }

        /// <summary>
        /// Registers a ping type for use when creating a beacon
        /// </summary>
        /// <param name="pingName">The name of the new ping type</param>
        /// <param name="sprite">The sprite that is associated with the ping</param>
        /// <returns>The newly registered PingType</returns>
        public PingType RegisterNewPingType(string pingName, Sprite sprite)
        {
            return RegisterNewPingType(pingName, new Atlas.Sprite(sprite));   
        }

        /// <summary>
        /// Registers a ping type for use when creating a beacon
        /// </summary>
        /// <param name="pingName">The name of the new ping type</param>
        /// <param name="sprite">The sprite that is associated with the ping</param>
        /// <returns>The newly registered PingType</returns>
        public PingType RegisterNewPingType(string pingName, Atlas.Sprite sprite)
        {
            return PingTypePatcher.AddPingType(pingName, sprite);
        }

        /// <summary>
        /// Returns nullable containing a ping type associated with a name, if it exists.
        /// </summary>
        /// <param name="pingName">The name associated with the PingType you want.</param>
        /// <returns>A nullable containing a PingType if one was associated with the given pingName</returns>
        public PingType? GetPingType(string pingName)
        {
            return PingTypePatcher.GetPingType(pingName);
        }
    }
}
