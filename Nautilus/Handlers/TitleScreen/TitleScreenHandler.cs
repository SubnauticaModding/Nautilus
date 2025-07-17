using System;
using System.Collections.Generic;
using BepInEx;
using Nautilus.Patchers;

namespace Nautilus.Handlers.TitleScreen;

/// <summary>
/// Handles custom additions to the main menu (Title screen).
/// </summary>
public static class TitleScreenHandler
{
    /// <summary>
    /// Register title screen additions with Nautilus for automatic handling.
    /// </summary>
    /// <param name="plugin">The plugin for your mod.</param>
    /// <param name="customTitleData">The custom title data for your additions.</param>
    public static void RegisterTitleScreenObject(BaseUnityPlugin plugin, CustomTitleData customTitleData)
    {
        MainMenuPatcher.RegisterTitleObjectData(plugin.Info.Metadata.GUID, customTitleData);
    }

    /// <summary>
    /// Approve another mod to make specific title screen additions when your mod is installed.
    /// </summary>
    /// <param name="plugin">The plugin for your mod.</param>
    /// <param name="collaborationData">The collaboration data for the plugins you want to approve.</param>
    public static void ApproveTitleCollaboration(BaseUnityPlugin plugin, CollaborationData collaborationData)
    {
        MainMenuPatcher.CollaborationData.Add(plugin.Info.Metadata.GUID, collaborationData);
    }

    /// <summary>
    /// A data class containing additions for Nautilus to add to the main menu when your mod is selected as the current theme.
    /// </summary>
    public class CustomTitleData
    {
        /// <summary>
        /// The localization key for the name of your mod. Will be shown in the selectable themes option.
        /// </summary>
        public readonly string localizationKey;
        
        /// <summary>
        /// The additions to the main menu that should be active when your mod is selected.
        /// </summary>
        public readonly TitleAddon[] addons;
        
        /// <summary>
        /// Creates a new instance of <see cref="CustomTitleData"/>
        /// </summary>
        /// <param name="localizationKey">The localization key for the name of your mod. Will be shown in the selectable themes option.</param>
        /// <param name="addons">The additions to the main menu that should be active when your mod is selected.</param>
        public CustomTitleData(string localizationKey, params TitleAddon[] addons)
        {
            this.localizationKey = localizationKey;

            this.addons = addons;
        }
    }

    /// <summary>
    /// A data class containing info allowing other mods to add mod-specific additions to the title screen.
    /// </summary>
    public class CollaborationData
    {
        internal Dictionary<string, Type[]> modApprovedAddons;

        /// <summary>
        /// Creates a new instance of <see cref="CollaborationData"/> with specific approved addon types.
        /// </summary>
        /// <param name="modApprovedAddons">The GUIDs for the approved mods and their allowed addon types.</param>
        public CollaborationData(Dictionary<string, Type[]> modApprovedAddons)
        {
            this.modApprovedAddons = modApprovedAddons;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CollaborationData"/> with all addon types approved.
        /// </summary>
        /// <param name="GUIDs">The GUIDs of the mods to approve.</param>
        public CollaborationData(string[] GUIDs)
        {
            modApprovedAddons = new();
            foreach (var guid in GUIDs)
            {
                modApprovedAddons.Add(guid, new[] { typeof(ApproveAllAddons) });
            }
        }
    }
}