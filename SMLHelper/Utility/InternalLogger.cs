namespace SMLHelper.V2.Utility
{
    using System;
    using System.IO;
    using System.Reflection;
    using BepInEx.Logging;

    internal enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
    }

    internal static class InternalLogger
    {
        internal static bool Initialized = false;
        private static ManualLogSource logSource;

        internal static bool EnableDebugging { get; private set; }
        internal static void SetDebugging(bool value)
        {
            string configPath = Path.Combine(Path.Combine(BepInEx.Paths.ConfigPath, Assembly.GetExecutingAssembly().GetName().Name), "EnableDebugLogs.txt");

            File.WriteAllText(configPath, value.ToString());
            EnableDebugging = value;
        }

        internal static void Initialize(ManualLogSource logger)
        {
            if (Initialized) return;
            logSource = logger;
            Initialized = true;

            string configPath = Path.Combine(Path.Combine(BepInEx.Paths.ConfigPath, Assembly.GetExecutingAssembly().GetName().Name), "EnableDebugLogs.txt");

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, "False");
                EnableDebugging = false;

                return;
            }

            string fileContents = File.ReadAllText(configPath);

            try
            {
                EnableDebugging = bool.Parse(fileContents);

                Log($"Enable debug logs set to: {EnableDebugging}", LogLevel.Info);
            }
            catch (Exception)
            {
                File.WriteAllText(configPath, "False");
                EnableDebugging = false;

                Log("Error reading EnableDebugLogs.txt configuration file. Defaulted to false", LogLevel.Warn);
            }
        }

        internal static void Debug(string text) => Log(text, LogLevel.Debug);
        internal static void Info(string text) => Log(text, LogLevel.Info);
        internal static void Warn(string text) => Log(text, LogLevel.Warn);
        internal static void Error(string text) => Log(text, LogLevel.Error);

        internal static void Debug(string text, params object[] args) => Log(text, LogLevel.Debug, args);
        internal static void Info(string text, params object[] args) => Log(text, LogLevel.Info, args);
        internal static void Warn(string text, params object[] args) => Log(text, LogLevel.Warn, args);
        internal static void Error(string text, params object[] args) => Log(text, LogLevel.Error, args);

        internal static void Log(string text, LogLevel level = LogLevel.Info)
        {
            if(!Initialized)
            {
                if(level >= LogLevel.Info || EnableDebugging)
                    Console.WriteLine($"[SMLHelper/{level}] {text}");
                return;
            }

            switch(level)
            {
                case LogLevel.Debug when EnableDebugging:
                    logSource.LogDebug(text);
                    break;
                case LogLevel.Info:
                    logSource.LogInfo(text);
                    break;
                case LogLevel.Warn:
                    logSource.LogWarning(text);
                    break;
                case LogLevel.Error:
                    logSource.LogError(text);
                    break;
            }
        }

        internal static void Log(string text, LogLevel level = LogLevel.Info, params object[] args)
        {
            if(args != null && args.Length > 0)
                text = string.Format(text, args);

            Log(text, level);
        }

        internal static void Announce(string text, LogLevel level = LogLevel.Info, bool logToFile = false)
        {
            ErrorMessage.AddMessage(text);

            if (logToFile)
                Log(text, level);
        }

        internal static void Announce(string text, LogLevel level = LogLevel.Info, bool logToFile = false, params object[] args)
        {
            ErrorMessage.AddMessage(string.Format(text, args));

            if (logToFile)
                Log(text, level, args);
        }
    }
}
