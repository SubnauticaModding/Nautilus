## How does the game handle background colors for items?
The game has a built-in enum called `BackgroundType`, which sits in the `CraftData` class. The possible values for this enum are listed below.

```csharp
public enum BackgroundType
{
    Normal,
    Blueprint,
    PlantWater,
    PlantWaterSeed,
    PlantAir,
    PlantAirSeed,
    ExosuitArm
}
```

## How can I create a custom background type?
To create a new custom background type, you will need to register an image as the background for some `BackgroundType` instance.  

Fortunately, the custom enums system has made this step really simple. All you will have to do is name your brand new `BackgroundType` instance, then register an image for it.
```csharp
private void Awake()
{
    var myCustomBackground = EnumHandler.AddEntry<CraftData.BackgroundType>("CustomBackground")
            .WithBackground(ImageUtils.LoadSpriteFromFile(pathToImage));
}
```

And that's it. Now you can use the new `CraftData.BackgroundType` instance anywhere you want.

## How can I change an item's background?
To edit an item's background type, you need to call the `CraftDataHandler.SetBackgroundType` method sitting in the `Nautilus.Handlers` namespace


### Examples
The following example demonstrates the usage of `SetBackgroundType` That makes the titanium background color green.

```csharp
CraftDataHandler.SetBackgroundType(TechType.Titanium, CraftData.BackgroundType.PlantAirSeed);
```

Similarly, if we wanted to set the titanium's background to our custom background from earlier, it would look like the following:
```csharp
CraftDataHandler.SetBackgroundType(TechType.Titanium, myCustomBackground);
```