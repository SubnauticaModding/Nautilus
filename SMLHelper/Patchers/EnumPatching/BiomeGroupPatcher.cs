using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMLHelper.V2.Patchers.EnumPatching
{
    internal class BiomeGroupPatcher
    {
        private const string BiomeGroupEnumName = "BiomeGroup";
        internal static readonly int startingIndex = 8002;
        internal static readonly EnumCacheManager<BiomeGroup> cacheManager =
            new EnumCacheManager<BiomeGroup>(
                enumTypeName: BiomeGroupEnumName,
                startingIndex: startingIndex,
                bannedIDs: ExtBannedIdManager.GetBannedIdsFor(BiomeGroupEnumName,PreRegisteredBiomeGroups()));

        internal static BiomeGroup AddBiomeGroup(string name)
        {
            EnumTypeCache cache = cacheManager.RequestCacheForTypeName(name) ?? new EnumTypeCache()
            {
                Name = name,
                Index = cacheManager.GetNextAvailableIndex()
            };
            BiomeGroup biomeGroup = (BiomeGroup)cache.Index;
            cacheManager.Add(biomeGroup, cache.Index, cache.Name);
            return biomeGroup;
        }
        private static List<int> PreRegisteredBiomeGroups()
        {
            var bannedIndices = new List<int>();
            Array enumValues = Enum.GetValues(typeof(BiomeGroup));
            foreach(object enumValue in enumValues)
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
