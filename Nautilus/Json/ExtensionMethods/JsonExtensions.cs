using System.Threading.Tasks;
using Nautilus.Utility;
using Newtonsoft.Json;

namespace Nautilus.Json.ExtensionMethods;

/// <summary>
/// Extension methods for parsing objects as JSON data.
/// </summary>
public static class JsonExtensions
{
    /// <param name="jsonObject">The object instance to load the properties into.</param>
    /// <typeparam name="T">The type of the <paramref name="jsonObject"/>.</typeparam>
    extension<T>(T jsonObject) where T : class
    {
        /// <summary>
        /// Loads the JSON properties from a file on disk into the <paramref name="jsonObject"/>.
        /// </summary>
        /// <param name="path">The file path to the JSON file to parse.</param>
        /// <param name="createIfNotExist">Whether a new JSON file should be created with default values if it does not
        /// already exist.</param>
        /// <param name="jsonConverters">The <see cref="JsonConverter"/>s to be used for deserialization.</param>
        /// <seealso cref="SaveJson{T}(T, string, JsonConverter[])"/>
        public void LoadJson(string path = null,
            bool createIfNotExist = true, params JsonConverter[] jsonConverters)
        {
            JsonUtils.Load(jsonObject, path, createIfNotExist, jsonConverters);
        }

        /// <inheritdoc cref="LoadJson{T}"/>
        public async Task LoadJsonAsync(string path = null,
            bool createIfNotExist = true, params JsonConverter[] jsonConverters)
        {
            await JsonUtils.LoadAsync(jsonObject, path, createIfNotExist, jsonConverters);
        }

        /// <summary>
        /// Saves the fields and properties of the <paramref name="jsonObject"/> as JSON properties to the file on disk.
        /// </summary>
        /// <param name="path">The file path at which to save the JSON file.</param>
        /// <param name="jsonConverters">The <see cref="JsonConverter"/>s to be used for serialization.</param>
        public void SaveJson(string path = null,
            params JsonConverter[] jsonConverters)
        {
            JsonUtils.Save(jsonObject, path, jsonConverters);
        }

        /// <inheritdoc cref="SaveJson{T}"/>
        public async Task SaveJsonAsync(string path = null,
            params JsonConverter[] jsonConverters)
        {
            await JsonUtils.SaveAsync(jsonObject, path, jsonConverters);
        }
    }
}