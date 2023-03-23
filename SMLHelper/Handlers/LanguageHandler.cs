using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace SMLHelper.Handlers
{
    using Patchers;
    using Utility;

    /// <summary>
    /// A handler for adding custom language lines.
    /// </summary>
    public static class LanguageHandler
    {
        /// <summary>
        /// Allows you to define a language entry into the game.
        /// </summary>
        /// <param name="lineId">The ID of the entry, this is what is used to get the actual text.</param>
        /// <param name="text">The actual text related to the entry.</param>
        /// <param name="language">The language for this specific entry. Defaults to English.</param>
        public static void SetLanguageLine(string lineId, string text, string language = "English")
        {
            LanguagePatcher.AddCustomLanguageLine(lineId, text, language);
        }

        /// <summary>
        /// <para>Registers a folder path as a Multi-Language json files folder.</para>
        /// Please make sure that the passed folder contains json files that are properly named after the language each json file localizes.
        /// </summary>
        /// <param name="languageFolderName">the folder name. This folder is expected to be found at ModFolder/<paramref name="languageFolderName"/>.</param>
        public static void RegisterLocalizationFolder(string languageFolderName = "Localization")
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            callingAssembly = callingAssembly == Assembly.GetExecutingAssembly()
                ? ReflectionHelper.CallingAssemblyByStackTrace()
                : callingAssembly;
            
            var path = Path.Combine(Path.GetDirectoryName(callingAssembly.Location)!, languageFolderName);
            if (!Directory.Exists(path))
            {
                InternalLogger.Error($"Directory '{path}' does not exist. Skipping localization registration.");
                return;
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var content = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file));
                if (content is null)
                {
                    InternalLogger.Warn($"Localization file '{file}' is empty, skipping registration.");
                    continue;
                }
                
                var languageName = Path.GetFileNameWithoutExtension(file);
                RegisterLocalization(languageName, content);
            }
        }

        /// <summary>
        /// Registers language entries for a specific language.
        /// </summary>
        /// <param name="language">The language to register the entries to.</param>
        /// <param name="languageStrings">The language entries to register.</param>
        public static void RegisterLocalization(string language, Dictionary<string, string> languageStrings)
        {
            if (string.IsNullOrEmpty(language) || languageStrings is null || languageStrings.Count <= 0)
            {
                InternalLogger.Error($"Localization registration failed. Language name or values are empty or null. Stacktrace: {Environment.StackTrace}");
                return;
            }
            
            LanguagePatcher.AddCustomLanguageLines(language, languageStrings);
        }

        /// <summary>
        /// Allows you to set the display name of a specific <see cref="TechType"/>.
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> whose display name that is to be changed.</param>
        /// <param name="text">The new display name for the chosen <see cref="TechType"/>.</param>
        /// <param name="language">The language for this entry. Defaults to English.</param>
        public static void SetTechTypeName(TechType techType, string text, string language = "English")
        {
            LanguagePatcher.AddCustomLanguageLine(techType.AsString(), text, language);
        }

        /// <summary>
        /// Allows you to set the tooltip of a specific <see cref="TechType"/>.
        /// </summary>
        /// <param name="techType">The <see cref="TechType"/> whose tooltip that is to be changed.</param>
        /// <param name="text">The new tooltip for the chosen <see cref="TechType"/>.</param>
        /// <param name="language">The language for this entry. Defaults to English.</param>
        public static void SetTechTypeTooltip(TechType techType, string text, string language = "English")
        {
            LanguagePatcher.AddCustomLanguageLine($"Tooltip_{techType.AsString()}", text, language);
        }
    }
}
