namespace SMLHelper.V2.Patchers.EnumPatching
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using Utility;

    internal class TechGroupPatcher
    {
        private const string TechGroupEnumName = "TechGroup";

        internal const int startingIndex = 15; // The default TechGroup contains indexes 0 through 14

        internal static readonly EnumCacheManager<TechGroup> cacheManager =
            new EnumCacheManager<TechGroup>(
                enumTypeName: TechGroupEnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(TechGroupEnumName, PreRegisteredTechGroupTypes()));

        internal static TechGroup AddTechGroup(string name)
        {
            EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
            {
                Name = name,
                Index = cacheManager.GetNextAvailableIndex()
            };

            TechGroup techGroup = (TechGroup)cache.Index;

            if(cacheManager.Add(techGroup, cache.Index, cache.Name))
            {

                if (!uGUI_BlueprintsTab.groups.Contains(techGroup))
                    uGUI_BlueprintsTab.groups.Add(techGroup);

                if (!CraftData.groups.ContainsKey(techGroup))
                    CraftData.groups[techGroup] = new Dictionary<TechCategory, List<TechType>>();

                InternalLogger.Log($"Successfully added TechGroup: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
            }
            else
            {
                InternalLogger.Log($"Failed adding TechGroup: '{name}' to Index: '{cache.Index}', Already Existed!", LogLevel.Warn);
            }


            return techGroup;
        }

        private static List<int> PreRegisteredTechGroupTypes()
        {
            // Make sure to exclude already registered CraftTreeTypes.
            // Be aware that this approach is still subject to race conditions.
            // Any mod that patches after this one will not be picked up by this method.
            // For those cases, there are additional ways of excluding these IDs.

            var bannedIndices = new List<int>();

            Array enumValues = Enum.GetValues(typeof(TechGroup));

            foreach (object enumValue in enumValues)
            {
                if (enumValue == null)
                    continue; // Saftey check

                int realEnumValue = (int)enumValue;

                if (realEnumValue < startingIndex)
                    continue; // This is possibly a default tree
                // Anything below this range we won't ever assign

                if (bannedIndices.Contains(realEnumValue))
                    continue;// Already exists in list

                bannedIndices.Add(realEnumValue);
            }

            InternalLogger.Log($"Finished known TechGroup exclusion. {bannedIndices.Count} IDs were added in ban list.", LogLevel.Info);

            return bannedIndices;
        }

        internal static void Patch()
        {
            IngameMenuHandler.Main.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());

            InternalLogger.Log($"Added {cacheManager.ModdedKeysCount} TechGroups succesfully into the game.");
            InternalLogger.Log("TechGroupPatcher is done.", LogLevel.Debug);
        }
    }
}
