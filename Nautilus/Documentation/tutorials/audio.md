# FMOD and Custom Audio

This is a tutorial on adding custom sounds to Subnautica using FMOD.

## What is FMOD?

[FMOD](https://www.fmod.com/) is the proprietary sound engine used by Subnautica. The specific version used by
Subnautica
is [FMOD for Unity](https://www.fmod.com/unity), also known as FMODUnity.

The developers of Subnautica used a program called FMOD Studio, software specifically designed to create sound events,
buses, effects, etc. for use in games. Unfortunately, modders cannot utilize this until the game's metadata is released.

Therefore, FMOD support for mods is relatively limited. However, many options still exist, as this guide will show.

## Can I Use Unity Audio?

While Unity audio (AudioSources, AudioClips, etc.) does function in Subnautica thanks to the BepInEx pack, it has
various issues:

- It is not affected by the volume slider.
- Any effects, such as muffling and reverb, are not supported.
- These sounds continue to play while the game is paused.
- A problematic setup can cause ear-piercing screeching sounds.
- There is a risk of it using the wrong sound device.
- Unity audio cannot directly be used in any of the game's sound systems.

There is, however, no issue with converting an AudioClip to an FMOD Event.

## Important Concepts

### Sounds

A `Sound` in FMOD is a struct which essentially points to byte data of an audio clip. Sounds also have a `MODE`, as
explained below. Sounds are necessary to create events.

To create a Sound, see [AudioUtils.CreateSound](https://subnauticamodding.github.io/Nautilus/api/Nautilus.Utility.AudioUtils.html#Nautilus_Utility_AudioUtils_CreateSound_UnityEngine_AudioClip_FMOD_MODE_) and similar methods in the
same class. In Nautilus, you can create a Sound from a Unity AudioClip (with an Asset Bundle) or a raw sound file.

You cannot (or at least generally should not) directly play a Sound. Instead, register the sound as an Event.

### Events

FMOD events are what the game actually recognizes, interacts with and executes to play audio. Events
can contain one sound or multiple random sounds, are assigned a bus, and can have various effects added.

Every FMOD Event has a path and an ID. In custom sounds these are interchangeable. The path/ID is required to play the
sound.

To directly register a sound (or multiple) as an event, use the methods in
the [CustomSoundHandler](xref:Nautilus.Handlers.CustomSoundHandler).

### FMODAssets

This is a concept created by Unknown Worlds, rather than being a part of FMOD. An `FMODAsset` is a type of
[ScriptableObject](https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html)
that contains fields for the event's `path` and `id`. These are used by the vast majority of the game's sound systems.
In most cases, the game's system only uses the path, but it's a good practice to set both values (though for custom
sounds, the two are interchangeable).

To quickly create an FMOD Asset, you can call
[AudioUtils.GetFmodAsset](https://subnauticamodding.github.io/Nautilus/api/Nautilus.Utility.AudioUtils.html#Nautilus_Utility_AudioUtils_GetFmodAsset_System_String_System_String_),
store the result, and use it as needed.
It is recommended that you cache this reference to prevent memory leaks.

[List of FMOD Events for SN1](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-FMODEvents.txt)

[List of FMOD Events for BZ](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/BZ-FMODEvents.txt)

#### Common ways to play FMOD Assets:

1. Assigning it to the `asset` field on one of the following built-in components. These components do not typically play
   the event automatically.
    - `FMOD_CustomEmitter`
    - `FMOD_CustomLoopingEmitter`
    - `FMOD_StudioEventEmitter`
2. Using `Utils.PlayFMODAsset(asset, position)`
    - Don't use the overload that doesn't take a position— this typically results in silence, regardless of mode.
    - Do not confuse `Utils` with `UWE.Utils`. These are two different classes.

### Buses

In FMOD, "buses" are used to organize sounds into categories, typically for volume control and special effects. For
instance, most buses are affected by various aspects of the game, such as being inside a base or swimming underwater.
This is how muffling effects are controlled.

Every sound event must be assigned a valid bus. You can find a complete list of bus paths for SN1
[here](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/SN1-FMODBuses.txt),
and a list for BZ
[here](https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Documentation/resources/BZ-FMODBuses.txt).

### Modes

FMOD sounds can have various 'modes' applied to them. At least some must be applied for any sound to work properly. This
is especially important for the distinction between directional (3D) and non-directional (2D) audio.

#### Recommended modes:

- For 3D sounds: [AudioUtils.StandardSoundModes_3D](xref:Nautilus.Utility.AudioUtils.StandardSoundModes_3D):
  `MODE._3D | MODE.ACCURATETIME | MODE._3D_LINEARSQUAREROLLOFF`
- For 2D sounds: [AudioUtils.StandardSoundModes_2D](xref:Nautilus.Utility.AudioUtils.StandardSoundModes_2D):
  `MODE._2D | MODE.ACCURATETIME`
- For music: [AudioUtils.StandardSoundModes_Stream](xref:Nautilus.Utility.AudioUtils.StandardSoundModes_Stream):
  `MODE._2D | MODE.ACCURATETIME | MODE.CREATESTREAM` (this is a complicated topic of optimization)

## The FModSoundBuilder

The [FModSoundBuilder](xref:Nautilus.FMod.FModSoundBuilder) is a helpful builder class that simplifies the registration
of audio in mods.

### Step 1: Define the audio source

The FModSoundBuilder requires a 'CustomSoundSource' to function properly. The two implementations of this in Nautilus
that you can use are the `AssetBundleSoundSource`, which takes in a loaded AssetBundle (so that you load clips by their
clip name), and the `ModFolderSoundSource`, which loads clips directly from a folder within your mod folder. Keep in
mind that the folder should be devoid of any other unrelated files if you choose to use that option.

Example 1 (AssetBundleSoundSource):

```csharp
// Load the asset bundle (keep in mind, you should cache the AssetBundle in an actual project, and never load one twice)
AssetBundle bundle = AssetBundleLoadingUtils.LoadFromAssetsFolder(Assembly, "assetbundlename");
// Tell the system to load clips from the given asset bundle by their name
CustomSoundSourceBase soundSource = new AssetBundleSoundSource(bundle);
```

Example 2 (ModFolderSoundSource)

```csharp
// Tell the system to load clips from a folder called "SoundsFolder", which is directly inside your mod folder
CustomSoundSourceBase soundSource = new ModFolderSoundSource("SoundsFolder");
```

### Step 2: Create the builder

```csharp
FModSoundBuilder builder = new FModSoundBuilder(soundSource);
```

### Step 3: Register events

> [!WARNING]
> Do not use any file extensions with the FModSoundBuilder class. Extensions should be excluded from any strings here.
> For example, it would be 'CreatureSound1', not 'CreatureSound1.mp3'.

Call the builder's `CreateNewEvent(id, bus)` method, passing in the ID/event path you want to register, and an existing
bus path.

Now, use the returned value to set any settings as necessary using the fluent syntax setup.
See [IFModSoundBuilder](xref:Nautilus.FMod.Interfaces.IFModSoundBuilder) for a list of possible functions.

Be sure to always call the `Register` method on the builder when you are done with a sound.

Examples:

```csharp
// Registers a generic underwater sound
builder.CreateNewEvent("EpicExplosionSound", Nautilus.Utility.AudioUtils.BusPaths.UnderwaterAmbient)
    .SetMode3D(3, 70) // Distance falloff starts at 3 meters, and it cannot be heard beyond 70 meters.
    .SetSound("ExplosionSound") // This could load ExplosionSound.mp3, ExplosionSound.wav, or an AssetBundle clip with the name, depending on the source and setup.
    .Register(); // Never forget to register your audio.

// Registers a 2D user interface sound
builder.CreateNewEvent("NewButtonSound", "bus:/master/SFX_for_pause/PDA_pause/all/SFX")
    .SetMode2D()
    .SetSounds(true, "NewButtonSound1", "NewButtonSound2") // This loads two sounds. The one that plays each time is random.
    .Register();

// Registers a creature sound
builder.CreateNewEvent("NewCreatureSound", "bus:/master/SFX_for_pause/PDA_pause/all/SFX/creatures")
    .SetMode3D(1, 20) // Distance falloff starts at 1 meter, and it cannot be heard beyond 20 meters.
    .SetSounds(true, s => s.StartsWith("CreatureSound")) // Loads all files that start with "CreatureSound", such as "CreatureSound1", "CreatureSound2", etc.
    .Register();

// Registers custom music
builder.CreateNewEvent("EpicBiomeMusic", AudioUtils.BusPaths.Music)
    .SetModeMusic() // Sets the mode to be 2D and optimized for music.
    .SetFadeDuration(2) // The music will take 2 seconds to fade out if stopped while playing.
    .SetSounds(true, "BiomeTrack1", "BiomeTrack2", "BiomeTrack3") // The music will play one of these tracks at random.
    .Register();
```

If you wanted to, you could now play one of those sounds like this:

```csharp
Utils.PlayFMODAsset(Nautilus.Utility.AudioUtils.GetFmodAsset("NewCreatureSound"), Player.main.transform.position);
```