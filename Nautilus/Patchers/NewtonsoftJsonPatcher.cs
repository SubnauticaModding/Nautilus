using HarmonyLib;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using Nautilus.Utility;
using System.Reflection.Emit;
using Nautilus.Handlers;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

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
        InternalLogger.Warn("BEGINNING TRANSPILER!");
        bool found = false;
        int i = 0;
        foreach (var instruction in instructions)
        {
            InternalLogger.Warn(i + ": " + instruction.opcode + " ~ " + instruction.operand);
            if (instruction.operand != null)
            {
                InternalLogger.Warn(instruction.operand.GetType().ToString());
            }
            yield return instruction;
            if (instruction.opcode == OpCodes.Stloc_S && (instruction.operand is LocalBuilder builder && builder.LocalIndex == 6))
            {
                InternalLogger.Error("FOUND!!");
                yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)6);
                var func = AccessTools.Method(typeof(NewtonsoftJsonPatcher), nameof(NewtonsoftJsonPatcher.IsEnumValueModded));
                yield return Transpilers.EmitDelegate(IsEnumValueModded);
                var stelem = instructions.Last((instr) => instr.opcode == OpCodes.Stelem_Ref);
                var endOfForLoop = stelem.labels[0];
                InternalLogger.Error("E: " + endOfForLoop);
                yield return new CodeInstruction(OpCodes.Brtrue_S, endOfForLoop);
                found = true;
            }
            i++;
        }
        if (found)
        {
            InternalLogger.Warn("TRANSPILER SUCCEEDED!");
        }
        else
        {
            InternalLogger.Warn("TRANSPILER FAILED!");
        }
    }

    private static bool IsEnumValueModded(string text)
    {
        InternalLogger.Warn(text);
        bool isCustom = EnumBuilder<TechType>.CacheManager.ContainsKey(text);
        InternalLogger.Log("Custom: " + isCustom);
        return isCustom;
    }
}