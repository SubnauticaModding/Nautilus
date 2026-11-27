namespace Nautilus.Patchers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Utility;
using UnityEngine;

internal static class LanguagePatcher
{
    private const string FallbackLanguage = "English";
        
    private static readonly Dictionary<string, Dictionary<string, string>> _customLines = new();
    private static readonly Dictionary<string, string> _lineAliases = new();
    private static string _currentLanguage = FallbackLanguage;

    static LanguagePatcher()
    {
        string savedLanguagePath = PlayerPrefs.GetString("Language", null); // tries to find the last loaded language, sets the new variable to null if the last loaded language cannot be found
        if (!string.IsNullOrWhiteSpace(savedLanguagePath))
        {
            _currentLanguage = Path.GetFileNameWithoutExtension(savedLanguagePath);
        }
    }

    internal static void RepatchCheck(ref Language __instance, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (_lineAliases.TryGetValue(key, out string sourceLineId))
        {
            if (TryResolveLineAlias(__instance, key, sourceLineId, out string aliasValue) &&
                (!__instance.strings.TryGetValue(key, out string currentAliasValue) || aliasValue != currentAliasValue))
            {
                InsertCustomLines(ref __instance);
            }

            return;
        }

        if ((!_customLines.TryGetValue(_currentLanguage, out var customStrings) || !customStrings.TryGetValue(key, out var customValue)) && 
            (!_customLines.TryGetValue(FallbackLanguage, out customStrings) || !customStrings.TryGetValue(key, out customValue)))
        {
            return;
        }

        if (!__instance.strings.TryGetValue(key, out string currentValue) || customValue != currentValue)
        {
            InsertCustomLines(ref __instance);
        }
    }

    internal static void InsertCustomLines(ref Language __instance)
    {
        if (!_customLines.TryGetValue(FallbackLanguage, out var fallbackStrings) & !_customLines.TryGetValue(_currentLanguage, out var currentStrings))
        {
            ApplyLineAliases(__instance);
            return;
        }

        fallbackStrings ??= new();
        currentStrings ??= new();

        foreach (var fallbackString in fallbackStrings)
        {
            // Allow mixed-in English if the current language doesn't have a translation for a key. 
            if (currentStrings.TryGetValue(fallbackString.Key, out var currentValue))
                __instance.strings[fallbackString.Key] = currentValue;
            else
                __instance.strings[fallbackString.Key] = fallbackString.Value;
        }
            
        if (_currentLanguage == FallbackLanguage)
        {
            ApplyLineAliases(__instance);
            __instance.ParseMetaData();
            return;
        }
            
        var diffStrings = currentStrings.Except(fallbackStrings);
		
        // Just in case there are current language strings that aren't in the fallback language, we implement them as well.
        foreach (var currentOnlyString in diffStrings)
        {
            __instance.strings[currentOnlyString.Key] = currentOnlyString.Value;
        }
        
        ApplyLineAliases(__instance);
        __instance.ParseMetaData();
    }

    private static void ApplyLineAliases(Language language)
    {
        foreach (var alias in _lineAliases)
        {
            if (TryResolveLineAlias(language, alias.Key, alias.Value, out string value))
            {
                language.strings[alias.Key] = value;
            }
        }
    }

    private static bool TryResolveLineAlias(Language language, string lineId, string sourceLineId, out string value)
    {
        value = null;

        if (_customLines.TryGetValue(_currentLanguage, out var currentStrings) && currentStrings.TryGetValue(lineId, out string currentValue))
        {
            value = currentValue;
            return true;
        }

        if (language.strings.TryGetValue(sourceLineId, out string sourceValue) && !string.IsNullOrEmpty(sourceValue))
        {
            value = sourceValue;
            return true;
        }

        if (_customLines.TryGetValue(FallbackLanguage, out var fallbackStrings) && fallbackStrings.TryGetValue(lineId, out string fallbackValue))
        {
            value = fallbackValue;
            return true;
        }

        return false;
    }
        
    private static void LoadLanguageFilePrefix(string language)
    {
        _currentLanguage = Path.GetFileNameWithoutExtension(language);
    }

    internal static void Patch(Harmony harmony)
    {
        HarmonyMethod repatchCheckMethod = new(AccessTools.Method(typeof(LanguagePatcher), nameof(RepatchCheck)));
        HarmonyMethod insertLinesMethod = new(AccessTools.Method(typeof(LanguagePatcher), nameof(InsertCustomLines)));
        HarmonyMethod loadLanguagesMethod = new(AccessTools.Method(typeof(LanguagePatcher), nameof(LoadLanguageFilePrefix)));

        harmony.Patch(AccessTools.Method(typeof(Language), nameof(LanguageSDF.Initialize)), prefix: insertLinesMethod);
        harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.GetKeysFor)), prefix: insertLinesMethod);
        harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.TryGet)), prefix: repatchCheckMethod);
        harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.GetMetaData)), prefix: repatchCheckMethod);
        harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.Contains)), prefix: repatchCheckMethod);
        harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.LoadLanguageFile)), prefix: loadLanguagesMethod);

        InternalLogger.Log("LanguagePatcher is done.", LogLevel.Debug);
    }

    internal static void AddCustomLanguageLine(string lineId, string text, string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            language = FallbackLanguage;
        }

        if (!_customLines.ContainsKey(language))
            _customLines[language] = new();

        _customLines[language][lineId] = text;
    }

    internal static void AddDefaultLanguageLine(string lineId, string text)
    {
        if (!_customLines.ContainsKey(FallbackLanguage))
            _customLines[FallbackLanguage] = new();

        var fallbackStrings = _customLines[FallbackLanguage];

        if (!fallbackStrings.ContainsKey(lineId))
        {
            fallbackStrings[lineId] = text;
        }
    }

    internal static void AddLanguageLineAlias(string lineId, string sourceLineId)
    {
        if (string.IsNullOrWhiteSpace(lineId) || string.IsNullOrWhiteSpace(sourceLineId))
        {
            return;
        }

        _lineAliases[lineId] = sourceLineId;
    }

    internal static void AddCustomLanguageLines(string language, Dictionary<string, string> languageStrings)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            language = FallbackLanguage;
        }

        if (!_customLines.ContainsKey(language))
            _customLines[language] = new();

        var customStrings = _customLines[language];
            
        foreach (var languageString in languageStrings)
        {
            customStrings[languageString.Key] = languageString.Value;
        }
    }
}