# Adding a Custom Title Screen

Nautilus provides a way for mods to add their own title screen additions without interfering with those from other mods.

## Understanding Addons

Nautilus uses a modular system for title screen additions, allowing for a near endless amount of customizability.

The foundation of these additions is the `TitleAddon`. This class is inherited from by many other addons and provides basic functionality, should you wish to create your own custom addon.

## How the Title System Works

Nautilus's title screen management is accessed primarily through the [`TitleScreenHandler`](https://subnauticamodding.github.io/Nautilus/api/Nautilus.Handlers.TitleScreen.TitleScreenHandler.html) class. It contains methods to register `TitleAddon`s, as well as for registering mod collabs, which we'll go into later.

The `TitleScreenHandler` contains a subclass called `CustomTitleData`, which is used to register `TitleAddon`s to Nautilus. It takes in a localization key for the name of your mod, which will be shown in the theme selector, and your title addons.

Once you have your title data, you can register it with Nautilus by calling `TitleScreenHandler.RegisterTitleScreenObject`. This method takes in a key for your data, so that if you create multiple Nautilus knows how to differentiate between the two.

> [!NOTE]
> This tutorial will refer to registered `CustomTitleDatas` as "themes". For example, if a mod registered a custom object and some music, that would be considered the mod's theme.

## Adding an Object

To add an object to the title screen, you will need to use the `WorldObjectTitleAddon` class. Create a new instance of this class, and you will see that it takes in a `Func<GameObject>`.

This `Func` is used to spawn your object when you enter the main menu. The reason it is a Func and not a GameObject that you can pass in is so that Nautilus can handle spawning it back in when you exit to the main menu from a save.

The recommended way to do this is to create a [local function](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/local-functions) or [anonymous function](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions) and then pass that into the constructor. An example of this is shown below.

```csharp
GameObject SpawnObject()
{
    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    obj.transform.position = new Vector3(-27, 2.5f, 38);
    obj.transform.rotation = Quaternion.Euler(270, 325.7f, 0);
    obj.AddComponent<SkyApplier>();

    MaterialUtils.ApplySNShaders(obj);

    return obj;
}

var objectAddon = new WorldObjectTitleAddon(SpawnObject);
```

> [!WARNING]
> For proper shading and fade transitions, your object must have a SkyApplier component and [Subnautica materials applied](https://subnauticamodding.github.io/Nautilus/api/Nautilus.Utility.MaterialUtils.html#methods).

This code will place a default cube to the left of the Subnautica logo once registered with Nautilus.

You may have also noticed the optional parameters: `fadeInTime` and `requiredGUIDs`. The `requiredGUIDs` will be discussed later, but the `fadeInTime` controls how long the fade in duration is for your registered object. Nautilus automatically fades in objects using Subnautica's `Renderer.fadeAmount` when switching between themes, and this variable controls how long the transition is. By default, it is set to one second.

## Adding Music

Similarly to adding a custom object, you will need to use the `MusicTitleAddon` to register custom music with Nautilus.

If you have Thunderkit and automatic sound registration, this becomes extremely simple, although it requires more set up.
Here are various examples of how to create a music addon:

## [Manual Load (From Asset Bundle)](#tab/manualloadassetbundle)

```cs
private MusicTitleAddon GetMusicAddon()
{
    // Creates a sound using the standard modes for a "streamed" sound, loading the audio clip by name from the Asset Bundle
    var sound = AudioUtils.CreateSound(MyAssetBundle.LoadAsset<AudioClip>(audioClipName), AudioUtils.StandardSoundModes_Stream);

    // Register the sound under the Music bus
    CustomSoundHandler.RegisterCustomSound(soundId, sound, AudioUtils.BusPaths.Music);
    
    return new MusicTitleAddon(AudioUtils.GetFmodAsset(soundId));
}
```
## [Manual Load (From mod folder)](#tab/manualloadmodfolder)

```cs
private MusicTitleAddon GetMusicAddon()
{
    // Creates a sound using the standard modes for a "streamed" sound, loading the audio at the given file path
    var sound = AudioUtils.CreateSound(soundFilePath, AudioUtils.StandardSoundModes_Stream);

    // Register the sound under the Music bus
    CustomSoundHandler.RegisterCustomSound(soundId, sound, AudioUtils.BusPaths.Music);
    
    return new MusicTitleAddon(AudioUtils.GetFmodAsset(soundId));
}
```
## [Thunderkit](#tab/thunderkit)

```cs
private MusicTitleAddon GetMusicAddon()
{
    return new MusicTitleAddon(MyAssetBundle.LoadAsset<FMODAsset>("MyMusic"));
}
```
---

## Changing the Sky

To change the sky, including the time of day, exposure, fog, etc., you will need to create a `SkyChangeTitleAddon`. This addon can be used to achieve an effect identical to the Return of the Ancients demo title screen, which is set to night.

The main parameters are the `fadeInDuration`, and `settings` (of type `Settings`, a subclass of `SkyChangeTitleAddon`). The fade duration determines how long the game should take to transition from the default sky to your custom sky. The settings, however, are slightly more complicated.

The parameters are fully explained on the [API documentation](https://subnauticamodding.github.io/Nautilus/api/Nautilus.Handlers.TitleScreen.SkyChangeTitleAddon.Settings.html), but here is a quick rundown of them:
- `timeOfDay`: The time of day of the in game sky. Similar to a 24-hour Earth time.
- `exposure`: The exposure (brightness) of the sky.
- `rayleighScattering`: The strength of light scattering in the sky. Will affect how blue the sky looks and what the atmosphere looks like when the sun is at grazing angles.
- `fogDensity`: The density of the fog.

All of these have default values set to that of the vanilla sky, but you can mess around with these to find settings that look good to you.

For example, this code creates a `SkyChangeTitleAddon` with a transition time of 2 seconds that sets the time to in-game midnight and increases fog.

```csharp
var skyChangeAddon = new SkyChangeTitleAddon(2f, new SkyChangeTitleAddon.Settings(0f, fogDensity: 0.001f));
```

## Registering Your Addons

To register your title addons, you will need to create a `TitleScreenHandler.CustomTitleData` to hold the data. This class also takes in a localization key for the name of your mod, which will be displayed in the theme selector.

> [!NOTE]
> See the [localization tutorial](localization.md) for information on how to create localization lines.

After entering your mod name localization key, you can add the rest of your title addons, like so.
```csharp
var customData = new TitleScreenHandler.CustomTitleData("MyModLocalizationKey", objectAddon, musicAddon, skyChangeAddon);
```

After creating your custom data, you will need to register it with Nautilus by calling `TitleScreenHandler.RegisterTitleScreenObject`. This method takes in a key for your custom title data, so that if you add multiple custom title datas Nautilus knows how to keep track of which is active. The key needs to be unique within the data added by your mod.

Here is an example of how to register your data:
```csharp
TitleScreenHandler.RegisterTitleScreenObject("MyEpicTitleData", customData);
```

Now that you have registered your data, a selector will pop up in the bottom left of the screen when in Subnautica's main menu. You can use the arrows to switch between the default title screen and your custom one, and, if you have other mods installed that add custom title screens, those too.

## Title Collaborations

As you may have already noticed, all `TitleAddons` have an optional parameter for `requiredGUIDs`. This is so if two mods want to create a collaboration title screen, one mod can create the addons and have the other mod required to be installed for them to show up. To make a mod required for an addon, simply add its GUID to the required GUID parameters.

A mod's GUID is the string you place in the `[BepInPlugin()]` attribute in your plugin class. To find another mod's GUID, you can open up their mod DLL in dnSpy or some other DLL viewing program, and navigate to their plugin to find the `BepInPlugin` attribute. You can then copy their GUID from there.

If you were to try this however, you might find that your addons do not show up even when the required mod is installed. This is because mods that are required by `TitleAddons` need to approve addons from other mods. This can be done by calling `TitleScreenHandler.ApproveTitleCollaboration` with your mod's `CollaborationData`.

`CollaborationData` has multiple constructors, depending on how strict you want to be approving mods. You can either approve all types of `TitleAddons` using the constructor that only takes in GUIDs, or you can be more specific and whitelist only certain types by using the constructor with a GUID/Type array dictionary.

Examples:
```csharp
// Approve all collaborations from the map mod. The "this" keyword is the instance of your plugin.
// This method assumes that it is being called from your plugin.
TitleScreenHandler.ApproveTitleCollaboration(this, new TitleScreenHandler.CollaborationData(new string[] { "sn.subnauticamap.mod" } ))

// Only approves collabs from the map mod that are WorldObjectTitleAddons
TitleScreenHandler.ApproveTitleCollaboration(this, new TitleScreenHandler.CollaborationData(new () {
     { "sn.subnauticamap.mod", new Type[] { typeof(WorldObjectTitleAddon) } }
     } ));
```

## Extending the Given Addons

If you find that the base `TitleAddons` are not suited for your specific title requirements, you can make a new class that inherits from `TitleAddon` to create your own behavior. These classes can be registered.

This simple addon logs a message to the in-game `ErrorMessage` class when the addon is enabled and disabled.

```csharp
using Nautilus.Handlers.TitleScreen;

public class MyCoolAddon : TitleAddon
{
    // Pass the required GUIDs for collaborations to the TitleAddon base
    public MyCoolAddon(params string[] requiredGUIDs) : base(requiredGUIDs) { }

    protected override void OnEnable()
    {
        ErrorMessage.AddError("MyCoolAddon enabled!");
    }

    protected override void OnDisable()
    {
        ErrorMessage.AddError("MyCoolAddon disabled!");
    }
}
```

There are also additional methods that can be overridden if additional behavior is required. Here are the two others that can be extended:
- `OnInitialize()`: Called once when all the addons are loaded in. Can be used for one time setups, such as in the `WorldObjectTitleAddon` where it is used to spawn the object passed in.
- `OnEnterLoadScreen()`: Called when the player starts loading a save. Can be used to cleanup persistent changes, such as in the `SkyChangeTitleAddon`.