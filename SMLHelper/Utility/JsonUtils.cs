namespace SMLHelper.V2.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using SMLHelper.V2.Interfaces;
    using Oculus.Newtonsoft.Json;
    using Oculus.Newtonsoft.Json.Converters;

    /// <summary>
    /// A collection of utilities for interacting with JSON files.
    /// </summary>
    public static class JsonUtils
    {
        private static readonly JsonConverter[] defaultJsonConverters = new JsonConverter[]
        {
            new JsonConverters.KeyCodeConverter(),
            new StringEnumConverter(),
            new VersionConverter()
        };

        /// <summary>
        /// Loads a given <see cref="IJsonFile"/>'s options from the JSON file on disk and populates it.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IJsonFile"/> to use for
        /// deserialization.</typeparam>
        /// <param name="jsonFile">The <seealso cref="IJsonFile"/> to load.</param>
        /// <param name="saveDefaultsIfNotExist">Whether a config file containing default values should
        /// be created if it does not already exist.</param>
        /// <param name="jsonConverters">The <see cref="JsonConverter"/>s to use when
        /// deserializing. By default, <see cref="JsonConverters.KeyCodeConverter"/>,
        /// <see cref="StringEnumConverter"/> and <see cref="VersionConverter"/> will be used.</param>
        /// <returns>A <seealso cref="IJsonFile"/> with its properties and fields populated from the 
        /// associated JSON file.</returns>
        /// <seealso cref="Load{T}(bool, JsonConverter[])"/>
        /// <seealso cref="Save{T}(T, JsonConverter[])"/>
        /// <example>
        /// <code>
        /// using System;
        /// using SMLHelper.V2.Options;
        /// using SMLHelper.V2.Utility;
        /// using UnityEngine;
        /// 
        /// public class MyConfig : ConfigFile
        /// {
        ///     public KeyCode ActivationKey { get; set; } = KeyCode.Backspace;
        /// }
        /// 
        /// public static class MyMod
        /// {
        ///     public static MyConfig Config = JsonUtils.Load(new MyConfig());
        ///     public static void Initialize() {
        ///         string activationKey = KeyCodeUtils.KeyCodeToString(Config.ActivationKey);
        ///         Console.WriteLine($"[MyMod] LOADED: ActivationKey = {activationKey}");
        ///         // Will print "[MyMod] LOADED: ActivationKey = Backspace" in Player.log on first run,
        ///         // and whatever value is saved in the config.json for the "ActivationKey" 
        ///         // property on subsequent runs.
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T Load<T>(T jsonFile, bool saveDefaultsIfNotExist = true,
            params JsonConverter[] jsonConverters) where T : IJsonFile
        {
            var path = jsonFile.JsonFilePath;
            if (Directory.Exists(Path.GetDirectoryName(path)) && File.Exists(path))
            {
                try
                {
                    var converters = new List<JsonConverter>(jsonConverters);
                    converters.AddRange(defaultJsonConverters);

                    string serializedJson = File.ReadAllText(path);
                    jsonFile = JsonConvert.DeserializeObject<T>(
                        serializedJson, converters.ToArray()
                    );
                }
                catch (Exception)
                {
                    Logger.Announce("Could not parse JSON file, " +
                        $"loading default values: {path}", LogLevel.Error, true);
                }
            }
            else if (saveDefaultsIfNotExist)
            {
                Save(jsonFile);
            }
            return jsonFile;
        }

        /// <summary>
        /// Loads a given <see cref="IJsonFile"/>'s options from the JSON file on disk and populates it.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IJsonFile"/> to use for deserialization.</typeparam>
        /// <param name="saveDefaultsIfNotExist">Whether a config file containing default values should
        /// be created if it does not already exist.</param>
        /// <param name="jsonConverters">The <see cref="JsonConverter"/>s to use when
        /// deserializing. By default, <see cref="JsonConverters.KeyCodeConverter"/>,
        /// <see cref="StringEnumConverter"/> and <see cref="VersionConverter"/> will be used.</param>
        /// <returns>A <seealso cref="IJsonFile"/> with its properties and fields populated from the 
        /// associated JSON file.</returns>
        /// <seealso cref="Load{T}(T, bool, JsonConverter[])"/>
        /// <seealso cref="Save{T}(T, JsonConverter[])"/>
        /// <example>
        /// <code>
        /// using System;
        /// using SMLHelper.V2.Options;
        /// using SMLHelper.V2.Utility;
        /// using UnityEngine;
        /// 
        /// public class MyConfig : ConfigFile
        /// {
        ///     public KeyCode ActivationKey { get; set; } = KeyCode.Backspace;
        /// }
        /// 
        /// public static class MyMod
        /// {
        ///     public static MyConfig Config = JsonUtils.Load&lt;MyConfig&gt;();
        ///     public static void Initialize() {
        ///         string activationKey = KeyCodeUtils.KeyCodeToString(Config.ActivationKey);
        ///         Console.WriteLine($"[MyMod] LOADED: ActivationKey = {activationKey}");
        ///         // Will print "[MyMod] LOADED: ActivationKey = Backspace" in Player.log on first run,
        ///         // and whatever value is saved in the config.json for the "ActivationKey" 
        ///         // property on subsequent runs.
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T Load<T>(bool saveDefaultsIfNotExist = true,
            params JsonConverter[] jsonConverters) where T : IJsonFile, new()
            => Load(new T(), saveDefaultsIfNotExist, jsonConverters);

        /// <summary>
        /// Saves the <see cref="IJsonFile"/>'s currently set properties and fields to it associated JSON 
        /// file on disk.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IJsonFile"/> to use for deserialization.</typeparam>
        /// <param name="jsonFile">The <seealso cref="IJsonFile"/> to save.</param>
        /// <param name="jsonConverters">The <see cref="JsonConverter"/>s to use when serializing. By
        /// default, <see cref="JsonConverters.KeyCodeConverter"/>, <see cref="StringEnumConverter"/> and
        /// <see cref="VersionConverter"/> will be used.</param>
        /// <seealso cref="Load{T}(T, bool, JsonConverter[])"/>
        /// <seealso cref="Load{T}(bool, JsonConverter[])"/>
        /// <example>
        /// <code>
        /// using System;
        /// using SMLHelper.V2.Options;
        /// using SMLHelper.V2.Utility;
        /// using UnityEngine;
        /// 
        /// public class MyConfig : ConfigFile
        /// {
        ///     public KeyCode ActivationKey { get; set; } = KeyCode.Backspace;
        /// }
        /// 
        /// public static class MyMod
        /// {
        ///     public static MyConfig Config = JsonUtils.Load(new MyConfig());
        ///     public static void Initialize() {
        ///         string activationKey = KeyCodeUtils.KeyCodeToString(Config.ActivationKey);
        ///         Console.WriteLine($"[MyMod] LOADED: ActivationKey = {activationKey}");
        ///         // Will print "[MyMod] LOADED: ActivationKey = Backspace" in Player.log on first run,
        ///         // and whatever value is saved in the config.json for the "ActivationKey" 
        ///         // property on subsequent runs.
        ///         Config.ActivationKey = KeyCode.Mouse2;
        ///         JsonUtils.Save(Config);
        ///         // The "ActivationKey" property in config.json will now read "MouseButtonMiddle"
        ///     }
        /// }
        /// </code>
        /// </example>
        public static void Save<T>(T jsonFile, params JsonConverter[] jsonConverters) where T : IJsonFile
        {
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            using (var jsonTextWriter = new JsonTextWriter(stringWriter)
            {
                Indentation = 4,
                Formatting = Formatting.Indented
            })
            {
                var converters = new List<JsonConverter>(jsonConverters);
                converters.AddRange(defaultJsonConverters);

                var jsonSerializer = new JsonSerializer();
                foreach (var jsonConverter in converters)
                {
                    jsonSerializer.Converters.Add(jsonConverter);
                }
                jsonSerializer.Serialize(jsonTextWriter, jsonFile);
            }

            var path = jsonFile.JsonFilePath;
            var fileInfo = new FileInfo(path);
            fileInfo.Directory.Create(); // Only creates the directory if it doesn't already exist
            File.WriteAllText(path, stringBuilder.ToString());
        }
    }
}
