using FMOD;

namespace Nautilus.FMod.Interfaces;

/// <summary>
/// This interface is used to integrate with <see cref="Handlers.CustomSoundHandler"/>.
/// </summary>
public interface IFModSound
{
    /// <summary>
    /// Defines how to play sound in this object.
    /// </summary>
    /// <param name="channel">The channel on which the sound was created.</param>
    /// <returns>If the sound was reported as played.</returns>
    bool TryPlaySound(out Channel channel);
}