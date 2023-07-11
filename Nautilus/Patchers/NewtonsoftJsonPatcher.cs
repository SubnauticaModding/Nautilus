using HarmonyLib;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using Nautilus.Utility;
using System.Reflection.Emit;
using Nautilus.Handlers;
using System.Linq;
using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Nautilus.Patchers;

internal static class NewtonsoftJsonPatcher
{
    // key: enum type, value: cache manager instances
    private static Dictionary<Type, object> _cachedCacheManagers = new();
    // key: enum type, value: methodinfo of the cache manager containskey method (string overload)
    private static Dictionary<Type, MethodInfo> _cachedCacheManagerContainsStringKeyMethods = new();
    // key: enum type, value: methodinfo of the cache manager containskey method (object overload)
    private static Dictionary<Type, MethodInfo> _cachedCacheManagerContainsEnumKeyMethods = new();
    // key: enum type, value: methodinfo of the cache manager ValueToName method
    private static Dictionary<Type, MethodInfo> _cachedCacheManagerValueToNameMethods = new();

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.InitializeValuesAndNames)),
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.InitializeValuesAndNamesTranspiler))));

        // I had to do this because I was unable to search for nullable parameters (something like typeof(string?) does not work):
        var toStringMethod = typeof(EnumUtils)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First((meth) => meth.Name == "TryToString" && !meth.GetParameters().Types().Contains(typeof(bool)));

        harmony.Patch(toStringMethod, postfix: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.EnumUtilsTryToStringPostfix))));

        harmony.Patch(AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.ParseEnum)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.EnumUtilsParseEnumPostfix))));

        harmony.Patch(AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.MatchName)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.EnumUtilsMatchNamePostfix))));
    }

    private static IEnumerable<CodeInstruction> InitializeValuesAndNamesTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        bool found = false;
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (!found && instruction.opcode == OpCodes.Stloc_S && (instruction.operand is LocalBuilder builder && builder.LocalIndex == 6))
            {
                // load the text variable to the eval stack
                yield return new CodeInstruction(OpCodes.Ldloc_S, (byte) 6);
                // load the type local variable (type of enum) to the eval stack
                yield return new CodeInstruction(OpCodes.Ldloc_0, (byte) 6);
                // call the IsEnumValueModded method
                var func = AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.IsEnumValueModdedByString));
                yield return Transpilers.EmitDelegate(IsEnumValueModdedByString);
                // find the label at the bottom of the for loop
                var stelem = instructions.Last((instr) => instr.opcode == OpCodes.Stelem_Ref);
                var endOfForLoop = stelem.labels[0];
                // insert a jump IF AND ONLY IF the IsEnumValueModded method returned true
                yield return new CodeInstruction(OpCodes.Brtrue_S, endOfForLoop);
                found = true;
            }
        }
        InternalLogger.Log("NewtonsoftJsonPatcher.InitializeValuesAndNamesTranspiler succeeded: " + found);
    }

    private static bool IsEnumValueModdedByString(string text, Type enumType)
    {
        UpdateCachedEnumCacheManagers(enumType);
        return (bool) _cachedCacheManagerContainsStringKeyMethods[enumType].Invoke(_cachedCacheManagers[enumType], new object[] { text });
    }

    private static bool IsEnumValueModdedByObject(object value, Type enumType)
    {
        UpdateCachedEnumCacheManagers(enumType);
        return (bool) _cachedCacheManagerContainsEnumKeyMethods[enumType].Invoke(_cachedCacheManagers[enumType], new object[] { value });
    }

    private static void EnumUtilsTryToStringPostfix(Type enumType, ref bool __result, object value, ref string name)
    {
        if (__result == true)
            return;
        var isEnumCustom = IsEnumValueModdedByObject(value, enumType);
        if (!isEnumCustom)
            return;
        name = (string) _cachedCacheManagerValueToNameMethods[enumType].Invoke(_cachedCacheManagers[enumType], new object[] { value });
        __result = true;
    }

    private static void EnumUtilsParseEnumPostfix(Type enumType)
    {
        _lastModifiedEnumType = enumType;
    }

    // carries over from ParseEnum to MatchName
    private static Type _lastModifiedEnumType;

    // this method is only ever called by ParseEnum
    private static void EnumUtilsMatchNamePostfix(ref int? __result, string value)
    {
        // if the enum value was already found by the original method, we don't need to worry about this

        if (__result.HasValue)
            return;

        // don't run for enum types that don't have existing cached values

        if (_lastModifiedEnumType == null || !CacheManagerExists(_lastModifiedEnumType))
            return;

        if (EnumCacheProvider.CacheManagers.TryGetValue(_lastModifiedEnumType, out var enumCacheManager))
        {
            if (enumCacheManager.TryParse(value, out var enumValue))
            {
                __result = (int) enumValue;
            }
        }
    }

    private static bool CacheManagerExists(Type enumType)
    {
        return EnumCacheProvider.TryGetManager(enumType, out _);
    }

    // makes sure a cache manager of the given enum exists
    private static void UpdateCachedEnumCacheManagers(Type enumType)
    {
        if (!_cachedCacheManagers.ContainsKey(enumType))
        {
            var enumBuilderType = typeof(EnumBuilder<>).MakeGenericType(enumType);
            var cacheManager = enumBuilderType.GetProperty("CacheManager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            _cachedCacheManagers[enumType] = cacheManager;
            _cachedCacheManagerContainsEnumKeyMethods.Add(enumType, AccessTools.Method(cacheManager.GetType(), "ContainsEnumKey"));
            _cachedCacheManagerContainsStringKeyMethods.Add(enumType, AccessTools.Method(cacheManager.GetType(), "ContainsStringKey"));
            _cachedCacheManagerValueToNameMethods.Add(enumType, AccessTools.Method(cacheManager.GetType(), "ValueToName"));
        }
    }
}