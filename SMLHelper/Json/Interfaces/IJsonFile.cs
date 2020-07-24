namespace SMLHelper.V2.Json.Interfaces
{
#if SUBNAUTICA
    using Oculus.Newtonsoft.Json;
#elif BELOWZERO
    using Newtonsoft.Json;
#endif

    /// <summary>
    /// A simple interface for a JSON file framework.
    /// </summary>
    public interface IJsonFile
    {
        /// <summary>
        /// The file path at which the JSON file is accessible for reading and writing.
        /// </summary>
        [JsonIgnore]
        string JsonFilePath { get; }

        /// <summary>
        /// The <see cref="JsonConverter"/>s that should always be used when reading/writing JSON data.
        /// </summary>
        [JsonIgnore]
        JsonConverter[] AlwaysIncludedJsonConverters { get; }

        /// <summary>
        /// A method for loading the JSON properties from disk.
        /// </summary>
        /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
        /// already exist.</param>
        /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for
        /// deserialization.</param>
        /// <seealso cref="Save(JsonConverter[])"/>
        void Load(bool createFileIfNotExist = true, params JsonConverter[] jsonConverters);

        /// <summary>
        /// A method for saving the JSON properties to disk.
        /// </summary>
        /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for serialization.</param>
        /// <seealso cref="Load(bool, JsonConverter[])"/>
        void Save(params JsonConverter[] jsonConverters);
    }
}
