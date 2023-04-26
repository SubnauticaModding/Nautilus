using BepInEx.Logging;
using Nautilus.Patchers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for various scanner related data.
/// </summary>
public static class PDAHandler 
{
    /// <summary>
    /// Sound asset used for unlocking most PDA entries, which is a short but pleasant sound. Path is '<c>event:/tools/scanner/new_encyclopediea</c>'.
    /// </summary>
    public static FMODAsset UnlockBasic { get; } = AudioUtils.GetFmodAsset("event:/tools/scanner/new_encyclopediea");

    /// <summary>
    /// <para>Subnautica:<br/>Sound asset for unlocking important PDA entries, where PDA says "Integrating new PDA data." Path is '<c>event:/loot/new_PDA_data</c>'.</para>
    /// <para>Below Zero:<br/>Sound asset for unlocking more important (generally story related) PDA entries. Path is '<c>event:/bz/ui/story_unlocked</c>'.</para>
    /// </summary>
#if SUBNAUTICA
    public static FMODAsset UnlockImportant { get; } = AudioUtils.GetFmodAsset("event:/loot/new_PDA_data");
#else
    public static FMODAsset UnlockImportant { get; } = AudioUtils.GetFmodAsset("event:/bz/ui/story_unlocked");
#endif
    

    /// <summary>
    /// Edits how many fragments must be scanned before unlocking the techtype's blueprint.
    /// </summary>
    /// <param name="techType">Can be either techtype of the fragment or the crafted item.</param>
    /// <param name="fragmentCount">The number of fragments to scan.</param>
    public static void EditFragmentsToScan(TechType techType, int fragmentCount)
    {
        if (fragmentCount <= 0)
        {
            fragmentCount = 1;
        }

        PDAPatcher.FragmentCount[techType] = fragmentCount;
        
        if (uGUI.isMainLevel)
            PDAPatcher.InitializePostfix();
    }

    /// <summary>
    /// Edits the time it takes to finish scanning a fragment.
    /// </summary>
    /// <param name="techType">Can be either techtype of the fragment or the crafted item.</param>
    /// <param name="scanTime">The relative time spent on scanning. Default value is 1.</param>
    public static void EditFragmentScanTime(TechType techType, float scanTime)
    {
        if (scanTime <= 0f)
        {
            scanTime = 1f;
        }

        PDAPatcher.FragmentScanTime[techType] = scanTime;
        
        if (uGUI.isMainLevel)
            PDAPatcher.InitializePostfix();
    }

    /// <summary>
    /// Adds in a custom <see cref="PDAScanner.EntryData"/>. ***Cannot be used to Change the values of a techtype that has data already!***
    /// </summary>
    /// <param name="entryData">The <see cref="PDAScanner.EntryData"/> of the entry. Must be populated when passed in.</param>
    public static void AddCustomScannerEntry(PDAScanner.EntryData entryData)
    {
        if (PDAPatcher.CustomEntryData.ContainsKey(entryData.key))
        {
            InternalLogger.Log($"{entryData.key} already has custom PDAScanner.EntryData. Replacing with latest.", LogLevel.Debug);
        }

        PDAPatcher.CustomEntryData[entryData.key] = entryData;
        
        if (uGUI.isMainLevel)
            PDAPatcher.InitializePostfix();
    }

    /// <summary>
    /// Adds in a custom <see cref="PDAScanner.EntryData"/>.
    /// </summary>
    /// <param name="key">The scanned object's <see cref="TechType"/>. In case of fragments, the fragment <see cref="TechType"/> is the key.</param>
    /// <param name="blueprint">The <paramref name="blueprint"/> when unlocked when scanned. In case of fragments, this is the actual <see cref="TechType"/> that unlocks when all fragments are scanned.</param>
    /// <param name="isFragment">Whether the <paramref name="key"/> is a fragment or not.</param>
    /// <param name="totalFragmentsRequired">The total amount of objects of <paramref name="key"/> that need to be scanned to unlock the <paramref name="blueprint"/> and <paramref name="encyclopediaKey"/>.</param>
    /// <param name="scanTime">The amount of time it takes to finish one scan. In seconds.</param>
    /// <param name="destroyAfterScan">Whether the object should be destroyed after the scan is finished.</param>
    /// <param name="encyclopediaKey">The key to the encyclopedia entry.</param>
    public static void AddCustomScannerEntry(TechType key, TechType blueprint, bool isFragment, string encyclopediaKey, int totalFragmentsRequired = 2, float scanTime = 2f, bool destroyAfterScan = true)
    {
        if (encyclopediaKey == null) encyclopediaKey = string.Empty;
        AddCustomScannerEntry(new PDAScanner.EntryData()
        {
            key = key,
            blueprint = blueprint,
            isFragment = isFragment,
            totalFragments = totalFragmentsRequired,
            scanTime = scanTime,
            destroyAfterScan = destroyAfterScan,
            encyclopedia = encyclopediaKey
        });
    }

    /// <summary>
    /// Adds a custom log entry.
    /// </summary>
    /// <param name="key">The key to refer to this entry.</param>
    /// <param name="languageKey">The subtitles language key for this entry.</param>
    /// <param name="icon">The icon that will be used in the Log tab for this entry. if <c>null</c> It will use the default log entry icon.</param>
    /// <param name="sound">The sound that will be played once this entry is triggered or played in the Log tab.<br/>
    /// If <c>null</c> the Play button in the Log tab will disappear and a sound wont play when this entry is triggered.</param>
    public static void AddLogEntry(string key, string languageKey, FMODAsset sound, Sprite icon = null)
    {
        PDALog.EntryData entry = new()
        {
            key = languageKey,
            icon = icon,
            sound = sound
        };
        
        PDALogPatcher.CustomEntryData[key] = entry;
        
        if (uGUI.isMainLevel)
            PDALogPatcher.InitializePostfix();
    }

    /// <summary>
    /// Adds custom entry.
    /// </summary>
    /// <param name="entry">The <see cref="PDAEncyclopedia.EntryData"/> entry.</param>
    public static void AddEncyclopediaEntry(PDAEncyclopedia.EntryData entry)
    {
        if(PDAEncyclopediaPatcher.CustomEntryData.ContainsKey(entry.key))
        {
            InternalLogger.Log($"{entry.key} already has custom PDAEncyclopedia.EntryData. Replacing with latest.", LogLevel.Debug);
        }

        PDAEncyclopediaPatcher.CustomEntryData[entry.key] = entry;
        
        if (uGUI.isMainLevel)
            PDAEncyclopediaPatcher.InitializePostfix();
    }

    /// <summary>
    /// Registers a single encyclopedia entry into the game.
    /// </summary>
    /// <param name="key">Key (internal ID) of this PDA entry, primarily used for the language system.</param>
    /// <param name="path"><para>Path to this entry in the databank.</para>
    /// <para>To find examples of this string, open "Subnautica_Data\StreamingAssets\SNUnmanagedData\LanguageFiles\English.json" and search for "EncyPath".
    /// Remember to omit the "EncyPath_" prefix from these language keys. An example of a proper value is: "Lifeforms/Fauna/Leviathans".</para>
    /// </param>
    /// <param name="title">Displayed title of the PDA entry in English. If set to null, you must implement your own translations. Language key is 'Ency_{<paramref name="key"/>}'.</param>
    /// <param name="desc">Displayed description of the PDA entry in English. If set to null, you must implement your own translations. Language key is 'EncyDesc_{<paramref name="key"/>}'.</param>
    /// <param name="image">Databank entry image. Can be null.</param>
    /// <param name="popupImage">Small popup image in the notification. Can be null.</param>
    /// <param name="unlockSound">Sound on unlock. Typical values are <see cref="UnlockBasic"/> and <see cref="UnlockImportant"/>. If unassigned, will have a default value of <see cref="UnlockBasic"/>.</param>
    /// <param name="voiceLog">Audio player that will be displayed inside this PDA entry, typically used for voice logs. Can be null.</param>
    public static void AddEncyclopediaEntry(string key, string path, string title, string desc, Texture2D image = null, Sprite popupImage = null, FMODAsset unlockSound = null, FMODAsset voiceLog = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            InternalLogger.Error($"Attempting to add encyclopedia entry with null path for ClassId '{key}'!");
            return;
        }

        var encyNodes = path.Split('/');

        if (unlockSound == null)
        {
            unlockSound = UnlockBasic;
        }

        var encyEntryData = new PDAEncyclopedia.EntryData()
        {
            key = key,
            nodes = encyNodes,
            path = path,
            image = image,
            popup = popupImage,
            sound = unlockSound,
            audio = voiceLog
        };

        if (!string.IsNullOrEmpty(title)) LanguageHandler.SetLanguageLine("Ency_" + key, title);
        if (!string.IsNullOrEmpty(desc)) LanguageHandler.SetLanguageLine("EncyDesc_" + key, desc);

        AddEncyclopediaEntry(encyEntryData);
    }
}