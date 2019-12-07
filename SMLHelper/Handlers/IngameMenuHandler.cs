namespace SMLHelper.V2.Handlers
{
    using System;
    using SMLHelper.V2.Interfaces;
    using SMLHelper.V2.Patchers;

    /// <summary>
    /// A handler class that offers simple ways to tap into functionality of the in game menu.
    /// </summary>
    public class IngameMenuHandler : IIngameMenuHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IIngameMenuHandler Main { get; } = new IngameMenuHandler();

        private IngameMenuHandler() { } // Hide constructor

        /// <summary>
        /// Registers a simple <see cref="Action"/> method to invoke whenever the player saves the game via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke.</param>
        public static void RegisterOnSaveEvent(Action onSaveAction)
        {
            Main.RegisterOnSaveEvent(onSaveAction);
        }

        /// <summary>
        /// Removes a method previously added through <see cref="RegisterOnSaveEvent(Action)"/> so it is no longer invoked when saving the game.<para/>
        /// If you plan on using this, do not register an anonymous method.
        /// </summary>
        /// <param name="onSaveAction">The method invoked.</param>
        public static void UnregisterOnSaveEvent(Action onSaveAction)
        {
            Main.UnregisterOnSaveEvent(onSaveAction);
        }

        /// <summary>
        /// Registers a simple <see cref="Action"/> method to invoke whenever the player saves the game via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke.</param>
        public static void RegisterOneTimeUseOnSaveEvent(Action onSaveAction)
        {
            Main.RegisterOneTimeUseOnSaveEvent(onSaveAction);
        }

        /// <summary>
        /// Registers a simple <see cref="Action"/> method to invoke whenever the player saves the game via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke.</param>
        void IIngameMenuHandler.RegisterOnSaveEvent(Action onSaveAction)
        {
            IngameMenuPatcher.OnSaveEvents += onSaveAction;
        }

        /// <summary>
        /// Removes a method previously added through <see cref="RegisterOnSaveEvent(Action)"/> so it is no longer invoked when saving the game.<para/>
        /// If you plan on using this, do not register an anonymous method.
        /// </summary>
        /// <param name="onSaveAction">The method invoked.</param>
        void IIngameMenuHandler.UnregisterOnSaveEvent(Action onSaveAction)
        {
            IngameMenuPatcher.OnSaveEvents -= onSaveAction;
        }

        /// <summary>
        /// Registers a simple <see cref="Action"/> method to invoke the <c>first time</c> the player saves the game via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke. This action will not be invoked a second time.</param>
        void IIngameMenuHandler.RegisterOneTimeUseOnSaveEvent(Action onSaveAction)
        {
            IngameMenuPatcher.AddOneTimeUseSaveEvent(onSaveAction);
        }
    }
}
