namespace SMLHelper.V2.Patchers
{
    using Commands;
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using QModManager.API;
    using System.Reflection;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using Logger = Logger;

    internal static class ConsoleCommandsPatcher
    {
        private static Dictionary<string, ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand>();

        private static Color CommandColor = new Color(1, 1, 0);
        private static Color ParameterTypeColor = new Color(0, 1, 1);
        private static Color ParameterInputColor = new Color(1, 0, 0);
        private static Color ParameterOptionalColor = new Color(0, 1, 0);
        private static Color ModOriginColor = new Color(0, 1, 0);
        private static Color ModConflictColor = new Color(0.75f, 0.75f, 0.75f);

        public static void Patch(Harmony harmony)
        {
            harmony.PatchAll(typeof(ConsoleCommandsPatcher));
            Logger.Debug("ConsoleCommandsPatcher is done.");
        }

        public static void AddCustomCommand(string command, MethodInfo targetMethod, bool isDelegate = false, object target = null)
        {
            var consoleCommand = new ConsoleCommand(command, targetMethod, isDelegate, target);

            if (ConsoleCommands.TryGetValue(consoleCommand.CommandName, out ConsoleCommand alreadyDefinedCommand))
            {
                string error = $"Could not register custom command {GetColoredString(consoleCommand)} for mod " +
                    $"{GetColoredString(consoleCommand.QMod)}\n" +
                    $"{GetColoredString(alreadyDefinedCommand.QMod, ModConflictColor)} already registered this command!";

                LogAndAnnounce(error, LogLevel.Error);

                return;
            }

            if (!consoleCommand.HasValidInvoke())
            {
                string error = $"Could not register custom command {GetColoredString(consoleCommand)} for mod " +
                    $"{GetColoredString(consoleCommand.QMod)}\n" +
                    "Target method must be static.";

                LogAndAnnounce(error, LogLevel.Error);

                return;
            }

            if (!consoleCommand.HasValidParameterTypes())
            {
                string error = $"Could not register custom command {GetColoredString(consoleCommand)} for mod " +
                    $"{GetColoredString(consoleCommand.QMod)}\n" +
                    "The following parameters have unsupported types:\n" +
                    consoleCommand.GetInvalidParameters().Select(param => GetColoredString(param)).Join(delimiter: "\n") +
                    "Supported parameter types:\n" +
                    Parameter.SupportedTypes.Select(type => type.Name).Join();

                LogAndAnnounce(error, LogLevel.Error);

                return;
            }

            ConsoleCommands.Add(consoleCommand.CommandName, consoleCommand);
        }

        public static void ParseCustomCommands(Type type)
        {
            foreach (MethodInfo targetMethod in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var customCommandAttribute = targetMethod.GetCustomAttribute<ConsoleCommandAttribute>(false);
                if (customCommandAttribute != null)
                    AddCustomCommand(customCommandAttribute.Command, targetMethod);
            }
        }

        [HarmonyPatch(typeof(DevConsole), nameof(DevConsole.Submit))]
        [HarmonyPrefix]
        private static bool DevConsole_Submit_Prefix(string value, out bool __result)
        {
            __result = false;

            if (handleCommand(value))
            {
                __result = true;
                return false;
            }

            return true;
        }

        private static bool handleCommand(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            Logger.Debug($"Console command: {value}");

            value = value.Trim();
            string[] components = value.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var commandString = components[0].ToLowerInvariant();

            if (!ConsoleCommands.TryGetValue(commandString, out ConsoleCommand command))
            {
                Logger.Debug($"No command listener registered: {value}.");
                return false;
            }

            IEnumerable<string> parameters = components.Skip(1);

            if (!command.TryParseParameters(parameters, out object[] parsedParameters))
            {
                if (parsedParameters != null)
                {
                    string invalidParameter = null;
                    string parameterTypeName = null;
                    for (int i = 0; i < parsedParameters.Length; i++)
                    {
                        if (parsedParameters[i] == null)
                        {
                            invalidParameter = parameters.ElementAt(i);
                            parameterTypeName = command.ParameterTypes[i].Name;
                            break;
                        }
                    }

                    string error = $"{GetColoredString(invalidParameter, ParameterInputColor)} is not a valid " +
                        $"{GetColoredString(parameterTypeName, ParameterTypeColor)}!";

                    LogAndAnnounce(error, LogLevel.Error);
                }

                string parameterInfoString = $"{GetColoredString(command.CommandName, CommandColor)} " +
                    "expects the following parameters\n" +
                    command.Parameters.Select(param => GetColoredString(param)).Join(delimiter: "\n");

                LogAndAnnounce(parameterInfoString, LogLevel.Error);

                if (parameters.Any())
                    Logger.Announce($"Received parameters: {parameters.Join()}", LogLevel.Error, true);

                return true;
            }

            string result = command.Invoke(parsedParameters);

            if (result != null)
            {
                LogAndAnnounce($"{GetColoredString($"[{command.QMod.DisplayName}]", ModOriginColor)} {result}", LogLevel.Info);
            }

            return true;
        }

        private static void LogAndAnnounce(string message, LogLevel level)
        {
            Logger.Announce(message);
            Logger.Log(message.StripXML(), level);
        }

        private static string GetColoredString(IQMod mod)
        {
            return GetColoredString(mod, ModOriginColor);
        }

        private static string GetColoredString(IQMod mod, Color color)
        {
            return GetColoredString(mod.DisplayName, color);
        }

        private static string GetColoredString(ConsoleCommand command)
        {
            return GetColoredString(command.CommandName, CommandColor);
        }

        private static string GetColoredString(Parameter parameter)
        {
            return $"{parameter.Name}: {GetColoredString(parameter.ParameterType.Name, ParameterTypeColor)}" +
                (parameter.IsOptional ? $" {GetColoredString("(optional)", ParameterOptionalColor)}" : string.Empty);
        }

        private static string GetColoredString(string str, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
        }

        private static Regex xmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        public static string StripXML(this string source)
        {
            return xmlRegex.Replace(source, string.Empty);
        }
    }
}
