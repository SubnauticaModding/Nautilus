namespace SMLHelper.V2
{
    using System;
    using System.Reflection;
    using Harmony;
    using Patchers;
    using QModManager.API.ModLoading;

    /// <summary>
    /// WARNING: This class is for use only by QModManager.
    /// </summary>
    [QModCore]
    public class Initializer
    {
#if SUBNAUTICA
        /// <summary>
        /// WARNING: This method is for use only by QModManager.
        /// </summary>
        [QModPrePatch]
        public static void SetUpLogger()
        {
            Logger.Initialize();

            Logger.Log($"Loading v{Assembly.GetExecutingAssembly().GetName().Version} for Subnautica", LogLevel.Info);

        }

        /// <summary>
        /// WARNING: This method is for use only by QModManager.
        /// </summary>
        [QModPostPatch("0B8AB3339D45F229633494237AEF79BB")]
        public static void RunPatchers()
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
            //TooltipPatcher.Patch(harmony); // Disabled
        }
    }
}
