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
        public float minTimeToNextScreen;
        public string storyGoalRequirement;
        
        public LoadingScreenData(Sprite loadingScreenImage, float minTimeToNextScreen = 7f, string storyGoalRequirement = null)
        {
            this.loadingScreenImage = loadingScreenImage;
            this.minTimeToNextScreen = minTimeToNextScreen;
            this.storyGoalRequirement = storyGoalRequirement;
        }
    }
}