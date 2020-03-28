
namespace SMLHelper.V2.Options
{
    using System.IO;
    using System.Reflection;
    using SMLHelper.V2.Interfaces;

    /// <summary>
    /// A simple default implementation of an <see cref="IJsonFile"/>.
    /// </summary>
    /// <seealso cref="V2.Utility.JsonUtils.Load{T}(T, bool, Oculus.Newtonsoft.Json.JsonConverter[])"/>
    /// <seealso cref="V2.Utility.JsonUtils.Save{T}(T, Oculus.Newtonsoft.Json.JsonConverter[])"/>
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
    public abstract class ConfigFile : IJsonFile
    {
        /// <summary>
        /// The file path at which the JSON file is accessible for reading and writing.
        /// </summary>
        public string JsonFilePath { get; private set; }

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
        /// public static class MyConfig : ConfigFile
        /// {
        ///     public KeyCode ActivationKey { get; set; } = KeyCode.Escape;
        ///     public MyConfig() : base("options", "Config Files") { }
        ///     // The config file will be stored at the path "QMods\YourModName\Config Files\options.json"
        /// }
        /// </code>
        /// </example>
        protected ConfigFile(string fileName = "config", string subfolder = null)
        {
            var path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "config";
            }
            JsonFilePath = Path.Combine(
                path,
                string.IsNullOrEmpty(subfolder) ? string.Empty : subfolder,
                $"{fileName}.json"
            );
        }
    }
}
