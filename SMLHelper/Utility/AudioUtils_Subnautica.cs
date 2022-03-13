#if SUBNAUTICA
namespace SMLHelper.V2.Utility
{
    public static partial class AudioUtils
    {
        /// <summary>
        /// A list of the relevant FMOD bus paths the game uses.
        /// </summary>
        public static class BusPaths
        {
            /// <summary>
            /// Used for underwater creature SFXs. Tied to the master volume.
            /// </summary>
            public const string UnderwaterCreatures = "bus:/master/all/SFX/creatures";

            /// <summary>
            /// Used for surface creature SFXs that dont get muted when at the surface of the ocean. Tied to the master volume.
            /// </summary>
            public const string SurfaceCreatures = "bus:/master/all/SFX/creatures surface";

            /// <summary>
            /// Used for PDA voices. Tied to the voice volume.
            /// </summary>
            public const string PDAVoice = "bus:/master/all/all voice/AI voice";

            /// <summary>
            /// Used for encyclopedia VOs. Tied to the voice volume.
            /// </summary>
            public const string VoiceOvers = "bus:/master/all/all voice/VOs";

            /// <summary>
            /// Used for main music. Tied to the music volume.
            /// </summary>
            public const string Music = "bus:/master/nofilter/music";

            /// <summary>
            /// Used for environmental music. Tied to the music volume.
            /// </summary>
            public const string EnvironmentalMusic = "bus:/master/nofilter/music/mutable music";

            /// <summary>
            /// Used for underwater ambience SFXs. Tied to the ambient volume.
            /// </summary>
            public const string UnderwaterAmbient = "bus:/master/all/SFX/backgrounds";

            /// <summary>
            /// Used for ambience SFXs that dont get muted when at the surface of the ocean. Tied to the ambient volume.
            /// </summary>
            public const string SurfaceAmbient = "bus:/master/all/SFX/backgrounds/surface";

            /// <summary>
            /// Used for player and hand-held tools SFXs. Tied to the master volume.
            /// </summary>
            public const string PlayerSFXs = "bus:/master/all/SFX/reverbsend";
        }
    }
}
#endif