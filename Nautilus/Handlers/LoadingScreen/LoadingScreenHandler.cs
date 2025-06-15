using System;
using BepInEx;
using UnityEngine;

namespace Nautilus.Handlers.LoadingScreen;

/// <summary>
/// Handles custom loading screens
/// </summary>
public static class LoadingScreenHandler
{
    /// <summary>
    /// Register loading screens with Nautilus. Note that custom loading screens will only appear when your mod is selected as the current theme
    /// </summary>
    /// <param name="plugin">The plugin registering the loading screens</param>
    /// <param name="loadingScreenDatas">The loading screens to register</param>
    public static void RegisterLoadingScreen(BaseUnityPlugin plugin, LoadingScreenData[] loadingScreenDatas)
    {
        LoadingScreenSetter.LoadingScreenDatas.Add(plugin.Info.Metadata.GUID, loadingScreenDatas);
    }

    /// <summary>
    /// A data class containing info for Nautilus to register a custom loading screen
    /// </summary>
    public class LoadingScreenData
    {
        /// <summary>
        /// The image shown while loading
        /// </summary>
        public readonly Sprite loadingScreenImage;
        
        /// <summary>
        /// The priority over other loading screens registered
        /// </summary>
        public readonly int priority;
        
        /// <summary>
        /// The time between loading screen transitions
        /// </summary>
        public readonly float timeToNextScreen;
        
        /// <summary>
        /// The story goal required for the loading screen to show
        /// </summary>
        public readonly string storyGoalRequirement;
        
        /// <summary>
        /// A custom requirement for the loading screen to show
        /// </summary>
        public readonly Func<bool> customRequirement;
        
        /// <summary>
        /// Creates a new instance of <see cref="LoadingScreenData"/>
        /// </summary>
        /// <param name="loadingScreenImage">The image shown while loading</param>
        /// <param name="priority">The priority over other loading screens registered</param>
        /// <param name="timeToNextScreen">The time between loading screen transitions</param>
        /// <param name="storyGoalRequirement">The story goal required for the loading screen to show</param>
        /// <param name="customRequirement">A custom requirement for the loading screen to show</param>
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