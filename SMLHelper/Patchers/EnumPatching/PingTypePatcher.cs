namespace SMLHelper.Patchers.EnumPatching
{
    using System;
    using System.Collections.Generic;
    using SMLHelper.Assets;
    using Handlers;
    using Utility;
    using UnityEngine;
    using BepInEx.Logging;

    internal class PingTypePatcher
    {
        private const string EnumName = "PingType";
        internal static readonly int startingIndex = 1000;

        internal static readonly EnumCacheManager<PingType> cacheManager =
            new(
                enumTypeName: EnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(EnumName, PreRegisteredPingTypes()));

        private static List<int> PreRegisteredPingTypes()
        {
            List<int> preRegistered = new();
            foreach (PingType type in Enum.GetValues(typeof(PingType)))
            {
                int typeCode = (int) type;
                if (typeCode >= startingIndex && !preRegistered.Contains(typeCode))
                {
                    preRegistered.Add(typeCode);
                }
            }
            
            InternalLogger.Log($"Finished known PingType exclusion. {preRegistered.Count} IDs were added in ban list.");
            return preRegistered;
        }

#if SUBNAUTICA
        internal static PingType AddPingType(string name, Atlas.Sprite sprite)
        {
            PingType pingType = PingType.None;
            foreach(var pair in PingManager.sCachedPingTypeStrings.valueToString)
            {
                if(pair.Value == name)
                {
                    pingType = pair.Key;
                    break;
                }
            }

            if(pingType == PingType.None)
            {
                var cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
                {
                    Name = name,
                    Index = cacheManager.GetNextAvailableIndex()
                };

                pingType = (PingType)cache.Index;
                if(cacheManager.Add(pingType, cache.Index, cache.Name))
                {
                    ModSprite.Add(SpriteManager.Group.Pings, cache.Name, sprite);

                    if(PingManager.sCachedPingTypeStrings.valueToString.ContainsKey(pingType) == false)
                        PingManager.sCachedPingTypeStrings.valueToString.Add(pingType, name);

                    if(PingManager.sCachedPingTypeTranslationStrings.valueToString.ContainsKey(pingType) == false)
                        PingManager.sCachedPingTypeTranslationStrings.valueToString.Add(pingType, name);

                    InternalLogger.Log($"Successfully added PingType: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
                }
                else
                {
                    InternalLogger.Log($"Failed adding PingType: '{name}' to Index: '{cache.Index}', Already Existed!", LogLevel.Warning);
                }
            }
            else
            {
                InternalLogger.Log($"Failed adding PingType: '{name}', Already Existed!", LogLevel.Warning);
            }
            return pingType;
        }
#endif

        internal static PingType AddPingType(string name, Sprite sprite)
        {
            PingType pingType = PingType.None;
            foreach(KeyValuePair<PingType, string> pair in PingManager.sCachedPingTypeStrings.valueToString)
            {
                if(pair.Value == name)
                {
                    pingType = pair.Key;
                    break;
                }
            }

            if(pingType == PingType.None)
            {
                EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
                {
                    Name = name,
                    Index = cacheManager.GetNextAvailableIndex()
                };

                pingType = (PingType)cache.Index;
                if(cacheManager.Add(pingType, cache.Index, cache.Name))
                {
                    ModSprite.Add(SpriteManager.Group.Pings, cache.Name, sprite);

                    if(PingManager.sCachedPingTypeStrings.valueToString.ContainsKey(pingType) == false)
                    {
                        PingManager.sCachedPingTypeStrings.valueToString.Add(pingType, name);
                    }

                    if(PingManager.sCachedPingTypeTranslationStrings.valueToString.ContainsKey(pingType) == false)
                    {
                        PingManager.sCachedPingTypeTranslationStrings.valueToString.Add(pingType, name);
                    }

                    InternalLogger.Log($"Successfully added PingType: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
                }
                else
                {
                    InternalLogger.Log($"Failed adding PingType: '{name}' to Index: '{cache.Index}', Already Existed!", LogLevel.Warning);
                }
            }
            else
            {
                InternalLogger.Log($"Failed adding PingType: '{name}', Already Existed!", LogLevel.Warning);
            }
            return pingType;
        }

        internal static void Patch()
        {
            IngameMenuHandler.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            InternalLogger.Log($"Added {cacheManager.ModdedKeysCount} PingTypes succesfully into the game.");

            InternalLogger.Log("PingTypePatcher is done.", LogLevel.Debug);
        }
    }
}
