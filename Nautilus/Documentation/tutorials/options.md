# Adding Options

Most mods will require/use some sort of user configuration from hotkeys to values to colors. Nautilus offers a wrapper to help easily create an entry in the settings for your mods options.

Depending on the use case you can use a super simple [ConfigFile](#configfile) or a more detailed and customizable [ModOptions](#modoptions).

## ConfigFile

A [ConfigFile](xref:Nautilus.Json.ConfigFile) is the simplest way of managing your mod's options automatically. This is ideal if you just want a basic plain config with little custom functionality.

The [ConfigFile](xref:Nautilus.Json.ConfigFile) by itself is just a class which automatically stores persistent data to disk but with the addition of custom [Attributes](xref:Nautilus.Options.Attributes) you can make it display nicely in game too.

### Creating your `ConfigFile`

The following code is an example of how you could represent an integer value with a slider in game:

```csharp
using Nautilus.Json;
using Nautilus.Options.Attributes;

/// The Menu attribute allows us to set the title of our section within the "Mods" tab of the options menu.
[Menu("My Options Menu")]
public class MyConfig : ConfigFile
{
    /// A Slider attribute is used to represent a numeric value as a slider in the options menu with a
    /// minimum and maximum value. By default, the minimum value is 0 and maximum is 100.
    /// 
    /// In this example we are setting a minimum value of 0 and a maximum of 50, with a
    /// DefaultValue of 25 which will be represented by a notch on the slider.
    [Slider("My slider", 0, 50, DefaultValue = 25)]
    /// This is the actual definition for the value which should be saved. Its value controls the
    /// initial value that our option will have upon opening the game for the first time.
    public int SliderValue = 15;
}
```

See [Attributes](xref:Nautilus.Options.Attributes) for a full list of available control types and attribute arguments.

## ModOptions

A [ModOptions](xref:Nautilus.Options.ModOptions) is the more advanced way of managing your mod's options. This is ideal if you require more customization than is provided or want to manage your mods config separately (eg via BepInEx config).

The [ModOptions](xref:Nautilus.Options.ModOptions) by itself is just a class which helps display your options nicely in game and relies on other classes to persist data across game loads (eg BepInEx config). [OptionItems](xref:Nautilus.Options.OptionItem) are added to your section with the `AddItem` function. When options are added to the list *they will only be created once*, this means that if they are changed outside of Nautilus (ex. BepInEx ConfigurationManager) they will not be updated inside of the Nautilus mods window. In order to refresh your values on every opening of the menu you must override `ModOptions.BuildModOptions` which is executed once every time the menu is opened.

### Creating your `ModOptions`

The following code is an example of how you could represent an integer value with a slider in game:

```csharp
using Nautilus.Options;

/// Inherit from the abstract ModOptions
public class MyModOptions : ModOptions
{
    /// The base ModOptions class takes a string name as an argument
    public MyModOptions() : base("My Mod Options")
    {
        /// A ModSliderOption is used to draw a numeric value as a slider in the options menu with a
        /// minimum and maximum value.
        /// 
        /// In this example we are setting a minimum value of 0 a maximum of 50,  a
        /// DefaultValue of 25 (which will be represented by a notch on the slider)
        /// and an initial value of 15.
        AddItem(ModSliderOption.Create("SliderId", "My Slider", 0, 50, 15, 25));
    }
}
```

### Saving your config

As mentioned above, a [ModOptions](xref:Nautilus.Options.ModOptions) does not handle saving and retrieving config from disk and as such it does not persist across saves by default. One of the most convenient ways of accounting for this is to utilize BepInEx as a config store (we have it, may as well use it!).

There are a few ways to accomplish this:

(1) (**Recommended**) Using the builtin extensions for converting BepInEx `ConfigEntry` instances to `OptionItem` instances. See [ConfigEntryExtensions](xref:Nautilus.Options.ConfigEntryExtensions) for possible options.

(2) Manually hooking to an external (likely BepInEx) config with individual OnChanged. See [reacting to changed values (individual)](#individual-onchanged) below for further explanation on the details.

(3) Manually hooking to an external (likely BepInEx) config with the global OnChanged. See [reacting to changed values (global)](#global-onchanged) below for further explanation on the details.

In all cases, in order to support changes made outside of Nautilus (ex. BepInEx ConfigurationManager) you will need to override `ModOptions.BuildModOptions` which is executed once every time the menu is opened.

### Reacting to changed values

Nautilus provides three different ways of dealing with changed values when using `ModOptions`:

(1) Adding a listener to an `OptionItem` OnChanged.

(2) Adding a listener to the global OnChanged event. This method is the most similar to SML 2.0's change handler.

(3) A combination of (1) and (2)

#### Individual OnChanged

Every `ModOption` has a certain subclass of `OptionEventArgs` (eg `ToggleChangedEventArgs`) which is passed to the option's OnChanged event. You can add your own function to the OnChanged for it to be run whenever the value is changed as shown below:

```csharp
using Nautilus.Options;

public class MyModOptions : ModOptions
{
    public MyModOptions() : base("My Mod Options")
    {
        OnChanged += Options_Changed;

        var Slider1 = ModSliderOption.Create("Slider1", "My Slider", 0, 50, 15, 25);
        Slider1_OnChanged += Slider1_OnChanged;
        AddItem(Slider1);

        var Toggle1 = ModToggleOption.Create("Toggle1", "My Toggle", true);
        Toggle1_OnChanged += Toggle1_OnChanged;
        AddItem(Toggle1);

        var Toggle2 = ModToggleOption.Create("Toggle2", "My Other Toggle", true);
        Toggle2_OnChanged += (object sender, ToggleChangedEventArgs e) => { /// Handle changes here };
        
        
        AddItem(Toggle2);
    }

    private void Slider1_OnChanged(object sender, SliderChangedEventArgs e)
    {
        /// Handle changes here
    }

    private void Toggle1_OnChanged(object sender, ToggleChangedEventArgs e)
    {
        /// Handle changes here
    }
}
```

#### Global OnChanged

The base `ModOptions` has an `OnChanged` which every individual option `OnChanged` bubbles up to. You can add your own handler function to this class but you will need to differentiate between each type/individual option which can be done multiple ways. An example class may look like:

```csharp
using Nautilus.Options;

public class MyModOptions : ModOptions
{
    public MyModOptions() : base("My Mod Options")
    {
        OnChanged += Options_Changed;

        AddItem(ModSliderOption.Create("Slider1", "My Slider", 0, 50, 15, 25));
        AddItem(ModToggleOption.Create("Toggle1", "My Toggle", true));
        AddItem(ModToggleOption.Create("Toggle2", "My Other Toggle", true));
    }

    private void Options_Changed(object sender, OptionEventArgs e)
    {
        switch (e)
        {
            case SliderChangedEventArgs sliderArgs:
                switch (sliderArgs.Id)
                {
                    case "Slider1":
                        /// Handle changes here
                        break;
                }
                break;
            case ToggleChangedEventArgs toggleArgs:
                switch (toggleArgs.Id)
                {
                    case "Toggle1":
                        /// Handle changes here
                        break;
                    case "Toggle2":
                        /// Handle changes here
                        break;
                }
                break;
        }
    }
}
```