using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMLHelper.V2.Patchers.EnumPatching
{
    internal class BiomeTypePatcher
    {
        private const string BiomeTypeEnumName = "BiomeType";
        internal static readonly int startingIndex = 10017;
        internal static readonly EnumCacheManager<BiomeType> cacheManager =
            new EnumCacheManager<BiomeType>(
                enumTypeName: BiomeTypeEnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(BiomeTypeEnumName, PreRegisteredBiomeGroups()));

        internal static BiomeType AddBiomeGroup(string name)
        {
            EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
            {
                Name = name,
                Index = cacheManager.GetNextAvailableIndex()
            };
            BiomeType biomeType = (BiomeType)cache.Index;
            cacheManager.Add(biomeType, cache.Index, cache.Name);
            return biomeType;
        }
        private static List<int> PreRegisteredBiomeGroups()
        {
            var bannedIndices = new List<int>();
            Array enumValues = Enum.GetValues(typeof(BiomeType));
            foreach (object enumValue in enumValues)
            {
                if (enumValue == null)
                    continue;
                int realEnumValue = (byte)enumValue;
                if (realEnumValue < startingIndex)
                    continue;
                if (bannedIndices.Contains(realEnumValue))
                    continue;
                bannedIndices.Add(realEnumValue);
            }
            return bannedIndices;
        }
        internal static void Patch()
        {
            IngameMenuHandler.Main.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());
        }
    }
}
