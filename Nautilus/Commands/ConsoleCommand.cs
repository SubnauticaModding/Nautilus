using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Nautilus.Extensions;

namespace Nautilus.Commands;

/// <summary>
/// Represents a console command.
/// </summary>
internal class ConsoleCommand
{
    /// <summary>
    /// The string that triggers the command.
    /// </summary>
    public string Trigger { get; }

    /// <summary>
    /// The QMod that registered the command.
    /// </summary>
    public string ModName { get; }

    /// <summary>
    /// The parameters for the command.
    /// </summary>
    public IReadOnlyList<Parameter> Parameters { get; }
    /// <summary>
    /// The minimum number of parameters required to invoke the command.
    /// </summary>
    public int RequiredParameterCount { get; }

    /// <summary>
    /// The types of the parameters.
    /// </summary>
    public Type[] ParameterTypes { get; }

    private Type DeclaringType { get; }
    private string MethodName { get; }
    private bool IsMethodStatic { get; }
    private bool IsDelegate { get; }
    private object Instance { get; }

    /// <summary>
    /// Creates an instance of <see cref="ConsoleCommand"/>.
    /// </summary>
    /// <param name="trigger">The string that triggers the command.</param>
    /// <param name="targetMethod">The method targeted by the command.</param>
    /// <param name="isDelegate">Whether or not the method is a delegate.</param>
    /// <param name="instance">The instance the method belongs to.</param>
    public ConsoleCommand(string trigger, MethodInfo targetMethod, bool isDelegate = false, object instance = null)
    {
        Trigger = trigger.ToLowerInvariant();
        DeclaringType = targetMethod.DeclaringType;
        MethodName = targetMethod.Name;
        IsMethodStatic = targetMethod.IsStatic;
        IsDelegate = isDelegate;
        Instance = instance;
        ModName = DeclaringType.Assembly.GetName().Name;
        Parameters = targetMethod.GetParameters().Select(param => new Parameter(param)).ToList();
        ParameterTypes = Parameters.Select(param => param.ParameterType).ToArray();
        RequiredParameterCount = Parameters.Count(param => !param.IsOptional);
    }

    /// <summary>
    /// Determines whether the targeted method is valid in terms of whether it is static or delegate.
    /// </summary>
    /// <returns></returns>
    public bool HasValidInvoke()
    {
        return IsDelegate || Instance != null || IsMethodStatic;
    }

    /// <summary>
    /// Returns a list of all invalid parameters.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Parameter> GetInvalidParameters()
    {
        return Parameters.Where(p => p.ValidState != Parameter.ValidationError.Valid);
    }

    /// <summary>
    /// Attempts to parse input parameters into appropriate types as defined in the target method.
    /// </summary>
    /// <param name="input">The parameters as input by the user.</param>
    /// <param name="parsedParameters">The parameters that have been successfully parsed.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="number">
    /// <item>The number of input items consumed.</item>
    /// <item>The number of command parameters that were successfully parsed.</item>
    /// </list>
    /// </returns>
    public (int consumed, int parsed) TryParseParameters(IReadOnlyList<string> input, out object[] parsedParameters)
    {
        parsedParameters = null;

        // Detect incorrect number of parameters (allow for optional)
        int paramCount = Parameters.Count;
        int inputCount = input.Count;
        int paramsArrayLength = Math.Max(0, input.Count - (paramCount - 1));

        if (inputCount < RequiredParameterCount)
        {
            return default;
        }

        parsedParameters = new object[paramCount];
        for (int i = 0; i < paramCount; i++)
        {
            Type paramType = Parameters[i].ParameterType;
            parsedParameters[i] = paramType.TryUnwrapArrayType(out Type elementType)
                ? Array.CreateInstance(elementType, paramsArrayLength)
                : DBNull.Value;
        }

        int consumed = 0;
        int parsed = 0;
        while (consumed < inputCount)
        {
            if (parsed >= paramCount) break;
            
            Parameter parameter = Parameters[parsed];
            string inputItem = input[consumed];

            object parsedItem;
            try
            {
                parsedItem = parameter.Parse(inputItem);
            }
            catch (Exception)
            {
                return (consumed, parsed);
            }
            consumed++;

            if (parameter.ParameterType.IsArray)
            {
                Array parsedArr = (Array)parsedParameters[parsed];
                parsedArr.SetValue(parsedItem, consumed - parsed - 1);
                if (consumed >= inputCount)
                {
                    parsed++;
                }
            }
            else
            {
                parsedParameters[parsed] = parsedItem;
                parsed++;
            }
        }

        // Optional parameters that weren't passed by the user
        // at this point all required parameters should've been parsed
        for (int i = parsed; i < paramCount; i++)
        {
            if (parsedParameters[i] == DBNull.Value)
                parsedParameters[i] = Type.Missing;
            parsed++;
        }

        return (consumed, parsed);
    }

    /// <summary>
    /// Invokes the command with the given parameters.
    /// </summary>
    /// <param name="parameters">The command parameters.</param>
    /// <returns>The string returned from the command.</returns>
    public string Invoke(object[] parameters)
    {
        if (Instance != null)
        {
            return Traverse.Create(Instance).Method(MethodName, ParameterTypes).GetValue(parameters)?.ToString();
        }
        else
        {
            return Traverse.Create(DeclaringType).Method(MethodName, ParameterTypes).GetValue(parameters)?.ToString();
        }
    }
}