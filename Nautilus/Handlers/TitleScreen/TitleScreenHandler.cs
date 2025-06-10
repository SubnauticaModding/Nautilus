using System;
using System.Collections.Generic;
using Nautilus.Patchers;
using UnityEngine;

namespace Nautilus.Handlers.TitleScreen;

public static class TitleScreenHandler
{
    public static void RegisterTitleScreenObject(string guid, CustomTitleData customTitleData)
    {
        MainMenuPatcher.TitleObjectDatas.Add(guid, customTitleData);
    }

    public class CustomTitleData
    {
        public readonly string localizationKey;
        public readonly Dictionary<Type, TitleAddon> addons;
        public GameObject functionalityRoot;
        
        public CustomTitleData(string localizationKey, params TitleAddon[] addons)
        {
            this.localizationKey = localizationKey;

            this.addons = new();
            foreach (var addon in addons)
            {
                this.addons.Add(addon.GetType(), addon);
            }
        }
    }
}