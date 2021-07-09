using QModManager.API;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace SMLHelper.V2.Json
{
    using Attributes;
    using Converters;
    using Interfaces;

    /// <summary>
    /// An abstract implementation of <see cref="IJsonFile"/> intended for use with caching per-save data.
    /// </summary>
    public abstract class SaveDataCache : JsonFile
    {
        private UserStoragePC userStorage;
        private UserStoragePC UserStorage => userStorage ??= PlatformUtils.main.GetUserStorage() as UserStoragePC;
        private string SaveSlot => SaveLoadManager.main.GetCurrentSlot();
        private string QModId { get; init; }

        private static readonly JsonConverter[] alwaysIncludedJsonConverters = new JsonConverter[]
        {
            new Vector3Converter(), new QuaternionConverter()
        };

        private bool InGame => SaveSlot != "test";

        private string jsonFileName = null;
        private string JsonFileName => jsonFileName ??= GetType().GetCustomAttribute<FileNameAttribute>() switch
        {
            FileNameAttribute fileNameAttribute => fileNameAttribute.FileName,
            _ => QModId
        };

        /// <summary>
        /// The <see cref="JsonConverter"/>s that should always be used when reading/writing JSON data.
        /// </summary>
        /// <seealso cref="alwaysIncludedJsonConverters"/>
        public override JsonConverter[] AlwaysIncludedJsonConverters
            => base.AlwaysIncludedJsonConverters.Concat(alwaysIncludedJsonConverters).ToArray();

        /// <summary>
        /// The file path at which the JSON file is accessible for reading and writing.
        /// </summary>
        public override string JsonFilePath => Path.Combine(UserStorage.savePath, SaveSlot, QModId, $"{JsonFileName}.json");

        /// <summary>
        /// Creates a new instance of <see cref="SaveDataCache"/>, parsing the file name from <see cref="FileNameAttribute"/>
        /// if declared, or with default values otherwise.
        /// </summary>
        public SaveDataCache()
        {
            QModId = QModServices.Main.FindModByAssembly(GetType().Assembly).Id;
        }

        /// <summary>
        /// Loads the JSON properties from the file on disk into the <see cref="SaveDataCache"/>.
        /// </summary>
        /// <param name="createFileIfNotExist">Whether a new JSON file should be created with default values if it does not
        /// already exist.<br/>
        /// Don't set this to <see langword="true"/> unless you know exactly what you're doing.</param>
        /// <seealso cref="Save()"/>
        /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
        /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
        public override void Load(bool createFileIfNotExist = false)
        {
            if (InGame)
            {
                base.Load(createFileIfNotExist);
                Logger.Log($"[{QModId}] Loaded save data from {JsonFileName}.json");
            }
            else
            {
                throw new InvalidOperationException($"[{QModId}] Cannot load save data when not in game!");
            }
        }

        /// <summary>
        /// Saves the current fields and properties of the <see cref="SaveDataCache"/> as JSON properties to the file on disk.
        /// </summary>
        /// <seealso cref="Load(bool)"/>
        /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
        /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
        public override void Save()
        {
            if (InGame)
            {
                base.Save();
                Logger.Log($"[{QModId}] Saved save data to {JsonFileName}.json");
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
        /// already exist.<br/>
        /// Don't set this to <see langword="true"/> unless you know exactly what you're doing.</param>
        /// <param name="jsonConverters">Optional <see cref="JsonConverter"/>s to be used for serialization.
        /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
        /// <seealso cref="SaveWithConverters(JsonConverter[])"/>
        /// <seealso cref="Load(bool)"/>
        /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
        public override void LoadWithConverters(bool createFileIfNotExist = false, params JsonConverter[] jsonConverters)
        {
            if (InGame)
            {
                base.LoadWithConverters(createFileIfNotExist, jsonConverters);
                Logger.Log($"[{QModId}] Loaded save data from {JsonFileName}.json");
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
        /// The <see cref="AlwaysIncludedJsonConverters"/> will always be used, regardless of whether you pass them.</param>
        /// <seealso cref="LoadWithConverters(bool, JsonConverter[])"/>
        /// <seealso cref="Save"/>
        /// <exception cref="InvalidOperationException">Thrown when the player is not in-game.</exception>
        public override void SaveWithConverters(params JsonConverter[] jsonConverters)
        {
            if (InGame)
            {
                base.SaveWithConverters(jsonConverters);
                Logger.Log($"[{QModId}] Saved save data to {JsonFileName}.json");
            }
            else
            {
                throw new InvalidOperationException($"[{QModId}] Cannot save save data when not in game!");
            }
        }
    }
}
