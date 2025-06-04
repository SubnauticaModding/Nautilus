using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Nautilus.Json.Attributes;
using Nautilus.Json.Interfaces;
using Nautilus.Utility;
using Newtonsoft.Json;

namespace Nautilus.Json;

/// <summary>
/// An abstract implementation of <see cref="IJsonFile"/> intended for use with caching per-save data.
/// </summary>
public abstract class SaveDataCache : JsonFile
{
    private string QModId { get; init; }

    private bool InGame => !string.IsNullOrWhiteSpace(SaveLoadManager.GetTemporarySavePath());

    private string jsonFileName = null;
    private string JsonFileName => jsonFileName ??= GetType().GetCustomAttribute<FileNameAttribute>() switch
    {
        FileNameAttribute fileNameAttribute => fileNameAttribute.FileName,
        _ => QModId
    };

    /// <summary>
    /// The file path at which the JSON file is accessible for reading and writing.
    /// </summary>
    public override string JsonFilePath => Path.Combine(SaveLoadManager.GetTemporarySavePath(), QModId, $"{JsonFileName}.json");

    /// <summary>
    /// Creates a new instance of <see cref="SaveDataCache"/>, parsing the file name from <see cref="FileNameAttribute"/>
    /// if declared, or with default values otherwise.
    /// </summary>
    public SaveDataCache()
    {
        QModId = GetType().Assembly.GetName().Name;
    }

    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="SaveDataCache"/>.
    /// <br />
    /// If you are overriding this in a subclass, be aware that <see cref="Nautilus.Handlers.SaveDataHandler"/>.
    /// <see cref="Nautilus.Handlers.SaveDataHandler.RegisterSaveDataCache{T}"/> calls the <em>async</em> load function
    /// <see cref="LoadAsync"/>!
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <seealso cref="LoadAsync"/>
    /// <seealso cref="Save()"/>
    /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override void Load(bool createFileIfNotExist = true)
    {
        if (InGame)
        {
            base.Load(createFileIfNotExist);
            InternalLogger.Log($"[{QModId}] Loaded save data from {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot load save data when not in game!");
        }
    }
    
    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="SaveDataCache"/>.
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <seealso cref="Load"/>
    /// <seealso cref="SaveAsync"/>
    /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override async Task LoadAsync(bool createFileIfNotExist = true)
    {
        if (InGame)
        {
            await base.LoadAsync(createFileIfNotExist);
            InternalLogger.Log($"[{QModId}] Loaded save data from {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot load save data when not in game!");
        }
    }

    /// <summary>
    /// Saves the current fields and properties of the <see cref="SaveDataCache"/> as JSON properties to the file on disk.
    /// <br />
    /// If you are overriding this in a subclass, be aware that <see cref="Nautilus.Handlers.SaveDataHandler"/>.
    /// <see cref="Nautilus.Handlers.SaveDataHandler.RegisterSaveDataCache{T}"/> calls the <em>async</em> saving
    /// function <see cref="SaveAsync"/>!
    /// </summary>
    /// <seealso cref="Load(bool)"/>
    /// <seealso cref="SaveAsync"/>
    /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override void Save()
    {
        if (InGame)
        {
            base.Save();
            InternalLogger.Log($"[{QModId}] Saved save data to {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot save save data when not in game!");
        }
    }
    
    /// <summary>
    /// Saves the current fields and properties of the <see cref="SaveDataCache"/> as JSON properties to the file on disk.
    /// </summary>
    /// <seealso cref="LoadAsync(bool)"/>
    /// <seealso cref="Save"/>
    /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override async Task SaveAsync()
    {
        if (InGame)
        {
            await base.SaveAsync();
            InternalLogger.Log($"[{QModId}] Saved save data to {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot save save data when not in game!");
        }
    }

    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="SaveDataCache"/>.
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for serialization.
    /// The <see cref="JsonFile.AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
    /// <seealso cref="Load(bool)"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override void LoadWithConverters(bool createFileIfNotExist = true, params JsonConverter[] jsonConverters)
    {
        if (InGame)
        {
            base.LoadWithConverters(createFileIfNotExist, jsonConverters);
            InternalLogger.Log($"[{QModId}] Loaded save data from {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot load save data when not in game!");
        }
    }
    
    /// <summary>
    /// Loads the JSON properties from the file on disk into the <see cref="SaveDataCache"/>.
    /// </summary>
    /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
    /// already exist.</param>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for serialization.
    /// The <see cref="JsonFile.AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="SaveWithConvertersAsync(JsonConverter[])"/>
    /// <seealso cref="Load(bool)"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override async Task LoadWithConvertersAsync(bool createFileIfNotExist = true, params JsonConverter[] jsonConverters)
    {
        if (InGame)
        {
            await base.LoadWithConvertersAsync(createFileIfNotExist, jsonConverters);
            InternalLogger.Log($"[{QModId}] Loaded save data from {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot load save data when not in game!");
        }
    }

    /// <summary>
    /// Saves the current fields and properties of the <see cref="SaveDataCache"/> as JSON properties to the file on disk.
    /// </summary>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for deserialization.
    /// The <see cref="JsonFile.AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
    /// <seealso cref="Save"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override void SaveWithConverters(params JsonConverter[] jsonConverters)
    {
        if (InGame)
        {
            base.SaveWithConverters(jsonConverters);
            InternalLogger.Log($"[{QModId}] Saved save data to {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot save save data when not in game!");
        }
    }
    
    /// <summary>
    /// Saves the current fields and properties of the <see cref="SaveDataCache"/> as JSON properties to the file on disk.
    /// </summary>
    /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for deserialization.
    /// The <see cref="JsonFile.AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
    /// <seealso cref="LoadWithConvertersAsync(bool, JsonConverter[])"/>
    /// <seealso cref="Save"/>
    /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
    public override async Task SaveWithConvertersAsync(params JsonConverter[] jsonConverters)
    {
        if (InGame)
        {
            await base.SaveWithConvertersAsync(jsonConverters);
            InternalLogger.Log($"[{QModId}] Saved save data to {JsonFileName}.json");
        }
        else
        {
            throw new InvalidOperationException($"[{QModId}] Cannot save save data when not in game!");
        }
    }
}