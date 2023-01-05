namespace SMLHelper.FMod.Interfaces
{
    using FMOD;

    /// <summary>
    /// This interface is used to integrate with <see cref="Handlers.CustomSoundHandler"/>.
    /// </summary>
    public interface IFModSound
    {
        /// <summary>
        /// Defines how to play sound in this object.
        /// </summary>
        /// <returns>The channel the sound was played on</returns>
        Channel PlaySound();
    }
}