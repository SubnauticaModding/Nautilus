# Custom Loading Screens

Nautilus provides a way for modders to add custom loading screens, which is tied in with the custom title screen system.
> [!NOTE]
> This tutorial builds off the one for the [custom title screens](title-addons.md) tutorial, so if you have not already, it's highly recommended to go through that first.

## System Summary

The loading screen system consists of two parts.
1. The current custom title screen, handled by Nautilus's `TitleScreenHandler`
2. Custom `LoadingScreenData`, which handles image registration and how loading screens are selected.

For a loading screen to be available to be selected, the specified `CustomTitleData` key must be the active theme. Once that check has passed, Nautilus looks at the parameters on each `LoadingScreenData` itself.

Here are all the parameters required for creating a `LoadingScreenData` instance:
- `loadingScreenImage`: The loading screen sprite. This is the actual image that will show.
- `priority`: The priority of this loading screen over other registered screens.
- `timeToNextScreen`: The duration in seconds for which this loading screen will stay active.
- `storyGoalRequirement`: A story goal requirement for the loading screen to be enabled (not required).
- `customRequirement`: A custom requirement for the loading screen to be enabled (not required).

## Loading Screen Example

```csharp
private void RegisterLoadingScreens()
{
    // The "ModTitleAddons" theme will need to be active for these screens to show up
    LoadingScreenHandler.RegisterLoadingScreen("ModTitleAddons", new []
    {
        // Register a simple loading screen with just a sprite
        new LoadingScreenHandler.LoadingScreenData(Plugin.AssetBundle.LoadAsset<Sprite>("LoadingSprite1")),
        // Register a loading screen with the "MyEpicStoryGoal" story goal requirement
        new LoadingScreenHandler.LoadingScreenData(Plugin.AssetBundle.LoadAsset<Sprite>("LoadingSprite2"), storyGoalRequirement: "MyEpicStoryGoal"),
        // Register a loading screen that only appears when the "storyBool" in the mod's save data is set to true
        new LoadingScreenHandler.LoadingScreenData(Plugin.AssetBundle.LoadAsset<Sprite>("LoadingSprite3"), customRequirement: () => Plugin.SaveData.storyBool),
    })
}
```