namespace SMLHelper.Patchers.EnumPatching
{
    using BepInEx.Logging;
    using Crafting;
    using Handlers;
    using System;
    using System.Collections.Generic;
    using Utility;

    internal class CraftTreeTypePatcher
    {
        private const string CraftTreeTypeEnumName = "CraftTreeType";

        internal const int startingIndex = 11; // The default CraftTree.Type contains indexes 0 through 10

        internal static readonly EnumCacheManager<CraftTree.Type> cacheManager =
            new(
                enumTypeName: CraftTreeTypeEnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(CraftTreeTypeEnumName, PreRegisteredCraftTreeTypes()));

        internal static ModCraftTreeRoot CreateCustomCraftTreeAndType(string name, out CraftTree.Type craftTreeType)
        {
            EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
            {
                Name = name,
                Index = cacheManager.GetNextAvailableIndex()
            };

            craftTreeType = (CraftTree.Type)cache.Index;

            if(cacheManager.Add(craftTreeType, cache.Index, cache.Name))
            {
                InternalLogger.Log($"Successfully added CraftTree Type: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
            }
            else
            {
                InternalLogger.Log($"Failed adding CraftTree Type: '{name}' to Index: '{cache.Index}', Already Existed!", LogLevel.Warning);
            }

            if(!CraftTreePatcher.CustomTrees.TryGetValue(craftTreeType, out ModCraftTreeRoot customTreeRoot))
            {
                customTreeRoot = new ModCraftTreeRoot(craftTreeType, name);
            }

            CraftTreePatcher.CustomTrees[craftTreeType] = customTreeRoot;

            return customTreeRoot;
        }

        private static List<int> PreRegisteredCraftTreeTypes()
        {
            // Make sure to exclude already registered CraftTreeTypes.
            // Be aware that this approach is still subject to race conditions.
            // Any mod that patches after this one will not be picked up by this method.
            // For those cases, there are additional ways of excluding these IDs.

            List<int> bannedIndices = new();

            Array enumValues = Enum.GetValues(typeof(CraftTree.Type));

            foreach (object enumValue in enumValues)
            {
                if (enumValue == null)
                {
                    continue; // Saftey check
                }

                int realEnumValue = (int)enumValue;

                if (realEnumValue < startingIndex)
                {
                    continue; // This is possibly a default tree
                }
                // Anything below this range we won't ever assign

                if (bannedIndices.Contains(realEnumValue))
                {
                    continue;// Already exists in list
                }

                bannedIndices.Add(realEnumValue);
            }

            InternalLogger.Log($"Finished known CraftTreeType exclusion. {bannedIndices.Count} IDs were added in ban list.", LogLevel.Info);

            return bannedIndices;
        }

        internal static void Patch()
        {
            IngameMenuHandler.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            InternalLogger.Log($"Added {cacheManager.ModdedKeysCount} CraftTreeTypes succesfully into the game.");
            InternalLogger.Log("CraftTreeTypePatcher is done.", LogLevel.Debug);
        }
    }
}
