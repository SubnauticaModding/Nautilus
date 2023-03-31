namespace SMLHelper;

using System;
using BepInEx;
using HarmonyLib;
using Patchers;
using Utility;
using UnityEngine;


/// <summary>
/// WARNING: This class is for use only by Bepinex.
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Initializer : BaseUnityPlugin
{
    internal static readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

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
        InternalLogger.Info($"Loading v{PluginInfo.PLUGIN_VERSION} for Subnautica");
#elif BELOWZERO
            InternalLogger.Info($"Loading v{PluginInfo.PLUGIN_VERSION} for BelowZero");
#endif

        PrefabDatabasePatcher.PrePatch(harmony);
        EnumPatcher.Patch(harmony);
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
    }
}