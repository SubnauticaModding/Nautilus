using System;
using System.Collections.Generic;
using BepInEx;
using Nautilus.Patchers;
using Nautilus.Utility;
using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public static class TitleScreenHandler
{
    public static void RegisterTitleScreenObject(BaseUnityPlugin plugin, CustomTitleData customTitleData)
    {
        MainMenuPatcher.RegisterTitleObjectData(plugin.Info.Metadata.GUID, customTitleData);
    }

    public static void ApproveTitleCollaboration(BaseUnityPlugin plugin, CollaborationData collaborationData)
    {
        MainMenuPatcher.CollaborationData.Add(plugin.Info.Metadata.GUID, collaborationData);
    }

    public class CustomTitleData
    {
        public readonly string localizationKey;
        public readonly TitleAddon[] addons;
        
        public CustomTitleData(string localizationKey, params TitleAddon[] addons)
        {
            this.localizationKey = localizationKey;

            this.addons = addons;
        }
    }

    public class CollaborationData
    {
        public Dictionary<string, Type[]> modApprovedAddons;

        public CollaborationData(Dictionary<string, Type[]> modApprovedAddons)
        {
            this.modApprovedAddons = modApprovedAddons;
        }

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