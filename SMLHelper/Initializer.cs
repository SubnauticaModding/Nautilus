using SMLHelper.Handlers;

namespace SMLHelper
{
    using System;
    using System.Collections;
    using System.Reflection;
    using BepInEx;
    using HarmonyLib;
    using Patchers;
    using Utility;
    using UnityEngine;
    using SMLHelper.API;
    using SMLHelper.Assets;
    using System.Collections.Generic;


    /// <summary>
    /// WARNING: This class is for use only by Bepinex.
    /// </summary>
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Initializer: BaseUnityPlugin
    {
        private const string
            MODNAME = "SMLHelper",
            GUID = "com.ahk1221.smlhelper",
            VERSION = "2.15.0.2";

        internal static readonly Harmony harmony = new(GUID);

        /// <summary>
        /// WARNING: This method is for use only by Bepinex.
        /// </summary>
        [Obsolete("This method is for use only by Bepinex.", true)]
        Initializer()
        {
            GameObject obj = UWE.Utils.GetEntityRoot(this.gameObject) ?? this.gameObject;
            obj.EnsureComponent<SceneCleanerPreserve>();

            InternalLogger.Initialize(Logger);
#if SUBNAUTICA
            InternalLogger.Info($"Loading v{VERSION} for Subnautica");
#elif BELOWZERO
            InternalLogger.Info($"Loading v{VERSION} for BelowZero");
#endif

            PrefabDatabasePatcher.PrePatch(harmony);
            EnumPatcher.Patch(harmony);
            EnergyMixinPatcher.Patch(harmony);
            ChargerPatcher.Patch(harmony);

            PatchCraftingTabs();
            StartCoroutine(InitializePatches());
        }

        internal static void PatchCraftingTabs()
        {
            InternalLogger.Info("Separating batteries and power cells into their own fabricator crafting tabs");

            // Remove original crafting nodes
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, CbDatabase.ResCraftTab, CbDatabase.ElecCraftTab, TechType.Battery.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, CbDatabase.ResCraftTab, CbDatabase.ElecCraftTab, TechType.PrecursorIonBattery.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, CbDatabase.ResCraftTab, CbDatabase.ElecCraftTab, TechType.PowerCell.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, CbDatabase.ResCraftTab, CbDatabase.ElecCraftTab, TechType.PrecursorIonPowerCell.ToString());

            // Add a new set of tab nodes for batteries and power cells
            CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, CbDatabase.BatteryCraftTab, "Batteries", SpriteManager.Get(TechType.Battery), CbDatabase.ResCraftTab);
            CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, CbDatabase.PowCellCraftTab, "Power Cells", SpriteManager.Get(TechType.PowerCell), CbDatabase.ResCraftTab);

            // Move the original batteries and power cells into these new tabs
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.Battery, CbDatabase.BatteryCraftPath);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonBattery, CbDatabase.BatteryCraftPath);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PowerCell, CbDatabase.PowCellCraftPath);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonPowerCell, CbDatabase.PowCellCraftPath);
        }


        private IEnumerator InitializePatches()
        {
            Type chainLoader = typeof(BepInEx.Bootstrap.Chainloader);

            FieldInfo _loaded = chainLoader.GetField("_loaded", BindingFlags.NonPublic | BindingFlags.Static);
            while(!(bool)_loaded.GetValue(null))
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(2);

            CraftDataPatcher.Patch(harmony);
            CraftTreePatcher.Patch(harmony);
            ConsoleCommandsPatcher.Patch(harmony);
            LanguagePatcher.Patch(harmony);
            PrefabDatabasePatcher.PostPatch(harmony);
            SpritePatcher.Patch(harmony);
            KnownTechPatcher.Patch(harmony);
            OptionsPanelPatcher.Patch(harmony);
            ItemsContainerPatcher.Patch(harmony);
            PDALogPatcher.Patch(harmony);
            PDAPatcher.Patch(harmony);
            PDAEncyclopediaPatcher.Patch(harmony);
            ItemActionPatcher.Patch(harmony);
            LootDistributionPatcher.Patch(harmony);
            WorldEntityDatabasePatcher.Patch(harmony);
            LargeWorldStreamerPatcher.Patch(harmony);
            SaveUtilsPatcher.Patch(harmony);
            TooltipPatcher.Patch(harmony);
            SurvivalPatcher.Patch(harmony);
            CustomSoundPatcher.Patch(harmony);
            EatablePatcher.Patch(harmony);
            MaterialUtils.Patch();




            UpdateCollection(BatteryCharger.compatibleTech, CbDatabase.BatteryItems);
            UpdateCollection(PowerCellCharger.compatibleTech, CbDatabase.PowerCellItems);
        }

        private static void UpdateCollection(HashSet<TechType> compatibleTech, List<TechType> toBeAdded)
        {
            if(toBeAdded.Count == 0)
                return;

            // Make sure all custom batteries are allowed in the battery charger
            for(int i = toBeAdded.Count - 1; i >= 0; i--)
            {
                TechType entry = toBeAdded[i];

                if(compatibleTech.Contains(entry))
                    continue;

                compatibleTech.Add(entry);
            }
        }
    }
}
