namespace SMLHelper.V2
{
    using System;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections.Generic;
    using Harmony;

    internal static class PatchUtils
    {
        internal static void PatchDictionary<KeyType, ValueType>(Dictionary<KeyType, ValueType> original, IDictionary<KeyType, ValueType> patches)
        {
            foreach (KeyValuePair<KeyType, ValueType> entry in patches)
            {
                original[entry.Key] = entry.Value;
            }
        }

        internal static void PatchList<ValueType>(List<ValueType> original, IList<ValueType> patches)
        {
            foreach (ValueType entry in patches)
            {
                original.Add(entry);
            }
        }

        // use methods from 'typeWithPatchMethods' class as harmony patches
        // valid method need to have HarmonyPatch and Harmony[Prefix/Postfix/Transpiler] attributes
        // if typeWithPatchMethods is null, we use type from which this method is called
        internal static void PatchClass(HarmonyInstance harmony, Type typeWithPatchMethods = null)
        {
            MethodInfo _getTargetMethod(HarmonyMethod hm) => AccessTools.Method(hm.declaringType, hm.methodName, hm.argumentTypes);

            if (typeWithPatchMethods == null)
                typeWithPatchMethods = new StackTrace().GetFrame(1).GetMethod().ReflectedType;

            foreach (var method in typeWithPatchMethods.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                HarmonyMethod _method_if<H>() => method.IsDefined(typeof(H), false)? new HarmonyMethod(method): null;

                if (Attribute.GetCustomAttribute(method, typeof(HarmonyPatch)) is HarmonyPatch harmonyPatch)
                    harmony.Patch(_getTargetMethod(harmonyPatch.info), _method_if<HarmonyPrefix>(), _method_if<HarmonyPostfix>(), _method_if<HarmonyTranspiler>());
            }
        }
    }
}
