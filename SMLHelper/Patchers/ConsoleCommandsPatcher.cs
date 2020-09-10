namespace SMLHelper.V2.Patchers
{
    using ExtensionMethods;
    using System.Collections.Generic;
    using HarmonyLib;
    using System.Reflection;
    using System;
    using System.Linq;
    using System.Globalization;
    using QModManager.API;
    using SMLHelper.V2.Commands;
    using FMOD;

    internal static class ConsoleCommandsPatcher
    {
        private static Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();

        private static Dictionary<Type, Func<string, object>> typeConverters = new Dictionary<Type, Func<string, object>>()
        {
            [typeof(string)] = (s) => s,
            [typeof(bool)] = (s) => bool.Parse(s),
            [typeof(int)] = (s) => int.Parse(s, CultureInfo.InvariantCulture.NumberFormat),
            [typeof(float)] = (s) => float.Parse(s, CultureInfo.InvariantCulture.NumberFormat),
            [typeof(double)] = (s) => double.Parse(s, CultureInfo.InvariantCulture.NumberFormat)
        };

        private const string COMMAND_COLOR = "#ffff00ff";
        private const string PARAM_TYPE_COLOR = "#00ffffff";
        private const string PARAM_INPUT_COLOR = "#ff0000ff";
        private const string PARAM_OPTIONAL_COLOR = "#00ff00ff";
        private const string MOD_ORIGIN_COLOR = "#00ff00ff";
        private const string MOD_CONFLICT_COLOR = "#c0c0c0ff";

        public static void Patch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony);

            parseCustomCommands();

            Logger.Debug("ConsoleCommandsPatcher is done.");
        }

        public static void AddCustomCommand(string command, MethodInfo targetMethod)
        {
            command = command.ToLowerInvariant();

            if (commands.TryGetValue(command, out MethodInfo alreadyExists))
            {
                Logger.Announce($"Could not register custom command <color={COMMAND_COLOR}>{command}</color> for mod " +
                    $"<color={MOD_ORIGIN_COLOR}>{targetMethod.GetQMod().DisplayName}</color>:\n" +
                    $"<color={MOD_CONFLICT_COLOR}>{alreadyExists.GetQMod().DisplayName}</color> already registered this command!",
                    LogLevel.Error, true);

                return;
            }

            if (!targetMethod.IsStatic)
            {
                Logger.Announce($"Could not register custom command <color={COMMAND_COLOR}>{command}</color> for mod " +
                    $"<color={MOD_ORIGIN_COLOR}>{targetMethod.GetQMod().DisplayName}</color>:\n" +
                    $"Target method must be static.",
                    LogLevel.Error, true);

                return;
            }

            foreach (ParameterInfo param in targetMethod.GetParameters())
            {
                if (!typeConverters.ContainsKey(param.ParameterType))
                {
                    Logger.Announce($"Could not register custom command <color={COMMAND_COLOR}>{command}</color> for mod " +
                        $"<color={MOD_ORIGIN_COLOR}>{targetMethod.GetQMod().DisplayName}</color>:\n" +
                        $"Parameter type <color={PARAM_TYPE_COLOR}>{param.ParameterType.Name}</color> " +
                        $"is not supported.\n" +
                        $"Supported parameter types:\n" +
                        $"{typeConverters.Keys.Select(x => $"<color={PARAM_TYPE_COLOR}>{x.Name}</color>").Join()}",
                        LogLevel.Error, true);

                    return;
                }
            }

            commands.Add(command, targetMethod);
        }

        private static void parseCustomCommands()
        {
            foreach (IQMod qmod in QModServices.Main.GetAllMods())
            {
                if (qmod == null || !qmod.IsLoaded)
                    continue;

                foreach (Type type in qmod.LoadedAssembly.GetTypes())
                {
                    if (type.IsNotPublic || type.IsEnum)
                        continue;

                    foreach (MethodInfo targetMethod in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        var customCommandAttribute = targetMethod.GetCustomAttribute<ConsoleCommandAttribute>(false);
                        if (customCommandAttribute != null)
                            AddCustomCommand(customCommandAttribute.Command, targetMethod);
                    }
                }
            }
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(DevConsole), nameof(DevConsole.Submit))]
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

            var command = components[0];

            if (!commands.TryGetValue(command, out MethodInfo targetMethod))
            {
                Logger.Debug($"No command listener registered: {value}.");
                return false;
            }

            IEnumerable<string> parameters = components.Skip(1);

            if (!validateUserParameters(command, targetMethod, parameters, out object[] validatedParameters))
                return true;

            string result = targetMethod.Invoke(null, BindingFlags.OptionalParamBinding | BindingFlags.InvokeMethod,
                null, validatedParameters, CultureInfo.InvariantCulture)?.ToString();

            if (result != null)
                Logger.Announce($"<color={MOD_ORIGIN_COLOR}>[{targetMethod.GetQMod().DisplayName}]</color>" +
                    $" {result.ToString(CultureInfo.InvariantCulture)}", LogLevel.Info, true);

            return true;
        }

        private static bool validateUserParameters(string command, MethodInfo targetMethod, IEnumerable<string> parameters,
            out object[] validatedParameters)
        {
            ParameterInfo[] methodParameters = targetMethod.GetParameters();
            validatedParameters = null;

            if (methodParameters.Length < parameters.Count() ||
                methodParameters.Where(param => !param.IsOptional).Count() > parameters.Count())
            {
                Logger.Announce($"<color={COMMAND_COLOR}>{command}</color> expects the following parameters:\n" +
                    $"{getParameterInfoString(methodParameters)}", LogLevel.Error, true);

                if (parameters.Any())
                    Logger.Announce($"Received parameters: {parameters.Join()}", LogLevel.Error, true);

                return false;
            }

            validatedParameters = new object[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; i++)
            {
                Type type = methodParameters[i].ParameterType;

                if (i >= parameters.Count())
                {
                    validatedParameters[i] = Type.Missing;
                    continue;
                }

                string param = parameters.ElementAt(i);

                try
                {
                    validatedParameters[i] = typeConverters[type](param);
                }
                catch (Exception)
                {
                    Logger.Announce($"<color={PARAM_INPUT_COLOR}>{param}</color> is not a valid " +
                        $"<color={PARAM_TYPE_COLOR}>{type.Name}</color>!",
                        LogLevel.Error, true);
                    Logger.Announce($"<color={COMMAND_COLOR}>{command}</color> expects the following parameters:\n" +
                        $"{getParameterInfoString(methodParameters)}", LogLevel.Error, true);
                    Logger.Announce($"Received parameters: {parameters.Join()}", LogLevel.Error, true);
                    return false;
                }
            }

            return true;
        }

        private static string getParameterInfoString(ParameterInfo[] methodParameters)
        {
            return methodParameters
                .Select(param => $"{param.Name}: " +
                    $"<color={PARAM_TYPE_COLOR}>{param.ParameterType.Name}</color>" +
                    $"<color={PARAM_OPTIONAL_COLOR}>{(param.IsOptional ? " (optional)" : string.Empty)}</color>")
                .Join(delimiter: "\n");
        }
    }
}
