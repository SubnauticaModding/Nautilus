namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Collections.Generic;
    using Assets;
    using Harmony;
    using Abstract;

    internal partial class CraftDataPatcher : IPatch
    {
        #region Internal Fields

        private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

        #endregion

        #region Group Handling

        internal static void AddToCustomGroup(TechGroup group, TechCategory category, TechType techType, TechType after)
        {
            Dictionary<TechGroup, Dictionary<TechCategory, List<TechType>>> groups = CraftData.groups;
            Dictionary<TechCategory, List<TechType>> techGroup = groups[group];
            if (techGroup == null)
            {
                // Should never happen, but doesn't hurt to add it.
                Logger.Log("Invalid TechGroup!", LogLevel.Error);
                return;
            }

            List<TechType> techCategory = techGroup[category];
            if (techCategory == null)
            {
                Logger.Log($"Invalid TechCategory Combination! TechCategory: {category} TechGroup: {group}", LogLevel.Error);
                return;
            }

            int index = techCategory.IndexOf(after);

            if (index == -1) // Not found
            {
                techCategory.Add(techType);
                Logger.Log($"Added \"{techType.AsString():G}\" to groups under \"{group:G}->{category:G}\"", LogLevel.Debug);
            }
            else
            {
                techCategory.Insert(index + 1, techType);

                Logger.Log($"Added \"{techType.AsString():G}\" to groups under \"{group:G}->{category:G}\" after \"{after.AsString():G}\"", LogLevel.Debug);
            }
        }

        internal static void RemoveFromCustomGroup(TechGroup group, TechCategory category, TechType techType)
        {
            Dictionary<TechGroup, Dictionary<TechCategory, List<TechType>>> groups = CraftData.groups;
            Dictionary<TechCategory, List<TechType>> techGroup = groups[group];
            if (techGroup == null)
            {
                // Should never happen, but doesn't hurt to add it.
                Logger.Log("Invalid TechGroup!", LogLevel.Error);
                return;
            }

            List<TechType> techCategory = techGroup[category];
            if (techCategory == null)
            {
                Logger.Log($"Invalid TechCategory Combination! TechCategory: {category} TechGroup: {group}", LogLevel.Error);
                return;
            }

            techCategory.Remove(techType);

            Logger.Log($"Removed \"{techType.AsString():G}\" from groups under \"{group:G}->{category:G}\"", LogLevel.Debug);
        }

        #endregion

        #region Patching

        public void Patch(HarmonyInstance harmony)
        {
#if SUBNAUTICA
            PatchForSubnautica();
#elif BELOWZERO
            PatchForBelowZero(harmony);
#endif
            harmony.Patch(AccessTools.Method(typeof(CraftData), nameof(CraftData.PreparePrefabIDCache)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftDataPatcher), nameof(PreparePrefabIDCachePostfix))));

            Logger.Log("CraftDataPatcher is done.", LogLevel.Debug);
        }

        private static void PreparePrefabIDCachePostfix()
        {
            Dictionary<TechType, string> techMapping = CraftData.techMapping;
            Dictionary<string, TechType> entClassTechTable = CraftData.entClassTechTable;

            foreach (ModPrefab prefab in ModPrefab.Prefabs)
            {
                techMapping[prefab.TechType] = prefab.ClassID;
                entClassTechTable[prefab.ClassID] = prefab.TechType;
            }
        }

        #endregion
    }
}
