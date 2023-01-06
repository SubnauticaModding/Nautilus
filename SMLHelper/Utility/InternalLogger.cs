namespace SMLHelper.Utility
{
    using System.Linq;
    using System;
    using System.Reflection;
    using BepInEx.Bootstrap;
    using BepInEx.Configuration;
    using BepInEx.Logging;

    internal static class InternalLogger
    {
        internal static bool Initialized = false;
        private static ManualLogSource Logger { get; set; }
        private static FieldInfo ConfigConsoleDisplayedLevel { get; } = typeof(ConsoleLogListener).GetField("ConfigConsoleDisplayedLevel", BindingFlags.Static | BindingFlags.NonPublic);
        private static ConfigEntry<LogLevel> consoleLogLevel { get; } = ConfigConsoleDisplayedLevel.GetValue(null) as ConfigEntry<LogLevel>;
        private static FieldInfo ConfigDiskConsoleDisplayedLevel { get; } = typeof(Chainloader).GetField("ConfigDiskConsoleDisplayedLevel", BindingFlags.Static | BindingFlags.NonPublic);
        private static ConfigEntry<LogLevel> diskLogLevel { get; } = ConfigDiskConsoleDisplayedLevel.GetValue(null) as ConfigEntry<LogLevel>;
        internal static DiskLogListener DiskLogListener { get; private set; }
        internal static bool EnableDebugging => (DiskLogListener != null && (DiskLogListener.DisplayedLogLevel & LogLevel.Debug) != LogLevel.None) || (consoleLogLevel != null && (consoleLogLevel.Value & LogLevel.Debug) != LogLevel.None);

        internal static void SetDebugging(bool value)
        {
            if(value)
            {
                if(consoleLogLevel != null && (consoleLogLevel.Value & LogLevel.Debug) == LogLevel.None)
                {
                    consoleLogLevel.Value = consoleLogLevel.Value | LogLevel.Debug;
                }

                if(diskLogLevel != null && (DiskLogListener.DisplayedLogLevel & LogLevel.Debug) == LogLevel.None)
                {
                    diskLogLevel.Value = diskLogLevel.Value | LogLevel.Debug;
                }

                if(DiskLogListener != null && (DiskLogListener.DisplayedLogLevel & LogLevel.Debug) == LogLevel.None)
                {
                    DiskLogListener.DisplayedLogLevel = DiskLogListener.DisplayedLogLevel | LogLevel.Debug;
                }
            }
            else
            {
                if(consoleLogLevel != null && (consoleLogLevel.Value & LogLevel.Debug) != LogLevel.None)
                {
                    consoleLogLevel.Value = consoleLogLevel.Value & ~LogLevel.Debug;
                }

                if(diskLogLevel != null && (DiskLogListener.DisplayedLogLevel & LogLevel.Debug) != LogLevel.None)
                {
                    diskLogLevel.Value = diskLogLevel.Value & ~LogLevel.Debug;
                }

                if(DiskLogListener != null && (DiskLogListener.DisplayedLogLevel & LogLevel.Debug) != LogLevel.None)
                {
                    DiskLogListener.DisplayedLogLevel = DiskLogListener.DisplayedLogLevel & ~LogLevel.Debug;
                }
            }
        }

        internal static void Initialize(ManualLogSource logger)
        {
            if (Initialized)
            {
                return;
            }

            if(BepInEx.Logging.Logger.Listeners.Where((x) => x is DiskLogListener).FirstOrFallback(DiskLogListener) is DiskLogListener logListener)
            {
                DiskLogListener = logListener;
            }

            Logger = logger;
            Log($"Enable debug logs set to: {EnableDebugging}", LogLevel.Info);

            Initialized = true;
        }

        internal static void Debug(string text)
        {
            Log(text, LogLevel.Debug);
        }

        internal static void Info(string text)
        {
            Log(text, LogLevel.Info);
        }

        internal static void Warn(string text)
        {
            Log(text, LogLevel.Warning);
        }

        internal static void Error(string text)
        {
            Log(text, LogLevel.Error);
        }

        internal static void Debug(string text, params object[] args)
        {
            Log(text, LogLevel.Debug, args);
        }

        internal static void Info(string text, params object[] args)
        {
            Log(text, LogLevel.Info, args);
        }

        internal static void Warn(string text, params object[] args)
        {
            Log(text, LogLevel.Warning, args);
        }

        internal static void Error(string text, params object[] args)
        {
            Log(text, LogLevel.Error, args);
        }

        internal static void Log(string text, LogLevel level = LogLevel.Info)
        {
            if(!Initialized)
            {
                if(level >= LogLevel.Info || EnableDebugging)
                {
                    Console.WriteLine($"[SMLHelper/{level}] {text}");
                }

                return;
            }

            Logger.Log(level, text);
        }

        internal static void Log(string text, LogLevel level = LogLevel.Info, params object[] args)
        {
            if(args != null && args.Length > 0)
            {
                text = string.Format(text, args);
            }

            Log(text, level);
        }

        internal static void Announce(string text, LogLevel level = LogLevel.Info, bool logToFile = false)
        {
            ErrorMessage.AddMessage(text);

            if (logToFile)
            {
                Log(text, level);
            }
        }

        internal static void Announce(string text, LogLevel level = LogLevel.Info, bool logToFile = false, params object[] args)
        {
            ErrorMessage.AddMessage(string.Format(text, args));

            if (logToFile)
            {
                Log(text, level, args);
            }
        }
    }
}
