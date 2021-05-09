namespace SMLHelper.V2.Patchers.EnumPatching
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using SMLHelper.V2.Utility;

    internal class EnumPatcher
    {
        internal static void Patch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony);

            Logger.Log("EnumPatcher is done.", LogLevel.Debug);
        }

        [PatchUtils.Postfix]
        [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
        private static void Postfix_GetValues(Type enumType, ref Array __result)
        {
            if (enumType == typeof(TechType))
            {
                __result = GetValues(TechTypePatcher.cacheManager, __result);
            }
            else if (enumType == typeof(CraftTree.Type))
            {
                __result = GetValues(CraftTreeTypePatcher.cacheManager, __result);
            }
            else if (enumType == typeof(PingType))
            {
                __result = GetValues(PingTypePatcher.cacheManager, __result);
            }
            else if (enumType == typeof(TechGroup))
            {
                __result = GetValues(TechGroupPatcher.cacheManager, __result);
            }
            else if (enumType == typeof(TechCategory))
            {
                __result = GetValues(TechCategoryPatcher.cacheManager, __result);
            }
        }

        private static T[] GetValues<T>(EnumCacheManager<T> cacheManager, Array __result) where T : Enum
        {
            var list = new List<T>();
            foreach (T type in __result)
            {
                list.Add(type);
            }

            list.AddRange(cacheManager.ModdedKeys);
            return list.ToArray();
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(Enum), nameof(Enum.IsDefined))]
        private static bool Prefix_IsDefined(Type enumType, object value, ref bool __result)
        {
            if (IsDefined(TechTypePatcher.cacheManager, enumType, value) ||
                IsDefined(CraftTreeTypePatcher.cacheManager, enumType, value) ||
                IsDefined(PingTypePatcher.cacheManager, enumType, value) ||
                IsDefined(TechGroupPatcher.cacheManager, enumType, value) ||
                IsDefined(TechCategoryPatcher.cacheManager, enumType, value))
            {
                __result = true;
                return false;
            }

            return true;
        }

        private static bool IsDefined<T>(EnumCacheManager<T> cacheManager, Type enumType, object value) where T : Enum
        {
            return enumType.Equals(typeof(T)) && cacheManager.ContainsKey((T)value);
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(Enum), nameof(Enum.Parse), new[] { typeof(Type), typeof(string), typeof(bool) })]
        private static bool Prefix_Parse(Type enumType, string value, bool ignoreCase, ref object __result)
        {
            if (enumType == typeof(TechType) && TechTypePatcher.cacheManager.TryParse(value, out TechType techType))
            {
                __result = techType;
                return false;
            }
            else if (enumType == typeof(CraftTree.Type) && CraftTreeTypePatcher.cacheManager.TryParse(value, out CraftTree.Type craftTreeType))
            {
                __result = craftTreeType;
                return false;
            }
            else if (enumType == typeof(PingType) && PingTypePatcher.cacheManager.TryParse(value, out PingType pingType))
            {
                __result = pingType;
                return false;
            }
            else if (enumType == typeof(TechGroup) && TechGroupPatcher.cacheManager.TryParse(value, out TechGroup techGroup))
            {
                __result = techGroup;
                return false;
            }
            else if (enumType == typeof(TechCategory) && TechCategoryPatcher.cacheManager.TryParse(value, out TechCategory techCategory))
            {
                __result = techCategory;
                return false;
            }
            return true;
        }

        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(Enum), nameof(Enum.ToString), new Type[] { })]
        private static bool Prefix_ToString(Enum __instance, ref string __result)
        {
            switch (__instance)
            {
                case TechType techType when TechTypePatcher.cacheManager.TryGetValue(techType, out string techTypeName):
                    __result = techTypeName;
                    return false;

                case CraftTree.Type craftTreeType when CraftTreeTypePatcher.cacheManager.TryGetValue(craftTreeType, out var craftTreeTypeName):
                    __result = craftTreeTypeName;
                    return false;

                case PingType pingType when PingTypePatcher.cacheManager.TryGetValue(pingType, out var pingTypeName):
                    __result = pingTypeName;
                    return false;

                case TechGroup techGroup when TechGroupPatcher.cacheManager.TryGetValue(techGroup, out var techGroupName):
                    __result = techGroupName;
                    return false;

                case TechCategory techCategory when TechCategoryPatcher.cacheManager.TryGetValue(techCategory, out var techCategoryName):
                    __result = techCategoryName;
                    return false;

                default:
                    return true;
            }
        }
    }
}
