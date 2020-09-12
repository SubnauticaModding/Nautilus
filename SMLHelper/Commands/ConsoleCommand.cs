namespace SMLHelper.V2.Commands
{
    using HarmonyLib;
    using QModManager.API;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal class ConsoleCommand
    {
        public string CommandName { get; }
        public Type DeclaringType { get; }
        public Type ReturnType { get; }
        public string MethodName { get; }
        public bool IsMethodStatic { get; }
        public bool IsDelegate { get; }
        public object TargetInstance { get; }
        public IQMod QMod { get; }
        public IEnumerable<Parameter> Parameters { get; }
        public Type[] ParameterTypes { get; }

        public ConsoleCommand(string command, MethodInfo targetMethod, bool isDelegate = false, object target = null)
        {
            CommandName = command.ToLowerInvariant();
            DeclaringType = targetMethod.DeclaringType;
            ReturnType = targetMethod.ReturnType;
            MethodName = targetMethod.Name;
            IsMethodStatic = targetMethod.IsStatic;
            IsDelegate = isDelegate;
            TargetInstance = target;
            QMod = QModServices.Main.GetMod(DeclaringType.Assembly);
            Parameters = targetMethod.GetParameters().Select(param => new Parameter(param));
            ParameterTypes = Parameters.Select(param => param.ParameterType).ToArray();
        }

        public bool HasValidInvoke() => IsDelegate || TargetInstance != null || IsMethodStatic;

        public bool HasValidParameterTypes()
        {
            foreach (Parameter parameter in Parameters)
            {
                if (!parameter.IsValidParameterType)
                    return false;
            }

            return true;
        }

        public IEnumerable<Parameter> GetInvalidParameters()
            => Parameters.Where(param => !param.IsValidParameterType);

        public string Invoke(object[] arguments)
        {
            if (TargetInstance != null)
                return Traverse.Create(TargetInstance).Method(MethodName, ParameterTypes).GetValue(arguments).ToString();
            else
                return Traverse.Create(DeclaringType).Method(MethodName, ParameterTypes).GetValue(arguments).ToString();
        }

        public bool TryParseParameters(IEnumerable<string> inputParameters, out object[] parsedParameters)
        {
            parsedParameters = null;

            if (Parameters.Count() < inputParameters.Count() ||
                Parameters.Where(param => !param.IsOptional).Count() > inputParameters.Count())
            {
                return false;
            }

            parsedParameters = new object[Parameters.Count()];
            for (int i = 0; i < Parameters.Count(); i++)
            {
                Parameter parameter = Parameters.ElementAt(i);

                if (i >= inputParameters.Count())
                {
                    parsedParameters[i] = Type.Missing;
                    continue;
                }

                string input = inputParameters.ElementAt(i);

                try
                {
                    parsedParameters[i] = parameter.Parse(input);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
