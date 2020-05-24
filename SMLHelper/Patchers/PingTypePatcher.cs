namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Collections.Generic;
    using Harmony;
    using Handlers;
    using Utility;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#elif BELOWZERO
    using Sprite = UnityEngine.Sprite;
#endif
    
    internal class PingTypePatcher
    {
        private const string EnumName = "PingType";
        internal static readonly int startingIndex = 1000;
        internal static readonly List<int> bannedIndices = new List<int>();

        internal static readonly EnumCacheManager<PingType> cacheManager =
            new EnumCacheManager<PingType>(
                enumTypeName: EnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(EnumName, bannedIndices, PreRegisteredPingTypes()));

        private static List<int> PreRegisteredPingTypes()
        {
            var preRegistered = new List<int>();
            foreach (PingType type in Enum.GetValues(typeof(PingType)))
            {
                var typeCode = (int) type;
                if (typeCode >= startingIndex && !preRegistered.Contains(typeCode))
                {
                    preRegistered.Add(typeCode);
                }
            }
            
            Logger.Log($"Finished known PingType exclusion. {preRegistered.Count} IDs were added in ban list.");
            return bannedIndices;
        }

        internal static PingType AddPingType(string name, Sprite sprite)
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
            SpritePatcher.AddSprite(SpriteManager.Group.Pings, pingType.ToString(), sprite);
            
            if (PingManager.sCachedPingTypeStrings.valueToString.ContainsKey(pingType) == false)
                PingManager.sCachedPingTypeStrings.valueToString.Add(pingType, name);

            if (PingManager.sCachedPingTypeTranslationStrings.valueToString.ContainsKey(pingType) == false)
                PingManager.sCachedPingTypeTranslationStrings.valueToString.Add(pingType, name);
            
            Logger.Log($"Successfully added PingType: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
            return pingType;
        }

        internal static void Patch(HarmonyInstance harmony)
        {
            IngameMenuHandler.Main.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetValues)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Postfix_GetValues))));

            harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.IsDefined)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Prefix_IsDefined))));

            harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.Parse), new[] { typeof(Type), typeof(string), typeof(bool) }),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Prefix_Parse))));

            harmony.Patch(AccessTools.Method(typeof(PingType), nameof(PingType.ToString), new Type[] { }),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(PingTypePatcher), nameof(Prefix_ToString))));

            Logger.Log($"Added {cacheManager.ModdedKeysCount} PingTypes succesfully into the game.");
            Logger.Log("PingTypePatcher is done.", LogLevel.Debug);
        }
        
        private static void Postfix_GetValues(Type enumType, ref Array __result)
        {
            var list = new List<PingType>();
            foreach (PingType type in __result)
            {
                list.Add(type);
            }

            list.AddRange(cacheManager.ModdedKeys);
            __result = list.ToArray();
        }
        
        private static bool Prefix_IsDefined(Type enumType, object value, ref bool __result)
        {
            if (enumType == typeof(PingType) && cacheManager.ContainsKey((PingType) value))
            {
                __result = true;
                return false;
            }

            return true;
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
