namespace SMLHelper.Patchers.EnumPatching
{
    using System;
    using System.Collections.Generic;
    using Handlers;
    using Utility;

    internal class BackgroundTypePatcher
    {
        private const string BackgroundTypeEnumName = "BackgroundType";

        internal static readonly int startingIndex = 7; // last vanilla index is 6

        internal static readonly EnumCacheManager<CraftData.BackgroundType> cacheManager =
            new(
                enumTypeName: BackgroundTypeEnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(BackgroundTypeEnumName, PreRegisteredBackgroundTypes()));

        internal static CraftData.BackgroundType AddBackgroundType(string name)
        {
            EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
            {
                Name = name,
                Index = cacheManager.GetNextAvailableIndex()
            };

            CraftData.BackgroundType backgroundType = (CraftData.BackgroundType)cache.Index;

            if(cacheManager.Add(backgroundType, cache.Index, cache.Name))
            {
                InternalLogger.Log($"Successfully added Backgroundtype: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
            }
            else
            {
                InternalLogger.Log($"Failed adding Backgroundtype: '{name}' to Index: '{cache.Index}', Already Existed!", LogLevel.Warn);
            }

            return backgroundType;
        }

        private static List<int> PreRegisteredBackgroundTypes()
        {
            // Make sure to exclude already registered BackgroundTypes.
            // Be aware that this approach is still subject to race conditions.
            // Any mod that patches after this one will not be picked up by this method.
            // For those cases, there are additional ways of excluding these IDs.

            List<int> bannedIndices = new();

            Array enumValues = Enum.GetValues(typeof(CraftData.BackgroundType));

            foreach (object enumValue in enumValues)
            {
                if (enumValue == null)
                {
                    continue; // Saftey check
                }

                int realEnumValue = (byte)enumValue;

                if (realEnumValue < startingIndex)
                {
                    continue; // This is possibly a default Background
                }
                // Anything below this range we won't ever assign

                if (bannedIndices.Contains(realEnumValue))
                {
                    continue;// Already exists in list
                }

                bannedIndices.Add(realEnumValue);
            }

            InternalLogger.Log($"Finished known BackgroundType exclusion. {bannedIndices.Count} IDs were added in ban list.", LogLevel.Info);

            return bannedIndices;
        }

        internal static void Patch()
        {
            IngameMenuHandler.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            InternalLogger.Log($"Added {cacheManager.ModdedKeysCount} BackgroundTypes succesfully into the game.", LogLevel.Info);

            InternalLogger.Log("BackgroundTypePatcher is done.", LogLevel.Debug);
        }
    }
}
