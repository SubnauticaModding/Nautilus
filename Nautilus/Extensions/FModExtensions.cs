using FMOD;
using FMODUnity;
using Nautilus.Patchers;
using Nautilus.Utility;

namespace Nautilus.Extensions;

/// <summary>
/// Contains extension methods for the FMOD system. 
/// </summary>
public static class FModExtensions
{
    /// <summary>
    /// Adds a fade-out point for the specified sound.
    /// </summary>
    /// <param name="sound">The sound to add a fade-out to</param>
    /// <param name="seconds">The duration of the fade-out.</param>
    /// <remarks>Fades are only triggered when an emitter respects them. E.G: when calling <c>FMOD_CustomEmitter.Stop(STOP_MODE.ALLOWFADEOUT)</c>.</remarks>
    public static void AddFadeOut(this Sound sound, float seconds)
    {
        if (!sound.hasHandle())
        {
            InternalLogger.Error("AddFadeOut: Sound object is missing. Please provide a valid sound object.");
            return;
        }
        
        CustomSoundPatcher.FadeOuts[sound.handle] = new CustomSoundPatcher.FadeInfo(sound, seconds);
    }

    /// <summary>
    /// Adds a fade-out point for the specified channel.
    /// </summary>
    /// <param name="channel">The channel to add a fade-out to</param>
    /// <param name="seconds">The duration of the fade-out. The fade-out starts at the current time.</param>
    /// <param name="dspClock">The DSP clock at the point where the fade was added.<br/>
    /// DSP clock consists of 48_000 ticks per second. For more information, please refer to the <see href="https://documentation.help/fmod-studio-api/FMOD_Channel_GetDSPClock.html">FMOD docs</see>.</param>
    /// <remarks>This method only applies the fade-out one time. If you want the fade to stay everytime the sound is played, consider using <see cref="AddFadeOut(FMOD.Sound,float)"/>.</remarks>
    public static void AddFadeOut(this Channel channel, float seconds, out ulong dspClock)
    {
        if (!channel.hasHandle())
        {
            InternalLogger.Error("AddFadeOut: Channel object is invalid. Fade operation is cancelled.");
            dspClock = 0;
            return;
        }

        RuntimeManager.CoreSystem.getSoftwareFormat(out int samplesRate, out _, out _);

        channel.getDSPClock(out _, out ulong parentClock);
        channel.addFadePoint(parentClock, 1f);
        channel.addFadePoint(parentClock + (ulong)(samplesRate * seconds), 0f);
        dspClock = parentClock;
    }
}