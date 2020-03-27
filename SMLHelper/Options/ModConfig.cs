using System;
using System.IO;
using System.Reflection;
using System.Text;
using Oculus.Newtonsoft.Json;

namespace SMLHelper.V2.Options
{
    /// <summary>
    /// A framework for handling JSON config files for your mods.
    /// </summary>
    /// <example>
    /// <code>
    /// using System;
    /// using SMLHelper.V2.Options;
    /// using SMLHelper.V2.Utility;
    /// using UnityEngine;
    /// 
    /// public class Config : ModConfig {
    ///     public KeyCode ActivationKey { get; set; } = KeyCode.Backspace;
    ///     public Config() : base(fileName: "config") { }
    /// }
    /// 
    /// public class MyMod {
    ///     public static Config Config = ModConfig.Load(new Config());
    ///     public static void Initialize() {
    ///         string activationKey = KeyCodeUtils.KeyCodeToString(Config.ActivationKey);
    ///         Console.WriteLine($"[MyMod] LOADED: ActivationKey = {activationKey}");
    ///         // Will print "[MyMod] LOADED: ActivationKey = Backspace" in Player.log on first run,
    ///         // and whatever value they have saved in their config.json for the "ActivationKey"
    ///         // property on subsequent runs.
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ModConfig
    {
        [JsonIgnore]
        private readonly string filePath;
        [JsonIgnore]
        private readonly string fileName;
        [JsonIgnore]
        private readonly string subfolder;
        /// <summary>
        /// The full path to the config file.
        /// </summary>
        protected string ConfigPath => Path.Combine(
            filePath,
            string.IsNullOrEmpty(subfolder) ? string.Empty : subfolder,
            $"{fileName}.json"
        );

        /// <summary>
        /// Creates a new instance of <see cref="ModConfig"/>
        /// </summary>
        /// <param name="fileName">The name of the config file, "config" by default.</param>
        /// <param name="subfolder">Optional subfolder for the config file.</param>
        public ModConfig(string fileName = "config", string subfolder = null)
        {
            filePath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

            if (!string.IsNullOrEmpty(fileName))
            {
                this.fileName = fileName;
            }
            this.subfolder = subfolder;
        }

        /// <summary>
        /// Loads a given <see cref="ModConfig"/>'s options from the JSON file on disk and populates it.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ModConfig"/> to use for deserialization.</typeparam>
        /// <param name="config">The <seealso cref="ModConfig"/> to load.</param>
        /// <param name="saveDefaultConfigIfNotExist">Whether a config file creating default values should be
        /// created if it does not already exist.</param>
        /// <returns>A <seealso cref="ModConfig"/> with its properties and fields populated from the 
        /// associated JSON file.</returns>
        /// <example>
        /// <code>
        /// using System;
        /// using SMLHelper.V2.Options;
        /// using SMLHelper.V2.Utility;
        /// using UnityEngine;
        /// 
        /// public class Config : ModConfig {
        ///     public KeyCode ActivationKey { get; set; } = KeyCode.Backspace;
        ///     public Config() : base(fileName: "config") { }
        /// }
        /// 
        /// public class MyMod {
        ///     public static Config Config = ModConfig.Load(new Config());
        ///     public static void Initialize() {
        ///         string activationKey = KeyCodeUtils.KeyCodeToString(Config.ActivationKey);
        ///         Console.WriteLine($"[MyMod] LOADED: ActivationKey = {activationKey}");
        ///         // Will print "[MyMod] LOADED: ActivationKey = Backspace" in Player.log on first run,
        ///         // and whatever value they have saved in their config.json for the "ActivationKey"
        ///         // property on subsequent runs.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Save"/>
        public static T Load<T>(T config, bool saveDefaultConfigIfNotExist = true) where T : ModConfig
        {
            var path = config.ConfigPath;
            if (Directory.Exists(Path.GetDirectoryName(path)) && File.Exists(path))
            {
                try
                {
                    string serializedJSON = File.ReadAllText(path);
                    config = JsonConvert.DeserializeObject<T>(serializedJSON,
                        new JsonConverters.KeyCodeConverter());
                }
                catch (Exception ex)
                {   // Ideally, what we want is to pop up a QMM Dialog letting the user know exactly what mod
                    // and exactly what file is causing the error, with an option to open a JSON validator in
                    // browser to help them fix the problem.
                    // We'd also want this issue to stop QMods from considering the mod loaded properly, or
                    // expose a method for mods to check if any mods had config file errors, so that mods
                    // which rely on all mods loading properly before performing actions can act accordingly.
                    // For now, without copying QMM's dialog class into SML, this is the best we can do.
                    Logger.Announce($"Could not parse JSON file: {path}", LogLevel.Error, true);
                    throw ex;
                }
            }
            else if (saveDefaultConfigIfNotExist)
            {
                config.Save();
            }
            return config;
        }

        /// <summary>
        /// Saves the <see cref="ModConfig"/>'s currently set properties and fields to it associated JSON 
        /// file on disk.
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using SMLHelper.V2.Options;
        /// using SMLHelper.V2.Utility;
        /// using UnityEngine;
        /// 
        /// public class Config : ModConfig {
        ///     public KeyCode ActivationKey { get; set; } = KeyCode.Backspace;
        ///     public Config() : base(fileName: "config") { }
        /// }
        /// 
        /// public class MyMod {
        ///     public static Config Config = ModConfig.Load(new Config());
        ///     public static void Initialize() {
        ///         string activationKey = KeyCodeUtils.KeyCodeToString(Config.ActivationKey);
        ///         Console.WriteLine($"[MyMod] LOADED: ActivationKey = {activationKey}");
        ///         // Will print "[MyMod] LOADED: ActivationKey = Backspace" in Player.log on first run,
        ///         // and whatever value they have saved in their config.json for the "ActivationKey" 
        ///         // property on subsequent runs.
        ///         Config.ActivationKey = KeyCode.Mouse2;
        ///         Config.Save();
        ///         // The "ActivationKey" property in config.json will now read "MouseButtonMiddle"
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="Load{T}(T, bool)"/>
        public void Save()
        {
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            using (var jsonTextWriter = new JsonTextWriter(stringWriter)
            {
                Indentation = 4,
                Formatting = Formatting.Indented
            })
            {
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Converters.Add(new JsonConverters.KeyCodeConverter());
                jsonSerializer.Serialize(jsonTextWriter, this);
            }

            var path = ConfigPath;
            var fileInfo = new FileInfo(path);
            fileInfo.Directory.Create(); // Only creates the directory if it doesn't already exist
            File.WriteAllText(path, stringBuilder.ToString());
        }
    }
}
