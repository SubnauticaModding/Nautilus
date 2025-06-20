using System;
using FMOD;

namespace Nautilus.FMod.Interfaces;

/// <summary>
/// The interface for registering FMOD Sounds using the <see cref="FModSoundBuilder"/> class. 
/// </summary>
public interface IFModSoundBuilder
{
    /// <summary>
    /// Sets the current mode to <paramref name="mode"/>, overriding any previous value.
    /// </summary>
    /// <param name="mode">The set of mode flags to use for the sound.
    /// <see href="https://documentation.help/fmod-studio-api/FMOD_MODE.html">See here for documentation</see>.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    public IFModSoundBuilder SetMode(MODE mode);
    /// <summary>
    /// Sets the mode to <see cref="Nautilus.Utility.AudioUtils.StandardSoundModes_3D"/>.
    /// </summary>
    /// <param name="minDistance">The distance where volume falloff begins.</param>
    /// <param name="maxDistance">The maximum distance that the sound can be heard from.</param>
    /// <param name="looping">If true, the sound also uses the <see cref="FMOD.MODE.LOOP_NORMAL"/> flag.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    public IFModSoundBuilder SetMode3D(float minDistance, float maxDistance, bool looping = false);
    /// <summary>
    /// Sets the mode to <see cref="Nautilus.Utility.AudioUtils.StandardSoundModes_2D"/>.
    /// </summary>
    /// <param name="looping">If true, the sound also uses the <see cref="FMOD.MODE.LOOP_NORMAL"/> flag.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    public IFModSoundBuilder SetMode2D(bool looping = false);
    /// <summary>
    /// Sets the mode to <see cref="Nautilus.Utility.AudioUtils.StandardSoundModes_Stream"/>.
    /// </summary>
    /// <param name="looping">If true, the sound also uses the <see cref="FMOD.MODE.LOOP_NORMAL"/> flag.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    public IFModSoundBuilder SetModeMusic(bool looping = false);
    /// <summary>
    /// Sets the fade duration for the sound, which applies when it is stopped.
    /// </summary>
    /// <param name="fadeDuration">The length of the fade in seconds.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    public IFModSoundBuilder SetFadeDuration(float fadeDuration);
    /// <summary>
    /// Uses a sound from the sound source that matches the name defined in <see cref="clipName"/>. Does not use file extensions.
    /// </summary>
    /// <param name="clipName">The name of the sound to load. File extensions should not be used.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    public IFModSoundBuilder SetSound(string clipName);
    /// <summary>
    /// Loads multiple sounds for this sound event, locating each by name. Does not use file extensions.
    /// </summary>
    /// <param name="randomizeOrder">If true, the sounds will play in a random order instead of sequentially.</param>
    /// <param name="clipNames">The names of the sounds to load. File extensions should not be used.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    public IFModSoundBuilder SetSounds(bool randomizeOrder, params string[] clipNames);
    /// <summary>
    /// Loads multiple sounds for this sound event, using each one for which the predicate returns true. Does not use file extensions.
    /// </summary>
    /// <param name="randomizeOrder">If true, the sounds will play in a random order instead of sequentially.</param>
    /// <param name="predicate">The predicate that defines which sounds should be loaded.
    /// The file extension is not passed and should not be considered.</param>
    /// <returns>A reference to the builder for further setup.</returns>
    /// <remarks>This method traverses over every sound in the sound source, making it considerably slower than other methods.</remarks>
    public IFModSoundBuilder SetSounds(bool randomizeOrder, Func<string, bool> predicate);
    /// <summary>
    /// Finalizes and registers the FMOD event with the given settings.
    /// </summary>
    public void Register();
}