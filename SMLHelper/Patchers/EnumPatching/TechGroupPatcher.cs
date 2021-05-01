namespace SMLHelper.V2.Patchers.EnumPatching
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using SMLHelper.V2.Handlers;
    using Utility;

    internal class TechGroupPatcher
    {
        private const string TechGroupEnumName = "TechGroup";

        internal const int startingIndex = 0; // The default TechGroup contains indexes 0 through 14

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

            cacheManager.Add(techGroup, cache.Index, cache.Name);
            
            if (!uGUI_BlueprintsTab.groups.Contains(techGroup))
                uGUI_BlueprintsTab.groups.Add(techGroup);

            if (!CraftData.groups.ContainsKey(techGroup))
                CraftData.groups[techGroup] = new Dictionary<TechCategory, List<TechType>>();

            Logger.Log($"Successfully added TechGroup: '{name}' to Index: '{cache.Index}'", LogLevel.Debug);


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

            Logger.Log($"Finished known TechGroup exclusion. {bannedIndices.Count} IDs were added in ban list.", LogLevel.Info);

            return bannedIndices;
        }

        internal static void Patch()
        {
            IngameMenuHandler.Main.RegisterOneTimeUseOnSaveEvent(() => cacheManager.SaveCache());


            Logger.Log($"Added {cacheManager.ModdedKeysCount} TechGroups succesfully into the game.");
            Logger.Log("TechGroupPatcher is done.", LogLevel.Debug);
        }

    }

    /// <summary>
    /// Needed Patches to work around a ReadOnly Array in uGUI_BuilderMenu called groupsTechTypes
    /// </summary>
    public static class BuilderMenuPatches
    {
        internal static void Patch(Harmony harmony)
        {
            PatchUtils.PatchClass(harmony);
            Logger.Log("BuilderMenuPatches is done.", LogLevel.Debug);
        }

        /// <summary>
        /// The Replacement Array of Lists of Techtypes used by the BuilderMenu
        /// </summary>
        public static List<TechType>[] groupsTechTypes;


        /// <summary>
        /// Prefixes the ensure tech group tech type data initialized.
        /// </summary>
        /// <returns></returns>
        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(uGUI_BuilderMenu), nameof(uGUI_BuilderMenu.EnsureTechGroupTechTypeDataInitialized))]
        public static bool Prefix_EnsureTechGroupTechTypeDataInitialized()
        {
            if (!uGUI_BuilderMenu.groupsTechTypesInitialized)
            {
                groupsTechTypes = new List<TechType>[uGUI_BuilderMenu.groups.Count];
                for (int i = 0; i < uGUI_BuilderMenu.groups.Count; i++)
                {
                    groupsTechTypes[i] = new List<TechType>();
                    List<TechType> list = groupsTechTypes[i];
                    CraftData.GetBuilderGroupTech(uGUI_BuilderMenu.groups[i], list, false);
                    for (int j = 0; j < list.Count; j++)
                    {
                        TechType key = list[j];
                        uGUI_BuilderMenu.techTypeToTechGroupIdx.Add(key, i);
                    }
                }
                uGUI_BuilderMenu.groupsTechTypesInitialized = true;
            }
            return false;
        }

        /// <summary>
        /// Prefixes the get tech types for group.
        /// </summary>
        /// <param name="groupIdx">Index of the group.</param>
        /// <param name="__result">The result.</param>
        /// <returns></returns>
        [PatchUtils.Prefix]
        [HarmonyPatch(typeof(uGUI_BuilderMenu), nameof(uGUI_BuilderMenu.GetTechTypesForGroup))]
        public static bool Prefix_GetTechTypesForGroup(int groupIdx, ref List<TechType> __result)
        {
            __result = groupsTechTypes[groupIdx];
            return false;
        }
    }
}
