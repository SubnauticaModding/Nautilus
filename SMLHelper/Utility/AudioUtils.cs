namespace SMLHelper.V2.Utility
{
    using FMOD;

    /// <summary>
    /// Utilities for audio and sound
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Plays a sound globally. Must be a .wav file
        /// </summary>
        /// <param name="path">The path of the sound. Relative to the base game folder.</param>
        /// <returns></returns>
        public static RESULT PlaySound(string path)
        {
            Sound newSound = new Sound();
            ChannelGroup channels = new ChannelGroup();
            Channel newChannel = new Channel();
            var system = FMODUnity.RuntimeManager.LowlevelSystem;
            system.getMasterChannelGroup(out channels);
            system.createSound(path, MODE.DEFAULT, out newSound);
            return system.playSound(newSound, channels, false, out newChannel);
        }
    }
}
