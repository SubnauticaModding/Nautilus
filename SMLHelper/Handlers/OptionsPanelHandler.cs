namespace SMLHelper.V2.Handlers
{
    using Options;
    using Patchers;
    using Interfaces;
    using SMLHelper.V2.Json;
    using System.Linq;

    /// <summary>
    /// A handler class for registering your custom in-game mod options.
    /// </summary>
    public class OptionsPanelHandler : IOptionsPanelHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IOptionsPanelHandler Main { get; } = new OptionsPanelHandler();

        private OptionsPanelHandler()
        {
            // Hide constructor
        }

        /// <summary>
        /// Registers your mod options to the in-game menu.
        /// </summary>
        /// <param name="options">The mod options. Create a new child class inheriting from this one
        /// and add your options to it.</param>
        /// <seealso cref="ModOptions"/>
        public static void RegisterModOptions(ModOptions options)
        {
            Main.RegisterModOptions(options);
        }

        /// <summary>
        /// Registers your mod options to the in-game menu.
        /// </summary>
        /// <param name="options">The mod options. Create a new child class inheriting from this one
        /// and add your options to it.</param>
        /// <seealso cref="ModOptions"/>
        void IOptionsPanelHandler.RegisterModOptions(ModOptions options)
        {
            OptionsPanelPatcher.modOptions.Add(options.Name, options);
        }

        /// <summary>
        /// Generates an options menu based on the attributes and members declared in the <see cref="ConfigFile"/>
        /// and registers it to the in-game menu.
        /// </summary>
        /// <typeparam name="T">A class derived from <see cref="ConfigFile"/> to generate the options menu from.</typeparam>
        /// <returns>An instance of the <typeparamref name="T"/> : <see cref="ConfigFile"/> with values loaded
        /// from the config file on file on disk.</returns>
        public static T RegisterModOptions<T>() where T : ConfigFile, new()
            => Main.RegisterModOptions<T>();

        /// <summary>
        /// Generates an options menu based on the attributes and members declared in the <see cref="ConfigFile"/>
        /// and registers it to the in-game menu.
        /// </summary>
        /// <typeparam name="T">A class derived from <see cref="ConfigFile"/> to generate the options menu from.</typeparam>
        /// <returns>An instance of the <typeparamref name="T"/> : <see cref="ConfigFile"/> with values loaded
        /// from the config file on file on disk.</returns>
        T IOptionsPanelHandler.RegisterModOptions<T>()
        {
            var configModOptions = new ConfigModOptions<T>();
            RegisterModOptions(configModOptions);

            var modOptionsAttribute = typeof(T).GetCustomAttributes(typeof(MenuAttribute), true).SingleOrDefault() as MenuAttribute
                ?? new MenuAttribute();

            if (modOptionsAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.SaveGame))
                IngameMenuHandler.RegisterOnSaveEvent(() => configModOptions.Config.Save());
            if (modOptionsAttribute.SaveOn.HasFlag(MenuAttribute.SaveEvents.QuitGame))
                IngameMenuHandler.RegisterOnQuitEvent(() => configModOptions.Config.Save());

            return configModOptions.Config;
        }
    }
}
