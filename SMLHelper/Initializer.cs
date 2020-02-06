namespace SMLHelper.V2
{
    using System;
    using System.Reflection;
    using Harmony;
    using Patchers;
#if SUBNAUTICA
    using QModManager.API.ModLoading;
    using UnityEngine;
#endif

    /// <summary>
    /// WARNING: This class is for use only by QModManager.
    /// </summary>
#if SUBNAUTICA
    [QModCore]
#endif
    [Obsolete("This class is for use only by QModManager.", true)]
    public class Initializer
    {
#if SUBNAUTICA
        /// <summary>
        /// WARNING: This method is for use only by QModManager.
        /// </summary>
        [QModPrePatch]
        [Obsolete("This method is for use only by QModManager.", true)]
        public static void PrePatch()
        {
            Logger.Initialize();
    
            Logger.Log($"Loading v{Assembly.GetExecutingAssembly().GetName().Version} for Subnautica", LogLevel.Info);
        }

        /// <summary>
        /// WARNING: This method is for use only by QModManager.
        /// </summary>
        [QModPostPatch("E3DC72597463233E62D01BD222AD0C96")]
        [Obsolete("This method is for use only by QModManager.", true)]
        public static void PostPatch()
        {
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Logger.Error($"Caught exception while trying to initialize SMLHelper{Environment.NewLine}{e}");
            }
        }
#endif

#if BELOWZERO
        /// <summary>
        /// WARNING: This method is for use only by QModManager.
        /// </summary>
        [Obsolete("This method is for use only by QModManager.", true)]
        public static void Patch()
        {
            Logger.Initialize();

            Logger.Log($"Loading v{Assembly.GetExecutingAssembly().GetName().Version} for Below Zero", LogLevel.Info);
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Logger.Error($"Caught exception while trying to initialize SMLHelper{Environment.NewLine}{e}");
            }
        }
#endif

        private static void Initialize()
        {
            var harmony = HarmonyInstance.Create("com.ahk1221.smlhelper");
            FishPatcher.Patch(harmony);
            TechTypePatcher.Patch(harmony);
            CraftTreeTypePatcher.Patch(harmony);
            CraftDataPatcher.Patch(harmony);
            CraftTreePatcher.Patch(harmony);
            DevConsolePatcher.Patch(harmony);
            LanguagePatcher.Patch(harmony);
            PrefabDatabasePatcher.Patch(harmony);
            SpritePatcher.Patch();
            KnownTechPatcher.Patch(harmony);
            BioReactorPatcher.Patch(harmony);
            OptionsPanelPatcher.Patch(harmony);
            ItemsContainerPatcher.Patch(harmony);
            PDAPatcher.Patch(harmony);
            PDAEncyclopediaPatcher.Patch(harmony);
            ItemActionPatcher.Patch(harmony);
            LootDistributionPatcher.Patch(harmony);
            WorldEntityDatabasePatcher.Patch(harmony);
            IngameMenuPatcher.Patch(harmony);
            TooltipPatcher.Patch(harmony);
        }
    }
}
