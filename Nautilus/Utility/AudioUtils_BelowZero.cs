#if BELOWZERO
namespace Nautilus.Utility;

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
        public const string UnderwaterCreatures = "bus:/master/SFX_for_pause/PDA_pause/all/SFX/creatures underwater";

        /// <summary>
        /// Used for surface creature SFXs that dont get muted when at the surface of the ocean. Tied to the master volume.
        /// </summary>
        public const string SurfaceCreatures = "bus:/master/SFX_for_pause/PDA_pause/all/SFX/creatures surface";

        /// <summary>
        /// Used for PDA voices. Tied to the voice volume.
        /// </summary>
        public const string PDAVoice = "bus:/master/SFX_for_pause/PDA_pause/all/all voice/AI voice";

        /// <summary>
        /// Used for encyclopedia VOs. Tied to the voice volume.
        /// </summary>
        public const string VoiceOvers = "bus:/master/SFX_for_pause/PDA_pause/all/all voice/VOs";

        /// <summary>
        /// Used for main music. Tied to the music volume.
        /// </summary>
        public const string Music = "bus:/master/Music_no_pause/music";

        /// <summary>
        /// Used for environmental music. Tied to the music volume.
        /// </summary>
        public const string EnvironmentalMusic = "bus:/master/Music_no_pause/music/mutable music";

        /// <summary>
        /// Used for underwater ambience SFXs. Tied to the ambient volume.
        /// </summary>
        public const string UnderwaterAmbient = "bus:/master/SFX_for_pause/PDA_pause/all/SFX/backgrounds";

        /// <summary>
        /// Used for ambience SFXs that dont get muted when at the surface of the ocean. Tied to the ambient volume.
        /// </summary>
        public const string SurfaceAmbient = "bus:/master/SFX_for_pause/PDA_pause/all/SFX/backgrounds/surface";
        
        /// <summary>
        /// Used for player and hand-held tools SFXs. Tied to the master volume.
        /// </summary>
        public const string PlayerSFXs = "bus:/master/SFX_for_pause/PDA_pause/all/SFX/reverbsend";
    }
}
#endif