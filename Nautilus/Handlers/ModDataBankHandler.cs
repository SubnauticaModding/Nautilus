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
    private static bool isinit;
    private static List<PDAEncyclopedia.EntryData> waitlist = new();
    internal static void Initialize(uGUI_EncyclopediaTab tab)
    {
        LanguageHandler.SetLanguageLine("EncyPath_Mods", "Mods");
        isinit = true;
        foreach (var data in waitlist)
        {
            CompleteRegister(data);
        }
    }
    private static void CompleteRegister(PDAEncyclopedia.EntryData data)
    {
        if (isinit)
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
            waitlist.Add(data);
        }
    }
    /// <summary>
    /// Register mod with database using default values. 
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
    /// Register mod with database using specified values.
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
            string version = "";
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
