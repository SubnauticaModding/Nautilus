namespace SMLHelper.V2.Json
{
    using Converters;
    using ExtensionMethods;
    using Interfaces;
    using Oculus.Newtonsoft.Json;
    using Oculus.Newtonsoft.Json.Converters;
    using System.IO;
    using System.Reflection;
    using System.Linq;

    /// <summary>
    /// A simple implementation of <see cref="IJsonFile"/> for use with config files.
    /// </summary>
    public abstract class ConfigFile : IJsonFile
    {
        [JsonIgnore]
        private readonly string JsonFilename;
        [JsonIgnore]
        private readonly string JsonPath;

        /// <summary>
        /// The file path at which the JSON file is accessible for reading and writing.
        /// </summary>
        public string JsonFilePath => Path.Combine(JsonPath, $"{JsonFilename}.json");

        [JsonIgnore]
        private static readonly JsonConverter[] alwaysIncludedJsonConverters = new JsonConverter[] {
            new KeyCodeConverter(),
            new FloatConverter(),
            new StringEnumConverter(),
            new VersionConverter()
        };

        /// <summary>
        /// The <see cref="JsonConverter"/>s that should always be used when reading/writing JSON data.
        /// </summary>
        /// <seealso cref="alwaysIncludedJsonConverters"/>
        public JsonConverter[] AlwaysIncludedJsonConverters => alwaysIncludedJsonConverters;

        /// <summary>
        /// Creates a new instance of <see cref="ConfigFile"/>, parsing the filename and subfolder from a
        /// <see cref="ConfigFileAttribute"/> if declared, or with default values otherwise.
        /// </summary>
        public ConfigFile()
        {
            if (GetType().GetCustomAttributes(typeof(ConfigFileAttribute), true).FirstOrDefault() is ConfigFileAttribute configFile)
            {
                JsonFilename = configFile.Filename;
                JsonPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetCallingAssembly().Location),
                    configFile.Subfolder);
            }
            else
            {
                JsonFilename = "config";
                JsonPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConfigFile"/>.
        /// </summary>
        /// <param name="fileName">The name of the <see cref="ConfigFile"/>, "config" by default.</param>
        /// <param name="subfolder">Optional subfolder for the <see cref="ConfigFile"/>.</param>
        /// <example>
        /// <code>
        /// using SMLHelper.V2.Options;
        /// using UnityEngine;
        /// 
        /// public class MyConfig : ConfigFile
        /// {
        ///     public KeyCode ActivationKey { get; set; } = KeyCode.Escape;
        ///     public MyConfig() : base("options", "Config Files") { }
        ///     // The config file will be stored at the path "QMods\YourModName\Config Files\options.json"
        /// }
        /// </code>
        /// </example>
        protected ConfigFile(string fileName = "config", string subfolder = null)
        {
            JsonFilename = fileName;
            JsonPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetCallingAssembly().Location),
                string.IsNullOrEmpty(subfolder) ? string.Empty : subfolder);
        }

        /// <summary>
        /// Loads the JSON properties from the file on disk into the <see cref="ConfigFile"/>.
        /// </summary>
        /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
        /// already exist.</param>
        /// <seealso cref="Save()"/>
        /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
        public void Load(bool createFileIfNotExist = true)
            => this.LoadJson(JsonFilePath, createFileIfNotExist, AlwaysIncludedJsonConverters.Distinct().ToArray());

        /// <summary>
        /// Saves the current fields and properties of the <see cref="ConfigFile"/> as JSON properties to the file on disk.
        /// </summary>
        /// <seealso cref="Load(bool)"/>
        /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
        public void Save() => this.SaveJson(JsonFilePath,
            AlwaysIncludedJsonConverters.Distinct().ToArray());

        /// <summary>
        /// Loads the JSON properties from the file on disk into the <see cref="ConfigFile"/>.
        /// </summary>
        /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
        /// already exist.</param>
        /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for serialization.
        /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
        /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
        /// <seealso cref="Load(bool)"/>
        public void LoadWithConverters(bool createFileIfNotExist = true, params JsonConverter[] jsonConverters)
            => this.LoadJson(JsonFilePath, true,
                AlwaysIncludedJsonConverters.Concat(jsonConverters).Distinct().ToArray());

        /// <summary>
        /// Saves the current fields and properties of the <see cref="ConfigFile"/> as JSON properties to the file on disk.
        /// </summary>
        /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for deserialization.
        /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
        /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
        /// <seealso cref="Save"/>
        public void SaveWithConverters(params JsonConverter[] jsonConverters)
            => this.SaveJson(JsonFilePath,
                AlwaysIncludedJsonConverters.Concat(jsonConverters).Distinct().ToArray());
    }
}
