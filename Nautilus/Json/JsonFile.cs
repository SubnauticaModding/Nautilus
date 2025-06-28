using System;
using System.Linq;
using System.Threading.Tasks;
using Nautilus.Json.Converters;
using Nautilus.Json.Interfaces;
using Nautilus.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nautilus.Json;

/// <summary>
/// A simple abstract implementation of <see cref="IJsonFile"/>.
/// </summary>
public abstract class JsonFile : IJsonFile
{
    /// <summary>
    /// The file path at which the JSON file is accessible for reading and writing.
    /// </summary>
    [JsonIgnore]
    public abstract string JsonFilePath { get; }

    [JsonIgnore]
    private static readonly JsonConverter[] alwaysIncludedJsonConverters = new JsonConverter[] {
        new FloatConverter(),
        new KeyCodeConverter(),
        new StringEnumConverter(),
        new VersionConverter(),
        new Vector2Converter(),
        new Vector3Converter(),
        new Vector4Converter(),
        new Vector2IntConverter(),
        new Vector3IntConverter(),
        new QuaternionConverter(),
        new ColorConverter()
    };

    /// <summary>
    /// The <see cref="JsonConverter"/>s that should always be used when reading/writing JSON data.
    /// </summary>
    /// <seealso cref="alwaysIncludedJsonConverters"/>
    public virtual JsonConverter[] AlwaysIncludedJsonConverters => alwaysIncludedJsonConverters;

    /// <summary>
    /// An event that is invoked whenever the <see cref="JsonFile"/> is about to load data from disk.
    /// </summary>
    [JsonIgnore]
    public EventHandler<JsonFileEventArgs> OnStartedLoading;
    /// <summary>
    /// An event that is invoked whenever the <see cref="JsonFile"/> has finished loading data from disk.
    /// </summary>
    [JsonIgnore]
    public EventHandler<JsonFileEventArgs> OnFinishedLoading;

    /// <summary>
    /// An event that is invoked whenever the <see cref="JsonFile"/> is about to save data to disk.
    /// </summary>
    [JsonIgnore]
    public EventHandler<JsonFileEventArgs> OnStartedSaving;
    /// <summary>
    /// An event that is invoked whenever the <see cref="JsonFile"/> has finished saving data to disk.
    /// </summary>
    [JsonIgnore]
    public EventHandler<JsonFileEventArgs> OnFinishedSaving;

    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="JsonFile"/>.
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <seealso cref="LoadAsync"/>
    /// <seealso cref="Save()"/>
    /// <seealso cref="SaveAsync"/>
    /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
    public virtual void Load(bool createFileIfNotExist = true)
    {
        JsonFileEventArgs e = new(this);
        OnStartedLoading?.Invoke(this, e);
        JsonUtils.Load(this, JsonFilePath, createFileIfNotExist, AlwaysIncludedJsonConverters.Distinct().ToArray());
        OnFinishedLoading?.Invoke(this, e);
    }
    
    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="JsonFile"/>.
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <seealso cref="Load"/>
    /// <seealso cref="Save"/>
    /// <seealso cref="SaveAsync"/>
    /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
    public virtual async Task LoadAsync(bool createFileIfNotExist = true)
    {
        JsonFileEventArgs e = new(this);
        OnStartedLoading?.Invoke(this, e);
        await JsonUtils.LoadAsync(this, JsonFilePath, createFileIfNotExist,
            AlwaysIncludedJsonConverters.Distinct().ToArray());
        OnFinishedLoading?.Invoke(this, e);
    }

    /// <summary>
    /// Saves the current fields and properties of the <see cref="JsonFile"/> as JSON properties to the file on disk.
    /// </summary>
    /// <seealso cref="Load(bool)"/>
    /// <seealso cref="LoadAsync"/>
    /// <seealso cref="SaveAsync"/>
    /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
    public virtual void Save()
    {
        JsonFileEventArgs e = new(this);
        OnStartedSaving?.Invoke(this, e);
        JsonUtils.Save(this, JsonFilePath, AlwaysIncludedJsonConverters.Distinct().ToArray());
        OnFinishedSaving?.Invoke(this, e);
    }
    
    /// <summary>
    /// Saves the current fields and properties of the <see cref="JsonFile"/> as JSON properties to the file on disk.
    /// </summary>
    /// <seealso cref="Load(bool)"/>
    /// <seealso cref="LoadAsync"/>
    /// <seealso cref="Save"/>
    /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
    public virtual async Task SaveAsync()
    {
        JsonFileEventArgs e = new(this);
        OnStartedSaving?.Invoke(this, e);
        await JsonUtils.SaveAsync(this, JsonFilePath, AlwaysIncludedJsonConverters.Distinct().ToArray());
        OnFinishedSaving?.Invoke(this, e);
    }

    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="JsonFile"/>.
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for serialization.
    /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
    /// <seealso cref="Load(bool)"/>
    public virtual void LoadWithConverters(bool createFileIfNotExist = true, params JsonConverter[] jsonConverters)
    {
        JsonUtils.Load(this, JsonFilePath, true,
            AlwaysIncludedJsonConverters.Concat(jsonConverters).Distinct().ToArray());
    }
    
    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="JsonFile"/>.
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for serialization.
    /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="SaveWithConvertersAsync(JsonConverter[])"/>
    /// <seealso cref="LoadWithConverters"/>
    /// <seealso cref="Load(bool)"/>
    public virtual async Task LoadWithConvertersAsync(bool createFileIfNotExist = true, params JsonConverter[] jsonConverters)
    {
        await JsonUtils.LoadAsync(this, JsonFilePath, true,
            AlwaysIncludedJsonConverters.Concat(jsonConverters).Distinct().ToArray());
    }

    /// <summary>
    /// Saves the current fields and properties of the <see cref="JsonFile"/> as JSON properties to the file on disk.
    /// </summary>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for deserialization.
    /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
    /// <seealso cref="Save"/>
    public virtual void SaveWithConverters(params JsonConverter[] jsonConverters)
    {
        JsonUtils.Save(this, JsonFilePath,
            AlwaysIncludedJsonConverters.Concat(jsonConverters).Distinct().ToArray());
    }
    
    /// <summary>
    /// Saves the current fields and properties of the <see cref="JsonFile"/> as JSON properties to the file on disk.
    /// </summary>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for deserialization.
    /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
    /// <seealso cref="SaveWithConverters"/>
    /// <seealso cref="Save"/>
    public virtual async Task SaveWithConvertersAsync(params JsonConverter[] jsonConverters)
    {
        await JsonUtils.SaveAsync(this, JsonFilePath,
            AlwaysIncludedJsonConverters.Concat(jsonConverters).Distinct().ToArray());
    }
}