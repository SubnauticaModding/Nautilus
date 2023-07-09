using Nautilus.Json;
using Nautilus.Utility;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for registering your <see cref="SaveDataCache"/>.
/// </summary>
public static class SaveDataHandler
{
    /// <summary>
    /// Registers your <see cref="SaveDataCache"/> to be automatically loaded and saved whenever the game is.
    /// </summary>
    /// <typeparam name="T">A class derived from <see cref="SaveDataCache"/> to hold your save data.</typeparam>
    /// <returns>An instance of the <typeparamref name="T"/> : <see cref="SaveDataCache"/> with values loaded
    /// from the json file on disk whenever a save slot is loaded.</returns>
    public static T RegisterSaveDataCache<T>() where T : SaveDataCache, new()
    {
        T cache = new();

        SaveUtils.RegisterOnStartLoadingEvent(() => cache.Load());
        SaveUtils.RegisterOnSaveEvent(() => cache.Save());

        return cache;
    }
}