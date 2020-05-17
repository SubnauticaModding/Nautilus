using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace SMLHelper.V2.Patchers
{
    internal class PingTypePatcher
    {
        private const string TechTypeEnumName = "PingType";
        internal static readonly int startingIndex = 10;
        internal static readonly List<int> bannedIndices = new List<int>();
        private static readonly Dictionary<PingType, Atlas.Sprite> cachedSprites = new Dictionary<PingType, Atlas.Sprite>();

        internal static readonly EnumCacheManager<PingType> cacheManager =
            new EnumCacheManager<PingType>(
                enumTypeName: TechTypeEnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(TechTypeEnumName, bannedIndices, PreRegisteredCraftTreeTypes()));

        private static List<int> PreRegisteredCraftTreeTypes()
        {
            var declaredPingTypes = (PingType[]) Enum.GetValues(typeof(PingType));
            var bannedIndices = declaredPingTypes.Select(t => (int) t)
                .Where(t => t > startingIndex)
                .Distinct()
                .ToList();
            
            Logger.Log($"Finished known PingType exclusion. {bannedIndices.Count} IDs were added in ban list.");
            return bannedIndices;
        }

        internal static PingType AddPingType(string name, Atlas.Sprite sprite)
        {
            var cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
            {
                Name = name,
                Index = cacheManager.GetNextAvailableIndex()
            };

            if (cacheManager.IsIndexAvailable(cache.Index))
                cache.Index = cacheManager.GetNextAvailableIndex();

            var pingType = (PingType) cache.Index;
            cacheManager.Add(pingType, cache.Index, cache.Name);
            
            cachedSprites.Add(pingType, sprite);
            if (PingManager.sCachedPingTypeStrings.valueToString.ContainsKey(pingType) == false)
                PingManager.sCachedPingTypeStrings.valueToString.Add(pingType, name);

            if (PingManager.sCachedPingTypeTranslationStrings.valueToString.ContainsKey(pingType) == false)
                PingManager.sCachedPingTypeTranslationStrings.valueToString.Add(pingType, name);
            
            Logger.Log($"Successfully added PingType: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
            return pingType;
        }

        internal static PingType? GetPingType(string pingName)
        {
            try
            {
                var ping = Enum.Parse(typeof(PingType), pingName);
                return (PingType) ping;
            }
            catch (ArgumentException e)
            {
                return new PingType?();
            }
        }
        
        internal static void Patch(HarmonyInstance harmony)
        {
            IngameMenuHandler.Main.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetValues)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Postfix_GetValues))));

            harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.IsDefined)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Prefix_IsDefined))));

            harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) }),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Prefix_Parse))));

            harmony.Patch(AccessTools.Method(typeof(PingType), nameof(PingType.ToString), new Type[] { }),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Prefix_ToString))));
            
            harmony.Patch(AccessTools.Method(typeof(SpriteManager), nameof(SpriteManager.Get), new Type[] { typeof(SpriteManager.Group), typeof(string) }),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Prefix_SpriteManager_Get))));

            Logger.Log($"Added {cacheManager.ModdedKeysCount} PingTypes succesfully into the game.");
            Logger.Log("PingTypePatcher is done.", LogLevel.Debug);
        }

        private static bool Prefix_SpriteManager_Get(SpriteManager.Group group, string name, ref Atlas.Sprite __result)
        {
            if (group == SpriteManager.Group.Pings)
            {
                var pingType = GetPingType(name);
                if (pingType.HasValue && cachedSprites.ContainsKey(pingType.Value))
                {
                    __result = cachedSprites[pingType.Value];
                    return false;
                }
            }

            return true;
        }
        
        private static void Postfix_GetValues(Type enumType, ref Array __result)
        {
            if (enumType == typeof(PingType))
            {
                __result = __result.Cast<PingType>().Concat(cacheManager.ModdedKeys).ToArray();
            }
        }
        
        private static bool Prefix_IsDefined(Type enumType, object value, ref bool __result)
        {
            var result = enumType == typeof(PingType) && cacheManager.ContainsKey((PingType) value);
            __result |= result;
            return !result;
        }
        
        private static bool Prefix_Parse(Type enumType, string value, bool ignoreCase, ref object __result)
        {
            if (enumType == typeof(PingType) && cacheManager.TryParse(value, out var pingType))
            {
                __result = pingType;
                return false;
            }

            return true;
        }
        
        private static bool Prefix_ToString(Enum __instance, ref string __result)
        {
            if (__instance is PingType pingType)
            {
                if (cacheManager.TryGetValue(pingType, out var pingTypeName))
                {
                    __result = pingTypeName;
                    return false;
                }
            }

            return true;
        }
    }
}
