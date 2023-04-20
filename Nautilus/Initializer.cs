using System;
using BepInEx;
using HarmonyLib;
using Nautilus.Patchers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus;

/// <summary>
/// WARNING: This class is for use only by BepInEx.
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Initializer : BaseUnityPlugin
{
    private static readonly Harmony _harmony = new(PluginInfo.PLUGIN_GUID);

    /// <summary>
    /// WARNING: This method is for use only by BepInEx.
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

        PrefabDatabasePatcher.PrePatch(_harmony);
        EnumPatcher.Patch(_harmony);
        CraftDataPatcher.Patch(_harmony);
        CraftTreePatcher.Patch(_harmony);
        ConsoleCommandsPatcher.Patch(_harmony);
        LanguagePatcher.Patch(_harmony);
        PrefabDatabasePatcher.PostPatch(_harmony);
        SpritePatcher.Patch(_harmony);
        KnownTechPatcher.Patch(_harmony);
        OptionsPanelPatcher.Patch(_harmony);
        ItemsContainerPatcher.Patch(_harmony);
        PDALogPatcher.Patch(_harmony);
        PDAPatcher.Patch(_harmony);
        PDAEncyclopediaPatcher.Patch(_harmony);
        ItemActionPatcher.Patch(_harmony);
        LootDistributionPatcher.Patch(_harmony);
        WorldEntityDatabasePatcher.Patch(_harmony);
        LargeWorldStreamerPatcher.Patch(_harmony);
        SaveUtilsPatcher.Patch(_harmony);
        TooltipPatcher.Patch(_harmony);
        SurvivalPatcher.Patch(_harmony);
        CustomSoundPatcher.Patch(_harmony);
        EatablePatcher.Patch(_harmony);
        MaterialUtils.Patch();
    }
}