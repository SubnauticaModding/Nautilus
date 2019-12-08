namespace SMLHelper.V2.Interfaces
{
    using System;

    /// <summary>
    /// A handler class that offers simple ways to tap into functionality of the in game menu.
    /// </summary>
    public interface IIngameMenuHandler
    {
        /// <summary>
        /// Registers a simple <see cref="Action"/> method to invoke whenever the player saves the game via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke.</param>
        void RegisterOnSaveEvent(Action onSaveAction);

        /// <summary>
        /// Removes a method previously added through <see cref="RegisterOnSaveEvent(Action)"/> so it is no longer invoked when saving the game.<para/>
        /// If you plan on using this, do not register an anonymous method.
        /// </summary>
        /// <param name="onSaveAction">The method invoked.</param>
        void UnregisterOnSaveEvent(Action onSaveAction);

        /// <summary>
        /// Registers a simple <see cref="Action"/> method to invoke the <c>first time</c> the player saves the game via the in game menu.
        /// </summary>
        /// <param name="onSaveAction">The method to invoke. This action will not be invoked a second time.</param>
        void RegisterOneTimeUseOnSaveEvent(Action onSaveAction);
    }
}
