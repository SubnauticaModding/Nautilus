using System.Linq;

namespace SMLHelper.Patchers
{
    using System.Collections.Generic;
    using System.IO;
    using BepInEx.Logging;
    using HarmonyLib;
    using Newtonsoft.Json;
    using Handlers;
    using UnityEngine;
    using Utility;

    internal static class LanguagePatcher
    {
        private const string FallbackLanguage = "English";

        internal static readonly List<string> LanguagePaths = new();
        private static readonly Dictionary<string, string> _currentLanguageStrings = new();
        private static readonly Dictionary<string, string> _fallbackLanguageStrings = new();
        private static readonly Dictionary<string, Dictionary<string, string>> _customLines = new();
        private static string _currentLanguage = "English";

        static LanguagePatcher()
        {
            string savedLanguagePath = PlayerPrefs.GetString("Language", null); // tries to find the last loaded language, sets the new variable to null if the last loaded language cannot be found
            if (!string.IsNullOrEmpty(savedLanguagePath))
            {
                _currentLanguage = Path.GetFileNameWithoutExtension(savedLanguagePath);
            }
        }

        internal static void RepatchCheck(ref Language __instance, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
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
            var fallbackStrings = _customLines[FallbackLanguage];
            var currentStrings = _customLines[_currentLanguage];

            foreach (var fallbackString in fallbackStrings)
            {
                // Allow mixed-in English if the current language doesn't have a translation for a key. 
                if (currentStrings.TryGetValue(fallbackString.Key, out var currentValue))
                    __instance.strings[fallbackString.Key] = currentValue;
                else
                    __instance.strings[fallbackString.Key] = fallbackString.Value;
            }
            
            if (_currentLanguage == FallbackLanguage)
                return;
            
            var diffStrings = currentStrings.Except(fallbackStrings);
		
            // Just in case there are current language strings that aren't in the fallback language, we implement them as well.
            foreach (var currentOnlyString in diffStrings)
            {
                __instance.strings[currentOnlyString.Key] = currentOnlyString.Value;
            }
        }
        
        private static void LoadLanguageFilePrefix(string language)
        {
            _currentLanguage = Path.GetFileNameWithoutExtension(language);
            LoadLanguages(language);
        }

        internal static void LoadLanguages(string language = FallbackLanguage)
        {
            foreach (var languagePath in LanguagePaths)
            {
                LoadLanguageImpl(language, languagePath);
            }
        }
        
        internal static void LoadLanguageImpl(string language, string languageFolder)
        {
            string fallbackPath = Path.Combine(languageFolder, $"{FallbackLanguage}.json");
            var file = Path.Combine(languageFolder, language + ".json");
            if (!File.Exists(file)) // if the preferred language doesn't have a file, use english, and return.
            {
                file = fallbackPath;
                if (File.Exists(file))
                {
                    SetLanguages(file, false);
                    return;
                }
            }
            
            SetLanguages(file, false); // load the preferred language
            
            if (language != FallbackLanguage) SetLanguages(fallbackPath, true); // if the current language is not already the fallback, then load the fallback language. some mixed in english is much better than raw language keys.

            void SetLanguages(string fileToSet, bool loadIntoFallback)
            {
                var deserialize = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(fileToSet));
                if (deserialize is null)
                    return;

                var dictionary = loadIntoFallback
                    ? _fallbackLanguageStrings
                    : _currentLanguageStrings;
                
                dictionary.Clear();

                foreach (var kvp in deserialize)
                {
                    dictionary[kvp.Key] = kvp.Value;
                    // If we are loading a fallback language, we should ONLY set a language line if a translation for the key doesn't already exist. Fallback should never override current language.
                    if (!loadIntoFallback || _currentLanguageStrings.ContainsKey(kvp.Key))
                    {
                        LanguageHandler.SetLanguageLine(kvp.Key, kvp.Value, _currentLanguage);
                    }
                }
            }
        }

        internal static void Patch(Harmony harmony)
        {
            HarmonyMethod repatchCheckMethod = new(AccessTools.Method(typeof(LanguagePatcher), nameof(RepatchCheck)));
            HarmonyMethod insertLinesMethod = new(AccessTools.Method(typeof(LanguagePatcher), nameof(InsertCustomLines)));
            HarmonyMethod loadLanguagesMethod = new(AccessTools.Method(typeof(LanguagePatcher), nameof(LoadLanguageFilePrefix)));

            harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.ParseMetaData)), prefix: insertLinesMethod);
            harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.GetKeysFor)), prefix: insertLinesMethod);
            harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.TryGet)), prefix: repatchCheckMethod);
            harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.Contains)), prefix: repatchCheckMethod);
            harmony.Patch(AccessTools.Method(typeof(Language), nameof(Language.LoadLanguageFile)), prefix: loadLanguagesMethod);

            InternalLogger.Log("LanguagePatcher is done.", LogLevel.Debug);
        }

        internal static void AddCustomLanguageLine(string lineId, string text, string language)
        {
            if (!_customLines.ContainsKey(language))
                _customLines[language] = new();

            _customLines[language][lineId] = text;
        }

        internal static void AddCustomLanguageLines(string language, Dictionary<string, string> languageStrings)
        {
            if (!_customLines.ContainsKey(language))
                _customLines[language] = new();

            var customStrings = _customLines[language];
            
            foreach (var languageString in languageStrings)
            {
                customStrings[languageString.Key] = languageString.Value;
            }
        }
    }
}
