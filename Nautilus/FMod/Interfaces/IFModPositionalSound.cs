namespace Nautilus.FMod.Interfaces;

using FMOD;
using UnityEngine;

/// <summary>
/// This interface is used to integrate with <see cref="Handlers.CustomSoundHandler"/>.
/// </summary>
public interface IFModPositionalSound : IFModSound
{
    /// <summary>
    /// Defines how to play sound in this object.
    /// </summary>
    /// <param name="position">The position to play the sound at.</param>
    /// <param name="channel">The channel on which the sound was created.</param>
    /// <returns>If the sound was reported as played.</returns>
    bool TryPlaySound(Vector3 position, out Channel channel);
}
