using BepInEx;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nautilus.Handlers;
public class ModDataBankHandler
{
    private static bool isinit;
    private static List<PDAEncyclopedia.EntryData> waitlist = new();
    internal static void Initialize(uGUI_EncyclopediaTab tab)
    {
        LanguageHandler.SetLanguageLine("EncyPath_Mods", "Mods");
        isinit = true;
        foreach(var data in waitlist)
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
            PDAEncyclopedia.Add(data.key, true);
        }else
        {
            waitlist.Add(data);
        }

    }
    /// <summary>
    /// Register mod with default values. 
    /// </summary>
    /// <param name="assembly">Assembly for your mod.</param>
    public static void RegisterMod(Assembly assembly)
    {
        var bepinplugin = assembly.GetTypes().First(type => type.GetCustomAttributes(false).OfType<BepInPlugin>().Any());
        var bepinplugindata = bepinplugin.GetCustomAttributes(false).OfType<BepInPlugin>().FirstOrDefault();
        var entrydata = new PDAEncyclopedia.EntryData()
        {
            image = null,
            key = bepinplugindata.GUID,
            unlocked = true,
            path = "Mods",
            nodes = PDAEncyclopedia.ParsePath("Mods")
        };
        LanguageHandler.SetLanguageLine($"Ency_{bepinplugindata.GUID}", bepinplugindata.Name + " " + bepinplugindata.Version.ToString());
        LanguageHandler.SetLanguageLine($"EncyDesc_{bepinplugindata.GUID}", "A BepInEx plugin using Nautilus.");
        CompleteRegister(entrydata);
    }
    /// <summary>
    /// Register mod with specified values.
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
            if(!data.version.IsNullOrWhiteSpace())
            {
                version = " " + data.version;
            }
            LanguageHandler.SetLanguageLine($"Ency_{data.guid}", data.name + version);
        }
        CompleteRegister(entrydata);
    }
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
