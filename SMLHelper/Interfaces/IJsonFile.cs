namespace SMLHelper.V2.Interfaces
{
    using Oculus.Newtonsoft.Json;

    /// <summary>
    /// A simple interface for a JSON file framework.
    /// </summary>
    /// <seealso cref="Options.ConfigFile"/>
    /// <seealso cref="Utility.JsonUtils.Load{T}(T, bool, Oculus.Newtonsoft.Json.JsonConverter[])"/>
    /// <seealso cref="Utility.JsonUtils.Save{T}(T, Oculus.Newtonsoft.Json.JsonConverter[])"/>
    public interface IJsonFile
    {
        /// <summary>
        /// The file path at which the JSON file is accessible for reading and writing.
        /// </summary>
        [JsonIgnore]
        string JsonFilePath { get; }
    }
}
