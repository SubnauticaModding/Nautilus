namespace Nautilus.Priority;

using BepInEx;
using HarmonyLib;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class Initializer
{
    internal static string PatcherPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    private const string NautilusGUID = "com.snmodding.nautilus";
    private const string GUID = "com.snmodding.nautilus.priority";

    [Obsolete("Should not be used!", true)]
    public static void Finish()
    {
        var harmony = new Harmony(GUID);
        harmony.Patch(typeof(Utility).GetMethod(nameof(Utility.TopologicalSort)).MakeGenericMethod(typeof(string)),
                    postfix: new HarmonyMethod(typeof(Initializer).GetMethod(nameof(Postfix))));
    }

    public static void Postfix(ref IEnumerable<string> __result)
    {
        var list = new List<string>(__result);
        if (!list.Contains(NautilusGUID))
            return;

        list.Remove(NautilusGUID);
        list.Insert(0, NautilusGUID);

        __result = list.AsEnumerable();
    }

    [Obsolete("Should not be used!", true)]
    public static IEnumerable<string> TargetDLLs { get; } = new string[0];

    [Obsolete("Should not be used!", true)]
    public static void Patch(AssemblyDefinition ad) { }
}