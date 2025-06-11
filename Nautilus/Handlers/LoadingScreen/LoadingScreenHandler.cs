using System;
using BepInEx;
using UnityEngine;

namespace Nautilus.Handlers.LoadingScreen;

public static class LoadingScreenHandler
{
    public static void RegisterLoadingScreen(BaseUnityPlugin plugin, LoadingScreenData[] loadingScreenDatas)
    {
        LoadingScreenSetter.LoadingScreenDatas.Add(plugin.Info.Metadata.GUID, loadingScreenDatas);
    }

    public class LoadingScreenData
    {
        public Sprite loadingScreenImage;
        public int priority;
        public float timeToNextScreen;
        public string storyGoalRequirement;
        public Func<bool> customRequirement;
        
        public LoadingScreenData(Sprite loadingScreenImage, int priority = 1, float timeToNextScreen = 7f, string storyGoalRequirement = null, Func<bool> customRequirement = null)
        {
            this.loadingScreenImage = loadingScreenImage;
            this.priority = priority;
            this.timeToNextScreen = timeToNextScreen;
            this.storyGoalRequirement = storyGoalRequirement;
            this.customRequirement = customRequirement;
        }
    }
}