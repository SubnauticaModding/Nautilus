using System;
using System.Collections.Generic;
using Nautilus.Patchers;
using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public static class TitleScreenHandler
{
    public static void RegisterTitleScreenObject(string guid, CustomTitleData customTitleData)
    {
        MainMenuPatcher.RegisterTitleObjectData(guid, customTitleData);
    }

    public static void ApproveTitleCollaboration(string guid, CollaborationData collaborationData)
    {
        MainMenuPatcher.CollaborationData.Add(guid, collaborationData);
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

    public struct CollaborationData
    {
        public Dictionary<string, Type[]> modApprovedAddons;

        public CollaborationData(Dictionary<string, Type[]> modApprovedAddons)
        {
            this.modApprovedAddons = modApprovedAddons;
        }
    }
}