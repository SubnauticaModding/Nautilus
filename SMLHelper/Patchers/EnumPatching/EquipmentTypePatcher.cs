namespace SMLHelper.Patchers.EnumPatching
{
    using System;
    using System.Collections.Generic;
    using SMLHelper.Assets;
    using Handlers;
    using Utility;

    internal class EquipmentTypePatcher
    {
        private const string EnumName = "EquipmentType";
        internal static readonly int startingIndex = 1000;

        internal static readonly EnumCacheManager<EquipmentType> cacheManager =
            new(
                enumTypeName: EnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(EnumName, PreRegisteredEquipmentTypes()));

        private static List<int> PreRegisteredEquipmentTypes()
        {
            List<int> preRegistered = new();
            foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
            {
                int typeCode = (int) type;
                if (typeCode >= startingIndex && !preRegistered.Contains(typeCode))
                {
                    preRegistered.Add(typeCode);
                }
            }
            InternalLogger.Log($"Finished known EquipmentType exclusion. {preRegistered.Count} IDs were added in ban list.");
            return preRegistered;
        }

        internal static EquipmentType AddEquipmentType(string name)
        {
            EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
            {
                Name = name,
                Index = cacheManager.GetNextAvailableIndex()
            };
            EquipmentType equipmentType = (EquipmentType) cache.Index;
            if(cacheManager.Add(equipmentType, cache.Index, cache.Name))
            {
                InternalLogger.Log($"Successfully added EquipmentType: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);
            }
            else
            {
                InternalLogger.Log($"Failed adding EquipmentType: '{name}' to Index: '{cache.Index}', Already Existed!", LogLevel.Warn);
            }

            return equipmentType;
        }

        internal static void Patch()
        {
            IngameMenuHandler.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());
            InternalLogger.Log($"Added {cacheManager.ModdedKeysCount} EquipmentTypes succesfully into the game.");
            InternalLogger.Log("EquipmentTypePatcher is done.", LogLevel.Debug);
        }
    }
}
