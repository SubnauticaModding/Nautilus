using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nautilus.Utility;

/// <summary>
/// A collection of utilities for interacting with JSON files.
/// </summary>
public static class JsonUtils
{
    private static class Defaults
    {
        private static Dictionary<Type, object> _defaults = new();

        public static object GetValue(Type type)
        {
            if (_defaults.TryGetValue(type, out var result))
            {
                return result;
            }
            
            return _defaults[type] = Activator.CreateInstance(type);
        }
    }
    
    private static string GetDefaultPath<T>(Assembly assembly) where T : class
    {
        return Path.Combine(
            Path.Combine(Paths.ConfigPath, assembly.GetName().Name),
            $"{GetName<T>()}.json"
        );
    }

    private static string GetName<T>(bool camelCase = true) where T : class
    {
        string name = typeof(T).Name;
        if (camelCase)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            name = currentCulture.TextInfo.ToTitleCase(name)
                .Replace("_", string.Empty).Replace(" ", string.Empty);
            name = char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
        return name;
    }
    
    private static void PopulateDefaults<T>(T target, JsonObjectContract contract) where T : class
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var @default = Defaults.GetValue(target.GetType());
        
        foreach (var property in contract.Properties)
        {
            if (property.Writable && property.ValueProvider != null && !property.Ignored)
            {
                property.ValueProvider.SetValue(target, property.ValueProvider.GetValue(@default));
            }
        }
    }

    /// <summary>
    /// Create an instance of <typeparamref name="T"/>, populated with data from the JSON file at the given 
    /// <paramref name="path"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to initialise and populate with JSON data.</typeparam>
    /// <param name="path">The path on disk at which the JSON file can be found.</param>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not 
    /// already exist.</param>
    /// <param name="jsonConverters">An array of <see cref="JsonConverter"/>s to be used for deserialization.</param>
    /// <returns>The <typeparamref name="T"/> instance populated with data from the JSON file at
    /// <paramref name="path"/>, or default values if it cannot be found or an error is encountered while parsing the
    /// file.</returns>
    /// <seealso cref="Load{T}(T, string, bool, JsonConverter[])"/>
    /// <seealso cref="Save{T}(T, string, JsonConverter[])"/>
    public static T Load<T>(string path = null, bool createFileIfNotExist = true,
        params JsonConverter[] jsonConverters) where T : class, new()
    {
        if (string.IsNullOrEmpty(path))
        {
            path = GetDefaultPath<T>(Assembly.GetCallingAssembly());
        }

        if (Directory.Exists(Path.GetDirectoryName(path)) && File.Exists(path))
        {
            try
            {
                string serializedJson = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(
                    serializedJson, jsonConverters
                );
            }
            catch (Exception ex)
            {
                InternalLogger.Announce($"Could not parse JSON file, loading default values: {path}", LogLevel.Warning, true);
                InternalLogger.Error(ex.Message);
                InternalLogger.Error(ex.StackTrace);
                return new T();
            }
        }
        else if (createFileIfNotExist)
        {
            T jsonObject = new();
            Save(jsonObject, path, jsonConverters);
            return jsonObject;
        }
        else
        {
            return new T();
        }
    }

    /// <summary>
    /// Loads data from the JSON file at <paramref name="path"/> into the <paramref name="jsonObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="jsonObject"/> to populate with JSON data.</typeparam>
    /// <param name="jsonObject">The <typeparamref name="T"/> instance to popular with JSON data.</param>
    /// <param name="path">The path on disk at which the JSON file can be found.</param>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <param name="jsonConverters">An array of <see cref="JsonConverter"/>s to be used for deserialization.</param>
    /// <seealso cref="Load{T}(string, bool, JsonConverter[])"/>
    /// <seealso cref="Save{T}(T, string, JsonConverter[])"/>
    public static void Load<T>(T jsonObject, string path = null, bool createFileIfNotExist = true,
        params JsonConverter[] jsonConverters) where T : class
    {
        if (string.IsNullOrEmpty(path))
        {
            path = GetDefaultPath<T>(Assembly.GetCallingAssembly());
        }
        
        JsonSerializerSettings jsonSerializerSettings = new()
        {
            Converters = jsonConverters,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new DefaultContractResolver(),
        };
        
        if (Directory.Exists(Path.GetDirectoryName(path)) && File.Exists(path))
        {
            try
            {
                string serializedJson = File.ReadAllText(path);
                JsonConvert.PopulateObject(
                    serializedJson, jsonObject, jsonSerializerSettings
                );
            }
            catch (Exception ex)
            {
                InternalLogger.Announce($"Could not parse JSON file, instance values unchanged: {path}", LogLevel.Warning, true);
                InternalLogger.Error(ex.Message);
                InternalLogger.Error(ex.StackTrace);
            }
        }
        else if (createFileIfNotExist)
        {
            try
            {
                PopulateDefaults(jsonObject,
                    jsonSerializerSettings.ContractResolver.ResolveContract(jsonObject.GetType()) as JsonObjectContract);
                Save(jsonObject, path, jsonConverters);
            }
            catch (Exception e)
            {
                InternalLogger.Announce($"Could not create defaults, instance values unchanged: {path}", LogLevel.Warning, true);
                InternalLogger.Error(e.Message);
                InternalLogger.Error(e.StackTrace);
            }
        }
    }
    
    /// <inheritdoc cref="Load{T}(string,bool,Newtonsoft.Json.JsonConverter[])"/>
    public static async Task<T> LoadAsync<T>(string path = null, bool createFileIfNotExist = true,
        params JsonConverter[] jsonConverters) where T : class, new()
    {
        if (string.IsNullOrEmpty(path))
        {
            path = GetDefaultPath<T>(Assembly.GetCallingAssembly());
        }

        if (Directory.Exists(Path.GetDirectoryName(path)) && File.Exists(path))
        {
            try
            {
                using var reader = new StreamReader(File.OpenRead(path));
                string serializedJson = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(
                    serializedJson, jsonConverters
                );
            }
            catch (Exception ex)
            {
                InternalLogger.Announce($"Could not parse JSON file, loading default values: {path}", LogLevel.Warning, true);
                InternalLogger.Error(ex.Message);
                InternalLogger.Error(ex.StackTrace);
                return new T();
            }
        }
        else if (createFileIfNotExist)
        {
            T jsonObject = new();
            await SaveAsync(jsonObject, path, jsonConverters);
            return jsonObject;
        }
        else
        {
            return new T();
        }
    }

    /// <inheritdoc cref="Load{T}(T,string,bool,Newtonsoft.Json.JsonConverter[])"/>
    public static async Task LoadAsync<T>(T jsonObject, string path = null, bool createFileIfNotExist = true,
        params JsonConverter[] jsonConverters) where T : class
    {
        if (string.IsNullOrEmpty(path))
        {
            path = GetDefaultPath<T>(Assembly.GetCallingAssembly());
        }
        
        JsonSerializerSettings jsonSerializerSettings = new()
        {
            Converters = jsonConverters,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new DefaultContractResolver(),
        };
        
        if (Directory.Exists(Path.GetDirectoryName(path)) && File.Exists(path))
        {
            try
            {
                using var reader = new StreamReader(File.OpenRead(path));
                string serializedJson = await reader.ReadToEndAsync();
                JsonConvert.PopulateObject(
                    serializedJson, jsonObject, jsonSerializerSettings
                );
            }
            catch (Exception ex)
            {
                InternalLogger.Announce($"Could not parse JSON file, instance values unchanged: {path}", LogLevel.Warning, true);
                InternalLogger.Error(ex.Message);
                InternalLogger.Error(ex.StackTrace);
            }
        }
        else if (createFileIfNotExist)
        {
            try
            {
                PopulateDefaults(jsonObject,
                    jsonSerializerSettings.ContractResolver.ResolveContract(jsonObject.GetType()) as JsonObjectContract);
                await SaveAsync(jsonObject, path, jsonConverters);
            }
            catch (Exception e)
            {
                InternalLogger.Announce($"Could not create defaults, instance values unchanged: {path}", LogLevel.Warning, true);
                InternalLogger.Error(e.Message);
                InternalLogger.Error(e.StackTrace);
            }
        }
    }

    /// <summary>
    /// Saves the <paramref name="jsonObject"/> parsed as JSON data to the JSON file at <paramref name="path"/>,
    /// creating it if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="jsonObject"/> to parse into JSON data.</typeparam>
    /// <param name="jsonObject">The <typeparamref name="T"/> instance to parse into JSON data.</param>
    /// <param name="path">The path on disk at which to store the JSON file.</param>
    /// <param name="jsonConverters">An array of <see cref="JsonConverter"/>s to be used for serialization.</param>
    /// <seealso cref="Load{T}(T, string, bool, JsonConverter[])"/>
    /// <seealso cref="Load{T}(string, bool, JsonConverter[])"/>
    public static void Save<T>(T jsonObject, string path = null,
        params JsonConverter[] jsonConverters) where T : class
    {
        if (string.IsNullOrEmpty(path))
        {
            path = GetDefaultPath<T>(Assembly.GetCallingAssembly());
        }

        string json = ConvertToJson(jsonObject, jsonConverters);

        FileInfo fileInfo = new(path);
        fileInfo.Directory.Create(); // Only creates the directory if it doesn't already exist
        File.WriteAllText(path, json);
    }

    /// <inheritdoc cref="Save{T}"/>
    public static async Task SaveAsync<T>(T jsonObject, string path = null, 
        params JsonConverter[] jsonConverters) where T : class
    {
        if (string.IsNullOrEmpty(path))
        {
            path = GetDefaultPath<T>(Assembly.GetCallingAssembly());
        }

        string json = ConvertToJson(jsonObject, jsonConverters);

        FileInfo fileInfo = new(path);
        fileInfo.Directory.Create();
        using var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
        await writer.WriteAsync(json);
    }

    private static string ConvertToJson<T>(T jsonObject, params JsonConverter[] jsonConverters) where T : class
    {
        StringBuilder stringBuilder = new();
        StringWriter stringWriter = new(stringBuilder);
        using (JsonTextWriter jsonTextWriter = new(stringWriter))
        {
            jsonTextWriter.Indentation = 4;
            jsonTextWriter.Formatting = Formatting.Indented;
            JsonSerializer jsonSerializer = new();
            foreach (JsonConverter jsonConverter in jsonConverters)
            {
                jsonSerializer.Converters.Add(jsonConverter);
            }
            jsonSerializer.Serialize(jsonTextWriter, jsonObject);
        }

        return stringBuilder.ToString();
    }
}