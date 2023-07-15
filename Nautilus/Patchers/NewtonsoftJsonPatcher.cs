using HarmonyLib;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using Nautilus.Utility;
using System.Reflection.Emit;
using Nautilus.Handlers;
using System.Linq;
using System;
using System.Reflection;

namespace Nautilus.Patchers;

// Patches methods in the Newtonsoft.Json.Utilities.EnumUtils class to ensure that custom enums are handled properly, without error
internal static class NewtonsoftJsonPatcher
{
    // Key: Enum type / Value: Instances of the EnumCacheManager class
    private static Dictionary<Type, object> _cachedCacheManagers = new();
    // Key: Enum type / Value: MethodInfo of the EnumCacheManager.ContainsKey method (string overload)
    private static Dictionary<Type, MethodInfo> _cachedCacheManagerContainsStringKeyMethods = new();
    // Key: Enum type / Value: MethodInfo of the EnumCacheManager.ContainsKey method (object overload)
    private static Dictionary<Type, MethodInfo> _cachedCacheManagerContainsEnumKeyMethods = new();
    // Key: Enum type / Value: MethodInfo of the EnumCacheManager.ValueToName method
    private static Dictionary<Type, MethodInfo> _cachedCacheManagerValueToNameMethods = new();

    public static void Patch(Harmony harmony)
    {
        // Transpiler to skip the initialization of custom enum values:

        harmony.Patch(AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.InitializeValuesAndNames)),
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.InitializeValuesAndNamesTranspiler))));

        /* Postfix to allow custom enum values to be converted to strings:
        * I had to do this because I was unable to filter for methods based on their nullable parameters.
        * I can't just put typeof(string?) in the list of parameter types.
        * This method is *good enough* and attempts to find an overload of TryToString with a boolean... let's just hope Newtonsoft.Json isn't updated!
        */
        var toStringMethod = typeof(EnumUtils)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First((meth) => meth.Name == "TryToString" && !meth.GetParameters().Types().Contains(typeof(bool)));

        harmony.Patch(toStringMethod, postfix: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.EnumUtilsTryToStringPostfix))));

        // Prefix that checks an enum has custom entries, and if so, attempts to parse a custom enum value:

        harmony.Patch(AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.ParseEnum)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.EnumUtilsParseEnumPrefix))));
    }

    // Skip initialization of custom enum values
    private static IEnumerable<CodeInstruction> InitializeValuesAndNamesTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (!found && instruction.opcode == OpCodes.Stloc_S && (instruction.operand is LocalBuilder builder && builder.LocalIndex == 6))
            {
                // load the text variable to the eval stack
                yield return new CodeInstruction(OpCodes.Ldloc_S, (byte) 6);
                // load the type local variable (type of enum) to the eval stack
                yield return new CodeInstruction(OpCodes.Ldloc_0, (byte) 6);
                // call the IsEnumValueModdedByString method
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

    // Returns true if the enum string value is custom
    private static bool IsEnumValueModdedByString(string text, Type enumType)
    {
        UpdateCachedEnumCacheManagers(enumType);
        return (bool) _cachedCacheManagerContainsStringKeyMethods[enumType].Invoke(_cachedCacheManagers[enumType], new object[] { text });
    }

    // Returns true if the enum object value is custom
    private static bool IsEnumValueModdedByObject(object value, Type enumType)
    {
        UpdateCachedEnumCacheManagers(enumType);
        return (bool) _cachedCacheManagerContainsEnumKeyMethods[enumType].Invoke(_cachedCacheManagers[enumType], new object[] { value });
    }

    // Postfix to EnumUtils.TryToString that checks for custom enum values in the case that the method failed to find a built-in enum value name
    private static void EnumUtilsTryToStringPostfix(Type enumType, ref bool __result, object value, ref string name)
    {
        // Don't run if we already found a name
        if (__result == true)
            return;
        // Don't run if this enum value isn't modded
        var isEnumCustom = IsEnumValueModdedByObject(value, enumType);
        if (!isEnumCustom)
            return;
        name = (string) _cachedCacheManagerValueToNameMethods[enumType].Invoke(_cachedCacheManagers[enumType], new object[] { value });
        __result = true;
    }

    // Prefix to EnumUtils.ParseEnum that exits early if the enum value is custom (this is needed in order to avoid annoying exceptions)
    private static bool EnumUtilsParseEnumPrefix(ref object __result, Type enumType, string value)
    {
        if (TryParseCustomEnumValue(enumType, value, out var enumVal))
        {
            __result = enumVal;
            return false;
        }
        return true;
    }

    // Attempts to convert 'name' to an enum value (val)
    public static bool TryParseCustomEnumValue(Type enumType, string name, out int val)
    {
        val = 0;

        if (!CacheManagerExists(enumType))
            return false;

        if (EnumCacheProvider.CacheManagers.TryGetValue(enumType, out var enumCacheManager))
        {
            if (enumCacheManager.TryParse(name, out var enumValue))
            {
                val = (int) enumValue;
                return true;
            }
        }
        return false;
    }

    // Returns true if an enum has any custom values at all
    private static bool CacheManagerExists(Type enumType)
    {
        return EnumCacheProvider.TryGetManager(enumType, out _);
    }

    // If a cache manager of the given enum is not already cached, then cache it
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