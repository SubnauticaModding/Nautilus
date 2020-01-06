namespace SMLHelper.V2
{
    using System;
    using System.IO;
    using System.Reflection;

    internal enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
    }

    internal static class Logger
    {
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
            Console.WriteLine($"[SMLHelper/{level.ToString()}] {text}");
        }

        internal static void Log(string text, LogLevel level = LogLevel.Info, params object[] args)
        {
            if (args != null && args.Length > 0)
                text = string.Format(text, args);

            Console.WriteLine($"[SMLHelper/{level.ToString()}] {text}");
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
