using System.Reflection;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Nautilus.Patchers;
using Nautilus.Utility;

namespace Nautilus.Handlers;

/// <summary>
/// A handler class for registering your custom in-game mod options.
/// </summary>
public static class OptionsPanelHandler 
{

    /// <summary>
    /// Registers your mod options to the in-game menu.
    /// </summary>
    /// <param name="options">The mod options. Create a new child class inheriting from this one
    /// and add your options to it.</param>
    /// <seealso cref="ModOptions"/>
    public static void RegisterModOptions(ModOptions options)
    {
        OptionsPanelPatcher.modOptions.Add(options.Name, options);
    }

    /// <summary>
    /// Generates an options menu based on the attributes and members declared in the <see cref="ConfigFile"/>
    /// and registers it to the in-game menu.
    /// </summary>
    /// <typeparam name="T">A class derived from <see cref="ConfigFile"/> to generate the options menu from.</typeparam>
    /// <returns>An instance of the <typeparamref name="T"/> : <see cref="ConfigFile"/> with values loaded
    /// from the config file on disk.</returns>
    public static T RegisterModOptions<T>() where T : ConfigFile, new()
    {
        OptionsMenuBuilder<T> optionsMenuBuilder = new();
        RegisterModOptions(optionsMenuBuilder);
        optionsMenuBuilder.ConfigFileMetadata.Registered = true;

        MenuAttribute menuAttribute = typeof(T).GetCustomAttribute<MenuAttribute>(true)
                                      ?? new MenuAttribute(optionsMenuBuilder.Name);

        if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.SaveGame))
        {
            SaveUtils.RegisterOnSaveEvent(() => optionsMenuBuilder.ConfigFileMetadata.Config.Save());
        }

        if (menuAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.QuitGame))
        {
            SaveUtils.RegisterOnQuitEvent(() => optionsMenuBuilder.ConfigFileMetadata.Config.Save());
        }

        return optionsMenuBuilder.ConfigFileMetadata.Config;
    }
}