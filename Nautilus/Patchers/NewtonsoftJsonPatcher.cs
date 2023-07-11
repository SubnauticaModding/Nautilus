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

internal static class NewtonsoftJsonPatcher
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(EnumUtils), nameof(EnumUtils.InitializeValuesAndNames)),
            transpiler: new HarmonyMethod(AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.InitializeValuesAndNamesTranspiler))));
    }

    private static IEnumerable<CodeInstruction> InitializeValuesAndNamesTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
    {
        bool found = false;
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (instruction.opcode == OpCodes.Stloc_S && (instruction.operand is LocalBuilder builder && builder.LocalIndex == 6))
            {
                // load the text variable to the eval stack
                yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)6);
                // load the type local variable (type of enum) to the eval stack
                yield return new CodeInstruction(OpCodes.Ldloc_0, (byte) 6);
                // call the IsEnumValueModded method
                var func = AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.IsEnumValueModded));
                yield return Transpilers.EmitDelegate(IsEnumValueModded);
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

    private static bool IsEnumValueModded(string text, Type enumType)
    {
        if (!_cachedCacheManagerContainsKeyMethods.TryGetValue(enumType, out var containsKeyMethod))
        {
            var enumBuilderType = typeof(EnumBuilder<>).MakeGenericType(enumType);
            var cacheManager = enumBuilderType.GetProperty("CacheManager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            containsKeyMethod = AccessTools.Method(cacheManager.GetType(), "ContainsKey", new Type[] { typeof(string) });
            _cachedCacheManagers.Add(enumType, cacheManager);
            _cachedCacheManagerContainsKeyMethods.Add(enumType, containsKeyMethod);
        }
        bool isCustom = (bool)containsKeyMethod.Invoke(_cachedCacheManagers[enumType], new object[] { text });
        return isCustom;
    }

    // key: enum type, value: cache manager instances
    private static Dictionary<Type, object> _cachedCacheManagers = new();
    // key: enum type, value: methodinfo of the cache manager containskey method
    private static Dictionary<Type, MethodInfo> _cachedCacheManagerContainsKeyMethods = new();
}