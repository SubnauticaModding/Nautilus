namespace SMLHelper.V2.Patchers
{
    using Harmony;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class LanguagePatcher
    {
        private const string LanguageDir = "./QMods/Modding Helper/Language";
        private const string LanguageOrigDir = LanguageDir + "/Originals";
        private const string LanguageOverDir = LanguageDir + "/Overrides";
        private const char TextDelimiterOpen = '{';
        private const char TextDelimiterClose = '}';
        private const char KeyValueSeparator = ':';
        private static readonly Regex OverrideRegex = new Regex("(?<key>[\\w]+)\\s*:\\s*{(?<value>([~!@#$%^&*()\\-_=+\\[\\];:\"',<>\\/?]|{\\d+}|[\\w\\s\n])+)}(\n|\r\n)*", RegexOptions.Multiline);

        private static readonly Dictionary<string, Dictionary<string, string>> originalCustomLines = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, string> customLines = new Dictionary<string, string>();
        private static Type languageType = typeof(Language);

        internal static string GetCustomLine(string key)
        {
            return customLines[key];
        }

        internal static void Postfix(ref Language __instance)
        {
            // Direct access to private fields made possible by https://github.com/CabbageCrow/AssemblyPublicizer/
            // See README.md for details.
            Dictionary<string, string> strings = __instance.strings;
            foreach (KeyValuePair<string, string> a in customLines)
            {
                strings[a.Key] = a.Value;
            }
        }

        internal static void Patch(HarmonyInstance harmony)
        {
            if (!Directory.Exists(LanguageDir))
                Directory.CreateDirectory(LanguageDir);

            WriteOriginalCustomLines();

            ReadOverrideCustomLines();

            harmony.Patch(
                original: languageType.GetMethod("LoadLanguageFile", BindingFlags.NonPublic | BindingFlags.Instance),
                prefix: null,
                postfix: new HarmonyMethod(typeof(LanguagePatcher).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic)));

            Logger.Log("LanguagePatcher is done.", LogLevel.Debug);
        }

        private static void WriteOriginalCustomLines()
        {
            if (!Directory.Exists(LanguageOrigDir))
                Directory.CreateDirectory(LanguageOrigDir);

            if (originalCustomLines.Count == 0)
                return;

            int filesWritten = 0;
            foreach (string modKey in originalCustomLines.Keys)
            {
                WriteOriginalLinesFile(modKey);
                filesWritten++;
            }

            if (filesWritten > 0)
                Logger.Log($"Updated {filesWritten} original language files.", LogLevel.Debug);
        }

        private static void WriteOriginalLinesFile(string modKey)
        {
            Dictionary<string, string> modCustomLines = originalCustomLines[modKey];
            var text = new StringBuilder();
            foreach (string langLineKey in modCustomLines.Keys)
            {
                string value = modCustomLines[langLineKey];
                text.AppendLine($"{langLineKey}{KeyValueSeparator}{TextDelimiterOpen}{value}{TextDelimiterClose}");
            }

            File.WriteAllText($"{LanguageOrigDir}/{modKey}.txt", text.ToString(), Encoding.UTF8);
        }

        private static void ReadOverrideCustomLines()
        {
            if (!Directory.Exists(LanguageOverDir))
                Directory.CreateDirectory(LanguageOverDir);

            string[] files = Directory.GetFiles(LanguageOverDir);

            if (files.Length == 0)
                return;

            Logger.Log($"{files.Length} language override files found.", LogLevel.Debug);

            foreach (string file in files)
            {
                string modName = Path.GetFileNameWithoutExtension(file);

                if (!originalCustomLines.ContainsKey(modName))
                    continue; // Not for a mod we know about

                string text = File.ReadAllText(file, Encoding.UTF8);

                Dictionary<string, string> originalLines = originalCustomLines[modName];

                int overridesApplied = ExtractCustomLinesFromText(modName, text, originalLines);

                Logger.Log($"Applied {overridesApplied} language overrides to mod {modName}.", LogLevel.Info);
            }
        }

        internal static int ExtractCustomLinesFromText(string modName, string text, Dictionary<string, string> originalLines)
        {
            MatchCollection matches = OverrideRegex.Matches(text);

            int overridesApplied = 0;
            foreach (Match match in matches)
            {
                string key = match.Groups["key"].Value;
                string value = match.Groups["value"].Value;

                if (!originalLines.ContainsKey(key))
                {
                    Logger.Log($"Key '{key}' in language override file for '{modName}' did not match an original key.", LogLevel.Warn);
                    continue; // Skip keys we don't recognize
                }

                customLines[key] = value;

                overridesApplied++;
            }

            return overridesApplied;
        }

        internal static void AddCustomLanguageLine(string modAssemblyName, string lineId, string text)
        {
            if (!originalCustomLines.ContainsKey(modAssemblyName))
                originalCustomLines.Add(modAssemblyName, new Dictionary<string, string>());

            originalCustomLines[modAssemblyName][lineId] = text;
            customLines[lineId] = text;
        }
    }
}
