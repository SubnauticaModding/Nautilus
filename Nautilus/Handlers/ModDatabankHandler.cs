using BepInEx;
using Nautilus.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Nautilus.Handlers;
/// <summary>
/// A handler class for adding databank entries for mods.
/// </summary>
public static class ModDatabankHandler
{
    private static bool _isInit;
    private static List<PDAEncyclopedia.EntryData> _waitList = new();
    internal static bool _isEnabled = true;
    internal static void Initialize()
    {
        LanguageHandler.SetLanguageLine("EncyPath_Mods", "Mods");
        _isInit = true;
        foreach (var data in _waitList)
        {
            CompleteRegister(data);
        }
    }
    private static void CompleteRegister(PDAEncyclopedia.EntryData data)
    {
        if (_isInit && _isEnabled)
        {
            InternalLogger.Info($"{data.key} entry added.");
            PDAHandler.AddEncyclopediaEntry(data);
#if BZ_STABLE
            PDAEncyclopedia.Add(data.key, true, false);
#else
            PDAEncyclopedia.Add(data.key, true);
#endif
        }
        else
        {
            _waitList.Add(data);
        }
    }
    /// <summary>
    /// Automatically adds info about your mod to the game's databank under a tab named Mods using your mod's PluginInfo.
    /// </summary>
    /// <param name="info">The PluginInfo for your mod. Pass in Info from your BepInPlugin class</param>
    public static void RegisterMod(BepInEx.PluginInfo info)
    {
        var bepinplugindata = info.Metadata;
        var entrydata = new PDAEncyclopedia.EntryData()
        {
            image = null,
            key = bepinplugindata.GUID,
            unlocked = true,
            path = "Mods",
            nodes = PDAEncyclopedia.ParsePath("Mods")
        };
        LanguageHandler.SetLanguageLine($"Ency_{bepinplugindata.GUID}", $"{bepinplugindata.Name} {bepinplugindata.Version.ToString()}");
        LanguageHandler.SetLanguageLine($"EncyDesc_{bepinplugindata.GUID}", "A BepInEx plugin using Nautilus.");
        CompleteRegister(entrydata);
    }
    /// <summary>
    /// Automatically adds info about your mod to the game's databank under a tab named Mods using supplied ModData instance.
    /// </summary>
    /// <param name="data"></param>
    public static void RegisterMod(ModData data)
    {
        var entrydata = new PDAEncyclopedia.EntryData()
        {
            image = data.image,
            key = data.guid,
            unlocked = true,
            path = "Mods",
            nodes = PDAEncyclopedia.ParsePath("Mods")
        };
        if(data.desc != null)
        {
            LanguageHandler.SetLanguageLine($"EncyDesc_{data.guid}", data.desc);
        }
        if(data.name != null)
        {
            LanguageHandler.SetLanguageLine($"Ency_{data.guid}", $"{data.name} {data.version ?? ""}");
        }
        CompleteRegister(entrydata);
    }
    /// <summary>
    /// Data for the encyclopedia entry of your mod.
    /// </summary>
    public record struct ModData
    {
        /// <summary>
        /// Name of your mod, not optional.
        /// </summary>
        public required string name;
        /// <summary>
        /// GUID, just an identifier, not optional.
        /// </summary>
        public required string guid;
        /// <summary>
        /// Mod version, optional.
        /// </summary>
        public string version;
        /// <summary>
        /// Mod description, optional.
        /// </summary>
        public string desc;
        /// <summary>
        /// Databank image, optional.
        /// </summary>
        public Texture2D image;
    }
}
