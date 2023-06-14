using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace Nautilus.Utility;

internal static class PatchUtils
{
    internal static void PatchDictionary<K, V>(IDictionary<K, V> original, IDictionary<K, V> patches)
    {
        foreach (KeyValuePair<K, V> entry in patches)
        {
            original[entry.Key] = entry.Value;
        }
    }

    internal static void PatchList<T>(IList<T> original, IList<T> patches)
    {
        foreach (T entry in patches)
        {
            original.Add(entry);
        }
    }

    // for use with iterator methods, returns MoveNext method from the iterator type
    internal static MethodInfo GetIteratorMethod(MethodInfo method)
    {
        Type stateMachineType = method.GetCustomAttribute<StateMachineAttribute>()?.StateMachineType;
        return AccessTools.Method(stateMachineType, "MoveNext");
    }
}